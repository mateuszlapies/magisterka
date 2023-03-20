using Makaretu.Dns;
using Microsoft.AspNetCore.Hosting.Server;
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
                    if (!e.ServiceInstanceName.Labels.Contains<string>(instanceName.Labels[0]))
                    {
                        if (e.ServiceInstanceName.Labels.Contains<string>(serviceName.Labels[0]))
                        {
                            logger.Information("Service Instance discovered: {name}", e.ServiceInstanceName);
                            multicastService.SendQuery(e.ServiceInstanceName, type: DnsType.SRV);
                        }
                    }
                };

                multicastService.AnswerReceived += (s, e) =>
                {
                    IEnumerable<SRVRecord> services = e.Message.Answers.OfType<SRVRecord>();
                    foreach (SRVRecord service in services)
                    {
                        if (service.Target.Labels[0] != instanceName.Labels[0] && service.Name.Labels[2] == serviceName.Labels[0])
                        {
                            logger.Information("Host {host} for {service} has been discovered", service.Target, service.Name);
                            multicastService.SendQuery(service.Target, type: DnsType.A);
                        }
                    }

                    IEnumerable<AddressRecord> addresses = e.Message.Answers.OfType<AddressRecord>();
                    foreach (AddressRecord address in addresses)
                    {
                        logger.Information("Host {host} at {service} has been discovered", address.Name, address.Address);

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
