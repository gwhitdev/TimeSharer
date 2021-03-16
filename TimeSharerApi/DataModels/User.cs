using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
namespace TimeSharerApi.Models
{
    public class User : BaseEntity
    {
        public string AuthId { get; set; }
        public UserDetails Details { get; set; }
    }

    public class UserDetails
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> AssignedOrganisations { get; set; }
        public string AssociatedVolunteerId { get; set; }
    }
}
