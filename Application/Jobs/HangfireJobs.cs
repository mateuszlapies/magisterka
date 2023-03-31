using Hangfire;
using Networking.Services;

namespace Application.Jobs
{
    public class HangfireJobs : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                EndpointService.Init();
                BackgroundJob.Enqueue<SyncJob>(x => x.Run(default));
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => { }, cancellationToken);
        }
    }
}
