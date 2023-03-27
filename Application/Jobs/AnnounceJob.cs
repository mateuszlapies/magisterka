using Networking.Services;

namespace Application.Jobs
{
    public class AnnounceJob
    {
        private readonly ILogger<AnnounceJob> logger;

        public AnnounceJob(ILogger<AnnounceJob> logger)
        {
            this.logger = logger;
        }

        public void Run()
        {
            logger.LogInformation("Announcing service using mdns");
            EndpointService.Announce();
        }
    }
}
