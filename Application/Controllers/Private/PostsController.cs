using Application.Model;
using Blockchain.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers.Private
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly PublicContext publicContext;

        public PostsController(PublicContext publicContext)
        {
            this.publicContext = publicContext;
        }

        [HttpGet]
        public List<Post> GetPosts()
        {
            var posts = publicContext.Get<Post>();
            foreach (var post in posts)
            {
                post.User = publicContext.Get<User>(post.UserId);
            }
            return posts;
        }
    }
}
