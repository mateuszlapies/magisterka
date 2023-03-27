using Blockchain.Contexts;
using Blockchain.Model;
using Networking.Data.Enums;

namespace Application.Services
{
    public class LockService
    {
        private readonly Context context;

        public LockService(Context context)
        {
            this.context = context;
        }

        public (LockError, Guid?) Lock(Guid? lockId, string owner, Link newLink)
        {
            Link lockLink = null;
            if (lockId.HasValue)
            {
                lockLink = context.Get(lockId.Value);
            }
            Link last = context.GetLastLink();

            if (lockLink == null)
            {
                return (LockError.WrongLockId, last.Id);
            }

            if (lockLink != null && last != null)
            {
                if (lockLink.Id != last.Id)
                {
                    return (LockError.WrongLockId, last.Id);
                }
            }

            if (lockLink.Lock != null)
            {
                if (lockLink.Lock.Expires > DateTime.UtcNow || lockLink.Lock.Confirmed)
                {
                    return (LockError.LockIdIsAlreadyLocked, last.Lock.NextId);
                }
            }

            lockLink.Lock = new Lock()
            {
                NextId = newLink.Id,
                Owner = owner,
                Expires = DateTime.UtcNow.AddMinutes(1),
                Confirmed = false
            };

            context.Update(lockLink);
            context.Add(newLink);

            return (LockError.None, Guid.Empty);
        }

        public void Unlock(Guid id)
        {
            var link = context.Get(id);
            if (link != null)
            {
                if (link.Lock != null)
                {
                    var cleanup = context.Get(link.Lock.NextId);
                    while (cleanup != null)
                    {
                        context.Remove(cleanup);
                        if (cleanup.Lock != null)
                        {
                            cleanup = context.Get(cleanup.Lock.NextId);
                        }
                    }
                }
                context.Remove(link);
            }
        }

        public void Confirm(Guid id)
        {
            var link = context.Get(id);
            if (link != null && link.LastId != null)
            {
                var lastLink = context.Get(link.LastId);
                lastLink.Lock.Confirmed = true;
                context.Update(lastLink);
                context.Transfer(id);
            }
        }
    }
}
