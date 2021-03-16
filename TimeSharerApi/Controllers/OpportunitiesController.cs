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
    public class OpportunitiesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IOpportunityService _opportunitiesService;
        public Response<List<Opportunity>> OpportunityResponse { get; set; }

        public OpportunitiesController(ILoggerFactory loggerFactory, IOpportunityService opportunitiesService)
        {
            _logger = loggerFactory.CreateLogger<OpportunitiesController>();
            _opportunitiesService = opportunitiesService;
            OpportunityResponse = new();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Response<List<Opportunity>>))]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Trying to get list of opportunities");
                var opportunities = _opportunitiesService.Get().ToList();
                _logger.LogInformation($"Search completed. Found {opportunities.Count} records."); 
                OpportunityResponse.NumberOfRecordsFound = opportunities.Count;
                OpportunityResponse.Message = "Search completed.";
                OpportunityResponse.Data = opportunities;
                return Ok(new[] { OpportunityResponse });
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogError("Bad request");
            return BadRequest();
        }

        [HttpGet("{id}", Name = "GetOpportunityById")]
        [ProducesResponseType(200, Type = typeof(Response<List<Opportunity>>))]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            try
            {
                var result = _opportunitiesService.Get(id);
                List<Opportunity> opportunities = new() { result };
                if (opportunities.FirstOrDefault() == null)
                {
                    OpportunityResponse.Success = false;
                    OpportunityResponse.Message = $"Error: opportunity with id {id} cannot be found. ";
                    OpportunityResponse.Data = new();
                    _logger.LogInformation($"Not found: cannot find opportunity record with id {id}.");
                    return NotFound(new[] { OpportunityResponse });
                }
                OpportunityResponse.NumberOfRecordsFound = opportunities.Count;
                OpportunityResponse.Message = $"Found opportunity {id}.";
                OpportunityResponse.Data = opportunities;
                return Ok(new[] { OpportunityResponse });
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
        [ProducesResponseType(201, Type = typeof(Opportunity))]
        [ProducesResponseType(401)]
        public IActionResult Create([FromBody] Opportunity opportunityIn)
        {
            Opportunity opportunityToCreate = _opportunitiesService.Create(opportunityIn);
            return CreatedAtRoute(
                routeName: "GetOpportunityById",
                routeValues: new { id = opportunityToCreate.Id.ToString() },
                value: opportunityToCreate);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(OpportunityDetails))]
        public IActionResult Update(string id, [FromBody] OpportunityDetails opportunityDetailsIn)
        {
            id = id.ToLower();

            OpportunityResponse.Success = false;

            if(opportunityDetailsIn == null)
            {
                OpportunityResponse.Message = "Opportunity submitted was null";
                return BadRequest(new[] { OpportunityResponse });
            }

            try
            {
                var existing = _opportunitiesService.Get(id);

                if (existing == null)
                {
                    OpportunityResponse.Message = "Opportunity record not found";
                    return NotFound(new[] { OpportunityResponse });
                }

                var updated = _opportunitiesService.Update(id, opportunityDetailsIn);
                var opportunityFound = new List<Opportunity>() { _opportunitiesService.Get(id) };

                OpportunityResponse.Data = opportunityFound;
                if(!updated)
                {
                    OpportunityResponse.NumberOfRecordsFound = opportunityFound.Count;
                    OpportunityResponse.Message = "Update didn't work!";
                    return BadRequest(new[] { OpportunityResponse });
                }

                OpportunityResponse.Success = true;
                OpportunityResponse.NumberOfRecordsFound = opportunityFound.Count;
                OpportunityResponse.Message = "Opportunity record updated";
                return Ok(new[] { OpportunityResponse });
            }

            catch(MongoException ex)
            {
                _logger.LogError($"Error updating opportunity record in the DB: {ex.Message}");
            }
            catch(Exception ex)
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
                _logger.LogInformation($"Trying to delete opportunity record {id}");
                var exists = _opportunitiesService.Get(id);

                OpportunityResponse.Success = false;
                OpportunityResponse.Data = new();

                if(exists == null)
                {
                    OpportunityResponse.Message = "Opportunity record not found.";
                    return NotFound(new[] { OpportunityResponse });
                }

                bool opportunityRemoved = _opportunitiesService.Delete(id);

                if(!opportunityRemoved)
                {
                    OpportunityResponse.Message = "Opportunity record not deleted.";
                    return BadRequest(new[] { OpportunityResponse });
                }

                OpportunityResponse.Success = true;
                OpportunityResponse.Message = $"Opportunity record with id {id} deleted.";
                return Ok(new[] { OpportunityResponse });
            }
            catch(MongoException ex)
            {
                _logger.LogError($"Error removing from DB: {ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogInformation("Error: delete request cannot be handled, bad request.");
            return BadRequest();
            
        }
    }
}