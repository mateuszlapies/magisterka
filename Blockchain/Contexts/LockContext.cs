using Blockchain.Model;
using System.Security.Cryptography;

namespace Blockchain.Contexts
{
    public class LockContext : TempContext
    {
        public void Lock(Link lockLink, Link nextLink, string owner)
        {
            lockLink.Lock = new Lock()
            {
                NextId = nextLink.Id,
                Owner = owner,
                Expires = DateTime.UtcNow.AddMinutes(1),
                Confirmed = false
            };

            Update(lockLink);
            Add(nextLink);
        }

        public void Unlock(Guid id)
        {
            var link = Get(id);
            if (link != null)
            {
                var lockedLink = Get(link.LastId.Value);
                if (lockedLink.Lock != null)
                {
                    lockedLink.Lock = null;
                    Update(lockedLink);
                }
                Remove(link.Id);
            }
            CalculateLastLink();
        }

        public void Confirm(Guid id)
        {
            var link = Get(id);
            if (link != null)
            {
                if (link.LastId.HasValue)
                {
                    var lastLink = Get(link.LastId.Value);
                    lastLink.Lock.Confirmed = true;
                    Update(lastLink);
                }
                Transfer(id);
            }
            CalculateLastLink();
        }

        public Link Refresh(Guid id, RSAParameters parameters)
        {
            var link = Get(id);
            Remove(id);
            CalculateLastLink();
            return Add(id, link.Object, link.ObjectType, parameters);
        }

        private Link Add(Guid id, object obj, string objType, RSAParameters key)
        {
            Link last = GetLastLink();
            Link link = new()
            {
                Id = id,
                Object = obj,
                ObjectType = objType,
                LastId = last?.Id,
                LastLink = last,
                Signature = null
            };
            link.Signature = Sign(link, key);
            Temp.Insert(link);
            CalculateLastLink();
            return link;
        }

        public new Link Get(Guid id) => base.Get(id);
        public new Link GetLastLink() => base.GetLastLink();
        public new void Remove(Guid id) => base.Remove(id);
        public new void Clear() => base.Clear();
    }
}
