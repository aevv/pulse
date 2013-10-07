using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Pulse
{
    class Account
    {
        public static Account currentAccount;
        public Account(string user, string avatar, string pass)
        {
            AccountName = user;
            AvatarUrl = avatar;
            passHash = pass;
        }
        public string AccountName;
        public string AvatarUrl;
        public string passHash;
        public static void tryLoadAcc()
        {
            if (File.Exists("acc.config"))
            {
                string user = "", passhash = "";
                try
                {
                    foreach (string s in File.ReadAllLines("acc.config"))
                    {
                        string[] splitted = s.Split('=');
                        if (splitted[0].Equals("user"))
                        {
                            user = splitted[1];
                        }
                        else if (splitted[0].Equals("pass"))
                        {
                            passhash = splitted[1];
                        }
                    }
                    Pulse.Client.PacketWriter.sendLogin(Game.conn.Bw, user, passhash);
                }
                catch (Exception)
                {
                    Console.WriteLine("Login failed (from file).");
                }
            }
        }
        public static void saveAcc(string user, string hashedpass)
        {
            using (StreamWriter sw = new StreamWriter("acc.config"))
            {
                sw.WriteLine("user=" + user);
                sw.WriteLine("pass=" + hashedpass);
                sw.Flush();
            }
        }
    }
}
