using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/usercollections")]
    public class UserCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public UserCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection(
            [FromBody] IEnumerable<UserForCreationDto> userCollection)
        {
            if (userCollection == null)
            {
                return BadRequest();
            }

            var userEntities = Mapper.Map<IEnumerable<User>>(userCollection);

            foreach (var user in userEntities)
            {
                _libraryRepository.AddUser(user);
            }

            if(!_libraryRepository.Save())
            {
                throw new Exception("Create an author collection failed on save.");
            }

            var userCollectionToReturn = Mapper.Map<IEnumerable<UserDto>>(userEntities);
            var idAsStrings = string.Join(",", userCollectionToReturn.Select(a => a.Id));

            return CreatedAtRoute("GetUserCollection",
                new { ids = idAsStrings },
                userCollectionToReturn);
        }

        [HttpGet("({ids})", Name ="GetUserCollection")]
        public IActionResult GetUserCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var userEntities = _libraryRepository.GetUsers(ids);

            if (ids.Count() != userEntities.Count())
            {
                return NotFound();
            }
                
            var usersToReturn = Mapper.Map<IEnumerable<UserDto>>(userEntities);
            return Ok(usersToReturn);
        }
    }
}
