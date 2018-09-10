using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Helpers;
using AutoMapper;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Library.API.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        private ILibraryRepository _libraryRepository;       
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;

        private readonly JsonSerializerSettings _serializerSettings;

        public UsersController(ILibraryRepository libraryRepository,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;       
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;                     

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

        }

        [HttpGet(Name = "GetUsers")]
        [HttpHead]
        public IActionResult GetUsers(UserResourceParameters userResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if(!_propertyMappingService.ValidMappingExistsFor<UserDto, User>
                (userResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<UserDto>
                (userResourceParameters.Fields))
            {
                return BadRequest();
            }

            var usersFromRepo = _libraryRepository.GetUsers(userResourceParameters);


            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = usersFromRepo.TotalCount,
                    pageSize = usersFromRepo.PageSize,
                    currentPage = usersFromRepo.CurrentPage,
                    totalPages = usersFromRepo.TotalPages,

                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForUsers(userResourceParameters,
                    usersFromRepo.HasNext, usersFromRepo.HasPrevious);

                var users = Mapper.Map<IEnumerable<UserDto>>(usersFromRepo);
                var shapedUsers = users.ShapeData(userResourceParameters.Fields);

                var shapedUsersWithLinks = shapedUsers.Select(user =>
                {
                    var userAsDictionary = user as IDictionary<string, object>;
                    var userLinks = CreateLinksForUser(
                        (Guid)userAsDictionary["Id"], userResourceParameters.Fields);

                    userAsDictionary.Add("links", userLinks);
                    return userAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedUsersWithLinks,
                    links = links
                };
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = usersFromRepo.HasPrevious ? CreateUsersResouceUri(userResourceParameters, 
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = usersFromRepo.HasNext ? CreateUsersResouceUri(userResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = usersFromRepo.TotalCount,
                    pageSize = usersFromRepo.PageSize,
                    currentPage = usersFromRepo.CurrentPage,
                    totalPages = usersFromRepo.TotalPages,

                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                var users = Mapper.Map<IEnumerable<UserDto>>(usersFromRepo);
                return Ok(users.ShapeData(userResourceParameters.Fields));
            }
        }

        private string CreateUsersResouceUri(
            UserResourceParameters userResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetUsers",
                      new
                      {
                          fields = userResourceParameters.Fields,
                          orderBy = userResourceParameters.OrderBy,
                          firstname = userResourceParameters.FirstName,
                          lastname = userResourceParameters.LastName,
                          imei = userResourceParameters.IMEI,
                          pageNumber = userResourceParameters.PageNumber - 1,
                          pageSize = userResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetUsers",
                      new
                      {
                          fields = userResourceParameters.Fields,
                          orderBy = userResourceParameters.OrderBy,
                          firstname = userResourceParameters.FirstName,
                          lastname = userResourceParameters.LastName,
                          imei = userResourceParameters.IMEI,
                          pageNumber = userResourceParameters.PageNumber + 1,
                          pageSize = userResourceParameters.PageSize
                      });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link("GetUsers",
                    new
                    {
                        fields = userResourceParameters.Fields,
                        orderBy = userResourceParameters.OrderBy,
                        firstname = userResourceParameters.FirstName,
                        lastname = userResourceParameters.LastName,
                        imei = userResourceParameters.IMEI,
                        pageNumber = userResourceParameters.PageNumber,
                        pageSize = userResourceParameters.PageSize
                    });
            }
        }


        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetUser(Guid id, [FromQuery] string fields, UserResourceParameters userResourceParameters)
        {

            if (!_typeHelperService.TypeHasProperties<UserDto>
                (userResourceParameters.Fields))
            {
                return BadRequest();
            }

            var userFromRepo = _libraryRepository.GetUser(id);

            if (userFromRepo == null)
            {
                return NotFound();
            }

            var user = Mapper.Map<UserDto>(userFromRepo);

            var links = CreateLinksForUser(id, fields);

            var linkedResourceToReturn = user.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost("login")]
        public IActionResult Validate([FromBody] UsernamePasswordValidation credentials)
        {
            //Give T/F if user is validated
            Uid userObject = _libraryRepository.ValidateUser(credentials.username, credentials.password);
            return (userObject == null) ? Ok(null) : Ok(userObject);            
        }

        [HttpPost(Name = "CreateUser")]
        public IActionResult CreateUser([FromBody] UserForCreationDto user)
        {
            // Checks if the information object received is empty
            if (user == null)
            {
                return BadRequest();
            }

            //Checks if the same username already exists.
            if (_libraryRepository.UserExists(user.Username))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            user.Password = Helpers.GenerateHash.encryptPassword(user.Password);

            var userEntity = Mapper.Map<User>(user);
            
            _libraryRepository.AddUser(userEntity);

            if (! _libraryRepository.Save())
            {
                //Throwing exception here can hit the performance, but to simplify loggin it is better
                // than returning the status code from here. 
                throw new Exception("Creating an author failed on save.");
            }

            var userToReturn = Mapper.Map<UserDto>(userEntity);

            var links = CreateLinksForUser(userToReturn.Id, null);

            var linkedResourceToReturn = user.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetUser",
                new { id = userToReturn.Id },
                linkedResourceToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockUserCreation(Guid id)
        {
            if (_libraryRepository.UserExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteUser")]
        public IActionResult DeleteUser(Guid id)
        {
            var userFromRepo = _libraryRepository.GetUser(id);
            if (userFromRepo == null)
            {
                return NotFound();
            }

            //Deleting user deletes all its scores as well.
            _libraryRepository.DeleteUser(userFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting user {id} failed on save");
            }

            return NoContent();
        }

        private IEnumerable<LinkDto> CreateLinksForUser(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDto(_urlHelper.Link("GetUser", new { id = id }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new LinkDto(_urlHelper.Link("GetUser", new { id = id, fields = fields}),
                    "self",
                    "GET"));
            }

            links.Add(
                new LinkDto(_urlHelper.Link("DeleteUser", new { id = id }),
                "delete_user",
                "DELETE"));

            links.Add(
                new LinkDto(_urlHelper.Link("CreateScoreForUser", new { userId = id }),
                "create_score_for_user",
                "POST"));

            //GetScores == GetScoresForUser
            links.Add(
                new LinkDto(_urlHelper.Link("GetScores", new { userId = id }),
                "scores",
                "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForUsers(
            UserResourceParameters userResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateUsersResouceUri(userResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateUsersResouceUri(userResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateUsersResouceUri(userResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        [HttpOptions]
        public IActionResult GetAuthorOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }
    }
}
