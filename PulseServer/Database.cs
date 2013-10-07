/**Thread safe database access class. Uses singleton design pattern.
 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.IO;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using PulseServer.Game;


namespace PulseServer
{
    class Database
    {
        static Database s = null;
        private MySqlConnection connection;

        public MySqlConnection Connection
        {
            get { return connection; }
            // set { connection = value; }
        }
        private string connectionString = "";
        private Database()
        {
            using (StreamReader r = new StreamReader("db.ini"))
            {
                string all = r.ReadToEnd();
                connectionString = all;
            }
            connection = new MySqlConnection(connectionString);
        }
        public static Database getInstance()
        {
            lock (typeof(Database))
            {
                if (s == null)
                {
                    s = new Database();
                }
                return s;
            }
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
        public Session login(string user, string pass)
        {
            lock (typeof(Database))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine(connection.State.ToString());
                    MySqlCommand comm = new MySqlCommand("SELECT `md5`, `phpbb_users`.`user_id`, phpbb_users.user_avatar FROM `pulse_user` INNER JOIN `phpbb_users` on pulse_user.user_id=phpbb_users.user_id where phpbb_users.username=@username;", connection);
                    //     MySqlCommand comm = new MySqlCommand("select `md5` from `pulse_user` where `name` = @username", connection);
                    // MySqlCommand comm = new MySqlCommand("select `members_pass_salt`, `members_pass_hash`, `member_id` from `members` where `name` = @username", connection);
                    MySqlParameter param = new MySqlParameter("@username", user);
                    comm.Parameters.Add(param);

                    MySqlDataReader read = comm.ExecuteReader();
                    if (!read.HasRows)
                    {
                        connection.Close();
                        return null;
                    }
                    //read.NextResult();
                    read.Read();
                    string dbpass = (String)read.GetValue(0);
                    int id = read.GetInt32(1);
                    string avi = read.GetString(2);
                    /*    string salt = (String)read.GetValue(0);
                        string dbpass = (String)read.GetValue(1);
                        int id = read.GetInt32(2);//Int32.Parse((String)read.GetValue(2));
                        //    Console.WriteLine("id = " + id);
                        string salthash = CalculateMD5Hash(salt);
                        string passhash = (pass);
                        string hashed = CalculateMD5Hash(salthash + passhash);*/
                    if (pass == dbpass)
                    {
                        connection.Close();
                        return new Session(user, id, "http://exp.ulse.net/forum/download/file.php?avatar=" + avi);
                    }
                    else
                    {
                        Console.WriteLine("Failed login attempt with user " + user);
                    }
                    connection.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    connection.Close();
                }
            }
            return null;
        }
        public Session login_plaintext(string user, string pass)
        {
            return login(user, CalculateMD5Hash(pass));
        }
        public User getUser(string user)
        {
            lock (typeof(Database))
            {
                try
                {
                    connection.Open();
                    MySqlCommand comm = new MySqlCommand("SELECT phpbb_users.user_avatar, pulse_user.totalscore, pulse_user.playcount, phpbb_users.username,pulse_user.accuracy," +
                        "pulse_user.accounttype, pulse_user.level FROM `pulse_user` " +
                        "INNER JOIN `phpbb_users` on pulse_user.user_id=phpbb_users.user_id where lower(phpbb_users.username)=lower(@username);", connection);
                    MySqlParameter param = new MySqlParameter("@username", user);
                    comm.Parameters.Add(param);

                    MySqlDataReader read = comm.ExecuteReader();
                    if (!read.HasRows)
                    {
                        connection.Close();
                        return null;
                    }
                    User u = new User();
                    read.Read();
                    u.Name = user.ToLower();
                    u.RealName = read.GetString("username");
                    u.Level = read.GetInt16("level");
                    u.AccountType = read.GetInt32("accounttype");
                    u.Avatar = read.GetString("user_avatar");
                    u.Accuracy = read.GetFloat("accuracy");
                    u.TotalScore = read.GetInt32("totalscore");
                    u.Playcount = read.GetInt32("playcount");
                    u.Mode = User.PlayMode.IDLE;
                    return u;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    connection.Close();
                }
                return null;
            }
        }
    }
}
