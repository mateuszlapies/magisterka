using System.Security.Cryptography;

namespace TestUtils
{
    public class RSAHelper
    {
        private RSA rsa;

        public RSAHelper()
        {
            if (rsa == null)
            {
                rsa = RSA.Create();
            }
        }

        public RSAParameters GetParameters(bool includePrivate = false)
        {
            return rsa.ExportParameters(includePrivate);
        }

        public string GetPublicKey()
        {
            return Convert.ToBase64String(rsa.ExportRSAPublicKey());
        }

        public string GetOwner()
        {
            using RSACryptoServiceProvider rsaService = new();
            rsaService.ImportParameters(GetParameters(true));
            byte[] unsigned = rsa.ExportRSAPublicKey();
            byte[] signed = rsaService.SignData(unsigned, SHA256.Create());
            return Convert.ToBase64String(signed);
        }

        public static bool VerifyOwner(string publicKey, string owner)
        {
            using RSACryptoServiceProvider rsaService = new();
            byte[] unsigned = Convert.FromBase64String(publicKey);
            rsaService.ImportRSAPublicKey(unsigned, out int bytesRead);
            return rsaService.VerifyData(unsigned, SHA256.Create(), Convert.FromBase64String(owner));
        }
    }
}
