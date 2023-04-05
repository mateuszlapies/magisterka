﻿using Application.Data;
using Application.Services;
using Blockchain.Contexts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [EnableCors("Local")]
    [Route("api/[controller]/[action]")]
    public class StatusController : ControllerBase
    {
        private readonly UserService userService;
        private readonly RSAService rsaService;

        public StatusController(UserService userService, RSAService rsaService)
        {
            this.userService = userService;
            this.rsaService = rsaService;
        }

        [HttpGet]
        public Response<bool> Synced()
        {
            return new()
            {
                Object = Context.Synced
            };
        }

        [HttpGet]
        public new Response<bool> User()
        {
            return new()
            {
                Object = userService.GetUser(rsaService.GetPublicKey()) != null
            };
        }

        [HttpGet]
        public Response<bool> Username(string username)
        {
            return new()
            {
                Object = userService.GetUser(username) == null
            };
        }
    }
}
