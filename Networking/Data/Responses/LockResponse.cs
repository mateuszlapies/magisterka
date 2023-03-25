namespace Networking.Data.Responses
{
    public class LockResponse : Response
    {
        public Guid? LockInsteadId { get; set; }
        public bool Resync { get; set; }
    }
}
