using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
namespace Pulse.Client
{
    public class Connection
    {
        BinaryReader br;
        BinaryWriter bw;

        public BinaryWriter Bw
        {
            get { return bw; }
            set { bw = value; }
        }
        TcpClient tcp;
        const string host = "p.ulse.net";
        const int port = 7777;
        public Connection()
        {
            try
            {
                // Console.WriteLine((tcp.Client.LocalEndPoint as System.Net.IPEndPoint).Address.ToString());
                tcp = new TcpClient(host, port);

                br = new BinaryReader(tcp.GetStream());
                bw = new BinaryWriter(tcp.GetStream());
                registerHandlers();

                Thread t = new Thread(new ThreadStart(Listen));
                t.IsBackground = true;
                t.Start();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception connecting in Client.Connection " + e);
                //Game.pbox.addLine("It appears you cannot connect to the internet.", System.Drawing.Color.Red);
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
        public event Action<short, RecievePacket> recvPacket;
        public static void registerHandlers()
        {
            registerHandler((short)RecvHeaders.LOGIN_AUTH, new LoginAuthHandler());
            registerHandler((short)RecvHeaders.VERSION_CHECK, new VersionCheckHandler());
            registerHandler((short)RecvHeaders.SPECTATE_RECORD, new SpectateRecord());
            registerHandler((short)RecvHeaders.SPECTATE_HIT, new SpectateHit());
            registerHandler((short)RecvHeaders.SPECTATE_PRESS, new SpectatePress());
            registerHandler((short)RecvHeaders.SPECTATE_RELEASE, new SpectateRelease());
            registerHandler((short)RecvHeaders.SPECTATE_HEARTBEAT, new SpectateHeartbeat());
            registerHandler((short)RecvHeaders.SPECTATE_START, new SpectateStart());
            registerHandler((short)RecvHeaders.SPECTATE_FINISH, new SpectateFinish());
            registerHandler((short)RecvHeaders.SPECTATE_END, new SpectateEnd());
            registerHandler((short)RecvHeaders.SPECTATE_CANCEL, new SpectateCancel());
            registerHandler((short)RecvHeaders.SPECTATE_USERS, new SpectateUsers());
            registerHandler((short)RecvHeaders.USER_REQUEST_INFO, new UserHandler());
            registerHandler((short)RecvHeaders.SPECTATE_FAIL, new SpectateFail());
            registerHandler((short)RecvHeaders.SPECTATE_USERS_ME, new SpectateUsersMe());
        }
        int reconnectAttempts = 0;
        public void Listen()
        {
            while (true)
            {
                try
                {
                    while (true)
                    {
                        short header = br.ReadInt16();
                        Console.WriteLine((RecvHeaders)header);
                        if (handlers.ContainsKey(header))
                        {
                            //concept - handlers don't apply any changes to client themselves, just encapsulate info and return
                            RecievePacket p = handlers[header].handleData(br, bw, tcp);
                            recvPacket.Invoke(header, p);
                        }
                        else
                        {
                            Console.WriteLine("unknown header {0}", header);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Game.addToast("Connection to server has been closed");

                    lock (Game.ircLock)
                    {
                        if (Game.ircl != null)
                        {
                            Game.ircl.terminate();
                            Game.ircl = null;
                        }
                    }

                    Thread.Sleep(5000);
                    Console.WriteLine("attempting to reconnect...");
                    if (bw != null)
                    {
                        bw.Dispose();
                        bw = null;
                    }
                    if (br != null)
                    {
                        br.Dispose();
                        br = null;
                    }
                    if (tcp != null)
                    {
                        tcp.Close();
                        tcp = null;
                    }
                    while (tcp == null)
                    {
                        reconnectAttempts++;
                        if (reconnectAttempts >= 10)
                        {
                            break;
                        }
                        try
                        {
                            tcp = new TcpClient(host, port);
                            br = new BinaryReader(tcp.GetStream());
                            bw = new BinaryWriter(tcp.GetStream());
                        }
                        catch (Exception x)
                        {
                            if (tcp != null)
                            {
                                tcp.Close();
                            }
                            tcp = null;
                            Thread.Sleep(5000); //reconnect failed, sleep for another 5 secs
                            Console.WriteLine(x + "\nreconnect failed again, sleeping for another 5 secs");
                        }

                    }
                }
            }
            if (bw != null)
            {
                bw.Dispose();
                bw = null;
            }
            if (br != null)
            {
                br.Dispose();
                br = null;
            }
            if (tcp != null)
            {
                tcp.Close();
                tcp = null;
            }
            Console.WriteLine("stopped attempting reconnect");
        }
        public void reconnect()
        {
            if (bw != null)
            {
                bw.Dispose();
                bw = null;
            }
            if (br != null)
            {
                br.Dispose();
                br = null;
            }
            if (tcp != null)
            {
                tcp.Close();
                tcp = null;
            }
            while (tcp == null)
            {
                try
                {
                    tcp = new TcpClient(host, port);
                    br = new BinaryReader(tcp.GetStream());
                    bw = new BinaryWriter(tcp.GetStream());
                }
                catch (Exception e)
                {
                    if (tcp != null)
                    {
                        tcp.Close();
                    }
                    tcp = null;
                    Thread.Sleep(5000); //reconnect failed, sleep for another 5 secs
                    Console.WriteLine(e + "\nreconnect failed again, sleeping for another 5 secs");
                }

            }
            PacketWriter.sendLogin(bw, Account.currentAccount.AccountName, Account.currentAccount.passHash);
          /*  lock (Game.ircLock)
            {
                Game.ircl = new Client.irc.IrcClient("pulse|" + Account.currentAccount.AccountName, Account.currentAccount.AccountName);
                Game.ircl.realNick = Account.currentAccount.AccountName;
                Game.pbox.setIrc(Game.ircl);
            }*/
            Listen();
        }
        public static void Main1(String[] arg)
        {
            Connection c = new Connection();
            c.recvPacket += new Action<short, RecievePacket>(c_recvPacket);
            PacketWriter.sendLogin(c.Bw, "nop", "nop");
        }

        static void c_recvPacket(short arg1, RecievePacket arg2)
        {
            Console.WriteLine(arg1 + " " + arg2.info[1]);
        }
    }

    public class RecievePacket
    {
        public List<object> info;
        public RecievePacket(List<object> l)
        {
            info = l;
        }
    }
}
