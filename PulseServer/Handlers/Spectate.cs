using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using PulseServer.Headers;
using PulseServer.Game;

namespace PulseServer.Handlers
{
    class SpectateHit : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            //offset, difference, lane, type of hit
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            return new RecievePacket(l);
        }
    }
    class SpectateHook : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            //name of person to spec
            l.Add(br.ReadString());
            return new RecievePacket(l);
        }
    }
    class SpectateCancel : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            //name of person to cancel, easier this way
            l.Add(br.ReadString());
            return new RecievePacket(l);
        }
    }
    class SpectatePress : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            //offset, lane
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            return new RecievePacket(l);
        }
    }
    class SpectateRelease : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            //offset, lane
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            return new RecievePacket(l);
        }
    }
    class SpectateHeartbeat : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            //offset, score, combo, health
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadDouble());
            return new RecievePacket(l);
        }
    }
    class SpectateFinish : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            List<object> l = new List<object>();
            //score, maxcombo, perfect, great, ok, miss, modflags
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadDouble());
            return new RecievePacket(l);
        }
    }
    class SpectateFail : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            return null;
        }
    }
    class SpectateGot : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            return null;
        }
    }
}
