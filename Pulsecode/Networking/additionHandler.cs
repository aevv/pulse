using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pulse.Networking
{
    class additionHandler : Handler
    {
        public void handleData(BinaryReader br)
        {
            int a = br.ReadInt32();
            int b = br.ReadInt32();
            Console.WriteLine("added to " + (a + b));
        }
    }
}
