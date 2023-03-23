using Networking.Data;
using Networking.Hubs;

namespace Networking.Test
{
    public class HubInstancesTests
    {
        [Test]
        public void GetEndpointTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(HubInstances.GetEndpoint<SyncHub>(), Is.EqualTo(SyncHub.Endpoint));
                Assert.That(HubInstances.GetEndpoint<LockHub>(), Is.EqualTo(LockHub.Endpoint));
            });
        }
    }
}
