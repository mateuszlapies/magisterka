using LiteDB;

namespace Blockchain
{
    internal class Link
    {
        [BsonId]
        public Guid Id { get; set; }
        public object Object { get; set; }
        public string ObjectType { get; set; }
        public Signature Signature { get; set; }

        public Guid? LastId { get; set; }
        [BsonIgnore]
        public Link LastLink { get; set; }
    }
}
