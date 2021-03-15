using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeSharerApi.Models
{
    public class Opportunity : BaseEntity
    {
        public OpportunityDetails Details { get; set; }
    }

    public class OpportunityDetails
    {
        public string Title { get; set; }
        public bool Live { get; set; }
        public string Town { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> AssignedVolunteers { get; set; }
    }
}
