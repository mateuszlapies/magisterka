using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Services
{
    public class DiscoveryService
    {
        public List<HubConnection> Connections { get; set; }

        public DiscoveryService()
        {
            Connections = new List<HubConnection>();
        }
    }
}
