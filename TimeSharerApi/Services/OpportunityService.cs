using System;
using System.Collections.Generic;
using TimeSharerApi.Interfaces;
using TimeSharerApi.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
namespace TimeSharerApi.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<Opportunity> _opportunities;

        public OpportunityService(IDatabaseSettings settings, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OpportunityService>();
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _opportunities = database.GetCollection<Opportunity>(settings.OpportunitiesCollectionName);
        }

        public Opportunity Create(Opportunity opportunity)
        {
            try
            {
                _opportunities.InsertOne(opportunity);
                return opportunity;
            }
            catch( MongoException ex)
            {
                _logger.LogError($"Error creating opportunity: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            return new Opportunity();
        }

        public bool Delete(string id)
        {
            _logger.LogInformation($"Trying to delete opportunity with id {id}");
            try
            {
                DeleteResult removed = _opportunities.DeleteOne(opportuntity => opportuntity.Id == id);
                _logger.LogInformation($"Number of records deleted: {removed.DeletedCount}");
                return removed.DeletedCount == 1;
            }
            catch(MongoException ex)
            {
                _logger.LogError($"Error deleting record from DB: {ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogInformation("Could not process request to delete record");
            return false;
        }

        public List<Opportunity> Get()
        {
            _logger.LogInformation("Trying to get list of opportunities...");
            List<Opportunity> result = new();
            try
            {
                result = _opportunities.Find(opportunity => true).ToList();
                _logger.LogInformation($"Number of records found: {result.Count}");

            }
            catch(MongoException ex)
            {
                _logger.LogError($"Could not find list of records: {ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            if (result.Count > 0)
            {
                _logger.LogInformation($"Found {result.Count} records.");
                return result;
            }
            _logger.LogInformation("Search completed but did not find any records.");
            return new List<Opportunity>();

        }

        public Opportunity Get(string id)
        {
            if(ObjectId.TryParse(id, out _))
            {
                Opportunity foundOpportunity = new();
                try
                {
                    foundOpportunity = _opportunities.Find(opportunity => opportunity.Id == id).FirstOrDefault();
                }
                catch(MongoException ex)
                {
                    _logger.LogError($"Error trying to get opportunity record {id}: {ex.Message}");
                }
                catch(Exception ex)
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                return foundOpportunity;
            }
            else
            {
                _logger.LogError("ID could not be parsed");
                return new Opportunity();
            }
        }

        public bool Update(string id, OpportunityDetails opportunityDetailsIn)
        {
            var filter = Builders<Opportunity>.Filter.Eq(o => o.Id, id);
            var update = Builders<Opportunity>.Update
                .Set(opportunity => opportunity.Details, opportunityDetailsIn)
                .CurrentDate(o => o.UpdatedAt);
            try
            {
                _logger.LogInformation($"Trying to update opportunity {id}");
                var opportunity = _opportunities.UpdateOne(filter, update);
                var opportunityUpdatedId = opportunity.UpsertedId;
                var parsed = ObjectId.TryParse(id, out _);
                return opportunityUpdatedId == parsed;
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error trying to update the record with id {id}: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogInformation($"Did not update record {id}");
            return false;
        }
    }
}
