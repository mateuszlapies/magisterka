using Networking.Data.Enums;

namespace Networking.Data.Responses
{
    public class LockResponse : Response
    {
        public LockError LockError { get; set; }
        public Guid? LastId { get; set; }
    }
}
