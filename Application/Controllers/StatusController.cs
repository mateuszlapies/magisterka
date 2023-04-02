using Application.Data;
using Blockchain.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public Response<bool> Synced()
        {
            return new()
            {
                Object = Context.Synced
            };
        }
    }
}
