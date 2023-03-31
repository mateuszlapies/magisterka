using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Endpoints;

namespace Networking.Hubs
{
    public class LockEndpoint : Endpoint
    {
        public override string EndpointAddress { get { return "Lock"; } }

        public async Task<LockResponse> Lock(LockRequest request)
        {
            return await Request<LockRequest, LockResponse>(request, "Lock");
        }

        public async Task Unlock(Guid id)
        {
            await Request(id, "Unlock");
        }

        public async Task Confirm(Guid id)
        {
            await Request(id, "Confirm");
        }
    }
}
