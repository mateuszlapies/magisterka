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
        private readonly PerformContext performContext;

        public SyncJob(ILogger<SyncJob> logger, SyncContext syncContext, PerformContext performContext)
        {
            this.logger = logger;
            this.syncContext = syncContext;
            this.performContext = performContext;
        }

        public void Run()
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
                if (performContext.GetJobParameter<int>("RetryCount") > 4)
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
