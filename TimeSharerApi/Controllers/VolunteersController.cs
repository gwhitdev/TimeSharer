using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeSharerApi.Models;
using TimeSharerApi.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace TimeSharerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IVolunteerService _volunteersService;
        public Response<List<Volunteer>> VolunteersResponse { get; set; }

        public VolunteersController(ILoggerFactory loggerFactory, IVolunteerService volunteersService)
        {
            _logger = loggerFactory.CreateLogger<VolunteersController>();
            _volunteersService = volunteersService;
            VolunteersResponse = new();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Response<List<Volunteer>>))]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Trying to get list of volunteers");
                var volunteers = _volunteersService.Get().ToList();
                _logger.LogInformation($"Search completed. Found {volunteers.Count} volunteers.");
                VolunteersResponse.NumberOfRecordsFound = volunteers.Count;
                VolunteersResponse.Data = volunteers;
                VolunteersResponse.Message = "Search completed.";
                return Ok(new[] { VolunteersResponse });
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
            }
            _logger.LogError("Bad request");
            return BadRequest();
        }

        [HttpGet("{id}", Name = "GetvolunteerById")]
        [ProducesResponseType(200, Type = typeof(Volunteer))]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            try
            {
                var result = _volunteersService.Get(id);
                List<Volunteer> volunteers = new() { result };

                if (volunteers.FirstOrDefault() == null)
                {
                    VolunteersResponse.NumberOfRecordsFound = 0;
                    VolunteersResponse.Success = false;
                    VolunteersResponse.Data = new();
                    VolunteersResponse.Message = $"Error: volunteer with id {id} cannot be found. ";
                    _logger.LogInformation($"Not found: cannot find volunteer record with id {id}.");
                    return NotFound(new[] { VolunteersResponse });
                }
                VolunteersResponse.Data = volunteers;
                VolunteersResponse.NumberOfRecordsFound = volunteers.Count;
                VolunteersResponse.Message = $"Found volunteer: {id}.";
                return Ok(new[] { VolunteersResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogDebug($"Error getting volunteer record in the DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error: {ex.Message}");
            }
            return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Volunteer))]
        [ProducesResponseType(401)]
        public IActionResult Create([FromBody] Volunteer volunteerIn)
        {
            Volunteer volunteerToCreate = _volunteersService.Create(volunteerIn);
            return CreatedAtRoute(
                routeName: "GetvolunteerById",
                routeValues: new { id = volunteerToCreate.Id.ToString() },
                value: volunteerToCreate);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(VolunteerDetails))]
        public IActionResult Update(string id, [FromBody] VolunteerDetails volunteerDetailsIn)
        {
            id = id.ToLower();

            VolunteersResponse.Success = false;

            if (volunteerDetailsIn == null)
            {
                VolunteersResponse.Message = "volunteer submitted was null.";
                return BadRequest(new[] { VolunteersResponse });
            }

            if (!ModelState.IsValid)
            {
                VolunteersResponse.Message = $"Not all fields were supplied! {ModelState}";
                return BadRequest(new[] { VolunteersResponse });
            }

            try
            {
                var existing = _volunteersService.Get(id);

                if (existing == null)
                {
                    VolunteersResponse.Message = "volunteer record not found";
                    return NotFound(new[] { VolunteersResponse });
                }

                var updated = _volunteersService.Update(id, volunteerDetailsIn);
                var volunteerFound = new List<Volunteer>() { _volunteersService.Get(id) };

                VolunteersResponse.Data = volunteerFound;
                if (!updated)
                {
                    VolunteersResponse.NumberOfRecordsFound = volunteerFound.Count;
                    VolunteersResponse.Message = "Update didn't work!";
                    return BadRequest(new[] { VolunteersResponse });
                }
                VolunteersResponse.Success = true;
                VolunteersResponse.NumberOfRecordsFound = volunteerFound.Count;
                VolunteersResponse.Message = "volunteer record updated";
                return Ok(new[] { VolunteersResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogDebug($"Error updating volunteer record in the DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error: {ex.Message}");
            }
            _logger.LogInformation("Error: update request cannot be handled, bad request.");
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _logger.LogInformation($"Trying to delete volunteer record {id}");
            try
            {
                var exists = _volunteersService.Get(id);

                VolunteersResponse.Success = false;
                VolunteersResponse.Data = new List<Volunteer>();

                if (exists == null)
                {
                    VolunteersResponse.Message = "Volunteer record not found. Cannot delete.";
                    return NotFound(new[] { VolunteersResponse });
                }

                bool volunteerRemoved = _volunteersService.Delete(id);

                if (!volunteerRemoved)
                {
                    VolunteersResponse.Message = "Volunteer record not deleted";
                    return BadRequest(new[] { VolunteersResponse });
                }

                VolunteersResponse.Success = true;
                VolunteersResponse.Message = $"Volunteer record with id {id} deleted.";
                return Ok(new[] { VolunteersResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogDebug($"Error removing from DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error: {ex.Message}");
            }
            _logger.LogInformation("Error: delete request cannot be handled, bad request.");
            return BadRequest();
        }
    }
}