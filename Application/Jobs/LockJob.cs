using Application.Services;
using Blockchain.Contexts;
using Blockchain.Model;
using Hangfire;
using Hangfire.Server;
using Networking.Services;

namespace Application.Jobs
{
    [MaximumConcurrentExecutions(1)]
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new int[] { 10, 10, 10, 10, 10 })]
    public class LockJob
    {
        private readonly RSAService rsaService;
        private readonly LockContext lockContext;
        private readonly PerformContext performContext;

        public LockJob(RSAService rsaService, LockContext lockContext, PerformContext performContext)
        {
            this.lockContext = lockContext;
            this.rsaService = rsaService;
            this.performContext = performContext;
        }

        public void Run(Link link, string owner)
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
            }
        }
    }
}
