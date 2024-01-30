using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLog.Models
{
    public class LogDocument
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonElement]
        public string? LogTitle { get; set; }
        [BsonElement]
        public string? LogFrom { get; set; }
        [BsonElement]
        public string? FunctionNameLog { get; set; }
        [BsonElement]
        public string? ErrorMessage { get; set; }
        [BsonElement]
        public string? LogLevel { get; set; }
        [BsonElement]
        public string? SourceOrigin { get; set; }
        [BsonElement]
        public string? IP { get; set; }
        [BsonElement]
        public string? DateSourceError { get; set; }
        [BsonElement]
        public string? Description { get; set; }
        [BsonElement]
        public string? CorrelationId { get; set; }
        [BsonElement]
        public string? CreatedAt { get; set; }
        [BsonElement]
        public int CreatedBy { get; set; }
        [BsonElement]
        public string? LastModifedAt { get; set; }
        [BsonElement]
        public int LastModifedBy { get; set; }
        [BsonElement]
        public string? Flag { get; set; }
    }
}
