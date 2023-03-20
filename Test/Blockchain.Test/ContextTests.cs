using System.Security.Cryptography;
using Blockchain.Contexts;
using TestUtils;
using TestUtils.Classes;

namespace Blockchain.Test
{
    public class ContextTests
    {
        private readonly Context context;
        private readonly RSAParameters parameters;

        public ContextTests()
        {
            context = new Context();
            parameters = RSAHelper.GetPrivate();
        }

        [SetUp]
        public void Setup()
        {
            context.Clear();
        }

        [Test]
        public void AddSingleLinkTest()
        {
            TestObjectHelper.Add(context, parameters);
            Assert.Pass();
        }

        [Test]
        public void AddMultipleLinkTest()
        {
            TestObjectHelper.Add(context, parameters, 1000);
            Assert.Pass();
        }

        [Test]
        public void GetTest()
        {
            Guid id = TestObjectHelper.Add(context, parameters);
            TestObject obj = TestObjectHelper.Get(context, id);
            Assert.Multiple(() =>
            {
                Assert.That(obj, Is.Not.Null);
                Assert.That(obj.Integer, Is.EqualTo(TestObjectHelper.TestObject.Integer));
                Assert.That(obj.Long, Is.EqualTo(TestObjectHelper.TestObject.Long));
                Assert.That(obj.Double, Is.EqualTo(TestObjectHelper.TestObject.Double));
                Assert.That(obj.Float, Is.EqualTo(TestObjectHelper.TestObject.Float));
                Assert.That(obj.String, Is.EqualTo(TestObjectHelper.TestObject.String));
                Assert.That(obj.Timestamp.Ticks, Is.EqualTo(TestObjectHelper.TestObject.Timestamp.Ticks));
            });
        }

        [Test]
        public void VerifySingleLinkTest()
        {
            Guid id = TestObjectHelper.Add(context, parameters);
            Assert.That(context.Verify(id), Is.True);
        }

        [Test]
        public void VerifyMultipleLinksTest()
        {
            TestObjectHelper.Add(context, parameters, 100);
            Guid id = TestObjectHelper.Add(context, parameters);
            Assert.That(context.Verify(id), Is.True);
        }
    }
}