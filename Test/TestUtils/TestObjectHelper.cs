using Blockchain.Contexts;
using System.Security.Cryptography;
using TestUtils.Classes;

namespace TestUtils
{
    public class TestObjectHelper
    {
        private static readonly TestObject testObject = new()
        {
            Integer = int.MaxValue,
            Long = long.MaxValue,
            Double = double.MaxValue,
            Float = float.MaxValue,
            String = "Test",
            Timestamp = DateTime.UtcNow
        };

        public static TestObject TestObject { get { return testObject; } }

        public static Guid Add(Context context, RSAParameters parameters, int amount = 1)
        {
            Guid id = Guid.Empty;
            for (int i = 0; i < amount; i++)
            {
                id = context.Add<TestObject>(testObject, parameters);
            }
            return id;
        }

        public static TestObject Get(Context context, Guid id)
        {
            return (TestObject)context.Get(id).Object;
        }
    }
}