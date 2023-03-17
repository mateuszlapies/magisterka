using Blockchain.Model;

namespace Networking.Data.Responses
{
    public class SyncResponse : Response
    {
        public List<Link> Links { get; set; }
    }
}
