namespace Blockchain.Model
{
    public class Lock
    {
        public Guid NextId { get; set; }
        public string Owner { get; set; }
        public DateTime Expires { get; set; }
    }
}
