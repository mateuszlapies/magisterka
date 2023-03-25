using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
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
        public bool CreateUser(string username)
        {
            logger.LogInformation("Creating new user {username}", username);
            return userService.CreateUser(username);
        }
    }
}
