using Makaretu.Dns;
using Serilog;

namespace Networking.Endpoints.Instances
{
    public class EndpointInstances
    {
        private readonly ILogger logger;
        private List<EndpointInstance> Instances { get; set; }

        public EndpointInstances()
        {
            logger = Log.ForContext<EndpointInstances>();
            Instances = new List<EndpointInstance>();
        }

        public void Add<T>(AddressRecord address) where T : Endpoint
        {
            string host = address.Name.ToString();
            if (!Instances.Any(q => q.Host == host && q.Type == typeof(T)))
            {
                Instances.Add(new EndpointInstance()
                {
                    Host = host,
                    Type = typeof(T),
                    Address = address.Address.ToString()
                });
            }
        }

        public List<T> Get<T>() where T : Endpoint, new()
        {
            return Instances.Where(q => q.Type == typeof(T)).Select(s => new T() { InstanceAddress = s.Address }).ToList();
        }

        public int Count()
        {
            return Instances.Count();
        }

        public int Count<T>()
        {
            return Instances.Count(q => q.Type == typeof(T));
        }

        public void Clear()
        {
            Instances.Clear();
        }

        public void Clear<T>()
        {
            Instances.RemoveAll(r => r.Type == typeof(T));
        }
    }
}
