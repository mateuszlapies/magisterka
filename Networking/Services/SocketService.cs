using System.Linq;
using Blockchain.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Networking.Data.Requests;
using Networking.Data.Responses;

namespace Networking.Services
{
    public class SocketService
    {
        public SocketService()
        {
            HubService.Init();
        }

        public List<Link> Sync(Guid lastId)
        {
            SyncRequest request = new()
            {
                LastId = lastId
            };

            List<Task<SyncResponse>> tasks = new();

            foreach(HubConnection c in HubService.Connections().Values)
            {
                tasks.Add(c.InvokeAsync<SyncResponse>("Sync", request));
            }

            Task.WaitAll(tasks.ToArray());

            List<Link> links = new();
            tasks.ForEach(t => links.Union(t.Result.Links));
            return links;
        }

        public bool Lock(Guid lockId, Guid nextId, string owner)
        {
            LockRequest request = new()
            {
                LockId = lockId,
                NextId = nextId,
                Owner = owner
            };

            List<Task<LockResponse>> tasks = new();

            foreach(HubConnection c in HubService.Connections().Values) { 
                tasks.Add(c.InvokeAsync<LockResponse>("Lock", request));
            };

            Task.WaitAll(tasks.ToArray());

            KeyValuePair<bool, int> success = tasks.GroupBy(g => g.Result.Success).Select(s => new KeyValuePair<bool, int>(s.Key, s.Count())).OrderByDescending(o => o.Value).First();
            //TO DO: Implement behaviour when the guid is already locked
            return success.Key;
        }
    }
}