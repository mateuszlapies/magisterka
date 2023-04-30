using Application.Jobs;
using Model;
using Blockchain.Contexts;
using Blockchain.Model;
using Hangfire;

namespace Application.Services
{
    public class PostService
    {
        private readonly ILogger<UserService> logger;
        private readonly CreateContext createContext;
        private readonly LockContext lockContext;
        private readonly RSAService rsa;

        public PostService(ILogger<UserService> logger, CreateContext publicContext, LockContext lockContext, RSAService rsa)
        {
            this.logger = logger;
            this.createContext = publicContext;
            this.lockContext = lockContext;
            this.rsa = rsa;
        }

        public KeyValuePair<Post, User>? GetPost(Guid id)
        {
            var link = lockContext.Get<Post>(id);
            if (link != null)
            {
                var userLink = lockContext.Get<User>()
                    .Where(q => q.Signature.Owner == link.Signature.Owner)
                    .OrderByDescending(o => o.Timestamp)
                    .SingleOrDefault();
                if (userLink != null)
                {
                    var post = link.Object as Post;
                    var user = userLink.Object as User;
                    return new(post, user);
                }
            }
            return null;
        }

        public List<KeyValuePair<Post, User>> GetPosts(int first = 10)
        {
            List<KeyValuePair<Post, User>> posts = new();
            var postsId = lockContext.Get<Post>()
                .OrderByDescending(o => o.Timestamp)
                .Take(first)
                .Select(s => s.Id);
            foreach (var postId in postsId)
            {
                var post = GetPost(postId);
                if (post.HasValue)
                {
                    posts.Add(post.Value);
                }
            }
            return posts;
        }

        public string CreatePost(string message)
        {
            if (Context.Synced)
            {
                var link = createContext.Add<Post>(new Post()
                {
                    Message = message
                }, rsa.GetParameters(true));
                return BackgroundJob.Enqueue<LockJob>(x => x.Run(link.Id, default));
            }
            else
            {
                logger.LogError("Failed to create post. Database is not synced");
            }
            return string.Empty;
        }
    }
}
