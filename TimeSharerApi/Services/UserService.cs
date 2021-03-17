using System;
using System.Collections.Generic;
using TimeSharerApi.Interfaces;
using TimeSharerApi.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
namespace TimeSharerApi.Services
{
    public class UsersService : IUsersService
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<User> _users;
        //private readonly IMongoCollection<Volunteer> _volunteers;

        public UsersService(IDatabaseSettings settings, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UsersService>();
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }
        public User Create(User userIn)
        {
            try
            {
                _users.InsertOne(userIn);
                return userIn;
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Mongo error, record was not inserted. {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            throw new Exception("Could not create user in the DB.");
        }

        public bool Delete(string id)
        {
            _logger.LogInformation($"Trying to delete user with id {id}");
            try
            {
                DeleteResult removed = _users.DeleteOne(user => user.Id == id);
                _logger.LogInformation($"Number of records deleted: {removed.DeletedCount}");
                return removed.DeletedCount == 1;
            }
            catch (MongoException ex)
            {
                _logger.LogError($"Error deleting record from DB: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
            _logger.LogInformation("Could not process request to delete record");
            return false;
        }

        public List<User> Get()
        {
            _logger.LogInformation("Trying to get list of users...");
            List<User> result = new();
            try
            {
                result = _users.Find(user => true).ToList();
                _logger.LogInformation($"Number of records found: {result.Count}");

            }
            catch (MongoException ex)
            {
                _logger.LogError($"Could not find list of records: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            if (result.Count > 0)
            {
                _logger.LogInformation($"Found {result.Count} records.");
                return result;
            }
            _logger.LogInformation("Search completed but did not find any records.");
            return new List<User>();
        }

        public User Get(string id)
        {
            if (ObjectId.TryParse(id, out _))
            {
                User foundOpportunity = new();
                try
                {
                    foundOpportunity = _users.Find(user => user.Id == id).FirstOrDefault();
                }
                catch (MongoException ex)
                {
                    _logger.LogError($"Error trying to get opportunity record {id}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
                return foundOpportunity;
            }
            else
            {
                _logger.LogError("ID could not be parsed");
                throw new Exception("User ID could not be parsed");
            }
        }

        public bool Update(string id, UserDetails userDetailsIn)
        {
            var filter = Builders<User>.Filter.Eq(o => o.Id, id);
            var update = Builders<User>.Update
                .Set(user => user.Details, userDetailsIn)
                .CurrentDate(u => u.UpdatedAt);
            try
            {
                _logger.LogInformation($"Trying to update opportunity {id}");
                var user = _users.UpdateOne(filter, update);
                
                if (user.ModifiedCount == 1)
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
            return false;
        }

        public bool AddVolunteerIdToUser(string volunteerId, string userId)
        {
            User existingUser = Get(userId);
            if (existingUser == null) throw new Exception("Error. User does not exist.");

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(user => user.Details.AssociatedVolunteerId, volunteerId)
                .CurrentDate(u => u.UpdatedAt);
            try
            {
                var addIdToUser = _users.UpdateOne(filter, update);
                var addedId = addIdToUser.UpsertedId;
                if(addIdToUser.ModifiedCount == 1)
                {
                    _logger.LogInformation($"Record {userId} updated with volunteer id {addedId}");
                    return true;
                }
                return false;
            }
            catch(MongoException ex)
            {
                _logger.LogError($"Error trying to update {userId}. {ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error trying to update {userId}: {ex.Message}");
            }
            throw new Exception("Could not update DB");
        }
    }
}
