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

        public async Task Confirm(ConfirmRequest request)
        {
            await Request(request, "Confirm");
        }

        public async Task Unlock(UnlockRequest request)
        {
            await Request(request, "Unlock");
        }
    }
}
