using Makaretu.Dns;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Data
{
    public class HubInstances : Dictionary<DomainName, Dictionary<Type, HubConnection>>
    {
    }
}
