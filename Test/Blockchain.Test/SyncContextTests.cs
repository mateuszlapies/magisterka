using Blockchain.Contexts;
using Blockchain.Model;
using System.Security.Cryptography;
using TestUtils;
using TestUtils.Classes;

namespace Blockchain.Test
{
    public class SyncContextTests
    {
        private readonly RSAParameters parameters;
        private readonly SyncContext syncContext;
        private readonly CreateContext createContext;

        public SyncContextTests()
        {
            parameters = RSAHelper.GetPrivate();
            syncContext = new SyncContext();
            createContext = new CreateContext();
        }

        [SetUp]
        public void SetUp()
        {
            syncContext.Clear();
        }

        [Test]
        public void SyncSingleLinkTest()
        {
            Guid link = TestObjectHelper.Add(createContext, parameters);
            var (success, list) = syncContext.Get(null);
            Assert.That(success, Is.True);
            Assert.That(list.Count, Is.EqualTo(1));
            syncContext.Clear();
            var responses = new List<List<Link>>
            {
                list
            };
            syncContext.Sync(responses);
            var publicContext = new PublicContext();
            var response = publicContext.Get<TestObject>(link);
            Assert.That(response, Is.Not.Null);
        }

        [Test]
        public void SyncMultipleLinkTest()
        {
            Guid first = TestObjectHelper.Add(createContext, parameters);
            Guid last = TestObjectHelper.Add(createContext, parameters, 4);
            var (success, list) = syncContext.Get(null);
            Assert.That(success, Is.True);
            Assert.That(list.Count, Is.EqualTo(5));
            syncContext.Clear();
            var responses = new List<List<Link>>
            {
                list
            };
            syncContext.Sync(responses);
            var publicContext = new PublicContext();
            var response = publicContext.Get<TestObject>(first);
            Assert.That(response, Is.Not.Null);
            response = publicContext.Get<TestObject>(last);
            Assert.That(response, Is.Null);
            var all = publicContext.Get<TestObject>();
            Assert.That(all.Count, Is.EqualTo(1));
        }
    }
}
