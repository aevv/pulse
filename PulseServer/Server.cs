using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using PulseServer.Headers;
using PulseServer.Handlers;
using PulseServer.Game;

namespace PulseServer
{
    //currently to add a new networking function you have to
    // -add to both send/rec headers on both server and client
    // -add writer on one and handler on another
    // -register the new handler on whichever
    // -lastly handle the message appropriately (i.e with event invoker)
    // perhaps it should be simpler?
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
        public static double version = 0.251; //make readable via file?
        public const int port = 7777;
        public static object obj = new object();
        public static Dictionary<string, User> userList = new Dictionary<string, User>();
        public event Action<short, RecievePacket> recvPacket;
        public static double getVersion()
        {
            lock (obj)
            {
                if (System.IO.File.Exists("version.txt"))
                {
                    double temp = version;
                    if (Double.TryParse(System.IO.File.ReadAllText("version.txt"), out temp))
                    {
                        return temp;
                    }
                }
                return version;
            }
        }
        public static void listen()
        {
            Console.WriteLine("Server running version " + getVersion() + ". Starting server on port " + port);
            TcpListener listener = new TcpListener(port);
            registerHandlers();
            Console.WriteLine("registered {0} handlers", handlers.Count);
            listener.Start();
            new Thread(new ThreadStart(input)).Start();
            while (run) //multicliented through use of multiple threads, but should still be somewhat efficient as read() is a blocking method so most threads will be blocked
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
        public static void registerHandler(short header, Handler h)
        {
            if (!handlers.ContainsValue(h))
            {
                handlers.Add(header, h);
            }
        }
        public static void registerHandlers()
        {
            registerHandler((short)RecieveHeaders.ERROR, new ErrorHandler());
            registerHandler((short)RecieveHeaders.VER, new VersionHandler());
            registerHandler((short)RecieveHeaders.LOGIN, new LoginHandler());
            registerHandler((short)RecieveHeaders.SONG_START, new SongStart());
            registerHandler((short)RecieveHeaders.SPECTATE_HIT, new SpectateHit());
            registerHandler((short)RecieveHeaders.SPECTATE_HOOK, new SpectateHook());
            registerHandler((short)RecieveHeaders.SPECTATE_CANCEL, new SpectateCancel());
            registerHandler((short)RecieveHeaders.SPECTATE_RELEASE, new SpectateRelease());
            registerHandler((short)RecieveHeaders.SPECTATE_PRESS, new SpectatePress());
            registerHandler((short)RecieveHeaders.SPECTATE_HEARTBEAT, new SpectateHeartbeat());
            registerHandler((short)RecieveHeaders.SPECTATE_FINISH, new SpectateFinish());
            registerHandler((short)RecieveHeaders.USER_REQUEST, new UserRequest());
            registerHandler((short)RecieveHeaders.UPDATE_USER_DATA, new UpdateUserData());
            registerHandler((short)RecieveHeaders.SPECTATE_FAIL, new SpectateFail());
            registerHandler((short)RecieveHeaders.CLIENT_HEARTBEAT, new ClientHeartbeat());
            registerHandler((short)RecieveHeaders.SPECTATE_GOT_CHART, new SpectateGot());
        }

        static bool run = true;
        public static void input()
        {
            while (run)
            {
                string[] line = Console.ReadLine().ToLower().Split(' ');
                switch (line[0])
                {
                    case "exit":
                        run = false;
                        Environment.Exit(0);
                        break;
                    case "users":
                        Console.Write("Online users: ");
                        foreach (KeyValuePair<string, User> u in userList)
                        {
                            Console.Write(u.Value.Name + ", ");
                        }
                        Console.WriteLine("\nEnd of user list");
                        break;
                    case "info":
                        try
                        {
                            if (userList.ContainsKey(line[1]))
                            {
                                Console.WriteLine("User: {0}, mode: {1}, song: {2}", userList[line[1]].Name, userList[line[1]].Mode.ToString(), userList[line[1]].CurrentSong);
                            }
                        }
                        catch { }
                        break;
                    case "kick":

                            if (userList.ContainsKey(line[1]))
                            {
                                userList[line[1]].Handler.abort();
                            } else {
                                Console.WriteLine("userlist has no key");
                            }

                        break;
                }
            }
        }
    }
}
