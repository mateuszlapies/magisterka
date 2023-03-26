using Blockchain.Contexts;
using Blockchain.Model;
using Networking.Services;

namespace Application.Jobs
{
    public class AnnounceJob
    {
        private readonly ILogger<AnnounceJob> logger;
        private readonly Context context;

        public AnnounceJob(ILogger<AnnounceJob> logger)
        {
            this.logger = logger;
        }

        public void Run()
        {
            EndpointService.Announce();
        }
    }
}
