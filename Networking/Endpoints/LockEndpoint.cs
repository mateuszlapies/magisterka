using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Endpoints;

namespace Networking.Hubs
{
    public class LockEndpoint : Endpoint
    {
        public override string EndpointAddress { get { return "Lock"; } }

        public async Task<LockResponse> Request(LockRequest request)
        {
            return await Request<LockRequest, LockResponse>(request);
        }
    }
}
