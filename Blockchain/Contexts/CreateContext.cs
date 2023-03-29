using Blockchain.Model;
using System.Security.Cryptography;

namespace Blockchain.Contexts
{
    public class CreateContext : TempContext
    {
        public T Get<T>(Guid id)
        {
            return (T)Get(id).Object;
        }

        public List<T> Get<T>()
        {
            return Chain.Query().Where(q => q.ObjectType == typeof(T).ToString()).Select(x => (T)x.Object).ToList();
        }

        public new Guid Add<T>(T obj, RSAParameters key)
        {
            return base.Add<T>(obj, key).Id;
        }

        public new void Clear() => base.Clear();

        public new Guid? GetLastId() => TempContext.GetLastId();

        public new bool Verify(Guid id)
        {
            Link link = Get(id);
            if (link.LastId != null)
            {
                link.LastLink = Get(link.LastId.Value);
                return Verify(link) && Verify(link.LastId.Value);
            }
            else
            {
                return Verify(link);
            }
        }
    }
}
