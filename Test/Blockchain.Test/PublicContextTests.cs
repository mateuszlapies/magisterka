using Blockchain.Contexts;
using TestUtils;
using TestUtils.Classes;

namespace Blockchain.Test
{
    public class PublicContextTests
    {
        private readonly LockContext lockContext;
        private readonly CreateContext createContext;

        public PublicContextTests()
        {
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
            var id = TestObjectHelper.Add(createContext, RSAHelper.GetPrivate());
            lockContext.Confirm(id);

            var publicContext = new PublicContext();
            var obj = publicContext.Get<TestObject>(id);
            Assert.That(obj, Is.Not.Null);
        }

        [Test]
        public void GetMultipleTest()
        {
            var first = TestObjectHelper.Add(createContext, RSAHelper.GetPrivate());
            lockContext.Confirm(first);

            var second = TestObjectHelper.Add(createContext, RSAHelper.GetPrivate());
            lockContext.Lock(lockContext.Get(first), lockContext.Get(second), RSAHelper.GetOwner());
            lockContext.Confirm(second);

            var third = TestObjectHelper.Add(createContext, RSAHelper.GetPrivate());
            lockContext.Lock(lockContext.Get(second), lockContext.Get(third), RSAHelper.GetOwner());
            lockContext.Confirm(third);

            var fourth = TestObjectHelper.Add(createContext, RSAHelper.GetPrivate());

            var publicContext = new PublicContext();
            var list = publicContext.Get<TestObject>();
            Assert.That(list, Is.Not.Null);
            Assert.That(list, Has.Count.EqualTo(3));
            var unconfirmed = publicContext.Get<TestObject>(fourth);
            Assert.That(unconfirmed, Is.Null);
        }
    }
}
