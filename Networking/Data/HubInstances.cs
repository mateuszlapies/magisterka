using Microsoft.AspNetCore.SignalR.Client;

namespace Networking.Data
{
    public class HubInstances
    {
        private List<HubInstance> Instances { get; set; }

        public HubInstances()
        {
            Instances = new List<HubInstance>();
        }

        public void Add<T>(HubConnection connection)
        {
            Instances.Add(new HubInstance()
            {
                Type = typeof(T),
                Connection = connection
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
    }

    internal class HubInstance
    {
        public Type Type { get; set; }
        public HubConnection Connection { get; set; }
    }
}
