using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace PulseServer
{
    class Session
    {
        int id;
        string user;
        string avurl;
        public Session(string user, int id, string avurl)
        {
            this.user = user;
            this.id = id;
            this.avurl = avurl;
        }
        public string getAvatarUrl()
        {
            return avurl;
          /* // return "http://rep.ulse.net/uploads/profile/photo-" + id + ".png";
            MySqlConnection con = Database.getInstance().Connection;
            lock (typeof(Database))
            {
                try
                {
                    con.Open();
                    MySqlCommand comm = new MySqlCommand("select `pp_main_photo` from `profile_portal` where `pp_member_id` = @id", con);
                    MySqlParameter param = new MySqlParameter("@id", id);
                    comm.Parameters.Add(param);
                    MySqlDataReader read = comm.ExecuteReader();
                    read.Read();
                    string url = (string)read.GetString(0);
                    con.Close();
                    return url;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            return "";*/
        }
    }
}
