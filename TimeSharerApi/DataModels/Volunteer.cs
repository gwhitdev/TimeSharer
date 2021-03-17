using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeSharerApi.Models
{
    public class Volunteer : BaseEntity
    {
        public VolunteerDetails Details { get; set; }
    }

    public class VolunteerDetails
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string AssociatedUserId { get; set; }
        public string Name { get; set; }
        /*
        public string DateOfBirth { get; set; }
        public string Town { get; set; }
        public List<string> ListOfSkills { get; set; }
        [BsonElement("AssignedOpportunities")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> AssignedOpportunities { get; set; }
        public List<string> OrgsSharingDataWith { get; set; }
        public bool AskedToDelete { get; set; } = false;
        public bool OptedInAllProcessing { get; set; }
        public bool OptedInAllMarketing { get; set; }
        public bool TakingABreak { get; set; }*/
    }

    
}
