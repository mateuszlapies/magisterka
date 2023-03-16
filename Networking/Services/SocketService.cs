using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Networking.Data.Requests;
using Networking.Data.Responses;

namespace Networking.Services
{
    public class SocketService
    {
        private readonly DiscoveryService discoveryService;

        public SocketService(DiscoveryService service)
        {
            discoveryService = service;
        }

        public void Lock(Guid lockId, Guid nextId, string owner)
        {
            LockRequest request = new LockRequest()
            {
                LockId = lockId,
                NextId = nextId,
                Owner = owner
            };

            List<LockResponse> responses = new List<LockResponse>();

            discoveryService.Connections.ForEach(c => {
                responses.Add(c.InvokeAsync<LockResponse>("Lock", request).GetAwaiter().GetResult());
            });

            KeyValuePair<bool, int> success = responses.GroupBy(g => g.Success).Select(s => new KeyValuePair<bool, int>(s.Key, s.Count())).OrderByDescending(o => o.Value).First();
            if (success.Key)
            {

            }
        }
    }
}
