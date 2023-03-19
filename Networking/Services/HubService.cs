using Makaretu.Dns;
using Microsoft.AspNetCore.SignalR.Client;
using Networking.Data;

namespace Networking.Services
{
    public class HubService
    {
        public static HubInstances instances;

        private static MulticastService multicastService;
        private static ServiceDiscovery serviceDiscovery;

        private static readonly DomainName instanceName = new (Environment.MachineName);
        private static readonly DomainName serviceName = new ("mgr.local");
        private static readonly ServiceProfile serviceProfile = new (instanceName, serviceName, 8443);

        public static void Init()
        {
            if (instances == null)
            {
                instances = new();
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
            if (instances == null)
            {
                instances = new();
            }

            return instances;
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
