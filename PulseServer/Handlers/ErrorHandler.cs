using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace PulseServer.Handlers
{
    class ErrorHandler : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            string msg = br.ReadString();
            Console.WriteLine(msg);
            Logger.log(msg);
            return null;
        }
    }
}
