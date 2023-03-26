using Makaretu.Dns;
using Networking.Data;
using Networking.Endpoints.Instances;
using Networking.Hubs;
using System.Net;

namespace Networking.Test
{
    public class EndpointInstancesTests
    {
        private readonly EndpointInstances instances;

        public EndpointInstancesTests()
        {
            instances = new EndpointInstances();
        }

        [SetUp]
        public void SetUp()
        {
            instances.Clear();
        }

        [Test]
        public void AddCountTest()
        {
            instances.Add<SyncEndpoint>(GetAddressRecord());
            instances.Add<LockEndpoint>(GetAddressRecord());
            instances.Add<SyncEndpoint>(GetAddressRecord());
            Assert.That(instances.Count<SyncEndpoint>(), Is.EqualTo(1)); 
            Assert.That(instances.Count(), Is.EqualTo(2)); 
        }

        [Test]
        public void GetTest()
        {
            instances.Add<SyncEndpoint>(GetAddressRecord());
            var endpoints = instances.Get<SyncEndpoint>();
            Assert.That(endpoints.Count(), Is.EqualTo(1));
            var endpoint = endpoints.First();
            Assert.That(endpoint, Is.TypeOf<SyncEndpoint>());
            Assert.That(endpoint.InstanceAddress, Is.EqualTo("127.0.0.1"));
            Assert.That(endpoint.EndpointAddress, Is.EqualTo("Sync"));
        }

        private AddressRecord GetAddressRecord(string domain = "localhost", byte[]? ip = null)
        {
            ip = ip ?? new byte[] { 127, 0, 0, 1 };
            return AddressRecord.Create(new DomainName(domain), new IPAddress(ip));
        }
    }
}
