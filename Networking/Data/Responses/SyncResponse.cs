using Blockchain.Model;

namespace Networking.Data.Responses
{
    public class SyncResponse : Response
    {
        public List<string> Links { get; set; }
    }
}
