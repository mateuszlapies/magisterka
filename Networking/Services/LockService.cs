using Blockchain.Contexts;
using Blockchain.Model;
using Networking.Data.Enums;

namespace Networking.Services
{
    public class LockService
    {
        private readonly LockContext context;

        public LockService(LockContext context)
        {
            this.context = context;
        }

        public (LockError, Guid?) Lock(Link nextLink, string owner)
        {
            Link lockLink = null;
            if (nextLink.LastId.HasValue)
            {
                lockLink = context.Get(nextLink.LastId.Value);
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

            context.Lock(lockLink, nextLink, owner);

            return (LockError.None, Guid.Empty);
        }

        public void Unlock(Guid id, string signature)
        {
            context.Unlock(id, signature);
        }

        public void Confirm(Guid id, string signature)
        {
            context.Confirm(id, signature);
        }
    }
}
