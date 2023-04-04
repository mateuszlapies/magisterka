﻿using Blockchain.Model;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Data;
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

        public Link Refresh(Link link, RSAParameters parameters)
        {
            Link last = GetLastLink();
            link.LastId = last?.Id;
            link.LastLink = last;
            link.Signature = null;
            link.Signature = Sign(link, parameters);
            Temp.Insert(link);
            CalculateLastLink();
            return link;
        }

        public Link Get<T>(Guid id)
        {
            return Get<T>().Where(q => q.Id == id).SingleOrDefault();
        }

        public List<Link> Get<T>()
        {
            return Chain.Query().Where(q => q.ObjectType == typeof(T).ToString()).ToList();
        }

        public new Link Get(Guid id) => base.Get(id);
        public new Link GetLastLink() => base.GetLastLink();
        public new void Remove(Guid id) => base.Remove(id);
        public new void Clear() => base.Clear();
    }
}
