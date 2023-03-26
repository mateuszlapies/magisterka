using System.Security.Cryptography;

namespace Application.Services
{
    public class RSAService
    {
        private readonly RSA rsa;
        private static readonly string publicFile = "public.key";
        private static readonly string privateFile = "private.key";
        
        public RSAService()
        {
            rsa = RSA.Create();
            string _publicFile = GetFilePath(publicFile);
            string _privateFile = GetFilePath(privateFile);
            if (File.Exists(_publicFile) && File.Exists(_privateFile))
            {
                Import(_publicFile, _privateFile);
            } else
            {
                Export(_publicFile, _privateFile);
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

        private void Import(string pub, string pri)
        {
            rsa.ImportRSAPublicKey(File.ReadAllBytes(pub), out int bytesRead1);
            rsa.ImportRSAPrivateKey(File.ReadAllBytes(pri), out int bytesRead2);
        }

        private void Export(string pub, string pri)
        {
            File.WriteAllBytes(pub, rsa.ExportRSAPublicKey());
            File.WriteAllBytes(pri, rsa.ExportRSAPrivateKey());
        }

        private string GetFilePath(string file)
        {
            #if DEBUG
                if (!Directory.GetCurrentDirectory().Contains("bin"))
                {
                    return Path.Combine("bin/Debug/net7.0", file);
                }
                else
                {
                    return file;
                }
            #else
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), file);
            #endif
        }
    }
}
