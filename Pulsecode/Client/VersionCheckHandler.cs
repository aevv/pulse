using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
namespace Pulse.Client
{
    class VersionCheckHandler : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            double server = br.ReadDouble();
            List<object> l = new List<object>();
            l.Add(server);
            return new RecievePacket(l);
        }
    }
}
