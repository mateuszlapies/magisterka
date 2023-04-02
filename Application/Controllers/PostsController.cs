using Application.Data;
using Application.Model;
using Application.Services;
using Blockchain.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly PublicContext publicContext;
        private readonly PostService postService;

        public PostsController(PublicContext publicContext, PostService postService)
        {
            this.publicContext = publicContext;
            this.postService = postService;
        }

        [HttpGet]
        public Response<Post> GetPost(Guid id)
        {
            return new()
            {
                Object = publicContext.Get<Post>(id)
            };
        }

        [HttpGet]
        public Response<List<Post>> GetPosts()
        {
            var posts = publicContext.Get<Post>();
            foreach (var post in posts)
            {
                post.User = publicContext.Get<User>(post.UserId);
            }
            return new()
            {
                Object = posts
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