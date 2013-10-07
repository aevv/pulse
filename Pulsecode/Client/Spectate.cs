using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Client
{
    public class SpectateHit : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            return new RecievePacket(l);
        }
    }
    public class SpectatePress : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            return new RecievePacket(l);
        }
    }
    public class SpectateRelease : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            return new RecievePacket(l);
        }
    }
    public class SpectateStart : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadString());
            l.Add(br.ReadString());
            l.Add(br.ReadInt32());
            l.Add(br.ReadDouble());
            return new RecievePacket(l);
        }
    }
    public class SpectateHeartbeat : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadDouble());
            return new RecievePacket(l);
        }
    }
    public class SpectateStats : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            l.Add(br.ReadInt32());
            return new RecievePacket(l);
        }
    }
    public class SpectateUsers : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadString());
            return new RecievePacket(l);
        }
    }
    public class SpectateUsersMe : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            l.Add(br.ReadString());
            return new RecievePacket(l);
        }
    }
    public class SpectateCancel : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            return null;
        }
    }
    public class SpectateRecord : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            return null;
        }
    }
    public class SpectateFinish : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            List<object> l = new List<object>();
            //score maxcombo perfect great ok miss modflags
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
    public class SpectateEnd : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            return null;
        }
    }
    public class SpectateFail : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            return null;
        }
    }
}
