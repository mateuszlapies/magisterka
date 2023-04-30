using Blockchain.Contexts;
using System.Security.Cryptography;
using TestUtils;
using TestUtils.Classes;

namespace Blockchain.Test
{
    public class LockContextTests
    {
        private readonly RSAHelper rsa;
        private readonly LockContext lockContext;
        private readonly CreateContext createContext;
        private readonly PublicContext publicContext;

        public LockContextTests()
        {
            rsa = new RSAHelper();
            lockContext = new LockContext();
            createContext = new CreateContext();
            publicContext = new PublicContext();
        }

        [SetUp]
        public void SetUp()
        {
            lockContext.Clear();
        }

        [Test]
        public void LockTest()
        {
            Guid first = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Confirm(first, rsa.GetOwner());
            Guid last = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Lock(lockContext.Get(first), lockContext.Get(last), rsa.GetPublicKey());
            var link = lockContext.Get(first);
            Assert.Multiple(() =>
            {
                Assert.That(link, Is.Not.Null);
                Assert.That(link.Lock, Is.Not.Null);
                Assert.That(link.Lock.Confirmed, Is.False);
                Assert.That(link.Lock.NextId, Is.EqualTo(last));
                Assert.That(link.Lock.Owner, Is.EqualTo(rsa.GetPublicKey()));
            });
        }

        [Test]
        public void UnlockTest()
        {
            Guid first = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Confirm(first, rsa.GetOwner());
            Guid last = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Lock(lockContext.Get(first), lockContext.Get(last), rsa.GetPublicKey());
            lockContext.Unlock(last, rsa.GetOwner());
            var link = lockContext.Get(last);
            Assert.That(link, Is.Null);
            link = lockContext.Get(first);
            Assert.That(link, Is.Not.Null);
            Assert.That(link.Lock, Is.Null);
        }

        [Test]
        public void ConfirmTest()
        {
            Guid first = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Confirm(first, rsa.GetOwner());
            Guid last = TestObjectHelper.Add(createContext, rsa.GetParameters(true));
            lockContext.Lock(lockContext.Get(first), lockContext.Get(last), rsa.GetPublicKey());
            lockContext.Confirm(last, rsa.GetOwner());
            var link = lockContext.Get(first);
            Assert.Multiple(() =>
            {
                Assert.That(link, Is.Not.Null);
                Assert.That(link.Lock, Is.Not.Null);
                Assert.That(link.Lock.Confirmed, Is.True);
                Assert.That(link.Lock.NextId, Is.EqualTo(last));
                Assert.That(link.Lock.Owner, Is.EqualTo(rsa.GetPublicKey()));
            });
            var obj = publicContext.Get<TestObject>(last);
            Assert.That(obj, Is.Not.Null);
        }
    }
}
