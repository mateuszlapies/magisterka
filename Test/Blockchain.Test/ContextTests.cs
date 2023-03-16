using System.Security.Cryptography;
using LiteDB;

namespace Blockchain.Test
{
    public class ContextTests
    {
        private readonly Context context;
        private readonly RSAParameters parameters;
        private readonly TestObject testObject;

        public ContextTests()
        {
            context = new Context();
            RSA rsa = RSA.Create();
            parameters = rsa.ExportParameters(true);
            testObject = new TestObject()
            {
                Integer = int.MaxValue,
                Long = long.MaxValue,
                Double = double.MaxValue,
                Float = float.MaxValue,
                String = "Test",
                Timestamp = DateTime.UtcNow
            };
        }

        [SetUp]
        public void Setup()
        {
            context.Clear();
        }

        [Test]
        public void AddSingleLinkTest()
        {
            Add();
            Assert.Pass();
        }

        [Test]
        public void AddMultipleLinkTest()
        {
            Add(1000);
            Assert.Pass();
        }

        [Test]
        public void GetTest()
        {
            TestObject obj = Get();
            Assert.That(obj.Integer, Is.EqualTo(testObject.Integer));
            Assert.That(obj.Long, Is.EqualTo(testObject.Long));
            Assert.That(obj.Double, Is.EqualTo(testObject.Double));
            Assert.That(obj.Float, Is.EqualTo(testObject.Float));
            Assert.That(obj.String, Is.EqualTo(testObject.String));
            Assert.That(obj.Timestamp.Ticks, Is.EqualTo(testObject.Timestamp.Ticks));
        }

        [Test]
        public void VerifySingleLinkTest()
        {
            Guid id = Add();
            Assert.IsTrue(context.Verify(id));
        }

        [Test]
        public void VerifyMultipleLinksTest()
        {
            Add(100);
            Guid id = Add();
            Assert.IsTrue(context.Verify(id));
        }

        private Guid Add(int amount = 1)
        {
            Guid id = Guid.Empty;
            for (int i = 0; i < amount; i++)
            {
                id = context.Add<TestObject>(testObject, parameters);
            }
            return id;
        }

        private TestObject Get()
        {
            Guid id = Add();
            return context.Get<TestObject>(id);
        }
    }

    internal class TestObject
    {
        public int Integer { get; set; }
        public long Long { get; set; }
        public double Double { get; set; }
        public float Float { get; set; }
        public string String { get; set; }
        public DateTime Timestamp { get; set; }
    }
}