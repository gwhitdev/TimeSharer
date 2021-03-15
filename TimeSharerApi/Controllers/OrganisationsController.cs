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
    public class OrganisationsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IOrganisationService _organisationsService;
        public OrganisationResponseModel OrganisationResponse { get; set; }

        public OrganisationsController(ILoggerFactory loggerFactory, IOrganisationService organisationsService)
        {
            _logger = loggerFactory.CreateLogger<VolunteersController>();
            _organisationsService = organisationsService;
            OrganisationResponse = new OrganisationResponseModel();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(OrganisationResponseModel))]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Trying to get list of organisations");
                var organisations = _organisationsService.Get().ToList();
                _logger.LogInformation($"Search completed. Found {organisations.Count} organisations.");
                OrganisationResponse.Success = true;
                OrganisationResponse.NumberOfRecordsFound = organisations.Count;
                OrganisationResponse.Message = $"Search completed. Found {organisations.Count} organisations.";
                if (organisations.Count == 0) OrganisationResponse.Data = new List<Organisation>();
                OrganisationResponse.Data = organisations;
                return Ok(new[] { OrganisationResponse });
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
            }
            _logger.LogError("Bad request");
            return BadRequest();
        }
        
        [HttpGet("{id}", Name = "GetOrganisationById")]
        [ProducesResponseType(200, Type = typeof(Organisation))]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            try
            {
                var result = _organisationsService.Get(id);
                List<Organisation> organisation = new() { result };
                OrganisationResponse.NumberOfRecordsFound = organisation.Count;
                if (result == null)
                {
                    OrganisationResponse.Success = false;
                    OrganisationResponse.Message = $"Error: organisation with id {id} cannot be found. ";
                    OrganisationResponse.Data = new List<Organisation>();
                    _logger.LogInformation($"Not found: cannot find organisation record with id {id}.");
                    return NotFound(new[] { OrganisationResponse });
                }
                OrganisationResponse.Success = true;
                OrganisationResponse.Message = $"Found organisation: {id}.";
                OrganisationResponse.Data = organisation;
                return Ok(new[] { OrganisationResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogDebug($"Error getting organisation record in the DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error: {ex.Message}");
            }
            return BadRequest();
        }
        
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Organisation))]
        [ProducesResponseType(401)]
        public IActionResult Create([FromBody] Organisation organisationIn)
        {
            Organisation organisationToCreate = new Organisation();
            organisationToCreate = _organisationsService.Create(organisationIn);
            return CreatedAtRoute(
                routeName: "GetOrganisationById",
                routeValues: new { id = organisationToCreate.Id.ToString() },
                value: organisationToCreate);
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(OrganisationDetails))]
        public IActionResult Update(string id, [FromBody] OrganisationDetails organisationDetailsIn)
        {
            id = id.ToLower();

            OrganisationResponse.Success = false;
            OrganisationResponse.NumberOfRecordsFound = 0;
            OrganisationResponse.Data = new List<Organisation>();

            if (organisationDetailsIn == null)
            {
                OrganisationResponse.Message = "Organisation submitted was null.";   
                return BadRequest(new[] { OrganisationResponse });
            }

            if(!ModelState.IsValid)
            {
                OrganisationResponse.Message = $"Not all fields were supplied! {ModelState}";
                return BadRequest(new[] { OrganisationResponse });
            }

            try
            {
                var existing = _organisationsService.Get(id);

                if (existing == null)
                {
                    OrganisationResponse.Message = "Organisation record not found";
                    return NotFound(new[] { OrganisationResponse });
                }

                var updated = _organisationsService.Update(id, organisationDetailsIn);
                var organisationFound = new List<Organisation>() { _organisationsService.Get(id) };

                OrganisationResponse.Data = organisationFound;

                if (!updated)
                {
                    OrganisationResponse.NumberOfRecordsFound = organisationFound.Count;
                    OrganisationResponse.Message = "Update didn't work!";
                    return BadRequest(new[] { OrganisationResponse });
                }

                OrganisationResponse.Success = true;
                OrganisationResponse.NumberOfRecordsFound = organisationFound.Count;
                OrganisationResponse.Message = "Organisation record updated";
                return Ok(new[] { OrganisationResponse });
            }
            catch(MongoException ex)
            {
                _logger.LogDebug($"Error updating organisation record in the DB: {ex.Message}");
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
            _logger.LogInformation($"Trying to delete organisation record {id}");
            try
            {
                var exists = _organisationsService.Get(id);
                
                OrganisationResponse.Success = false;
                OrganisationResponse.Data = new List<Organisation>();

                if (exists == null)
                {   
                    OrganisationResponse.Message = "Volunteer record not found. Cannot delete.";   
                    return NotFound(new[] { OrganisationResponse });
                }

                bool volunteerRemoved = _organisationsService.Delete(id);

                if (!volunteerRemoved)
                {
                    OrganisationResponse.Message = "Volunteer record not deleted";
                    return BadRequest(new[] { OrganisationResponse });
                }

                OrganisationResponse.Success = true;
                OrganisationResponse.Message = $"Volunteer record with id {id} deleted.";
                return Ok(new[] { OrganisationResponse });
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