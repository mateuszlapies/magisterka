using Application.Jobs;
using Application.Model;
using Blockchain.Contexts;
using Hangfire;

namespace Application.Services
{
    public class PostService
    {
        private readonly ILogger<UserService> logger;
        private readonly CreateContext publicContext;
        private readonly LockContext lockContext;
        private readonly RSAService rsa;

        public PostService(ILogger<UserService> logger, CreateContext publicContext, LockContext lockContext, RSAService rsa)
        {
            this.logger = logger;
            this.publicContext = publicContext;
            this.lockContext = lockContext;
            this.rsa = rsa;
        }

        public string CreatePost(string message)
        {
            if (Context.Synced)
            {
                var user = lockContext.Get<User>().Single(q => q.Signature.Owner == rsa.GetPublicKey());
                var id = publicContext.Add<Post>(new Post()
                {
                    UserId = user.Id,
                    Message = message
                }, rsa.GetParameters(true));
                var link = lockContext.Get(id);
                return BackgroundJob.Enqueue<LockJob>(x => x.Run(link, rsa.GetOwner(), default));
            }
            else
            {
                logger.LogError("Failed to create post. Database is not synced");
            }
            return string.Empty;
        }
    }
}
