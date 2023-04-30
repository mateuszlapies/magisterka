using Blockchain.Contexts;
using TestUtils;
using TestUtils.Classes;

namespace Blockchain.Test
{
    public class CreateContextTests
    {
        private readonly RSAHelper rsa;
        private readonly CreateContext context;

        public CreateContextTests()
        {
            rsa = new RSAHelper();
            context = new CreateContext();
        }

        [SetUp]
        public void Setup()
        {
            context.Clear();
        }

        [Test]
        public void AddSingleLinkTest()
        {
            TestObjectHelper.Add(context, rsa.GetParameters(true));
            Assert.Pass();
        }

        [Test]
        public void AddMultipleLinkTest()
        {
            TestObjectHelper.Add(context, rsa.GetParameters(true), 1000);
            Assert.Pass();
        }

        [Test]
        public void GetTest()
        {
            Guid id = TestObjectHelper.Add(context, rsa.GetParameters(true));
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
            Guid id = TestObjectHelper.Add(context, rsa.GetParameters(true));
            Console.WriteLine(id);
            Assert.That(context.Verify(id), Is.True);
        }

        [Test]
        public void VerifyMultipleLinksTest()
        {
            TestObjectHelper.Add(context, rsa.GetParameters(true), 100);
            Guid id = TestObjectHelper.Add(context, rsa.GetParameters(true));
            Assert.That(context.Verify(id), Is.True);
        }
    }
}