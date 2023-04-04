using Blockchain.Model;
using Model;
using Networking.Data.Requests;
using Networking.Utils;

namespace Networking.Test
{
    public class LiteSerializerTests
    {
        [Test]
        public void ParserTest()
        {
            var link = new Link()
            {
                Id = Guid.NewGuid(),
                Object = new User()
                {
                    Name = "Test",
                },
                ObjectType = typeof(User).ToString(),
                Timestamp = DateTime.Now
            };

            var json = LiteSerializer.Serialize(link);
            var outputLink = LiteSerializer.Deserialize(json);

            Assert.That(outputLink, Is.Not.Null);
            Assert.That(outputLink.Id, Is.EqualTo(link.Id));
            Assert.That(outputLink.Object, Is.Not.Null);
            Assert.That(outputLink.Object.GetType(), Is.EqualTo(link.Object.GetType()));
            var obj = (User)outputLink.Object;
            Assert.That(obj.Name, Is.EqualTo("Test"));
            Assert.That(outputLink.ObjectType, Is.EqualTo(link.ObjectType));
            Assert.That(outputLink.Timestamp, Is.EqualTo(link.Timestamp));
        }
    }
}
