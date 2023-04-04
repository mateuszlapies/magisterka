using Blockchain.Model;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Hubs;
using Networking.Utils;
using Serilog;

namespace Networking.Services
{
    public class NetworkingService
    {
        private static readonly ILogger logger = Log.ForContext<NetworkingService>();

        public static List<List<Link>> Sync(Guid? lastId)
        {
            List<List<Link>> links = new();
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

                foreach(var t in tasks)
                {
                    if (t.Result.Success)
                    {
                        links.Add(LiteSerializer.Deserialize(t.Result.Links));
                    }
                }
                    
            } else
            {
                throw new Exception("No nodes have been found");
            }
            return links;
        }

        public static async Task Lock(Link link, string owner)
        {
            LockRequest request = new()
            {
                NextLink = LiteSerializer.Serialize(link),
                Owner = owner
            };

            var tasks = new List<Task>();

            var successes = new List<LockEndpoint>();
            var failures = new Dictionary<LockEndpoint, LockResponse>();
            var endpoints = EndpointService.Instances.Get<LockEndpoint>();

            foreach (var endpoint in endpoints) {
                tasks.Add(await Task.Factory.StartNew(async () =>
                {
                    var response = await endpoint.Lock(request);
                    if (response.Success)
                    {
                        successes.Add(endpoint);
                    }
                    else
                    {
                        failures.Add(endpoint, response);
                    }
                }));
            };

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();
            if (successes.Count > failures.Count)
            {
                foreach (var endpoint in endpoints)
                {
                    tasks.Add(await Task.Factory.StartNew(async () =>
                    {
                        await endpoint.Confirm(link.Id);
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            } else if (endpoints.Count > 0)
            {
                foreach (var endpoint in successes)
                {
                    tasks.Add(await Task.Factory.StartNew(async () =>
                    {
                        await endpoint.Unlock(link.Id);
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                throw new Exception("Lock failed");
            }
        }
    }
}