using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Model
{
    public class Lock
    {
        public Guid NextId { get; set; }
        public string Owner { get; set; }
    }
}
