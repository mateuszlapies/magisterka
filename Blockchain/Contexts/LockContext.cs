using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Blockchain.Model;

namespace Blockchain.Contexts
{
    public class LockContext : TempContext
    {
        public new Link Get(Guid id) => base.Get(id);
        public new void Remove(Guid id) => base.Remove(id);
    }
}
