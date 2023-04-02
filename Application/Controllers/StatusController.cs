using Application.Data;
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
