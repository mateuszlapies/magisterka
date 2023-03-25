using Blockchain.Contexts;
using Blockchain.Model;
using Microsoft.AspNetCore.Mvc;
using Networking.Data.Requests;
using Networking.Data.Responses;

namespace Application.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class LockController
    {
        private readonly Context context;

        public LockController(Context context)
        {
            this.context = context;
        }

        [HttpPost]
        public LockResponse Lock(LockRequest request)
        {
            Link link = context.Get(request.LockId);
            Link last = context.GetLastLink();

            if (link != null && last != null)
            {
                if (link.Id != last.Id)
                {
                    return new LockResponse()
                    {
                        Success = false,
                        Resync = true
                    };
                }
            }

            if (link.Lock != null)
            {
                if (link.Lock.Expires > DateTime.UtcNow)
                {
                    return new LockResponse()
                    {
                        Success = false,
                        Resync = false,
                        LockInsteadId = link.Lock.NextId
                    };
                }
            }

            link.Lock = new Lock()
            {
                NextId = request.NextId,
                Owner = request.Owner,
                Expires = DateTime.UtcNow.AddMinutes(1)
            };

            context.Update(link);

            return new LockResponse()
            {
                Success = true
            };
        }
    }
}
