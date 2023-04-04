using Blockchain.Model;
using LiteDB;

namespace Networking.Utils
{
    public static class LiteSerializer
    {
        private static readonly BsonMapper mapper = BsonMapper.Global;

        public static string Serialize(Link link)
        {
            var document = mapper.ToDocument<Link>(link);
            var value = mapper.Serialize<BsonDocument>(document);
            var str = JsonSerializer.Serialize(value);
            return str;
        }

        public static List<string> Serialize(List<Link> links)
        {
            var strList = new List<string>();
            foreach (var link in links)
            {
                var str = Serialize(link);
                strList.Add(str);
            }
            return strList;
        }

        public static Link Deserialize(string str)
        {
            var value = JsonSerializer.Deserialize(str);
            var document = mapper.Deserialize<BsonDocument>(value);
            var link = mapper.ToObject<Link>(document);
            return link;
        }

        public static List<Link> Deserialize(List<string> strList)
        {
            var links = new List<Link>();
            foreach (var str in strList)
            {
                var link = Deserialize(str);
                links.Add(link);
            }
            return links;
        }
    }
}
