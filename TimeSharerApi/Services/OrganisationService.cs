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
                _organisations.InsertOneAsync(organisation);
            }
            catch(MongoException ex)
            {
                _logger.LogError($"Mongo error, record was not inserted. {ex.Message}");
            }
            return organisation;
        }



        public List<Organisation> Get()
        {
            var result = _organisations.Find(organisation => true).ToList();
            if (result.Count > 0)
            {
                return result;
            }

            return null;
        }

        public Organisation Get(string id)
        {
            if(ObjectId.TryParse(id, out _))
            {
                Organisation foundOrganisation = new Organisation();
                try
                {
                     foundOrganisation = _organisations.Find<Organisation>(organisation => organisation.Id == id).FirstOrDefault();
                }
                catch(MongoException ex)
                {
                    _logger.LogError(ex.Message);
                }

                return foundOrganisation;
            }
            else
            {
                _logger.LogError("ID could not be parsed");
                return null;
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
                var parsed = ObjectId.TryParse(id, out _);
                if (organisationUpdatedId == parsed) return true;
                return false;
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error trying to update the record with id {id}: {ex.Message}");
                return false;
            }
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
            _logger.LogInformation($"Trying to delete organisation record with ID {id}");
            DeleteResult removed = _organisations.DeleteOne(organisation => organisation.Id == id);
            _logger.LogDebug($"DelectedCount: {removed.DeletedCount}");
            return removed.DeletedCount == 1;
        }
    }
}
