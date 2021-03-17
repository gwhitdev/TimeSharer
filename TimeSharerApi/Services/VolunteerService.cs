using System;
using MongoDB.Driver;
using TimeSharerApi.Models;
using TimeSharerApi.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace TimeSharerApi.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<Volunteer> _volunteers;
        //private readonly IMongoCollection<Organisation> _organisations;
        private readonly IUsersService _usersService;

        public VolunteerService(IDatabaseSettings settings, ILoggerFactory loggerFactory, IUsersService usersService)
        {
            _logger = loggerFactory.CreateLogger<VolunteerService>();

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _volunteers = database.GetCollection<Volunteer>(settings.VolunteersCollectionName);
            _usersService = usersService;
        }

        public List<Volunteer> Get()
        {
            var result = _volunteers.Find(volunteer => true).ToList();
            if(result.Count > 0)
            {
                return result;
            }

            return new List<Volunteer>();
        }

        public Volunteer Create(Volunteer volunteer)
        {
            try
            {
                _logger.LogInformation($"Trying to creaete volunteer record for {volunteer.Details.Name}");
                _volunteers.InsertOne(volunteer);
            }
            catch(MongoException ex)
            {
                _logger.LogError($"Could not create volunteer {ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            
            return volunteer;
        }

        public Volunteer Get(string id)
        {
            if (ObjectId.TryParse(id, out _))
            {
                Volunteer foundVolunteer = new Volunteer();
                try
                {
                    foundVolunteer = _volunteers.Find<Volunteer>(volunteer => volunteer.Id == id).FirstOrDefault();
                }
                catch (MongoException ex)
                {
                    _logger.LogDebug(ex.Message);
                }

                return foundVolunteer;
            }
            else
            {
                _logger.LogDebug("Id could not be parsed");
                return new Volunteer();
            }
            
            
        }

        public bool Update(string id, VolunteerDetails volunteerIn)
        {
            _logger.LogDebug($"id: {id}");

            var filter = Builders<Volunteer>.Filter.Eq(s => s.Id, id);
            var update = Builders<Volunteer>.Update
                .Set(volunteer => volunteer.Details, volunteerIn)
                .CurrentDate(s => s.UpdatedAt);
            
            try
            {
                var v = _volunteers.UpdateOne(filter, update);
                var volunteerdIdUpdated = v.UpsertedId;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.Message); 
            }
            return false;
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

        public bool AddUserIdToVolunteerRecord(string volunteerId, string userId)
        {
            _logger.LogInformation($"Trying to find volunteer {volunteerId}");
            Volunteer existingVolunteer = Get(volunteerId);
            if (existingVolunteer == null) throw new Exception("Error. Volunteer does not exist.");

            var filter = Builders<Volunteer>.Filter.Eq(v => v.Id, volunteerId);
            var update = Builders<Volunteer>.Update
                .Set(volunteer => volunteer.Details.AssociatedUserId, userId)
                .CurrentDate(u => u.UpdatedAt);
            try
            {
                var addIdToVolunteer = _volunteers.UpdateOne(filter, update);
                var addedId = addIdToVolunteer.UpsertedId;
                if (addIdToVolunteer.ModifiedCount == 1)
                {
                    _logger.LogInformation($"Record {volunteerId} updated with user id {addedId}");
                    return true;
                }
                return false;
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error trying to update {userId}. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error trying to update {userId}: {ex.Message}");
            }
            throw new Exception("Could not update DB");
        }
    }
}
