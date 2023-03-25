using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Networking.Endpoints
{
    public class Endpoint
    {
        public string InstanceAddress { get; set; }
        public virtual string EndpointAddress { get { return string.Empty; } }

        protected async Task<R> Request<T, R>(T request)
        {
            using StringContent jsonContent = new(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
            using (var handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            })
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(string.Format("https://{0}:44487/", InstanceAddress));
                    var response = await client.PostAsync(string.Format("{0}/{1}", "api", EndpointAddress), jsonContent);
                    return await response.Content.ReadFromJsonAsync<R>();
                }
            }   
        }
    }
}
