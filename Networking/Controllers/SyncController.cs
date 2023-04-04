using Blockchain.Contexts;
using Blockchain.Model;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Utils;

namespace Networking.Controllers
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

            if (request == null || !Context.Synced)
            {
                return response;
            }
            (response.Success, List<Link> list) = context.Get(request.LastId);
            response.Links = LiteSerializer.Serialize(list);
            return response;
        }
    }
}
