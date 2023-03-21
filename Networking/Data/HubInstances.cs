using System.Net;
using Makaretu.Dns;
using Microsoft.AspNetCore.SignalR.Client;
using Networking.Hubs;

namespace Networking.Data
{
    public class HubInstances
    {
        private List<HubInstance> Instances { get; set; }

        public HubInstances()
        {
            Instances = new List<HubInstance>();
        }

        public void Add<T>(AddressRecord address) where T : class
        {
            string host = address.Name.ToString();
            if (!Instances.Any(q => q.Host == host && q.Type == typeof(T)))
            {
                HubConnection connection = new HubConnectionBuilder()
                .WithUrl(string.Format("https://{0}:44487/{1}", address.Address, GetEndpoint<T>()), options =>
                {
                    options.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        return message;
                    };
                })
                .WithAutomaticReconnect()
                .Build();
                connection.StartAsync().GetAwaiter().GetResult();
                connection.Closed += Connection_Closed;
                Instances.Add(new HubInstance()
                {
                    Host = host,
                    Type = typeof(T),
                    Connection = connection
                });
            }
        }

        private Task Connection_Closed(Exception? arg)
        {
            return Task.Run(() =>
            {
                Instances.RemoveAll(q => q.Connection.State == HubConnectionState.Disconnected);
            });
        }

        public List<HubConnection> Get<T>()
        {
            return Instances.Where(q => q.Type == typeof(T)).Select(s => s.Connection).ToList();
        }

        public int Count<T>()
        {
            return Instances.Count(q => q.Type == typeof(T));
        }

        public static string GetEndpoint<T>() where T : class
        {
            return (string)typeof(T).GetProperty("Endpoint").GetValue(null, null);
        }
    }

    internal class HubInstance
    {
        public string Host { get; set; }
        public Type Type { get; set; }
        public HubConnection Connection { get; set; }
    }
}
