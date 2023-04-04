using Application.Data;
using Model;
using Application.Services;
using Blockchain.Contexts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [EnableCors("Local")]
    [Route("api/[controller]/[action]")]
    public class PostController : ControllerBase
    {
        private readonly PostService postService;

        public PostController( PostService postService)
        {
            this.postService = postService;
        }

        [HttpGet]
        public Response<KeyValuePair<Post, User>> GetPost(Guid id)
        {
            return new()
            {
                Object = postService.GetPost(id).GetValueOrDefault()
            };
        }

        [HttpGet]
        public Response<List<KeyValuePair<Post, User>>> GetPosts()
        {
            return new()
            {
                Object = postService.GetPosts()
            };
        }

        [HttpPut]
        public Response<string> CreatePost(Request<string> request)
        {
            return new()
            {
                Object = postService.CreatePost(request.Object)
            };
        }
    }
}