using Blockchain.Contexts;
using Model;
using TestUtils;

namespace Safety.Test
{
    public class DeleteTests
    {
        private readonly RSAHelper rsa;
        private TestCreateContext testCreateContext;
        private TestTempContext testTempContext;

        public DeleteTests()
        {
            rsa = new RSAHelper();
        }

        [SetUp]
        public void Setup()
        {
            testCreateContext = new TestCreateContext();
            testTempContext = new TestTempContext();
            testCreateContext.Clear();
        }

        [Test]
        public void DeleteSingleLinkTest()
        {
            var user = new User()
            {
                Name = "Test User 1"
            };
            var post1 = new Post()
            {
                Message = "Test Message 1"
            };
            var post2 = new Post()
            {
                Message = "Test Message 2"
            };
            var post3 = new Post()
            {
                Message = "Test Message 3"
            };
            var userLink = testCreateContext.Add(user, rsa.GetParameters(true));
            var postLink1 = testCreateContext.Add(post1, rsa.GetParameters(true));
            var postLink2 = testCreateContext.Add(post2, rsa.GetParameters(true));
            var postLink3 = testCreateContext.Add(post3, rsa.GetParameters(true));
            testTempContext.Transfer(userLink.Id);
            testTempContext.Transfer(postLink1.Id);
            testTempContext.Transfer(postLink2.Id);
            testTempContext.Transfer(postLink3.Id);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.True);
            testTempContext.Remove(postLink2.Id);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.False);
        }

        [Test]
        public void DeleteLastLinkTest()
        {
            var user = new User()
            {
                Name = "Test User 1"
            };
            var post1 = new Post()
            {
                Message = "Test Message 1"
            };
            var post2 = new Post()
            {
                Message = "Test Message 2"
            };
            var post3 = new Post()
            {
                Message = "Test Message 3"
            };
            var userLink = testCreateContext.Add(user, rsa.GetParameters(true));
            var postLink1 = testCreateContext.Add(post1, rsa.GetParameters(true));
            var postLink2 = testCreateContext.Add(post2, rsa.GetParameters(true));
            var postLink3 = testCreateContext.Add(post3, rsa.GetParameters(true));
            testTempContext.Transfer(userLink.Id);
            testTempContext.Transfer(postLink1.Id);
            testTempContext.Transfer(postLink2.Id);
            testTempContext.Transfer(postLink3.Id);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.True);
            testTempContext.Remove(postLink3.Id);
            Assert.That(testCreateContext.Verify(postLink2.Id), Is.True);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.False);
        }

        internal class TestCreateContext : CreateContext
        {

        }

        internal class TestTempContext : TempContext
        {
            public new void Transfer(Guid id)
            {
                base.Transfer(id);
            }

            public new void Remove(Guid id)
            {
                base.Remove(id);
            }
        }
    }
}