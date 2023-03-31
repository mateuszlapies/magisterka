using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers.Private
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController
    {
        private readonly ILogger<UserController> logger;
        private readonly UserService userService;

        public UserController(ILogger<UserController> logger, UserService userService)
        {
            this.logger = logger;
            this.userService = userService;
        }

        [HttpPost]
        public string CreateUser(string username)
        {
            logger.LogInformation("Creating new user {username}", username);
            return userService.CreateUser(username);
        }
    }
}
