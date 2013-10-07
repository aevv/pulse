using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.IO;
namespace Pulse.Networking
{
    class ClientHandler
    {
        BinaryReader br;
        public Thread myThread;
        public ClientHandler(TcpClient c)
        {
            client = c;
            br = new BinaryReader(c.GetStream());
        }
        TcpClient client;
        public void Run()
        {
            while (true)
            {

                try
                {
                    short header = br.ReadInt16();
                    if (Server.handlers.ContainsKey(header))
                    {
                        Server.handlers[header].handleData(br);
                    }
                    else
                    {
                        Console.WriteLine("unknown header " + header);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Client disconnected");
                    break;                    
                }
            }
            myThread.Abort();
        }
    }
}
