using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Pulse.Networking
{
    public interface Handler
    {
        void handleData(BinaryReader bw);
    }
}
