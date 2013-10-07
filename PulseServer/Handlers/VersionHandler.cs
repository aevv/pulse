using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using PulseServer.Headers;

namespace PulseServer.Handlers
{
    class VersionHandler : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            double ver = br.ReadDouble();
            Console.WriteLine("Client with ver " + ver + " requesting version check");
            
            bw.Write((short)SendHeaders.VERSION_CHECK);
            bw.Write(Server.getVersion()); //may need to change in the future when there is alot of traffic (so not reading every time)
            return null;
        }
    }
}
