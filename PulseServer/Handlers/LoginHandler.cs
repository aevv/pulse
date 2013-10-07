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
    class LoginHandler : Handler
    {
        public RecievePacket handleData(BinaryReader br, BinaryWriter bw, TcpClient client, ClientHandler ch)
        {
            string user = br.ReadString();
            string pass = br.ReadString(); //hashed
            bw.Write((short)SendHeaders.LOGIN_AUTH); //login_auth

            Database db = Database.getInstance();
            Session s = db.login(user, pass);

            if (s != null)
            {
                if (Server.userList.ContainsKey(user))
                {
                    Server.userList[user].Handler.abort();
                }
                bw.Write((byte)constants.LOGIN_SUCCESS);
                string avi = s.getAvatarUrl();
                bw.Write(avi);
                bw.Write(pass);
                User u = db.getUser(user);
                if (!string.IsNullOrEmpty(ch.UserName))
                {
                    Server.userList.Remove(ch.UserName);
                }
                Server.userList.Add(user.ToLower(), u);
                List<object> l = new List<object>();
                l.Add(u);
                bw.Write(user);
                return new RecievePacket(l);
            }
            else
            {
                bw.Write((byte)constants.LOGIN_FAILED);
            }
            bw.Write(user);
            return null;
        }

        enum constants : byte
        {

            LOGIN_SUCCESS = 0,
            LOGIN_FAILED = 1,
        }
    }
}
