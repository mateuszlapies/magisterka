using Blockchain.Model;
using LiteDB;
using System.Security.Cryptography;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Blockchain.Contexts
{
    public class Context
    {
        public bool Synced { get; set; }

        private Guid lastId;
        private readonly LiteDatabase database;
        private readonly ILiteCollection<Link> chain;

        public Context()
        {
            this.database = Database.Instance();
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

        public List<Link> Get()
        {
            return chain.FindAll().ToList();
        }

        public List<Link> Get<T>()
        {
            return chain.Query().Where(q => q.ObjectType == typeof(T).ToString()).ToList();
        }

        public void Add(Link link)
        {
            chain.Insert(link);
            lastId = link.Id;
        }

        public void Add(IEnumerable<Link> links)
        {
            chain.InsertBulk(links);
            CalculateLastLink();
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
            lastId = link.Id;
            return id;
        }

        public void Update(Link link)
        {
            chain.Update(link);
            CalculateLastLink();
        }

        public void Remove(Link link)
        {
            chain.Delete(link.Id);
            CalculateLastLink();
        }

        public void Remove(IEnumerable<Link> links)
        {
            chain.DeleteMany(d => links.Any(q => q.Id == d.Id));
            CalculateLastLink();
        }

        public bool Verify()
        {
            Guid id = GetLastLink().Id;
            return Verify(id);
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
            return Get(lastId);
        }

        private void CalculateLastLink()
        {
            Link link = chain.Query().Where(q => q.LastId == null).SingleOrDefault();
            Link temp = link;
            while (temp != null)
            {
                temp = chain.Query().Where(q => q.LastId == temp.Id).SingleOrDefault();
                if (temp != null)
                {
                    link = temp;
                }
            }
            lastId = link.Id;
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