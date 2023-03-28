using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Blockchain.Model;

namespace Blockchain.Contexts
{
    public class LockContext : TempContext
    {

        public new Link Get(Guid id)
        {
            if (IsTemp(id))
            {
                return Temp.FindOne(q => q.Id == id);
            } else
            {
                return Chain.FindOne(q => q.Id == id);
            }
        }
    }
}
