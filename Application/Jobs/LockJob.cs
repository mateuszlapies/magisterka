using Application.Services;
using Blockchain.Contexts;
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

        public async Task Run(Guid id, PerformContext performContext)
        {
            var link = lockContext.Get(id);
            int retries = performContext.GetJobParameter<int>("RetryCount");
            if (retries > 0)
            {
                lockContext.Unlock(id, rsaService.GetSignedPublic());
                link = lockContext.Refresh(link, rsaService.GetParameters(true));
            } else if (retries > 4)
            {
                lockContext.Unlock(id, rsaService.GetSignedPublic());
                link = null;
            }
            if (link != null)
            {
                if (link.LastId.HasValue)
                {
                    lockContext.Lock(lockContext.Get(link.LastId.Value), lockContext.Get(id), rsaService.GetPublicKey());
                }
                await NetworkingService.Lock(link, rsaService.GetPublicKey(), rsaService.GetSignedPublic());
                lockContext.Confirm(id, rsaService.GetSignedPublic());
            }
        }
    }
}
