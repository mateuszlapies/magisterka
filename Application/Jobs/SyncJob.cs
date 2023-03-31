using Blockchain.Contexts;
using Blockchain.Model;
using Hangfire;
using Hangfire.Server;
using Networking.Services;

namespace Application.Jobs
{
    [MaximumConcurrentExecutions(1)]
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new int[] { 10, 10, 10, 10, 10 })]
    public class SyncJob
    {
        private readonly ILogger<SyncJob> logger;
        private readonly SyncContext syncContext;

        public SyncJob(ILogger<SyncJob> logger, SyncContext syncContext)
        {
            this.logger = logger;
            this.syncContext = syncContext;
        }

        public void Run(PerformContext performContext)
        {
            EndpointService.Query();
            Guid? lastId = SyncContext.GetLastId();
            logger.LogInformation("Sending Sync requests for last link id: {lastId}", lastId);
            try
            {
                List<List<Link>> links = NetworkingService.Sync(lastId);
                logger.LogInformation("Received {amount} link(s)", links.Count);
                syncContext.Sync(links);
            } catch (Exception)
            {
                var retries = performContext.GetJobParameter<int>("RetryCount");
                if (retries > 4)
                {
                    syncContext.Sync(new List<List<Link>>());
                } else
                {
                    throw;
                }
            }
        }
    }
}
