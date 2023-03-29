using Blockchain.Model;
using LiteDB;
using System.Security.Cryptography;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Blockchain.Contexts
{
    public class Context
    {
        protected LiteDatabase Database { get; }
        public ILiteCollection<Link> Chain { get; }
        public static bool Synced { get; protected set; }

        private static Guid? lastId;

        protected Context()
        {
            Database = Blockchain.Database.Instance();
            Chain = Database.GetCollection<Link>("chain");
        }

        protected void Clear()
        {
            Chain.DeleteAll();
            CalculateLastLink();
        }

        protected Link Get(Guid id)
        {
            return Chain.FindOne(q => q.Id == id);
        }

        protected List<Link> Get()
        {
            return Chain.FindAll().ToList();
        }

        protected void Remove(Guid id)
        {
            Chain.Delete(id);
            CalculateLastLink();
        }

        protected void Remove(IEnumerable<Link> links)
        {
            Chain.DeleteMany(d => links.Any(q => q.Id == d.Id));
            CalculateLastLink();
        }

        protected bool Verify()
        {
            Guid? id = GetLastId();
            if (id.HasValue)
            {
                return Verify(id.Value);
            }
            return true;
        }

        protected bool Verify(Guid id)
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

        protected static Guid? GetLastId()
        {
            return lastId;
        }

        protected Link? GetLastLink()
        {
            if (lastId.HasValue)
            {
                return Get(lastId.Value);
            } else
            {
                return null;
            }
        }

        protected void CalculateLastLink()
        {
            Link link = Chain.Query().Where(q => q.LastId == null).SingleOrDefault();
            Link tempLink = link;
            while (tempLink != null)
            {
                tempLink = Chain.Query().Where(q => q.LastId == tempLink.Id).SingleOrDefault();
                if (tempLink != null)
                {
                    link = tempLink;
                }
            }
            lastId = link?.Id;
        }

        protected static string Serialize(Link link)
        {
            return JsonSerializer.Serialize(link);
        }

        protected static Signature Sign(Link link, RSAParameters key)
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

        protected static bool Verify(Link link)
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