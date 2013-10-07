using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using PulseServer;
namespace PulseServer.Handlers
{
    public interface Handler
    {
        RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch);
    }
}
