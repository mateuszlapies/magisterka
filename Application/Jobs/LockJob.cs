using Application.Services;
using Blockchain.Contexts;
using Blockchain.Model;
using Hangfire;
using Hangfire.Server;
using Networking.Services;

namespace Application.Jobs
{
    [MaximumConcurrentExecutions(1)]
    [AutomaticRetry(Attempts = 5)]
    public class LockJob
    {
        private readonly RSAService rsaService;
        private readonly LockContext lockContext;

        public LockJob(RSAService rsaService, LockContext lockContext)
        {
            this.lockContext = lockContext;
            this.rsaService = rsaService;
        }

        public void Run(Link link, string owner, PerformContext performContext)
        {
            int retries = performContext.GetJobParameter<int>("RetryCount");
            if (retries > 0)
            {
                lockContext.Unlock(link.Id);
                link = lockContext.Refresh(link.Id, rsaService.GetParameters(true));
            } else if (retries > 4)
            {
                lockContext.Unlock(link.Id);
                lockContext.Remove(link.Id);
                link = null;
            }
            if (link != null)
            {
                if (link.LastId.HasValue)
                {
                    lockContext.Lock(lockContext.Get(link.LastId.Value), lockContext.Get(link.Id), rsaService.GetOwner());
                }
                NetworkingService.Lock(link, owner);
               lockContext.Confirm(link.Id);
            }
        }
    }
}
