﻿using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Networking.Data.Enums;
using Networking.Data.Requests;
using Networking.Data.Responses;

namespace Application.Controllers
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
            var (error, id) = lockService.Lock(request.NextLink, request.Owner);
            return new LockResponse()
            {
                Success = error == LockError.None,
                LockError = error,
                LastId = id
            };
        }

        [HttpPost]
        public void Confirm(Guid id)
        {
            lockService.Confirm(id);
        }

        [HttpPost]
        public void Unlock(Guid id)
        {
            lockService.Unlock(id);
        }
    }
}
