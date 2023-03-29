using Blockchain.Contexts;
using Blockchain.Model;
using Microsoft.AspNetCore.Mvc;
using Networking.Data.Requests;
using Networking.Data.Responses;

namespace Application.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class SyncController
    {
        private readonly ILogger<SyncController> logger;
        private readonly SyncContext context;

        public SyncController(ILogger<SyncController> logger, SyncContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        [HttpPost]
        public SyncResponse Sync(SyncRequest request)
        {
            logger.LogInformation("Received Sync request for id: {id}", request?.LastId);
            SyncResponse response = new() { Success = false };

            if (request != null || !Context.Synced)
            {
                return response;
            }
            (response.Success, response.Links) = context.Get(request.LastId);
            return response;
        }
    }
}
