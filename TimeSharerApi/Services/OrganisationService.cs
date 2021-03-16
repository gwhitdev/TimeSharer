using System;
using MongoDB.Driver;
using TimeSharerApi.Models;
using TimeSharerApi.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace TimeSharerApi.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<Organisation> _organisations;
        //private readonly IMongoCollection<Volunteer> _volunteers;

        public OrganisationService(IDatabaseSettings settings, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OrganisationService>();
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _organisations = database.GetCollection<Organisation>(settings.OrganisationsCollectionName);
        }

        public Organisation Create(Organisation organisation)
        {
            try
            {
                _organisations.InsertOne(organisation);
                return organisation;
            }
            catch(MongoException ex)
            {
                _logger.LogError($"Mongo error, record was not inserted. {ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            throw new Exception("Could not create organisation in the DB.");
        }



        public List<Organisation> Get()
        {
            try
            {
                _logger.LogInformation("Trying to get organisations from DB...");
                var result = _organisations.Find(organisation => true).ToList();
                if (result.Count > 0)
                {
                    return result;
                }

                throw new Exception("No results returned");
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error searching DB! {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            throw new Exception("Could not get list of organisations from DB");
        }

        public Organisation Get(string id)
        {
            if(ObjectId.TryParse(id, out _))
            {
                Organisation foundOrganisation = new();
                try
                {
                     foundOrganisation = _organisations.Find(organisation => organisation.Id == id).FirstOrDefault();
                }
                catch(MongoException ex)
                {
                    _logger.LogError(ex.Message);
                }
                
                if (foundOrganisation.Id.Equals(id)) return foundOrganisation;
                _logger.LogInformation("Error: Sent and returned IDs do not match.");
                throw new Exception("Error: sent and returned IDs do not match");
                
            }
            else
            {
                _logger.LogError("ID could not be parsed");
                throw new Exception("Error: ID could not be parsed");
            }

        }

        public bool Update(string id, OrganisationDetails organisationIn)
        {
            var filter = Builders<Organisation>.Filter.Eq(o => o.Id, id);
            var update = Builders<Organisation>.Update
                .Set(organisaion => organisaion.Details, organisationIn)
                .CurrentDate(o => o.UpdatedAt);
            try
            {
                _logger.LogInformation($"Trying to update organisation with id {id}");
                var o = _organisations.UpdateOne(filter, update);
                var organisationUpdatedId = o.UpsertedId;
                if (o.ModifiedCount == 1)
                {
                    _logger.LogInformation($"Updated successfully");
                    return true;
                }
                throw new Exception("Update request completed but modified count does not equal 1.");
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
            throw new Exception("Error. Something went wrong and record was not updated.");
        }
        public bool Delete(Organisation organisationIn)
        {
            _logger.LogInformation($"Trying to delete organisation record with ID {organisationIn.Id}");
            DeleteResult removed = _organisations.DeleteOne(organisation => organisation.Id == organisationIn.Id);
            _logger.LogDebug($"DelectedCount: {removed.DeletedCount}");
            return removed.DeletedCount == 1;
        }

        public bool Delete(string id)
        {
            try
            {
                _logger.LogInformation($"Trying to delete organisation record with ID {id}");
                DeleteResult removed = _organisations.DeleteOne(organisation => organisation.Id == id);
                if(removed.DeletedCount == 1)
                {
                    _logger.LogDebug($"Record deleted successfully.");
                    return true;
                }
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error deleting record: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            throw new Exception("Could not delete record");            
        }
    }
}
