using Blockchain.Model;

namespace Networking.Data.Requests
{
    public class LockRequest
    {
        public Guid? LockId { get; set; }
        public Link NextLink { get; set; }
        public string Owner { get; set; }
    }
}
