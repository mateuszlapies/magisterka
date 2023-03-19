using Makaretu.Dns;
using Networking.Data;

namespace Networking.Services
{
    public class HubService
    {
        public static HubInstances Instances { get; set; }

        private static MulticastService multicastService;
        private static ServiceDiscovery serviceDiscovery;

        private static readonly DomainName instanceName = new (Environment.MachineName);
        private static readonly DomainName serviceName = new ("mgr.local");
        private static readonly ServiceProfile serviceProfile = new (instanceName, serviceName, 8443);

        public static void Init()
        {
            if (Instances == null)
            {
                Instances = new();
            }

            if (multicastService == null)
            {
                multicastService = new MulticastService();

                multicastService.AnswerReceived += AnswerReceived;
            }

            if (multicastService != null && serviceDiscovery == null)
            {
                serviceDiscovery = new ServiceDiscovery(multicastService);

                serviceDiscovery.QueryUnicastServiceInstances(serviceName);
            }
        }

        public static HubInstances Connections()
        {
            if (Instances == null)
            {
                Instances = new();
            }

            return Instances;
        }

        public static void Sync()
        {
            multicastService.QueryReceived += QueryReceived;
            serviceDiscovery.Advertise(serviceProfile);
        }

        private static void AnswerReceived(object? sender, MessageEventArgs e)
        {
            Console.WriteLine(sender);
        }

        private static void QueryReceived(object? sender, MessageEventArgs e)
        {
            Console.WriteLine(sender);
        }
    }
}
