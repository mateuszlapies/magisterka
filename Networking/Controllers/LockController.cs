using Networking.Services;
using Microsoft.AspNetCore.Mvc;
using Networking.Data.Enums;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Utils;

namespace Networking.Controllers
{
    [ApiController]
    [Route("/api/[controller]/[action]")]
    public class LockController
    {
        private readonly LockService lockService;

        public LockController(LockService lockService)
        {
            this.lockService = lockService;
        }

        [HttpPost]
        public LockResponse Lock(LockRequest request)
        {
            var (error, id) = lockService.Lock(LiteSerializer.Deserialize(request.NextLink), request.Owner);
            return new LockResponse()
            {
                Success = error == LockError.None,
                LockError = error,
                LastId = id
            };
        }

        [HttpPost]
        public void Confirm(ConfirmRequest request)
        {
            lockService.Confirm(request.Id);
        }

        [HttpPost]
        public void Unlock(UnlockRequest request)
        {
            lockService.Unlock(request.Id);
        }
    }
}
