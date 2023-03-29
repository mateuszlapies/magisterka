using Blockchain.Model;

namespace Networking.Data.Requests
{
    public class LockRequest
    {
        public Link NextLink { get; set; }
        public string Owner { get; set; }
    }
}
