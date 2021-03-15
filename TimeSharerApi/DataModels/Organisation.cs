using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeSharerApi.Models 
{
    public class Organisation : BaseEntity
    {
        public OrganisationDetails Details { get; set; }
    }

    public class OrganisationDetails
    {
        public string Name { get; set; }
        public string Town { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> OpportunityIds { get; set; }
    }
    
}
