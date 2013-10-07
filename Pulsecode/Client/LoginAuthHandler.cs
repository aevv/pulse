using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
namespace Pulse.Client
{
    class LoginAuthHandler : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            byte code = br.ReadByte();
            string avi = "";
            string phash = "";
            if (code == (byte)constants.LOGIN_SUCCESS)
            {
                avi = br.ReadString();
                phash = br.ReadString();
                Config.LocalScores = false;
            }
            string user = br.ReadString();
            List<object> l = new List<object>();
            l.Add(code);
            l.Add(avi);
            l.Add(user);
            l.Add(phash);
            return new RecievePacket(l);            
        }
    }
    enum constants : byte
    {
        LOGIN_SUCCESS = 0,
        LOGIN_FAILED = 1,
    }
}
