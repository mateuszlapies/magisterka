using Hangfire;

namespace Application.Jobs
{
    public class HangfireJobs : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                BackgroundJob.Enqueue<SyncJob>(x => x.Run());
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => { });
        }
    }
}
