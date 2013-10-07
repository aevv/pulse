using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
namespace Pulse.Client
{
    public interface Handler
    {
        RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client);
    }
}
