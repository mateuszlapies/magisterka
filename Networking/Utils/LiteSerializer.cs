using Blockchain.Model;
using System.Text.Json;

namespace Networking.Utils
{
    public static class LiteSerializer
    {

        public static string Serialize(Link link)
        {
            return JsonSerializer.Serialize(link);
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
            var link = JsonSerializer.Deserialize<Link>(str);
            Type type = Type.GetType(string.Format("{0}, {1}", link.ObjectType, "Model"));
            link.Object = JsonSerializer.Deserialize(link.Object.ToString(), type);
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
