using System.Security.Cryptography;
using Blockchain.Contexts;
using Model;
using TestUtils;
using TestUtils.Classes;

namespace Safety.Test
{
    public class BlockingTests
    {
        private readonly RSAHelper userOne;
        private readonly RSAHelper userTwo;
        private readonly RSAHelper userThree;
        private LockContext lockContext;
        private CreateContext createContext;
        private PublicContext publicContext;

        public BlockingTests()
        {
            userOne = new RSAHelper();
            userTwo = new RSAHelper();
            userThree = new RSAHelper();
        }

        [SetUp]
        public void Setup()
        {
            lockContext = new LockContext();
            createContext = new CreateContext();
            publicContext = new PublicContext();
            lockContext.Clear();
        }

        [Test]
        public void UnathorizedUnblockTest()
        {
            var userObjectOne = new User()
            {
                Name = "Test User 1"
            };
            var userObjectTwo = new User()
            {
                Name = "Test User 2"
            };
            var userObjectThree = new User()
            {
                Name = "Test User 3"
            };
            var post1 = new Post()
            {
                Message = "Test Message 1"
            };
            var post2 = new Post()
            {
                Message = "Test Message 2"
            };
            var userOneLink = createContext.Add(userObjectOne, userOne.GetParameters(true));
            lockContext.Confirm(userOneLink.Id, userOne.GetOwner());
            var userTwoLink = createContext.Add(userObjectTwo, userTwo.GetParameters(true));
            lockContext.Remove(userTwoLink.Id);
            lockContext.Lock(userOneLink, userTwoLink, userTwo.GetPublicKey());
            lockContext.Confirm(userTwoLink.Id, userTwo.GetOwner());
            var userThreeLink = createContext.Add(userObjectThree, userThree.GetParameters(true));
            lockContext.Remove(userThreeLink.Id);
            lockContext.Lock(userTwoLink, userThreeLink, userThree.GetPublicKey());
            lockContext.Confirm(userThreeLink.Id, userThree.GetOwner());
            var postLink1 = createContext.Add(post1, userOne.GetParameters(true));
            lockContext.Remove(postLink1.Id);
            lockContext.Lock(userThreeLink, postLink1, userOne.GetPublicKey());
            lockContext.Confirm(postLink1.Id, userOne.GetOwner());
            Assert.That(publicContext.Get<Post>(postLink1.Id), Is.Not.Null);
            var postLink2 = createContext.Add(post2, userTwo.GetParameters(true));
            lockContext.Remove(postLink2.Id);
            lockContext.Lock(postLink1, postLink2, userTwo.GetPublicKey());
            lockContext.Unlock(postLink2.Id, userThree.GetOwner());
            var outcome = lockContext.Get(postLink1.Id);
            Assert.That(outcome.Lock, Is.Not.Null);
            lockContext.Unlock(postLink2.Id, userTwo.GetOwner());
            outcome = lockContext.Get(postLink1.Id);
            Assert.That(outcome.Lock, Is.Null);
        }
    }
}
