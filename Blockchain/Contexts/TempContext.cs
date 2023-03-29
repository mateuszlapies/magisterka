using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Blockchain.Model;
using LiteDB;

namespace Blockchain.Contexts
{
    public class TempContext : Context
    {
        public ILiteCollection<Link> Temp { get; }

        private Guid? lastId;

        protected TempContext() : base()
        {
            Temp = Database.GetCollection<Link>("temp");
            Temp.DeleteAll();
        }

        protected new List<Link> Get()
        {
            var list = Chain.FindAll().ToList();
            list.AddRange(Temp.FindAll().ToList());
            return list;
        }

        protected new Link Get(Guid id)
        {
            if (IsTemp(id))
            {
                return Temp.FindOne(x => x.Id == id);
            } else
            {
                return base.Get(id);
            }
        }

        protected void Add(Link link)
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
            Temp.Insert(link);
        }

        protected Link Add<T>(T obj, RSAParameters key)
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
            return link;
        }

        protected void Add(IEnumerable<Link> links)
        {
            foreach (var link in links)
            {
                Add(link);
                CalculateLastLink();
            }
        }

        protected void Update(Link link)
        {
            if (IsTemp(link.Id))
            {
                Temp.Update(link);
            }
            else
            {
                Chain.Update(link);
            }
        }

        protected new void Clear()
        {
            base.Clear();
            Temp.DeleteAll();
        }

        protected new void Remove(Guid id)
        {
            if (IsTemp(id))
            {
                Temp.Delete(id);
            } else
            {
                base.Remove(id);
            }
        }

        protected void Transfer(Guid id)
        {
            List<Link> links = new();
            var firstLink = Temp.Query().Where(q => q.Id == id).Single();
            var link = firstLink;
            while (link.Lock != null && link.Lock.Confirmed)
            {
                links.Add(link);
                link = Temp.Query().Where(q => q.Id == link.Lock.NextId).SingleOrDefault();
            }

            link = Temp.Query().Where(q => q.Id == firstLink.LastId).SingleOrDefault();
            while (link != null && link.Lock != null && link.Lock.Confirmed)
            {
                var t = Temp.Query().Where(q => q.Id == link.LastId).SingleOrDefault();
                if (t != null && t.Lock != null && t.Lock.Confirmed)
                {
                    links.Add(link);
                }
                else
                {
                    links.Clear();
                }
                link = t;
            }
            if (links.Count > 0)
            {
                Chain.Insert(links);
                Temp.DeleteMany(q => links.Any(l => l.Id == q.Id));
            }
        }

        protected bool IsTemp(Guid id)
        {
            var t = Temp.Query().Where(q => q.Id == id).SingleOrDefault();
            var c = Chain.Query().Where(q => q.Id == id).SingleOrDefault();
            if (t == null)
            {
                return false;
            }
            else if (c == null)
            {
                return true;
            }
            else
            {
                throw new Exception("Link id not found");
            }
        }

        protected static bool VerifyOwner(string owner, string ownerHash)
        {
            using RSACryptoServiceProvider rsa = new();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(owner), out int bytesRead);
            byte[] unsigned = Convert.FromBase64String(owner);
            byte[] signed = Convert.FromBase64String(ownerHash);
            return rsa.VerifyData(unsigned, SHA256.Create(), signed);
        }

        protected new Guid? GetLastId()
        {
            return lastId;
        }

        protected new Link? GetLastLink()
        {
            if (lastId.HasValue)
            {
                return Get(lastId.Value);
            } else
            {
                return null;
            }
            
        }

        protected new void CalculateLastLink()
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
            if (Temp.Count() > 0)
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
            lastId = link?.Id;
        }

        protected static void SetSynced()
        {
            Synced = true;
        }
    }
}
