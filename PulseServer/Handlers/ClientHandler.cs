using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.IO;
using PulseServer.Game;
using PulseServer.Headers;
namespace PulseServer.Handlers
{
    public class ClientHandler
    {
        string userName;
        /// <summary>
        /// lower case
        /// </summary>
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }
        BinaryReader br;
        string spectateList = "";
        ClientHandler speccing = null;
        public BinaryReader Br
        {
            get { return br; }
            set { br = value; }
        }
        public Thread myThread;
        BinaryWriter bw;

        public BinaryWriter Bw
        {
            get { return bw; }
            set { bw = value; }
        }
        public ClientHandler(TcpClient c)
        {
            client = c;
            br = new BinaryReader(c.GetStream());
            bw = new BinaryWriter(c.GetStream());
        }
        Dictionary<string, ClientHandler> specs = new Dictionary<string, ClientHandler>();
        Dictionary<string, bool> specChart = new Dictionary<string, bool>();
        TcpClient client;
        public void Run()
        {
            client.ReceiveTimeout = 65000;
            while (true)
            {
                try
                {
                    short header = br.ReadInt16();
                    if (Server.handlers.ContainsKey(header))
                    {
                        Console.WriteLine((RecieveHeaders)header + " from " + (client.Client.RemoteEndPoint as System.Net.IPEndPoint).Address + ":" + (client.Client.RemoteEndPoint as System.Net.IPEndPoint).Port + " user:" + userName);
                        handlePacket(header, Server.handlers[header].handleData(br, bw, client, this));
                    }
                    else
                    {
                        Console.WriteLine("unknown header " + header);
                    }
                }
                catch (Exception e)
                {
                    //  Console.WriteLine(e);
                    if (userName != null)
                    {
                        Console.WriteLine(userName + " disconnected: " + e.Message);
                        if (specs.Count > 0)
                        {
                            foreach (KeyValuePair<string, ClientHandler> h in specs)
                            {
                                PacketWriter.sendSpectateCancel(h.Value);
                                h.Value.speccing = null;
                            }
                        }
                        if (speccing != null)
                        {
                            speccing.unspectate(this);
                        }
                        Server.userList.Remove(userName);
                        Console.WriteLine("removed " + userName);
                    }
                    break;
                }
            }
            if (userName != null)
                Server.userList.Remove(userName);
            myThread.Abort();
        }
        public void abort(Exception e)
        {
            Console.WriteLine(userName + " caused exception: " + e.Message);
            if (speccing != null)
            {
                speccing.unspectate(this);
            }
            if (specs.Count > 0)
            {
                foreach (KeyValuePair<string, ClientHandler> a in specs)
                {
                    PacketWriter.sendSpectateCancel(a.Value);
                }
            }
            Server.userList.Remove(userName);
            if (bw != null)
            {
                bw.Dispose();
            }
            if (br != null)
            {
                br.Dispose();
            }
            if (client != null)
            {
                client.Close();
            }
            myThread.Abort();
            Console.WriteLine("{0} aborted by thread {1}", myThread.Name, Thread.CurrentThread.Name);
        }
        public void abort()
        {
            abort(new Exception("default exception message"));
        }
        public void handlePacket(short header, RecievePacket packet)
        {
            if (header == (short)RecieveHeaders.SONG_START)
            {
                User u = Server.userList[userName];
                if (u.Name.Equals((string)packet.info[0]))
                {
                    u.CurrentChart = (string)packet.info[1];
                    u.CurrentSong = (string)packet.info[2];
                    u.Mode = (User.PlayMode)packet.info[3];
                    u.ModFlags = (int)packet.info[4];
                    u.Scroll = (double)packet.info[5];
                    u.Speed = (double)packet.info[6];
                }
                if (specs.Count > 0 && !u.CurrentChart.Equals("") && (u.Mode == User.PlayMode.INGAME || u.Mode == User.PlayMode.MULTI))
                {
                    foreach (KeyValuePair<string, ClientHandler> h in specs)
                    {
                        PacketWriter.sendSpectateStart(h.Value, userName, Server.userList[userName].CurrentChart, Server.userList[userName].ModFlags, Server.userList[userName].Scroll);
                        specChart[h.Key] = false;
                    }
                }
            }
            else if (header == (short)RecieveHeaders.LOGIN && packet != null)
            {
                User temp = (User)packet.info[0];
                userName = temp.Name.ToLower(); //tolower unnecessary, but more explicit that userName is lowercase
                temp.Handler = this;
                myThread.Name = userName;
            }
            else if (header == (short)RecieveHeaders.SPECTATE_HOOK)
            {
                if (Server.userList.ContainsKey((string)packet.info[0]))
                {
                    try
                    {
                        if (speccing != null)
                        {
                            speccing.unspectate(this);
                        }
                    }
                    catch
                    {

                    }
                    Server.userList[(string)packet.info[0]].Handler.spectate(this);
                    if (!Server.userList[(string)packet.info[0]].CurrentChart.Equals(""))
                    {
                        PacketWriter.sendSpectateStart(this, (string)packet.info[0], Server.userList[(string)packet.info[0]].CurrentChart,
                            Server.userList[(string)packet.info[0]].ModFlags, Server.userList[(string)packet.info[0]].Scroll);
                    }
                    speccing = Server.userList[(string)packet.info[0]].Handler;
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_HIT)
            {
                foreach (KeyValuePair<string, ClientHandler> h in specs)
                {
                    PacketWriter.sendSpectateHit(h.Value, (int)packet.info[0], (int)packet.info[1], (int)packet.info[2], (int)packet.info[3]);
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_RELEASE)
            {
                foreach (KeyValuePair<string, ClientHandler> h in specs)
                {
                    PacketWriter.sendSpectateRelease(h.Value, (int)packet.info[0], (int)packet.info[1]);
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_CANCEL)
            {
                if (Server.userList.ContainsKey((string)packet.info[0]))
                {
                    Server.userList[(string)packet.info[0]].Handler.unspectate(this);
                    speccing = null;
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_PRESS)
            {
                foreach (KeyValuePair<string, ClientHandler> h in specs)
                {
                    PacketWriter.sendSpectatePress(h.Value, (int)packet.info[0], (int)packet.info[1]);
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_HEARTBEAT)
            {
                foreach (KeyValuePair<string, ClientHandler> h in specs)
                {
                    PacketWriter.sendSpectateHeartbeat(h.Value, (int)packet.info[0], (int)packet.info[1], (int)packet.info[2], (int)packet.info[3], (double)packet.info[4]);
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_FINISH)
            {
                foreach (KeyValuePair<string, ClientHandler> h in specs)
                {
                    PacketWriter.sendSpectateFinish(h.Value, (int)packet.info[0], (int)packet.info[1], (int)packet.info[2], (int)packet.info[3],
                        (int)packet.info[4], (int)packet.info[5], (int)packet.info[6], (double)packet.info[7]);
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_FAIL)
            {
                foreach (KeyValuePair<string, ClientHandler> h in specs)
                {
                    PacketWriter.sendSpectateFail(h.Value);
                }
            }
            else if (header == (short)RecieveHeaders.SPECTATE_GOT_CHART)
            {
                if (speccing != null && speccing.specs.Keys.Contains(userName))
                {
                    speccing.specChart[userName] = true;
                }
            }
        }
        public void spectate(ClientHandler h)
        {
            if (!specs.ContainsKey(h.userName))
            {
                specs.Add(h.userName, h);
                specChart.Add(h.userName, false);
                if (specs.Count == 1)
                {
                    PacketWriter.sendSpectateRecord(this);
                }
                sendSpecs();
            }
        }
        public void unspectate(ClientHandler h)
        {
            if (specs.ContainsKey(h.userName))
            {
                specs.Remove(h.userName);
                if (specs.Count == 0)
                {
                    PacketWriter.endSpectateRecord(this);
                }
                else
                {
                    sendSpecs();
                }
            }
        }
        public void sendSpecs()
        {
            spectateList = "Spectating: ";
            foreach (KeyValuePair<string, ClientHandler> a in specs)
            {
                spectateList += a.Value.userName + "(" + (specChart[a.Key] ? "Y" : "N") + "),";
            }
            foreach (KeyValuePair<string, ClientHandler> a in specs)
            {
                PacketWriter.sendSpectateUsers(a.Value, spectateList);
            }
            PacketWriter.sendSpectateUsersMe(this, spectateList);
        }
    }
}
