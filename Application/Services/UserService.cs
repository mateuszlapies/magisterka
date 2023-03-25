using Application.Model;
using Blockchain.Contexts;
using Networking.Services;

namespace Application.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> logger;
        private readonly Context context;
        private readonly RSAService rsa;

        public UserService(ILogger<UserService> logger, Context context, RSAService rsa)
        {
            this.logger = logger;
            this.context = context;
            this.rsa = rsa;
        }

        public bool CreateUser(string username)
        {
            if (context.Synced)
            {
                if (!context.Get<User>().Any(q => (q.Object as User).UserName == username))
                {
                    var lastId = context.GetLastLink().Id;
                    var id = context.Add<User>(new User()
                    {
                        UserName = username,
                        PublicKey = rsa.GetPublicKey()
                    }, rsa.GetParameters(true));
                    if (NetworkingService.Lock(lastId, id, rsa.GetOwner()))
                    {
                        logger.LogInformation("Success");
                    } else
                    {
                        logger.LogError("Failure");
                    }
                    return true;
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
