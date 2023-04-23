using Blockchain.Contexts;
using Blockchain.Model;
using Model;
using TestUtils;

namespace Safety.Test
{
    public class UpdateTests
    {
        private TestCreateContext testCreateContext;
        private TestTempContext testTempContext;

        [SetUp]
        public void Setup()
        {
            testCreateContext = new TestCreateContext();
            testTempContext = new TestTempContext();
            testCreateContext.Clear();
        }

        [Test]
        public void UpdateLinkPrimaryDataTest()
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
            var userLink = testCreateContext.Add(user, RSAHelper.GetPrivate());
            var postLink1 = testCreateContext.Add(post1, RSAHelper.GetPrivate());
            var postLink2 = testCreateContext.Add(post2, RSAHelper.GetPrivate());
            var postLink3 = testCreateContext.Add(post3, RSAHelper.GetPrivate());
            testTempContext.Transfer(userLink.Id);
            testTempContext.Transfer(postLink1.Id);
            testTempContext.Transfer(postLink2.Id);
            testTempContext.Transfer(postLink3.Id);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.True);
            var link = testTempContext.Get(postLink1.Id);
            link.Timestamp = DateTime.UtcNow;
            testTempContext.Update(link);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.False);
        }

        [Test]
        public void UpdateLinkSecondaryDataTest()
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
            var userLink = testCreateContext.Add(user, RSAHelper.GetPrivate());
            var postLink1 = testCreateContext.Add(post1, RSAHelper.GetPrivate());
            var postLink2 = testCreateContext.Add(post2, RSAHelper.GetPrivate());
            var postLink3 = testCreateContext.Add(post3, RSAHelper.GetPrivate());
            testTempContext.Transfer(userLink.Id);
            testTempContext.Transfer(postLink1.Id);
            testTempContext.Transfer(postLink2.Id);
            testTempContext.Transfer(postLink3.Id);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.True);
            var link = testTempContext.Get(postLink2.Id);
            link.Lock = new Lock();
            testTempContext.Update(link);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.False);
        }

        [Test]
        public void UpdateLinkObjectTest()
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
            var userLink = testCreateContext.Add(user, RSAHelper.GetPrivate());
            var postLink1 = testCreateContext.Add(post1, RSAHelper.GetPrivate());
            var postLink2 = testCreateContext.Add(post2, RSAHelper.GetPrivate());
            var postLink3 = testCreateContext.Add(post3, RSAHelper.GetPrivate());
            testTempContext.Transfer(userLink.Id);
            testTempContext.Transfer(postLink1.Id);
            testTempContext.Transfer(postLink2.Id);
            testTempContext.Transfer(postLink3.Id);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.True);
            var link = testTempContext.Get(postLink1.Id);
            var obj = link.Object as Post;
            obj.Message = "Test Message";
            link.Object = obj;
            testTempContext.Update(link);
            Assert.That(testCreateContext.Verify(postLink3.Id), Is.False);
        }

        internal class TestCreateContext : CreateContext
        {

        }

        internal class TestTempContext : TempContext
        {
            public new Link Get(Guid id)
            {
                return base.Get(id);
            }

            public new void Update(Link link)
            {
                base.Update(link);
            }

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