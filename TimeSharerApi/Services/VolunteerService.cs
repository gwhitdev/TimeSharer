using System;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using TimeSharerApi.Models;
using TimeSharerApi.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using static System.Console;
using MongoDB.Bson;

namespace TimeSharerApi.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<Volunteer> _volunteers;
        //private readonly IMongoCollection<Organisation> _organisations;

        public VolunteerService(IDatabaseSettings settings, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<VolunteerService>();

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _volunteers = database.GetCollection<Volunteer>(settings.VolunteersCollectionName);
        }

        public List<Volunteer> Get()
        {
            var result = _volunteers.Find(volunteer => true).ToList();
            if(result.Count > 0)
            {
                return result;
            }

            return null;
        }

        public Volunteer Create(Volunteer volunteer)
        {
            _volunteers.InsertOne(volunteer);
            
            return volunteer;
        }

        public Volunteer Read(string id)
        {
            if (ObjectId.TryParse(id, out _))
            {
                try
                {
                    return _volunteers.Find<Volunteer>(volunteer => volunteer.Id == id).FirstOrDefault();
                }
                catch (MongoException ex)
                {
                    _logger.LogDebug(ex.Message);
                    return null;
                }
            }
            else
            {
                _logger.LogDebug("Id could not be parsed");
                return null;
            }
            
            
        }

        public bool Update(string id, Details volunteerIn)
        {
            _logger.LogDebug($"id: {id}");

            var filter = Builders<Volunteer>.Filter.Eq(s => s.Id, id);
            var update = Builders<Volunteer>.Update
                .Set(volunteer => volunteer.Details, volunteerIn)
                .CurrentDate(s => s.UpdatedAt);
            
            _logger.LogInformation($"{update}");
            try
            {
                var v = _volunteers.UpdateOne(filter, update);
                var volunteerdIdUpdated = v.UpsertedId;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.Message);
                return false;
                
            }
        }

        public bool Delete(Volunteer volunteerIn)
        {
            DeleteResult removed = _volunteers.DeleteOne(volunteer => volunteer.Id == volunteerIn.Id);
            return removed.DeletedCount == 1;
        }


        public bool Delete(string id)
        {
            DeleteResult removed = _volunteers.DeleteOne(volunteer => volunteer.Id == id);
            return removed.DeletedCount == 1;
        }
    }
}
