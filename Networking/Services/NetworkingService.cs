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

        public static bool Lock(Guid? lockId, Guid nextId, string owner)
        {
            LockRequest request = new()
            {
                LockId = lockId,
                NextId = nextId,
                Owner = owner
            };

            var tasks = new List<Task>();

            var successes = new List<LockEndpoint>();
            var failures = new Dictionary<LockEndpoint, LockResponse>();

            foreach(LockEndpoint c in EndpointService.Instances.Get<LockEndpoint>()) {
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    var response = await c.Request(request);
                    if (response.Success)
                    {
                        successes.Add(c);
                    }
                    else
                    {
                        failures.Add(c, response);
                    }
                }));
            };

            Task.WaitAll(tasks.ToArray());

            if (successes.Count == 0 && failures.Count == 0 && successes.Count > failures.Count)
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}