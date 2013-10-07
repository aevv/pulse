using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulseServer
{
    public class RecievePacket
    {
        public List<object> info;
        public RecievePacket(List<object> l)
        {
            info = l;
        }
    }
}
