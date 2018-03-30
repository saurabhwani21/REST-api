using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/users/{userId}/scores")]
    public class ScoresController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private ILogger<ScoresController> _logger;
        private IUrlHelper _urlHelper;

        public ScoresController(ILibraryRepository libraryRepository,
            ILogger<ScoresController> logger, IUrlHelper urlHelper)    
        {
            _logger = logger;
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }


        //public IActionResult GetScoresForUser(Guid userId)
        [HttpGet(Name = "GetScores")]
        public IActionResult GetScoresForUser(Guid userId, ScoresResourceParameters scoreResourceParameters)

        {
            if (!_libraryRepository.UserExists(userId))
            {
                return NotFound();
            }

            var scoresFromRepo = _libraryRepository.GetScoresForUser(userId, scoreResourceParameters);

            var previousPageLink = scoresFromRepo.HasPrevious ?
                CreateScoreResouceUri(scoreResourceParameters,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = scoresFromRepo.HasNext ?
                CreateScoreResouceUri(scoreResourceParameters,
                ResourceUriType.PreviousPage) : null;

            var paginationMetadata = new
            {
                totalCount = scoresFromRepo.TotalCount,
                pageSize = scoresFromRepo.PageSize,
                currentPage = scoresFromRepo.CurrentPage,
                totalPages = scoresFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var scoresForUser = Mapper.Map<IEnumerable<ScoreDto>>(scoresFromRepo);

            scoresForUser = scoresForUser.Select(score =>
            {
                score = CreateLinksForScore(score);
                return score;
            });

            var wrapper = new LinkedCollectionResourceWrapperDto<ScoreDto>(scoresForUser);
            return Ok(CreateLinksForScores(wrapper));
        }

        private string CreateScoreResouceUri(
           ScoresResourceParameters scoreResourceParameters,
           ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetScores",
                      new
                      {
                          instanceScore = scoreResourceParameters.InstanceScore,
                          pageNumber = scoreResourceParameters.PageNumber - 1,
                          pageSize = scoreResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetScores",
                      new
                      {
                          instanceScore = scoreResourceParameters.InstanceScore,
                          pageNumber = scoreResourceParameters.PageNumber + 1,
                          pageSize = scoreResourceParameters.PageSize
                      });

                default:
                    return _urlHelper.Link("GetScores",
                    new
                    {
                        instanceScore = scoreResourceParameters.InstanceScore,
                        pageNumber = scoreResourceParameters.PageNumber,
                        pageSize = scoreResourceParameters.PageSize
                    });
            }
        }

        [HttpGet("{id}", Name = "GetScoreForUser")]
        public IActionResult GetScoreForUser(Guid userId, Guid id)
        {
            if (!_libraryRepository.UserExists(userId))
            {
                return NotFound();
            }

            var scoreForUserFromRepo = _libraryRepository.GetScoreForUser(userId, id);
            if (scoreForUserFromRepo == null)
            {
                return NotFound();
            }

            var scoreForUser = Mapper.Map<ScoreDto>(scoreForUserFromRepo);
            return Ok(CreateLinksForScore(scoreForUser));
        }

        [HttpPost(Name = "CreateScoreForUser")]
        public IActionResult CreateScoreForUser(Guid userId,
            [FromBody] ScoreForCreationDto score)
        {
            //Check if the request is properly deserialized
            if (score == null)
            {
                return BadRequest();
            }

            //to validate constraints on input
            //if(!ModelState.IsValid)
            //{
            //    return new UnprocessableEntityObjectResult(ModelState);
            //}

            if (!_libraryRepository.UserExists(userId))
            {
                return NotFound();
            }

            var scoreEntity = Mapper.Map<Score>(score);

            _libraryRepository.AddScoreForUser(userId, scoreEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a score for user {userId} failed on save.");
            }

            var scoreToReturn = Mapper.Map<ScoreDto>(scoreEntity);

            return CreatedAtRoute("GetScoreForUser",
                new { userId = userId, id = scoreToReturn.Id },
                CreateLinksForScore(scoreToReturn));
        }

        [HttpDelete("{id}", Name = "DeleteScoreForUser")]
        public IActionResult DeleteScoreForUser(Guid userId, Guid id)
        {
            if (!_libraryRepository.UserExists(userId))
            {
                return NotFound();
            }

            var scoreForUserFromRepo = _libraryRepository.GetScoreForUser(userId, id);
            if (scoreForUserFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteScore(scoreForUserFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting score {id} for user {userId} failed on save.");
            }

            _logger.LogInformation(100, $"Score {id} for user {userId} was deleted.");

            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateScoreForUser")]
        public IActionResult UpdateScoreForUser(Guid userId, Guid id,
            [FromBody] ScoreForUpdateDto score)
        {
            if (score == null)
            {
                return BadRequest();        
            }

            //Check if the user exists
            if (!_libraryRepository.UserExists(userId))
            {
                return NotFound();
            }

            //Check if the score exists
            var scoreForUserFromRepo = _libraryRepository.GetScoreForUser(userId, id);
            if (scoreForUserFromRepo == null)
            {
                //Map the score from the requset body to score entity
                var scoreToAdd = Mapper.Map<Score>(score);
                scoreToAdd.Id = id;

                _libraryRepository.AddScoreForUser(userId, scoreToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting score {id} for user {userId} failed on save");             
                }

                var scoreToReturn = Mapper.Map<ScoreDto>(scoreToAdd);

                return CreatedAtRoute("GetScoreForUser",
                    new { userId = userId, id = scoreToReturn.Id },
                    scoreToReturn);
            }

            //map
            //apply update
            //map back to entity
            //all this is taken care by the automapper
            Mapper.Map(score, scoreForUserFromRepo);
            //Now the entity contains the updated info

            _libraryRepository.UpdateScoreForUser(scoreForUserFromRepo);

            if(!_libraryRepository.Save())
            {
                throw new Exception($"Updating score {id} for user {userId} failed on save.");
            }

            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateScoreForUser")]
        public IActionResult PartiallyUpdateScoreForUser(Guid userId, Guid id,
            [FromBody] JsonPatchDocument<ScoreForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.UserExists(userId))
            {
                return NotFound();
            }

            var scoreForUserFromRepo = _libraryRepository.GetScoreForUser(userId, id);
            if (scoreForUserFromRepo == null)
            {
                var scoreDto = new ScoreForUpdateDto();
                patchDoc.ApplyTo(scoreDto);

                var scoreToAdd = Mapper.Map<Score>(scoreDto);
                scoreToAdd.Id = id;

                _libraryRepository.AddScoreForUser(userId, scoreToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting score {id} for author {userId} failed on save");
                }

                var scoreToReturn = Mapper.Map<ScoreDto>(scoreToAdd);
                return CreatedAtRoute("GetScoreForUser",
                    new { userId = userId, id = scoreToReturn.Id },
                    scoreToReturn);
            }

            var scoreToPatch = Mapper.Map<ScoreForUpdateDto>(scoreForUserFromRepo);

            patchDoc.ApplyTo(scoreToPatch);

            //add validation

            Mapper.Map(scoreToPatch, scoreForUserFromRepo);

            _libraryRepository.UpdateScoreForUser(scoreForUserFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching score {id} for user {userId} failed on save");
            }

            return NoContent();
        }

        private ScoreDto CreateLinksForScore(ScoreDto score)
        {
            score.Links.Add(new LinkDto(_urlHelper.Link("GetScoreForUser",
                new { id = score.Id }),
                "self",
                "Get"));

            score.Links.Add(new LinkDto(_urlHelper.Link("DeleteScoreForUser",
                new { id = score.Id }),
                "delete_score",
                "DELETE"));

            score.Links.Add(new LinkDto(_urlHelper.Link("UpdateScoreForUser",
                new { id = score.Id }),
                "update_score",
                "PUT"));

            score.Links.Add(new LinkDto(_urlHelper.Link("PartiallyUpdateScoreForUser",
                new { id = score.Id }),
                "partially_update_score",
                "PATCH"));
            return score;
        }

        private LinkedCollectionResourceWrapperDto<ScoreDto> CreateLinksForScores(
            LinkedCollectionResourceWrapperDto<ScoreDto> scoresWrapper)
        {
            scoresWrapper.Links.Add(
                new LinkDto(_urlHelper.Link("GetScores", new { }),
                "self",
                "GET"));

            return scoresWrapper;
        }
    }
}
