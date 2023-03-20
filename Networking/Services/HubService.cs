using Makaretu.Dns;
using Microsoft.AspNetCore.SignalR.Client;
using Networking.Data;
using Networking.Hubs;
using Serilog;
using System.Linq;
using System.Net.NetworkInformation;

namespace Networking.Services
{
    public class HubService
    {
        private static readonly string service = "blockchain";
        private static readonly string instance = Environment.MachineName;

        private static readonly ILogger logger = Log.ForContext(typeof(HubService));

        public static HubInstances Instances { get; set; }

        private static MulticastService multicastService;
        private static ServiceDiscovery serviceDiscovery;

        private static readonly DomainName instanceName = new (instance);
        private static readonly DomainName serviceName = new (string.Format("_{0}._tcp", service));
        private static readonly ServiceProfile serviceProfile = new (instanceName, serviceName, 44487);

        public static void Init()
        {
            if (Instances == null)
            {
                Instances = new();
            }

            if (multicastService == null)
            {
                multicastService = new MulticastService();
                multicastService.UseIpv6 = false;
            }

            if (multicastService != null && serviceDiscovery == null)
            {
                serviceDiscovery = new ServiceDiscovery(multicastService);

                serviceDiscovery.Advertise(serviceProfile);

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
                    string name = e.ServiceInstanceName.ToString();
                    if (!name.Contains(instance) && name.Contains(service))
                    {
                        logger.Information("Service Instance discovered: {name}", e.ServiceInstanceName);
                        multicastService.SendQuery(e.ServiceInstanceName, type: DnsType.SRV);
                    }
                };

                multicastService.AnswerReceived += (s, e) =>
                {
                    IEnumerable<SRVRecord> services = e.Message.Answers.OfType<SRVRecord>();
                    foreach (SRVRecord srv in services)
                    {
                        if (!srv.Target.ToString().Contains(instance) && srv.Name.ToString().Contains(service))
                        {
                            logger.Information("Host {host} has been discovered", srv.Target, srv.Name);
                            multicastService.SendQuery(srv.Target, type: DnsType.A);
                        }
                    }

                    IEnumerable<AddressRecord> addresses = e.Message.Answers.OfType<AddressRecord>();
                    foreach (AddressRecord address in addresses)
                    {
                        if (address.Type == DnsType.A)
                        {
                            string addressName = address.Name.ToString();

                            if (!addressName.Contains(instance) && addressName.Contains(service))
                            {
                                if (address.Address.ToString().StartsWith("192.") || address.Address.ToString().StartsWith("10."))
                                {
                                    logger.Information("Host {host} at {service} has been discovered", address.Name, address.Address);
                                    HubConnection connection = new HubConnectionBuilder()
                                    .WithUrl(string.Format("https://{0}:44487/sync", address.Address), options =>
                                    {
                                        options.HttpMessageHandlerFactory = (message) =>
                                        {
                                            if (message is HttpClientHandler clientHandler)
                                                // always verify the SSL certificate
                                                clientHandler.ServerCertificateCustomValidationCallback +=
                                                    (sender, certificate, chain, sslPolicyErrors) => { return true; };
                                            return message;
                                        };
                                    })
                                    .WithAutomaticReconnect()
                                    .Build();
                                    connection.StartAsync().GetAwaiter().GetResult();
                                    Instances.Add<SyncHub>(connection);
                                }
                            }
                        }
                    }
                };

                multicastService.Start();
            }
        }

        public static void Sync()
        {
            serviceDiscovery.Announce(serviceProfile);
        }

        public static void Close()
        {
            serviceDiscovery.Unadvertise(serviceProfile);
            multicastService.Start();
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
