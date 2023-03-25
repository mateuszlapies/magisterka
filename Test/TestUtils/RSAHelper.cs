using Blockchain.Model;
using System.Security.Cryptography;
using System.Text;

namespace TestUtils
{
    public class RSAHelper
    {
        private static RSA rsa;

        private static void Initialize()
        {
            if (rsa == null)
            {
                rsa = RSA.Create();
            }
        }

        public static RSAParameters GetPublic()
        {
            Initialize();
            return rsa.ExportParameters(false);
        }

        public static RSAParameters GetPrivate()
        {
            Initialize();
            return rsa.ExportParameters(true);
        }

        public static string GetOwner()
        {
            using RSACryptoServiceProvider rsaService = new();
            rsaService.ImportParameters(GetPrivate());
            byte[] unsigned = rsa.ExportRSAPublicKey();
            byte[] signed = rsaService.SignData(unsigned, SHA256.Create());
            return Convert.ToBase64String(signed);
        }
    }
}
