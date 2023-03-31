using Blockchain.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers.Private
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public bool Synced()
        {
            return Context.Synced;
        }
    }
}
