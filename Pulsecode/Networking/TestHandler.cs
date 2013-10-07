using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pulse.Networking
{
    class TestHandler : Handler
    {
        public void handleData(BinaryReader bw)
        {
            Console.WriteLine(bw.ReadString());
        }
    }
}
