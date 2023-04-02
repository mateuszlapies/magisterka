using Application.Data;
using Application.Model;
using Application.Services;
using Blockchain.Contexts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [EnableCors("Local")]
    [Route("api/[controller]")]
    public class UserController
    {
        private readonly ILogger<UserController> logger;
        private readonly UserService userService;
        private readonly PublicContext publicContext;

        public UserController(ILogger<UserController> logger, UserService userService, PublicContext publicContext)
        {
            this.logger = logger;
            this.userService = userService;
            this.publicContext = publicContext;
        }

        [HttpGet]
        public Response<User> GetUser(Guid id)
        {
            return new()
            {
                Object = publicContext.Get<User>(id)
            };
        }

        [HttpGet]
        public Response<List<User>> GetUsers()
        {
            return new()
            {
                Object = publicContext.Get<User>()
            };
        }

        [HttpPost]
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
