using System.Net.Http.Json;

namespace Networking.Endpoints.Instances
{
    public class EndpointInstance
    {
        public string Host { get; set; }
        public Type Type { get; set; }
        public string Address { get; set; }

        public async Task<bool> Test()
        {
            using (var handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            })
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(string.Format("https://{0}:44487/", Address));
                    try
                    {
                        await client.GetAsync("/");
                        return true;
                    } catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }
    }
}
