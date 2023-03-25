using Blockchain.Contexts;
using Blockchain.Model;
using Networking.Data.Requests;
using Networking.Data.Responses;
using Networking.Hubs;
using System.Security.Cryptography;
using TestUtils;

namespace Networking.Test
{
    public class LockEndpointTests
    {
        private readonly Context context;
        private readonly RSAParameters parameters;

        private LockEndpoint lockEndpoint;

        public LockEndpointTests()
        {
            context = new Context();
            parameters = RSAHelper.GetPrivate();

            lockEndpoint = new LockEndpoint();
        }

        [SetUp]
        public void Setup()
        {
            context.Clear();
        }

        [Test]
        public async Task LockTestSuccess()
        {
            Guid id = TestObjectHelper.Add(context, parameters);
            LockRequest request = new LockRequest()
            {
                LockId = id,
                NextId = Guid.NewGuid(),
                Owner = RSAHelper.GetOwner()
            };
            LockResponse response = await lockEndpoint.Request(request);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.True);
            Assert.That(response.LockInsteadId, Is.Null);

            Link link = context.Get(id);
            
            Assert.That(link, Is.Not.Null);
            Assert.That(link.Lock, Is.Not.Null);
            Assert.That(link.Lock.NextId, Is.EqualTo(request.NextId));
            Assert.That(link.Lock.Owner, Is.EqualTo(request.Owner));
        }

        [Test]
        public async Task LockTestFailure()
        {
            Guid id = TestObjectHelper.Add(context, parameters);
            LockRequest request = new LockRequest()
            {
                LockId = id,
                NextId = Guid.NewGuid(),
                Owner = RSAHelper.GetOwner()
            };
            LockResponse response = await lockEndpoint.Request(request);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.True);
            Assert.That(response.LockInsteadId, Is.Null);

            Link link = context.Get(id);

            Assert.That(link, Is.Not.Null);
            Assert.That(link.Lock, Is.Not.Null);
            Assert.That(link.Lock.NextId, Is.EqualTo(request.NextId));
            Assert.That(link.Lock.Owner, Is.EqualTo(request.Owner));

            Guid nextId = request.NextId;

            request.NextId = Guid.NewGuid();

            response = await lockEndpoint.Request(request);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.False);
            Assert.That(response.LockInsteadId, Is.EqualTo(nextId));

            link = context.Get(id);

            Assert.That(link, Is.Not.Null);
            Assert.That(link.Lock, Is.Not.Null);
            Assert.That(link.Lock.NextId, Is.EqualTo(nextId));
            Assert.That(link.Lock.NextId, Is.Not.EqualTo(request.NextId));
            Assert.That(link.Lock.Owner, Is.EqualTo(request.Owner));
        }
    }
}