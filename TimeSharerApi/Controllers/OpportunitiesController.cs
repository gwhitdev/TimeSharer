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
        public OpportunityResponseModel OpportunityResponse { get; set; }

        public OpportunitiesController(ILoggerFactory loggerFactory, IOpportunityService opportunitiesService)
        {
            _logger = loggerFactory.CreateLogger<VolunteersController>();
            _opportunitiesService = opportunitiesService;
            OpportunityResponse = new OpportunityResponseModel();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(OpportunityResponseModel))]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Trying to get list of opportunities");
                var opportunities = _opportunitiesService.Get().ToList();
                _logger.LogInformation($"Search completed. Found {opportunities.Count} records.");
                OpportunityResponse.Success = true;
                OpportunityResponse.NumberOfRecordsFound = opportunities.Count;
                OpportunityResponse.Message = $"Search completed. Found {opportunities.Count} records.";
                if (opportunities.Count == 0) OpportunityResponse.Data = new List<Opportunity>();
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
        [ProducesResponseType(200, Type = typeof(OpportunityResponseModel))]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            try
            {
                var result = _opportunitiesService.Get(id);
                List<Opportunity> opportunity = new() { result };

                OpportunityResponse.NumberOfRecordsFound = opportunity.Count;
                if (result == null)
                {
                    OpportunityResponse.Success = false;
                    OpportunityResponse.Message = $"Error: opportunity with id {id} cannot be found. ";
                    OpportunityResponse.Data = new List<Opportunity>();
                    _logger.LogInformation($"Not found: cannot find opportunity record with id {id}.");
                    return NotFound(new[] { OpportunityResponse });
                }
                OpportunityResponse.Success = true;
                OpportunityResponse.Message = $"Found ingredient {id}.";
                OpportunityResponse.Data = opportunity;
                return Ok(new[] { OpportunityResponse });
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
        [ProducesResponseType(201, Type = typeof(Opportunity))]
        [ProducesResponseType(401)]
        public IActionResult Create([FromBody] Opportunity opportunityIn)
        {
            Opportunity opportunityToCreate = new Opportunity();
            opportunityToCreate = _opportunitiesService.Create(opportunityIn);
            return CreatedAtRoute(
                routeName: "GetOpportunityById",
                routeValues: new { id = opportunityToCreate.Id.ToString() },
                value: opportunityToCreate);
        }
    }
}