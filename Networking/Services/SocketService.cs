using Blockchain.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Hubs;
using Serilog;

namespace Networking.Services
{
    public class SocketService
    {
        private static readonly ILogger logger = Log.ForContext<SocketService>();

        public static List<Link> Sync(Guid? lastId)
        {
            List<Link> links = new();
            int count = HubService.Instances.Count<SyncHub>();
            logger.Information("{count} connections has been found fo Sync", count);
            if (count > 0)
            {
                SyncRequest request = new()
                {
                    LastId = lastId
                };

                List<Task<SyncResponse>> tasks = new();

                foreach (HubConnection c in HubService.Instances.Get<SyncHub>())
                {
                    tasks.Add(c.InvokeAsync<SyncResponse>("Sync", request));
                }

                Task.WaitAll(tasks.ToArray());

                tasks.ForEach(t => links = links.Union(t.Result.Links).ToList());
            }
            HubService.Sync();
            return links;
        }

        public static bool Lock(Guid lockId, Guid nextId, string owner)
        {
            LockRequest request = new()
            {
                LockId = lockId,
                NextId = nextId,
                Owner = owner
            };

            List<Task<LockResponse>> tasks = new();

            foreach(HubConnection c in HubService.Instances.Get<LockHub>()) { 
                tasks.Add(c.InvokeAsync<LockResponse>("Lock", request));
            };

            Task.WaitAll(tasks.ToArray());

            KeyValuePair<bool, int> success = tasks.GroupBy(g => g.Result.Success).Select(s => new KeyValuePair<bool, int>(s.Key, s.Count())).OrderByDescending(o => o.Value).First();
            //TO DO: Implement behaviour when the guid is already locked
            return success.Key;
        }
    }
}