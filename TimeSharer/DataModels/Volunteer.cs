using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeSharer.Models
{
    public class Volunteer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; protected set; }
        public DateTime UpdatedAt { get; set; }
        [BsonElement("__v")]
        public int __v { get; }
        public Details Details { get; set; }
    }

    public class Details
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string Town { get; set; }
        public List<string> ListOfSkills { get; set; } = new List<string>() { };
        //[BsonElement("AssignedOpportunities")]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public List<string> AssignedOpportunities { get; set; } = new List<string> { };
        public bool StopProccessingData { get; set; } = false;
        public bool ConfirmDataSharing { get; set; } = false;
    }

    
}
