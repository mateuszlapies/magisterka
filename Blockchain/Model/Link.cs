using LiteDB;
using System.Text.Json.Serialization;

namespace Blockchain.Model
{
    public class Link
    {
        [BsonId]
        public Guid Id { get; set; }
        public object Object { get; set; }
        public string ObjectType { get; set; }
        public Signature Signature { get; set; }
        public DateTime Timestamp { get; set; }

        public Guid? LastId { get; set; }
        [BsonIgnore]
        public Link LastLink { get; set; }

        [JsonIgnore]
        public Lock Lock { get; set; }
    }
}
