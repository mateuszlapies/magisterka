using Makaretu.Dns;
using Networking.Data;
using Serilog;
using System.Net;
using System.Net.NetworkInformation;

namespace Networking.Services
{
    public class HubService
    {
        public static readonly ILogger logger = Log.ForContext(typeof(HubService));

        public static HubInstances Instances { get; set; }

        private static MulticastService multicastService;
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

            if (multicastService == null)
            {
                multicastService = new MulticastService();
            }

            if (multicastService != null && serviceDiscovery == null)
            {
                serviceDiscovery = new ServiceDiscovery(multicastService);

                multicastService.NetworkInterfaceDiscovered += (s, e) =>
                {
                    foreach (NetworkInterface nic in e.NetworkInterfaces)
                    {
                        logger.Information("NIC discovered: {name}", nic.Name);
                    }
                    serviceDiscovery.QueryServiceInstances(serviceName);
                };

                serviceDiscovery.ServiceInstanceDiscovered += (s, e) =>
                {
                    logger.Information("Service Instance discovered: {name}", e.ServiceInstanceName);
                    multicastService.SendQuery(e.ServiceInstanceName, type: DnsType.SRV);
                };

                multicastService.AnswerReceived += (s, e) =>
                {
                    // Is this an answer to a service instance details?
                    var servers = e.Message.Answers.OfType<SRVRecord>();
                    foreach (var server in servers)
                    {
                        Console.WriteLine($"host '{server.Target}' for '{server.Name}'");

                        // Ask for the host IP addresses.
                        //multicastService.SendQuery(server.Target, type: DnsType.A);
                        //multicastService.SendQuery(server.Target, type: DnsType.AAAA);
                    }

                    // Is this an answer to host addresses?
                    var addresses = e.Message.Answers.OfType<AddressRecord>();
                    foreach (var address in addresses)
                    {
                        Console.WriteLine($"host '{address.Name}' at {address.Address}");
                    }
                };

                multicastService.Start();
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
    }
}
