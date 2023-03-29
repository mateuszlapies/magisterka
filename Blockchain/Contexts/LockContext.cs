using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Blockchain.Model;

namespace Blockchain.Contexts
{
    public class LockContext : TempContext
    {
        public new Link Get(Guid id) => base.Get(id);
        public new Link GetLastLink() => base.GetLastLink();
        public new void Add(Link link) => base.Add(link);
        public new void Update(Link link) => base.Update(link);
        public new void Remove(Guid id) => base.Remove(id);
        public new void Transfer(Guid id) => base.Transfer(id);
    }
}
