using Hangfire;

namespace Application.Jobs
{
    public class HangfireJobs : IHostedService
    {
        private List<string> jobsId;

        public HangfireJobs()
        {
            jobsId = new List<string>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                string syncId = "Sync";
                RecurringJob.AddOrUpdate<SyncJob>(syncId, x => x.Run(), Cron.Minutely);
                jobsId.Add(syncId);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                jobsId.ForEach(id => RecurringJob.RemoveIfExists(id));
            });
        }
    }
}
