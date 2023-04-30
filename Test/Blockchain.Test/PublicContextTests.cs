using Blockchain.Contexts;
using TestUtils;
using TestUtils.Classes;

namespace Blockchain.Test
{
    public class PublicContextTests
    {
        private readonly RSAHelper rsa;
        private readonly LockContext lockContext;
        private readonly CreateContext createContext;

        public PublicContextTests()
        {
            rsa = new RSAHelper();
            lockContext = new LockContext();
            createContext = new CreateContext();
        }

        [SetUp]
        public void SetUp()
        {
            lockContext.Clear();
        }


        [Test]
        public void GetSingleTest()
        {
            var id = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Confirm(id, rsa.GetOwner());

            var publicContext = new PublicContext();
            var obj = publicContext.Get<TestObject>(id);
            Assert.That(obj, Is.Not.Null);
        }

        [Test]
        public void GetMultipleTest()
        {
            var first = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Confirm(first, rsa.GetOwner());

            var second = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Lock(lockContext.Get(first), lockContext.Get(second), rsa.GetPublicKey());
            lockContext.Confirm(second, rsa.GetOwner());

            var third = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Lock(lockContext.Get(second), lockContext.Get(third), rsa.GetPublicKey());
            lockContext.Confirm(third, rsa.GetOwner());

            var fourth = TestObjectHelper.Add(createContext, rsa.GetParameters(true));

            var publicContext = new PublicContext();
            var list = publicContext.Get<TestObject>();
            Assert.That(list, Is.Not.Null);
            Assert.That(list, Has.Count.EqualTo(3));
            var unconfirmed = publicContext.Get<TestObject>(fourth);
            Assert.That(unconfirmed, Is.Null);
        }
    }
}
