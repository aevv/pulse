using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PulseServer.Headers;
using System.Net.Sockets;
using System.IO;
using PulseServer.Game;

namespace PulseServer.Handlers
{
    class UpdateUserData : Handler
    {

        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            if (!string.IsNullOrEmpty(ch.UserName))
            {
                User u = Database.getInstance().getUser(ch.UserName);
                if (u != null)
                {
                    if (Server.userList.ContainsKey(ch.UserName))
                    {
                        Server.userList[ch.UserName].Level = u.Level;
                        Server.userList[ch.UserName].TotalScore = u.TotalScore;
                        Server.userList[ch.UserName].Playcount = u.Playcount;
                        Server.userList[ch.UserName].Accuracy = u.Accuracy;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            return null;
        }
    }
    class UserRequest : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            string[] users = br.ReadString().ToLower().Split(';');
            foreach (string s in users)
            {
                //Console.WriteLine(s);
                if (!string.IsNullOrEmpty(s))
                {
                    if (Server.userList.ContainsKey(s))
                    {
                        //  User u = Database.getInstance().getUser(s); //refresh user info

                        if (Server.userList[s] != null)
                        {
                            PacketWriter.sendUserInfo(ch, Server.userList[s], s);
                        }
                        else
                        {
                            Console.WriteLine("user was null");
                        }
                    }
                    else
                    {
                        Console.WriteLine("server had no key " + s + " in userlist");
                        PacketWriter.sendUserInfo(ch, null, s);
                    }
                }
            }
            return null;
        }
    }
    class SongStart : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            string user = br.ReadString();
            string chartMd5 = br.ReadString();
            string song = br.ReadString();
            int type = br.ReadInt16();
            List<object> l = new List<object>();
            l.Add(user.ToLower());
            l.Add(chartMd5);
            l.Add(song);
            l.Add(type);
            //mod flags, scroll, speed
            l.Add(br.ReadInt32());
            l.Add(br.ReadDouble());
            l.Add(br.ReadDouble());
            return new RecievePacket(l);
        }
    }
    class GetUsers : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadInt32());
            l.Add(br.ReadString());
            return new RecievePacket(l);
        }
    }
    class ClientHeartbeat : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            return null;
        }
    }
}   
