using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace TimeSharerApi.Models
{
    public class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        [BsonElement("__v")]
        protected int _v { get; }
    }
}
