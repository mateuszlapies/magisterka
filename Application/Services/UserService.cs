using Application.Model;
using Blockchain.Contexts;
using Networking.Services;

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

        public bool CreateUser(string username)
        {
            if (Context.Synced)
            {
                if (!publicContext.Get<User>().Any(q => q.UserName == username))
                {
                    var id = publicContext.Add<User>(new User()
                    {
                        UserName = username,
                        PublicKey = rsa.GetPublicKey()
                    }, rsa.GetParameters(true));
                    var link = lockContext.Get(id);
                    if (NetworkingService.Lock(link, rsa.GetOwner()))
                    {
                        logger.LogInformation("Successfully created user {username}", username);
                        return true;
                    } else
                    {
                        logger.LogError("Failed to create user {username}. Failed to lock", username);
                        lockContext.Remove(link.Id);
                    }
                }
                else
                {
                    logger.LogError("Failed to create user {username}. User with this name already exists", username);
                }
            } else
            {
                logger.LogError("Failed to create user {username}. Database is not synced", username);
            }
            return false;
        }
    }
}
