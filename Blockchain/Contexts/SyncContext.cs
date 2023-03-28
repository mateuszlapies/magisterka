using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blockchain.Model;

namespace Blockchain.Contexts
{
    public class SyncContext : TempContext
    {
        public bool Sync(List<List<Link>> linkSets)
        {
            foreach (var links in linkSets)
            {
                Temp.InsertBulk(links);
                CalculateLastLink(true);
                Verify();
            }
            return true;
        }
    }
}
