using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
