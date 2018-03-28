using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Helpers;
using AutoMapper;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;

namespace Library.API.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;

        public UsersController(ILibraryRepository libraryRepository,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;

        }

        [HttpGet(Name = "GetUsers")]
        public IActionResult GetUsers(UserResourceParameters userResourceParameters)
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

            var previousPageLink = usersFromRepo.HasPrevious ? 
                CreateUsersResouceUri(userResourceParameters,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = usersFromRepo.HasNext ?
                CreateUsersResouceUri(userResourceParameters,
                ResourceUriType.PreviousPage) : null;

            var paginationMetadata = new
            {
                totalCount = usersFromRepo.TotalCount,
                pageSize = usersFromRepo.PageSize,
                currentPage = usersFromRepo.CurrentPage,
                totalPages = usersFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var users = Mapper.Map<IEnumerable<UserDto>>(usersFromRepo);
            return Ok(users.ShapeData(userResourceParameters.Fields));
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
            return Ok(user.ShapeData(fields));
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserForCreationDto user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            var userEntity = Mapper.Map<User>(user);


            _libraryRepository.AddUser(userEntity);

            if (! _libraryRepository.Save())
            {
                //Throwing exception here can hit the performance, but to simplify loggin it is better
                // than returning the status code from here. 
                throw new Exception("Creating an author failed on save.");
            }

            var userToReturn = Mapper.Map<UserDto>(userEntity);

            return CreatedAtRoute("GetUser",
                new { id = userToReturn.Id },
                userToReturn);
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

        [HttpDelete("{id}")]
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
    }
}
