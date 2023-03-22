using Blockchain.Contexts;
using Blockchain.Model;
using Microsoft.AspNetCore.SignalR;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Serilog;

namespace Networking.Hubs
{
    public class SyncHub : Hub, IHub
    {
        private readonly ILogger logger;
        private readonly Context context;

        private static readonly string _endpoint = "sync";
        public static string Endpoint { get { return _endpoint; } }

        public SyncHub(Context context)
        {
            this.logger = Log.ForContext<SyncHub>();
            this.context = context;
        }

        public SyncResponse Sync(SyncRequest request)
        {
            logger.Information("Received Sync request for id: {id}", request?.LastId);
            SyncResponse response = new() { Success = false };

            if (request.LastId.HasValue)
            {
                Link link = context.Get(request.LastId.Value);
                if (link == null)
                {
                    return response;
                }
                Link lastLink = context.GetLastLink();
                if (lastLink == null || lastLink.Id == link.Id)
                {
                    return response;
                }
                response.Links = new List<Link>();
                while(lastLink.Id != link.Id)
                {
                    response.Links.Add(lastLink);
                    lastLink = context.Get(lastLink.LastId.Value);
                }
                response.Success = true;
                return response;
            } else
            {
                response.Links = context.Get();
                response.Success = true;
                return response;
            }
        }
    }
}
