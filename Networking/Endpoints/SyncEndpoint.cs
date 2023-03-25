using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Endpoints;

namespace Networking.Hubs
{
    public class SyncEndpoint : Endpoint
    {
        public override string EndpointAddress { get { return "Sync"; } }

        public async Task<SyncResponse> Request(SyncRequest request)
        {
            return await Request<SyncRequest, SyncResponse>(request);
        }
    }
}
