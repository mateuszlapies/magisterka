using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain
{
    internal class Link
    {
        [BsonId]
        public Guid Id { get; set; }
        public object Object { get; set; }
        public Type ObjectType { get; set; }
        public string Hash { get; set; }
        public Guid? LastId { get; set; }
        [BsonIgnore]
        public Link LastLink { get; set; }
    }
}
