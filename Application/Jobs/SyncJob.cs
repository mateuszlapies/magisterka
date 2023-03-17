using Blockchain.Contexts;
using Blockchain.Model;
using Networking.Services;

namespace Application.Jobs
{
    public class SyncJob
    {
        private readonly ILogger<SyncJob> logger;
        private readonly Context context;
        private readonly SocketService socketService;
        
        public SyncJob(ILogger<SyncJob> logger, Context context, SocketService socketService)
        {
            this.logger = logger;
            this.context = context;
            this.socketService = socketService;            
        }

        public void Run()
        {
            Guid lastId = context.GetLastLink().Id;
            logger.LogInformation("Sending Sync requests {lastId}", lastId);
            List<Link> links = socketService.Sync(lastId);
            if (links.Count > 0)
            {
                logger.LogInformation("Received links {links}", links);
                context.Add(links);
                if (!context.Verify())
                {
                    logger.LogError("Wrong chain has been received");
                    context.Remove(links);
                }
            } else
            {
                logger.LogInformation("No new links has been found");
            }
            
        }
    }
}
