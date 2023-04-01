using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Blockchain.Model;
using LiteDB;

namespace Blockchain.Contexts
{
    public class TempContext : Context
    {
        public ILiteCollection<Link> Temp { get; }

        private static Guid? lastId;

        protected TempContext() : base()
        {
            Temp = Database.GetCollection<Link>("temp");
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
            var lastLink = Get(link.LastId.Value);
            if (lastLink != null)
            {
                if (lastLink.Lock.Expires <= DateTime.UtcNow)
                {
                    throw new ValidationException();
                }

                if (!VerifyOwner(link.Signature.Owner, lastLink.Lock.Owner))
                {
                    throw new ValidationException();
                }

                if (lastLink.Lock.NextId != link.Id)
                {
                    throw new ValidationException();
                }

                if (lastLink.Id != link.LastId)
                {
                    throw new ValidationException();
                }
            }
            if (!Temp.Exists(q => q.Id == link.Id))
            {
                Add(link);
            }
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
            CalculateLastLink();
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
            CalculateLastLink();
        }

        protected void Transfer(Guid id)
        {
            List<Link> links = new();
            var lastChainId = Context.GetLastId();
            var lastChainLink = base.GetLastLink();
            var link = Temp.Query().Where(q => q.LastId == lastChainId).Single();
            while (link != null && ((!link.LastId.HasValue && !lastChainId.HasValue) || (lastChainLink != null && lastChainLink.Lock != null && lastChainLink.Lock.Confirmed)))
            {
                links.Add(link);
                if (link.Lock != null)
                {
                    link = Temp.Query().Where(q => q.Id == link.Lock.NextId).SingleOrDefault();
                }
                else
                {
                    link = null;
                }
            }

            if (links.Count > 0 && links.Any(q => q.Id == id))
            {
                Chain.Insert(links);
                Temp.DeleteMany(q => links.Contains(q));
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
            byte[] unsigned = Convert.FromBase64String(owner);
            rsa.ImportRSAPublicKey(unsigned, out int bytesRead);
            byte[] signed = Convert.FromBase64String(ownerHash);
            return rsa.VerifyData(unsigned, SHA256.Create(), signed);
        }

        protected new static Guid? GetLastId()
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

        protected static Guid? GetLastChainId()
        {
            return Context.GetLastId();
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
                if (tempLink == null)
                {
                    tempLink = Temp.Query().Where(q => q.LastId == null).SingleOrDefault();
                    link = tempLink;
                }

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
            base.CalculateLastLink();
        }

        protected static void SetSynced()
        {
            Synced = true;
        }
    }
}
