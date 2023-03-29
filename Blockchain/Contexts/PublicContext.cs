using System.Security.Cryptography;
using Blockchain.Model;

namespace Blockchain.Contexts
{
    public class PublicContext : TempContext
    {
        public T Get<T>(Guid id)
        {
            return (T)Get(id).Object;
        }

        public List<T> Get<T>()
        {
            return Chain.Query().Where(q => q.ObjectType == typeof(T).ToString()).Select(x => (T)x.Object).ToList();
        }

        public new Guid Add<T>(T obj, RSAParameters key) => base.Add<T>(obj, key).Id;

        public new void Clear() => base.Clear();

        public new Guid? GetLastId() => base.GetLastId();

        public new bool Verify(Guid id) => base.Verify(id);
    }
}
