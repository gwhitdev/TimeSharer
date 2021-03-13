using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using TimeSharer.Models;
using TimeSharer.Interfaces;
using Microsoft.Extensions.Logging;

namespace TimeSharer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IVolunteerService _volunteersService;

        public VolunteersController(ILoggerFactory loggerFactory, IVolunteerService volunteerService)
        {
            _logger = loggerFactory.CreateLogger<VolunteersController>();
            _volunteersService = volunteerService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(VolunteersResponseModel))]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            var volunteers = _volunteersService.Get().ToList<Volunteer>();
            _logger.LogDebug($"volunteers: {volunteers.Count}");
            var response = new VolunteersResponseModel
            {
                Success = true,
                Message = "Received volunteer data",
                Data = volunteers
            };

            return Ok(new[] { response });
        }

        [HttpGet("{id}", Name = nameof(GetById))]
        [ProducesResponseType(200, Type = typeof(Volunteer))]
        [ProducesResponseType(400)]
        public IActionResult GetById(string id)
        {
            var result = _volunteersService.Read(id);
            List<Volunteer> volunteer = new List<Volunteer>() { result };

            var response = new VolunteersResponseModel();

            if (result == null)
            {
                response.Success = false;
                response.Message = "Bad Request.";
                response.Data = null;

                return BadRequest(new[] { response });
            }

            response.Success = true;
            response.Message = $"Found ingredient by ID: {id}.";
            response.Data = volunteer;

            return Ok(new[] { response });
        }


        [HttpPost]
        [ProducesResponseType(201, Type = typeof(VolunteersResponseModel))]
        [ProducesResponseType(401)]
        public IActionResult Create([FromBody] Volunteer volunteer)
        {
            Volunteer volunteerToCreate = new Volunteer();
            volunteerToCreate = _volunteersService.Create(volunteer);

            return CreatedAtRoute(
                routeName: nameof(GetById),
                routeValues: new { id = volunteerToCreate.Id.ToString() },
                value: volunteerToCreate);

        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] Details volunteerIn)
        {
            id = id.ToLower();

            var response = new VolunteersResponseModel();

            if (volunteerIn == null)
            {
                response.Success = false;
                response.Message = "Volunteer submitted was null";
                response.Data = null;
                return BadRequest(new[] { response });
            }

            if (!ModelState.IsValid)
            {
                response.Success = false;
                response.Message = "Not all fields were supplied! {ModelState}";
                response.Data = null;
                return BadRequest(new[] { response });
            }

            var existing = _volunteersService.Read(id);

            if (existing == null)
            {
                response.Success = false;
                response.Message = "Volunteer record not found";
                response.Data = null;
                return NotFound(new[] { response });
            }

            var updated = _volunteersService.Update(id, volunteerIn);
           
            if(!updated)
            {
                response.Success = false;
                response.Message = "Update didn't work";
                response.Data = null;
                return BadRequest(new[] { response });
            }

            response.Success = true;
            response.Message = "Volunteer record updated";
            response.Data = null;

            return Ok(new[] { response });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var response = new VolunteersResponseModel();

            var exists = _volunteersService.Read(id);
            if(exists == null)
            {
                response.Success = false;
                response.Message = "Volunteer record not found";
                response.Data = null;
                return NotFound(new[] { response });
            }

            bool volunteerRemoved = _volunteersService.Delete(id);
            if (!volunteerRemoved)
            {
                response.Success = false;
                response.Message = "Volunteer record not deleted";
                response.Data = null;
                return BadRequest(new[] { response });
            }

            response.Success = true;
            response.Message = "Volunteer record removed";
            response.Data = null;
            return Ok(new[] { response });
        }

    }
}