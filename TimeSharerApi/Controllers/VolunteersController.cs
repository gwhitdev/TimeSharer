using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using TimeSharerApi.Models;
using TimeSharerApi.Interfaces;
using Microsoft.Extensions.Logging;

namespace TimeSharerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IVolunteerService _volunteersService;
        public VolunteersResponseModel VolunteerResponse { get; set; }

        public VolunteersController(ILoggerFactory loggerFactory, IVolunteerService volunteerService)
        {
            _logger = loggerFactory.CreateLogger<VolunteersController>();
            _volunteersService = volunteerService;
            VolunteerResponse = new VolunteersResponseModel();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(VolunteersResponseModel))]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Trying to get list of volunteers from DB.");
                var volunteers = _volunteersService.Get().ToList<Volunteer>();
                _logger.LogDebug($"No. of volunteers found: {volunteers.Count}");
                if (volunteers.Count == 0)
                {
                    VolunteerResponse.Success = true;
                    VolunteerResponse.Message = "Search completed. No volunteer records found.";
                    VolunteerResponse.Data = new List<Volunteer>();
                }
                else
                {
                    VolunteerResponse.Success = true;
                    VolunteerResponse.Message = "Received volunteer data";
                    VolunteerResponse.Data = volunteers;
                }
                return Ok(new[] { VolunteerResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogDebug($"Error searching DB for volunteer records: {ex.Message}");
            }
            return NotFound();
        }

        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType(200, Type = typeof(Volunteer))]
        [ProducesResponseType(400)]
        public IActionResult GetById(string id)
        {
            try
            {
                var result = _volunteersService.Read(id);
                List<Volunteer> volunteer = new() { result };
                VolunteerResponse.NumberOfRecordsFound = volunteer.Count;
                if (result == null)
                {
                    VolunteerResponse.Success = false;
                    VolunteerResponse.Message = $"Error: volunteer {id} cannot be found. ";
                    VolunteerResponse.Data = new List<Volunteer>();
                    _logger.LogInformation($"Not found: cannot find organisation record with id {id}.");
                    return NotFound(new[] { VolunteerResponse });
                }
                VolunteerResponse.Success = true;
                VolunteerResponse.Message = $"Found volunteer: {id}.";
                VolunteerResponse.Data = volunteer;
                return Ok(new[] { VolunteerResponse });
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
        [ProducesResponseType(201, Type = typeof(VolunteersResponseModel))]
        [ProducesResponseType(401)]
        public IActionResult Create([FromBody] Volunteer volunteer)
        {
            Volunteer volunteerToCreate = new Volunteer();
            volunteerToCreate = _volunteersService.Create(volunteer);

            return CreatedAtRoute(
                routeName: "GetById",
                routeValues: new { id = volunteerToCreate.Id.ToString() },
                value: volunteerToCreate);

        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] VolunteerDetails volunteerIn)
        {
            id = id.ToLower();

            if (volunteerIn == null)
            {
                VolunteerResponse.Success = false;
                VolunteerResponse.Message = "Volunteer submitted was null";
                VolunteerResponse.Data = null;
                return BadRequest(new[] { VolunteerResponse });
            }

            if (!ModelState.IsValid)
            {
                VolunteerResponse.Success = false;
                VolunteerResponse.Message = "Not all fields were supplied! {ModelState}";
                VolunteerResponse.Data = null;
                return BadRequest(new[] { VolunteerResponse });
            }

            var existing = _volunteersService.Read(id);

            if (existing == null)
            {
                VolunteerResponse.Success = false;
                VolunteerResponse.Message = "Volunteer record not found";
                VolunteerResponse.Data = null;
                return NotFound(new[] { VolunteerResponse });
            }

            var updated = _volunteersService.Update(id, volunteerIn);
           
            if(!updated)
            {
                VolunteerResponse.Success = false;
                VolunteerResponse.Message = "Update didn't work";
                VolunteerResponse.Data = new List<Volunteer>() { _volunteersService.Read(id) };
                return BadRequest(new[] { VolunteerResponse });
            }

            VolunteerResponse.Success = true;
            VolunteerResponse.Message = "Volunteer record updated";
            VolunteerResponse.Data = new List<Volunteer>() { _volunteersService.Read(id) };

            return Ok(new[] { VolunteerResponse });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var exists = _volunteersService.Read(id);
            if(exists == null)
            {
                VolunteerResponse.Success = false;
                VolunteerResponse.Message = "Volunteer record not found. Cannot delete.";
                VolunteerResponse.Data = null;
                return NotFound(new[] { VolunteerResponse });
            }

            bool volunteerRemoved = _volunteersService.Delete(id);
            if (!volunteerRemoved)
            {
                VolunteerResponse.Success = false;
                VolunteerResponse.Message = "Volunteer record not deleted";
                VolunteerResponse.Data = null;
                return BadRequest(new[] { VolunteerResponse });
            }

            VolunteerResponse.Success = true;
            VolunteerResponse.Message = "Volunteer record removed";
            VolunteerResponse.Data = null;
            return Ok(new[] { VolunteerResponse });
        }

    }
}