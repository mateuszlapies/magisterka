using Blockchain.Model;
using LiteDB;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Blockchain.Contexts
{
    public class Context
    {
        public static bool Synced { get; set; }

        private Guid? lastId;
        private readonly LiteDatabase database;
        private readonly ILiteCollection<Link> chain;
        private readonly ILiteCollection<Link> temp;

        public Context()
        {
            database = Database.Instance();
            chain = database.GetCollection<Link>("chain");
            temp = database.GetCollection<Link>("temp");
            temp.DeleteAll();
        }

        public void Clear()
        {
            chain.DeleteAll();
            temp.DeleteAll();
        }

        public Link Get(Guid? id)
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
            var lastLink = GetLastLink();
            if (lastLink != null)
            {
                if (lastLink.Lock.Expires <= DateTime.UtcNow
                    || !VerifyOwner(link.Signature.Owner, lastLink.Lock.Owner)
                    || lastLink.Lock.NextId == link.Id && lastLink.Id == link.LastId)
                {
                    throw new ValidationException();
                }
            }
            temp.Insert(link);
        }

        public void Add(IEnumerable<Link> links)
        {
            foreach (var link in links)
            {
                Add(link);
            }
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
            temp.Insert(link);
            lastId = link.Id;
            return id;
        }

        public void Update(Link link)
        {
            if (IsTemp(link.Id))
            {
                temp.Update(link);
            } else
            {
                chain.Update(link);
            }
        }

        public void Remove(Link link)
        {
            if (IsTemp(link.Id))
            {
                temp.Delete(link.Id);
            } else
            {
                chain.Delete(link.Id);
            }
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

        public bool IsTemp(Guid id)
        {
            var t = temp.Query().Where(q => q.Id == id).SingleOrDefault();
            var c = temp.Query().Where(q => q.Id == id).SingleOrDefault();
            if (t == null)
            {
                return false;
            } else if (c == null)
            {
                return true;
            } else
            {
                throw new Exception("Link id not found");
            }
        }

        public void Transfer(Guid id)
        {
            List<Link> links = new List<Link>();
            var firstLink = temp.Query().Where(q => q.Id == id).Single();
            var link = firstLink;
            while (link.Lock != null && link.Lock.Confirmed)
            {
                links.Add(link);
                link = temp.Query().Where(q => q.Id == link.Lock.NextId).SingleOrDefault();
            }

            link = temp.Query().Where(q => q.Id == firstLink.LastId).SingleOrDefault();
            while (link != null && link.Lock.Confirmed)
            {
                var t = temp.Query().Where(q => q.Id == link.LastId).SingleOrDefault();
                if (t != null && t.Lock != null && t.Lock.Confirmed)
                {
                    links.Add(link);
                } else
                {
                    links.Clear();
                }
                link = t;
            }
            if (links.Count > 0)
            {
                chain.Insert(links);
                temp.DeleteMany(q => links.Any(l => l.Id == q.Id));
            }
        }

        public Link? GetLastLink()
        {
            if (lastId.HasValue)
            {
                return Get(lastId.Value);
            } else
            {
                return null;
            }
        }

        private void CalculateLastLink()
        {

            Link link = chain.Query().Where(q => q.LastId == null).SingleOrDefault();
            Link tempLink = link;
            while (tempLink != null)
            {
                tempLink = chain.Query().Where(q => q.LastId == tempLink.Id).SingleOrDefault();
                if (tempLink != null)
                {
                    link = tempLink;
                }
            }
            if (temp.Count() > 0)
            {
                while (tempLink != null)
                {
                    tempLink = temp.Query().Where(q => q.LastId == tempLink.Id).SingleOrDefault();
                    if (tempLink != null)
                    {
                        link = tempLink;
                    }
                }
            }
            lastId = link == null ? null : link.Id;
        }

        private string Serialize(Link link)
        {
            return JsonSerializer.Serialize(link);
        }

        private Signature Sign(Link link, RSAParameters key)
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

        private bool Verify(Link link)
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

        private bool VerifyOwner(string owner, string ownerHash)
        {
            using RSACryptoServiceProvider rsa = new();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(owner), out int bytesRead);
            byte[] unsigned = Convert.FromBase64String(owner);
            byte[] signed = Convert.FromBase64String(ownerHash);
            return rsa.VerifyData(unsigned, SHA256.Create(), signed);
        }
    }
}