using Blockchain.Contexts;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Hubs;
using System.Security.Cryptography;
using TestUtils;

namespace Networking.Test
{
    public class SyncHubTests
    {
        private readonly Context context;
        private readonly RSAParameters parameters;

        private readonly SyncHub syncHub;

        public SyncHubTests()
        {
            context = new Context();
            parameters = RSAHelper.GetPrivate();

            syncHub = new SyncHub(context);
        }

        [SetUp]
        public void Setup()
        {
            context.Clear();
        }

        [Test]
        public void SyncTestSuccessAll()
        {
            TestObjectHelper.Add(context, parameters, 1000);
            SyncRequest request = new SyncRequest();
            SyncResponse response = syncHub.Sync(request);
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.True);
            Assert.That(response.Links.Count, Is.EqualTo(1000));
        }

        [Test]
        public void SyncTestSuccessNotAll()
        {
            Guid lastId = TestObjectHelper.Add(context, parameters, 500);
            TestObjectHelper.Add(context, parameters, 500);
            SyncRequest request = new SyncRequest() { LastId = lastId };
            SyncResponse response = syncHub.Sync(request);
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.True);
            Assert.That(response.Links.Count, Is.EqualTo(500));
        }
    }
}
