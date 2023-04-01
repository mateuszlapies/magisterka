using Application.Model;
using Application.Services;
using Blockchain.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers.Private
{
    [ApiController]
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
        public User GetUser()
        {
            return publicContext.Get<User>().FirstOrDefault();
        }

        [HttpPost]
        public string CreateUser(string username)
        {
            logger.LogInformation("Creating new user {username}", username);
            return userService.CreateUser(username);
        }
    }
}
