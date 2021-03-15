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
                if(organisations.Count == 0)
                {
                    _logger.LogInformation($"Search completed. Found {organisations.Count} organisations.");
                    OrganisationResponse.Success = true;
                    OrganisationResponse.NumberOfRecordsFound = organisations.Count;
                    OrganisationResponse.Message = $"Search completed. Found {organisations.Count} organisations.";
                    OrganisationResponse.Data = new List<Organisation>();
                }
                else
                {
                    _logger.LogInformation($"Number of organisations found: {organisations.Count}");
                    OrganisationResponse.Success = true;
                    OrganisationResponse.NumberOfRecordsFound = organisations.Count;
                    OrganisationResponse.Message = $"Search completed.";
                    OrganisationResponse.Data = organisations;
                }
                return Ok(new[] { OrganisationResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogInformation($"Error getting organisations from DB: {ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
            }

            return NotFound();
        }
        
        [HttpGet("{id}", Name = nameof(GetById))]
        [ProducesResponseType(200, Type = typeof(OrganisationResponseModel))]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            try
            {
                var result = _organisationsService.Get(id);
                List<Organisation> organisation = new List<Organisation>() { result };

                if (result == null)
                {
                    OrganisationResponse.Success = false;
                    OrganisationResponse.NumberOfRecordsFound = organisation.Count;
                    OrganisationResponse.Message = $"Error: organisation with id {id} cannot be found. ";
                    OrganisationResponse.Data = null;
                    _logger.LogInformation($"Not found: cannot find organisation record with id {id}.");
                    return NotFound(new[] { OrganisationResponse });
                }

                OrganisationResponse.Success = true;
                OrganisationResponse.NumberOfRecordsFound = organisation.Count;
                OrganisationResponse.Message = $"Found ingredient by ID: {id}.";
                OrganisationResponse.Data = organisation;

                return Ok(new[] { OrganisationResponse });
            }
            catch (MongoException ex)
            {
                _logger.LogDebug($"Error updating organisation record in the DB: {ex.Message}");
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
                routeName: nameof(GetById),
                routeValues: new { id = organisationToCreate.Id.ToString() },
                value: organisationToCreate);
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(OrganisationDetails))]
        public IActionResult Update(string id, [FromBody] OrganisationDetails organisationDetailsIn)
        {
            id = id.ToLower();
            if (organisationDetailsIn == null)
            {
                OrganisationResponse.Success = false;
                OrganisationResponse.Message = "Organisation submitted was null.";
                OrganisationResponse.NumberOfRecordsFound = 0;
                OrganisationResponse.Data = new List<Organisation>();
                return BadRequest(new[] { OrganisationResponse });
            }

            if(!ModelState.IsValid)
            {
                OrganisationResponse.Success = false;
                OrganisationResponse.NumberOfRecordsFound = 0;
                OrganisationResponse.Message = $"Not all fields were supplied! {ModelState}";
                OrganisationResponse.Data = new List<Organisation>();
                return BadRequest(new[] { OrganisationResponse });
            }
            try
            {
                var existing = _organisationsService.Get(id);

                if (existing == null)
                {
                    OrganisationResponse.Success = false;
                    OrganisationResponse.NumberOfRecordsFound = 0;
                    OrganisationResponse.Message = "Organisation record not found";
                    OrganisationResponse.Data = new List<Organisation>();
                    return NotFound(new[] { OrganisationResponse });
                }

                var updated = _organisationsService.Update(id, organisationDetailsIn);
                var organisationFound = new List<Organisation>() { _organisationsService.Get(id) };
                if (!updated)
                {
                    OrganisationResponse.Success = false;
                    OrganisationResponse.NumberOfRecordsFound = organisationFound.Count;
                    OrganisationResponse.Message = "Update didn't work!";
                    OrganisationResponse.Data = organisationFound;
                    return BadRequest(new[] { OrganisationResponse });
                }
                OrganisationResponse.Success = true;
                OrganisationResponse.NumberOfRecordsFound = organisationFound.Count;
                OrganisationResponse.Message = "Organisation record updated";
                OrganisationResponse.Data = new List<Organisation>() { _organisationsService.Get(id) };
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
                if (exists == null)
                {
                    OrganisationResponse.Success = false;
                    OrganisationResponse.Message = "Volunteer record not found. Cannot delete.";
                    OrganisationResponse.Data = new List<Organisation>();
                    return NotFound(new[] { OrganisationResponse });
                }

                bool volunteerRemoved = _organisationsService.Delete(id);
                if (!volunteerRemoved)
                {
                    OrganisationResponse.Success = false;
                    OrganisationResponse.Message = "Volunteer record not deleted";
                    OrganisationResponse.Data = new List<Organisation>();
                    return BadRequest(new[] { OrganisationResponse });
                }
                OrganisationResponse.Success = true;
                OrganisationResponse.Message = $"Volunteer record with id {id} deleted.";
                OrganisationResponse.Data = null;
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