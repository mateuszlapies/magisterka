using Blockchain.Model;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Hubs;
using Serilog;

namespace Networking.Services
{
    public class NetworkingService
    {
        private static readonly ILogger logger = Log.ForContext<NetworkingService>();

        public static List<Link> Sync(Guid? lastId)
        {
            List<Link> links = new();
            int count = EndpointService.Instances.Count<SyncEndpoint>();
            logger.Information("{count} connections has been found for Sync", count);
            if (count > 0)
            {
                SyncRequest request = new()
                {
                    LastId = lastId
                };

                List<Task<SyncResponse>> tasks = new();

                foreach (SyncEndpoint c in EndpointService.Instances.Get<SyncEndpoint>())
                {
                    tasks.Add(c.Request(request));
                }

                Task.WaitAll(tasks.ToArray());

                tasks.ForEach(t => links = links.Union(t.Result.Links).ToList());
            }
            EndpointService.Sync();
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

            foreach(LockEndpoint c in EndpointService.Instances.Get<LockEndpoint>()) { 
                tasks.Add(c.Request(request));
            };

            Task.WaitAll(tasks.ToArray());

            KeyValuePair<bool, int> success = tasks.GroupBy(g => g.Result.Success).Select(s => new KeyValuePair<bool, int>(s.Key, s.Count())).OrderByDescending(o => o.Value).First();
            //TO DO: Implement behaviour when the guid is already locked
            return success.Key;
        }
    }
}