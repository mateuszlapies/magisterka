using System.Security.Cryptography;

namespace Blockchain.Contexts
{
    public class PublicContext
    {
        private readonly Context context;

        public PublicContext(Context context)
        {
            this.context = context;
        }

        public T Get<T>(Guid id)
        {
            return (T)context.Get(id).Object;
        }

        public Guid Add<T>(T obj, RSAParameters key)
        {
            return context.Add(obj, key);
        }
    }
}
