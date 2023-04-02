using Application.Jobs;
using Application.Model;
using Blockchain.Contexts;
using Hangfire;

namespace Application.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> logger;
        private readonly CreateContext publicContext;
        private readonly LockContext lockContext;
        private readonly RSAService rsa;

        public UserService(ILogger<UserService> logger, CreateContext publicContext, LockContext lockContext, RSAService rsa)
        {
            this.logger = logger;
            this.publicContext = publicContext;
            this.lockContext = lockContext;
            this.rsa = rsa;
        }

        public string CreateUser(string username)
        {
            if (Context.Synced)
            {
                if (!publicContext.Get<User>().Any(q => q.Name == username))
                {
                    var id = publicContext.Add<User>(new User()
                    {
                        Name = username
                    }, rsa.GetParameters(true));
                    var link = lockContext.Get(id);
                    return BackgroundJob.Enqueue<LockJob>(x => x.Run(link, rsa.GetOwner(), default));
                }
                else
                {
                    logger.LogError("Failed to create user {username}. User with this name already exists", username);
                }
            } else
            {
                logger.LogError("Failed to create user {username}. Database is not synced", username);
            }
            return string.Empty;
        }
    }
}
