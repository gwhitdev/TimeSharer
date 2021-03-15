using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeSharerApi.Models
{
    public class Opportunity : BaseEntity
    {
        public string Title { get; set; }
        public bool Live { get; set; }
        public string Town { get; set; }
        public List<string> AssignedVolunteers { get; set; }
    }
}
