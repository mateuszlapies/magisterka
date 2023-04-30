using Application.Jobs;
using Model;
using Blockchain.Contexts;
using Hangfire;

namespace Application.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> logger;
        private readonly CreateContext createContext;
        private readonly LockContext lockContext;
        private readonly RSAService rsa;

        public UserService(ILogger<UserService> logger, CreateContext createContext, LockContext lockContext, RSAService rsa)
        {
            this.logger = logger;
            this.createContext = createContext;
            this.lockContext = lockContext;
            this.rsa = rsa;
        }

        public bool CheckUser(string username)
        {
            return lockContext.Get<User>()
                .Where(q => (q.Object as User).Name == username)
                .Any();
        }

        public User GetUser(string owner)
        {
            var link = lockContext.Get<User>()
                .Where(q => q.Signature.Owner == owner)
                .OrderByDescending(o => o.Timestamp)
                .FirstOrDefault();

            return link?.Object as User;
        }

        public List<User> GetUsers()
        {
            List<User> users = new();
            var owners = lockContext.Get<User>()
                .DistinctBy(d => d.Signature.Owner)
                .Select(s => s.Signature.Owner);
            foreach (var owner in owners)
            {
                var user = GetUser(owner);
                if (user != null)
                {
                    users.Add(user);
                }
            }
            return users;
        }

        public string CreateUser(string username)
        {
            if (Context.Synced)
            {
                if (!createContext.Get<User>().Any(q => q.Name == username))
                {
                    var link = createContext.Add<User>(new User()
                    {
                        Name = username
                    }, rsa.GetParameters(true));
                    return BackgroundJob.Enqueue<LockJob>(x => x.Run(link.Id, default));
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
