using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Client
{

    class UserHandler : Handler
    {
        public RecievePacket handleData(System.IO.BinaryReader br, System.IO.BinaryWriter bw, System.Net.Sockets.TcpClient client)
        {
            //online bool, name,realname,,avatar,playcount,totalscore,mode,currentsong,currentchart, accuracy, level
            bool online = br.ReadBoolean();
            if (online)
            {
                //Console.WriteLine("name:" + br.ReadString());
                //Console.WriteLine("pc:" + br.ReadInt32());
                string user = br.ReadString();
                string rn = br.ReadString();
                string avatar = br.ReadString();
                int pc = br.ReadInt32();
                int totalscore = br.ReadInt32();
                PlayMode pm = (PlayMode)br.ReadInt32();
                string cs = br.ReadString();
                string cc = br.ReadString();
                float acc = br.ReadSingle();//float  
                int lvl = br.ReadInt32();
                lock (User.users)
                {
                    if (User.users.ContainsKey(user))
                    {
                        User.users[user].Name = user;
                        User.users[user].RealName = rn;
                        User.users[user].Avatar = avatar;
                        User.users[user].Playcount = pc;
                        User.users[user].TotalScore = totalscore;
                        User.users[user].Mode = pm;
                        User.users[user].CurrentSong = cs;
                        User.users[user].CurrentChart = cs;
                        User.users[user].Accuracy = acc;
                        User.users[user].Level = lvl;
                        User.users[user].UpdateGraphics = true;
                    }
                    else
                    {
                        User u = new User()
                        {
                            Name = user,
                            RealName = rn,
                            Avatar = avatar,
                            Playcount = pc,
                            TotalScore = totalscore,
                            Mode = pm,
                            CurrentSong = cs,
                            CurrentChart = cc,
                            Accuracy = acc,
                            Level = lvl
                        };

                        
                        User.users.Add(user, u);
                        u.UpdateGraphics = true;
                    }
                    Console.WriteLine("added/modified {0} score is {1}", user, totalscore);
                }
            }
            else
            {
                string removeuser = br.ReadString();
                lock (User.users)
                {
                    if (User.users.ContainsKey(removeuser))
                    {
                        Console.WriteLine("removed {0}", removeuser);
                        Console.WriteLine(User.users.Remove(removeuser) + " at removing " + removeuser);
                    }
                    else
                    {
                        Console.WriteLine("no key userhandler.cs");
                    }
                }
            }
            return null;
        }
    }
}
