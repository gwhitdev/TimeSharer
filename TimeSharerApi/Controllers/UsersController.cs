using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeSharerApi.Models;
using TimeSharerApi.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace TimeSharerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUsersService _usersService;
        public Response<List<User>> UserResponse { get; set; }

        public UsersController(ILoggerFactory loggerFactory, IUsersService usersService)
        {
            _logger = loggerFactory.CreateLogger<UsersController>();
            _usersService = usersService;
            UserResponse = new();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Response<List<User>>))]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Trying to get list of users");
                var users = _usersService.Get().ToList();
                _logger.LogInformation($"Search completed. Found {users.Count} records.");
                UserResponse.NumberOfRecordsFound = users.Count;
                UserResponse.Message = "Search completed.";
                UserResponse.Data = users;
                return Ok(new[] { UserResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogError("Bad request");
            return BadRequest();
        }

        [HttpGet("{id}", Name = "GetUserById")]
        [ProducesResponseType(200, Type = typeof(Response<List<User>>))]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            try
            {
                var result = _usersService.Get(id);
                List<User> users = new() { result };
                if (users.FirstOrDefault() == null)
                {
                    UserResponse.Success = false;
                    UserResponse.Message = $"Error: User with id {id} cannot be found. ";
                    UserResponse.Data = new();
                    _logger.LogInformation($"Not found: cannot find User record with id {id}.");
                    return NotFound(new[] { UserResponse });
                }
                UserResponse.NumberOfRecordsFound = users.Count;
                UserResponse.Message = $"Found User {id}.";
                UserResponse.Data = users;
                return Ok(new[] { UserResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error updating organisation record in the DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(User))]
        [ProducesResponseType(401)]
        public IActionResult Create([FromBody] User UserIn)
        {
            User UserToCreate = _usersService.Create(UserIn);
            return CreatedAtRoute(
                routeName: "GetUserById",
                routeValues: new { id = UserToCreate.Id.ToString() },
                value: UserToCreate);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(UserDetails))]
        public IActionResult Update(string id, [FromBody] UserDetails UserDetailsIn)
        {
            id = id.ToLower();

            UserResponse.Success = false;

            if (UserDetailsIn == null)
            {
                UserResponse.Message = "User submitted was null";
                return BadRequest(new[] { UserResponse });
            }

            try
            {
                var existing = _usersService.Get(id);

                if (existing == null)
                {
                    UserResponse.Message = "User record not found";
                    return NotFound(new[] { UserResponse });
                }

                var updated = _usersService.Update(id, UserDetailsIn);
                var UserFound = new List<User>() { _usersService.Get(id) };

                UserResponse.Data = UserFound;
                if (!updated)
                {
                    UserResponse.NumberOfRecordsFound = UserFound.Count;
                    UserResponse.Message = "Update didn't work!";
                    return BadRequest(new[] { UserResponse });
                }

                UserResponse.Success = true;
                UserResponse.NumberOfRecordsFound = UserFound.Count;
                UserResponse.Message = "User record updated";
                return Ok(new[] { UserResponse });
            }

            catch (MongoException ex)
            {
                _logger.LogError($"Error updating User record in the DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogError("Error: update request cannot be handled, bad request");
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _logger.LogInformation($"Trying to delete User record {id}");
                var exists = _usersService.Get(id);

                UserResponse.Success = false;
                UserResponse.Data = new();

                if (exists == null)
                {
                    UserResponse.Message = "User record not found.";
                    return NotFound(new[] { UserResponse });
                }

                bool UserRemoved = _usersService.Delete(id);

                if (!UserRemoved)
                {
                    UserResponse.Message = "User record not deleted.";
                    return BadRequest(new[] { UserResponse });
                }

                UserResponse.Success = true;
                UserResponse.Message = $"User record with id {id} deleted.";
                return Ok(new[] { UserResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error removing from DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogInformation("Error: delete request cannot be handled, bad request.");
            return BadRequest();

        }
    }
}