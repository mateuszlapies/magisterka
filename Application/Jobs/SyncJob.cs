using Blockchain.Contexts;
using Blockchain.Model;
using Hangfire;
using Networking.Services;

namespace Application.Jobs
{
    [AutomaticRetry(Attempts = 0)]
    [MaximumConcurrentExecutions(1)]
    public class SyncJob
    {
        private readonly ILogger<SyncJob> logger;
        private readonly SyncContext context;

        public SyncJob(ILogger<SyncJob> logger, SyncContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public void Run()
        {
            Guid? lastId = context.GetLastId();
            logger.LogInformation("Sending Sync requests for last link id: {lastId}", lastId);
            List<Link> links = NetworkingService.Sync(lastId);
            if (links.Count > 0)
            {
                logger.LogInformation("Received {amount} link(s)", links.Count);
                context.Add(links);
                if (!context.Verify())
                {
                    logger.LogError("Wrong chain has been received");
                    context.Remove(links);
                }
            } else
            {
                logger.LogInformation("No new links have been found");
            }
            LockContext.Synced = true;
            EndpointService.Query();
        }
    }
}
