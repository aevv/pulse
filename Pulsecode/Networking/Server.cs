using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
namespace Pulse.Networking
{
    class Server
    {   /* sample, will send two ints to server and server will print out sum
         * static void Main(string[] args)
        {
            TcpClient tcp = new TcpClient("localhost", 7777);
            BinaryWriter br = new BinaryWriter(tcp.GetStream());
            br.Write((short) 1339); //header
            br.Write(100); //a
            br.Write(1030); //b
            Console.ReadKey();
        }
         * 
         *   string example
         *    static void Main(string[] args)
        {
            TcpClient tcp = new TcpClient("localhost", 7777);

             BinaryWriter br = new BinaryWriter(tcp.GetStream());
            br.Write((short) 5); //header
            br.Write("hello world"); //string
            Console.ReadKey();
        }
         */
        public const int port = 7777;
        public static void listen()
        {
            Console.WriteLine("Starting server on port " + port);
            TcpListener listener = new TcpListener(port);
            registerHandlers();
            Console.WriteLine("registered {0} handlers", handlers.Count);
            listener.Start();
            while (true) //multicliented through use of multiple threads, but should still be somewhat efficient as read() is a blocking method so most threads will be blocked
            {
                TcpClient c = listener.AcceptTcpClient();
                Console.WriteLine("Client connected at {0}", (c.Client.RemoteEndPoint as IPEndPoint).Address.ToString());
                ClientHandler ch = new ClientHandler(c); 
                Thread t = new Thread(new ThreadStart(ch.Run));
                ch.myThread = t;
                t.Start();
            }
        }
        public static Dictionary<short, Handler> handlers = new Dictionary<short, Handler>();
        public static void registerHandler(short header,Handler h)
        {
            if (!handlers.ContainsValue(h))
            {
                handlers.Add(header, h);
            }
        }
        public static void registerHandlers()
        {
            registerHandler((short)Headers.TEST, new TestHandler());
            registerHandler((short)Headers.ADD, new additionHandler());
            registerHandler((short)Headers.AUTH, new AuthenticationHandler()); //worthless
            registerHandler((short)Headers.CHAT, new ChatHandler()); //worthless
        }
    }
    enum Headers : short
    {
        TEST = 5,
        ADD = 1339,
        AUTH = 6,
        CHAT = 7
    }
}
