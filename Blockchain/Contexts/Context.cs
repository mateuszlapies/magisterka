using Blockchain.Model;
using LiteDB;
using System.Security.Cryptography;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Blockchain.Contexts
{
    public class Context
    {
        protected static bool Synced { get; set; }

        private Guid? lastId;
        public ILiteCollection<Link> Chain { get; }
        public ILiteCollection<Link> Temp { get; }

        protected Context()
        {
            var database = Database.Instance();
            Chain = database.GetCollection<Link>("chain");
            Temp = database.GetCollection<Link>("temp");
            Temp.DeleteAll();
        }

        protected void Clear()
        {
            Chain.DeleteAll();
            Temp.DeleteAll();
        }

        protected Link Get(Guid id)
        {
            return Chain.FindOne(q => q.Id == id);
        }

        protected List<Link> Get()
        {
            return Chain.FindAll().ToList();
        }

        protected Guid Add<T>(T obj, RSAParameters key)
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
            Temp.Insert(link);
            lastId = link.Id;
            return id;
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

        protected Guid? GetLastId()
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

        protected void CalculateLastLink(bool temp = false)
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
            if (temp && Temp.Count() > 0)
            {
                while (tempLink != null)
                {
                    tempLink = Temp.Query().Where(q => q.LastId == tempLink.Id).SingleOrDefault();
                    if (tempLink != null)
                    {
                        link = tempLink;
                    }
                }
            }
            lastId = link == null ? null : link.Id;
        }

        protected string Serialize(Link link)
        {
            return JsonSerializer.Serialize(link);
        }

        protected Signature Sign(Link link, RSAParameters key)
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

        protected bool Verify(Link link)
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