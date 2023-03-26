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
                string annoId = "Anno";
                RecurringJob.AddOrUpdate<SyncJob>(syncId, x => x.Run(), Cron.Minutely);
                RecurringJob.AddOrUpdate<AnnounceJob>(annoId, x => x.Run(), Cron.Minutely);
                jobsId.Add(syncId);
                jobsId.Add(annoId);
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
