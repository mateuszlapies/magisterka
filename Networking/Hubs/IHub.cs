namespace Networking.Hubs
{
    public interface IHub
    {
        private static readonly string _endpoint = "sync";
        public static string Endpoint { get { return _endpoint; } }
    }
}
