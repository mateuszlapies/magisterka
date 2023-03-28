using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Blockchain.Model;

namespace Blockchain.Contexts
{
    public class TempContext : Context
    {
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

        protected void Add(IEnumerable<Link> links)
        {
            foreach (var link in links)
            {
                Add(link);
                CalculateLastLink(true);
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

        protected void Transfer(Guid id)
        {
            List<Link> links = new List<Link>();
            var firstLink = Temp.Query().Where(q => q.Id == id).Single();
            var link = firstLink;
            while (link.Lock != null && link.Lock.Confirmed)
            {
                links.Add(link);
                link = Temp.Query().Where(q => q.Id == link.Lock.NextId).SingleOrDefault();
            }

            link = Temp.Query().Where(q => q.Id == firstLink.LastId).SingleOrDefault();
            while (link != null && link.Lock.Confirmed)
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

        public bool VerifyOwner(string owner, string ownerHash)
        {
            using RSACryptoServiceProvider rsa = new();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(owner), out int bytesRead);
            byte[] unsigned = Convert.FromBase64String(owner);
            byte[] signed = Convert.FromBase64String(ownerHash);
            return rsa.VerifyData(unsigned, SHA256.Create(), signed);
        }
    }
}
