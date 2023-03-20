using Makaretu.Dns;
using Networking.Data;

namespace Networking.Services
{
    public class HubService
    {
        public static HubInstances Instances { get; set; }

        //private static MulticastService multicastService;
        private static ServiceDiscovery serviceDiscovery;

        private static readonly DomainName instanceName = new (Environment.MachineName);
        private static readonly DomainName serviceName = new ("_blockchain._tcp");
        private static readonly ServiceProfile serviceProfile = new (instanceName, serviceName, 7281);

        public static void Init()
        {
            if (Instances == null)
            {
                Instances = new();
            }

            //if (multicastService == null)
            //{
            //    multicastService = new MulticastService();

            //    multicastService.AnswerReceived += AnswerReceived;
            //}

            if (/*multicastService != null && */serviceDiscovery == null)
            {
                serviceDiscovery = new ServiceDiscovery(/*multicastService*/);

                serviceDiscovery.ServiceDiscovered += ServiceDiscovered;
                serviceDiscovery.ServiceInstanceDiscovered += ServiceInstanceDiscovered;
            }
        }

        public static void Sync()
        {
            serviceDiscovery.Advertise(serviceProfile);
        }

        public static void Close()
        {
            serviceDiscovery.Unadvertise(serviceProfile);
        }

        public static HubInstances Connections()
        {
            if (Instances == null)
            {
                Instances = new();
            }

            return Instances;
        }

        private static void ServiceInstanceDiscovered(object? sender, ServiceInstanceDiscoveryEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ServiceDiscovered(object? sender, DomainName e)
        {
            throw new NotImplementedException();
        }
    }
}
