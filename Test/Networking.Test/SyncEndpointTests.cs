using Blockchain.Contexts;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Hubs;
using System.Security.Cryptography;
using TestUtils;

namespace Networking.Test
{
    public class SyncEndpointTests
    {
        private readonly Context context;
        private readonly RSAParameters parameters;

        private readonly SyncEndpoint syncEndpoint;

        public SyncEndpointTests()
        {
            context = new Context();
            parameters = RSAHelper.GetPrivate();

            syncEndpoint = new SyncEndpoint();
        }

        [SetUp]
        public void Setup()
        {
            context.Clear();
        }

        [Test]
        public async Task SyncTestSuccessAll()
        {
            TestObjectHelper.Add(context, parameters, 1000);
            SyncRequest request = new();
            SyncResponse response = await syncEndpoint.Request(request);
            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);
                Assert.That(response.Links, Has.Count.EqualTo(1000));
            });
        }

        [Test]
        public async Task SyncTestSuccessNotAll()
        {
            Guid lastId = TestObjectHelper.Add(context, parameters, 500);
            TestObjectHelper.Add(context, parameters, 500);
            SyncRequest request = new() { LastId = lastId };
            SyncResponse response = await syncEndpoint.Request(request);
            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);
                Assert.That(response.Links, Has.Count.EqualTo(500));
            });
        }
    }
}
