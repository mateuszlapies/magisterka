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
        private readonly LockContext context;

        public SyncController(ILogger<SyncController> logger, LockContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        [HttpPost]
        public SyncResponse Sync(SyncRequest request)
        {
            logger.LogInformation("Received Sync request for id: {id}", request?.LastId);
            SyncResponse response = new() { Success = false };

            if (request != null && request.LastId.HasValue)
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
