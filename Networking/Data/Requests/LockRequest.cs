namespace Networking.Data.Requests
{
    public class LockRequest
    {
        public Guid LockId { get; set; }
        public Guid NextId { get; set; }
        public string Owner { get; set; }
    }
}
