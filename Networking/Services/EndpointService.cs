﻿using Makaretu.Dns;
using Networking.Endpoints.Instances;
using Networking.Hubs;
using Serilog;

namespace Networking.Services
{
    public class EndpointService
    {
        private static bool synced = false;
        public static EndpointInstances Instances { get; set; }

        private static readonly ILogger logger = Log.ForContext<EndpointService>();

        private static readonly string service = "blockchain";
        private static readonly string instance = Environment.MachineName;

        private static MulticastService multicastService;
        private static ServiceDiscovery serviceDiscovery;

        private static readonly DomainName instanceName = new (instance);
        private static readonly DomainName serviceName = new (string.Format("_{0}._tcp", service));
        private static readonly ServiceProfile serviceProfile = new (instanceName, serviceName, 44487);

        public static void Init()
        {
            Instances ??= new();

            multicastService ??= new MulticastService
                {
                    UseIpv6 = false
                };

            if (multicastService != null && serviceDiscovery == null)
            {
                serviceDiscovery = new ServiceDiscovery(multicastService);

                multicastService.NetworkInterfaceDiscovered += (s, e) =>
                {
                    foreach (var nic in e.NetworkInterfaces)
                    {
                        logger.Information("Network: {name}", nic.Name);
                    }
                    
                    serviceDiscovery.QueryServiceInstances(serviceName);
                };

                multicastService.QueryReceived += (s, e) =>
                {
                    if (synced && e.Message.IsQuery)
                    {
                        if (e.Message.Questions.Any(q => q.Type == DnsType.PTR && q.Name.ToString().Contains(service) && !q.Name.ToString().Contains(instance)))
                        {
                            serviceDiscovery.Announce(serviceProfile);
                        }
                    }
                };

                serviceDiscovery.ServiceInstanceDiscovered += (s, e) =>
                {
                    string name = e.ServiceInstanceName.ToString();
                    if (!name.Contains(instance) && name.Contains(service))
                    {
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
                                if (address.Address.ToString().StartsWith("10.")
                                    //|| address.Address.ToString().StartsWith("172.")
                                    || address.Address.ToString().StartsWith("192."))
                                {
                                    logger.Information("Establishing connections with host {host} at {address}", address.Name, address.Address);
                                    Instances.Add<SyncEndpoint>(address);
                                    Instances.Add<LockEndpoint>(address);
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
            if (!synced)
            {
                synced = true;
                serviceDiscovery.Advertise(serviceProfile);
                serviceDiscovery.Announce(serviceProfile);
            }
        }

        public static void Close()
        {
            serviceDiscovery.Unadvertise(serviceProfile);
            multicastService.Start();
        }

        public static EndpointInstances Connections()
        {
            Instances ??= new();

            return Instances;
        }
    }
}
