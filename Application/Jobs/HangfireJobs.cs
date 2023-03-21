using Hangfire;

namespace Application.Jobs
{
    public class HangfireJobs : IHostedService
    {
        private readonly List<string> jobsId;

        public HangfireJobs()
        {
            jobsId = new List<string>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                string syncId = "Sync";
                BackgroundJob.Enqueue<SyncJob>(x => x.Run());
                RecurringJob.AddOrUpdate<SyncJob>(syncId, x => x.Run(), Cron.Hourly);
                jobsId.Add(syncId);
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                jobsId.ForEach(id => RecurringJob.RemoveIfExists(id));
            }, cancellationToken);
        }
    }
}
