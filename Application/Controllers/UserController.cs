using Application.Data;
using Model;
using Application.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [EnableCors("Local")]
    [Route("api/[controller]/[action]")]
    public class UserController
    {
        private readonly ILogger<UserController> logger;
        private readonly UserService userService;

        public UserController(ILogger<UserController> logger, UserService userService)
        {
            this.logger = logger;
            this.userService = userService;
        }

        [HttpGet]
        public Response<User> GetUser(string owner)
        {
            return new()
            {
                Object = userService.GetUser(owner)
            };
        }

        [HttpGet]
        public Response<List<User>> GetUsers()
        {
            return new()
            {
                Object = userService.GetUsers()
            };
        }

        [HttpPut]
        public Response<string> CreateUser(Request<string> request)
        {
            logger.LogInformation("Creating new user {username}", request.Object);
            return new()
            {
                Object = userService.CreateUser(request.Object)
            };
        }
    }
}
