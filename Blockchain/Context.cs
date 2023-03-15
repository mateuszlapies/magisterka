using LiteDB;
using System.Security.Cryptography;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Blockchain
{
    public class Context
    {
        private readonly LiteDatabase database;
        private readonly ILiteCollection<Link> chain;

        public Context(string path)
        {
            database = new LiteDatabase(Path.Combine(path, "blockchain.db"));
            chain = database.GetCollection<Link>("chain");
        }

        public void AddLink<T>(T obj, RSAParameters key)
        {
            Link last = GetLastLink();
            Link link = new Link()
            {
                Id = Guid.NewGuid(),
                Object = obj,
                ObjectType = obj.GetType(),
                LastId = last.Id,
                LastLink = last
            };
            link.Hash = CalculateHash(link, key);
            //TO DO: Calculate hash
            chain.Insert(link);
        }

        private string CalculateHash(Link link, RSAParameters key)
        {
            string json = JsonSerializer.Serialize(link);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(key);
                byte[] unsigned = Encoding.Default.GetBytes(json);
                byte[] signed = rsa.SignData(unsigned, SHA256.Create());
                return Encoding.Default.GetString(signed);
            }
        }

        private bool VerifyHash(Link link, RSAParameters key)
        {
            //string hash = link.Hash;
            //string json = JsonSerializer
        }
            
        private Link GetLastLink()
        {
            Link link = chain.Query().Where(q => q.LastId == null).SingleOrDefault();
            while (link != null)
            {
                link = chain.Query().Where(q => q.LastId == link.Id).SingleOrDefault();
            }
            return link;
        }
    }
}