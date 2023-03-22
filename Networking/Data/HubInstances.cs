using System.Net;
using Makaretu.Dns;
using Microsoft.AspNetCore.SignalR.Client;
using Networking.Hubs;
using Serilog;

namespace Networking.Data
{
    public class HubInstances
    {
        private readonly ILogger logger;
        private List<HubInstance> Instances { get; set; }

        public HubInstances()
        {
            logger = Log.ForContext<HubInstances>();
            Instances = new List<HubInstance>();
        }

        public void Add<T>(AddressRecord address) where T : class
        {
            string host = address.Name.ToString();
            if (!Instances.Any(q => q.Host == host && q.Type == typeof(T)))
            {
                try
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
                    logger.Information("Connection has been established with {host} for {type}", host, typeof(T));
                } catch (Exception ex)
                {
                    logger.Error("Failed to establish connection with {host} for {type} {ex}", host, typeof(T), ex);
                }
            } else
            {
                logger.Information("Connection with {host} for {type} already exists", host, typeof(T));
            }
        }

        private Task Connection_Closed(Exception? arg)
        {
            return Task.Run(() =>
            {
                int count = Instances.RemoveAll(q => q.Connection.State == HubConnectionState.Disconnected);
                logger.Information("Removed {count} disconnected connection(s)", count);
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
