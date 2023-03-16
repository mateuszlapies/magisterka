using Blockchain.Model;
using LiteDB;
using System.Security.Cryptography;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Blockchain.Contexts
{
    public class Context
    {
        private readonly LiteDatabase database;
        private readonly ILiteCollection<Link> chain;

        public Context(Database database)
        {
            this.database = database.Instance();
            chain = this.database.GetCollection<Link>("chain");
        }

        public void Clear()
        {
            chain.DeleteAll();
        }

        public Link Get(Guid id)
        {
            return chain.FindOne(q => q.Id == id);
        }

        public Guid Add<T>(T obj, RSAParameters key)
        {
            Link last = GetLastLink();
            Guid id = Guid.NewGuid();
            Link link = new()
            {
                Id = id,
                Object = obj,
                ObjectType = obj.GetType().ToString(),
                LastId = last?.Id,
                LastLink = last,
                Signature = null
            };
            link.Signature = Sign(link, key);
            chain.Insert(link);
            return id;
        }

        public void Update(Link link)
        {
            chain.Update(link);
        }

        public bool Verify(Guid id)
        {
            Link link = Get(id);
            if (link.LastId != null)
            {
                link.LastLink = Get(link.LastId.Value);
                return Verify(link) && Verify(link.LastId.Value);
            }
            else
            {
                return Verify(link);
            }
        }

        public Link GetLastLink()
        {
            Link link = chain.Query().Where(q => q.LastId == null).SingleOrDefault();
            while (link?.LastId != null)
            {
                link = chain.Query().Where(q => q.LastId == link.Id).SingleOrDefault();
            }
            return link;
        }

        private static string Serialize(Link link)
        {
            return JsonSerializer.Serialize(link);
        }

        private static Signature Sign(Link link, RSAParameters key)
        {
            string json = Serialize(link);
            using RSACryptoServiceProvider rsa = new();
            rsa.ImportParameters(key);
            byte[] unsigned = Encoding.ASCII.GetBytes(json);
            byte[] signed = rsa.SignData(unsigned, SHA256.Create());
            Signature signature = new()
            {
                Hash = Convert.ToBase64String(signed),
                Owner = Convert.ToBase64String(rsa.ExportRSAPublicKey())
            };
            return signature;
        }

        private static bool Verify(Link link)
        {
            Signature hash = link.Signature;
            link.Signature = null;
            string json = Serialize(link);
            using RSACryptoServiceProvider rsa = new();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(hash.Owner), out int bytesRead);
            byte[] unsigned = Encoding.ASCII.GetBytes(json);
            byte[] signed = Convert.FromBase64String(hash.Hash);
            return rsa.VerifyData(unsigned, SHA256.Create(), signed);
        }
    }
}