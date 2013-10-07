#define showAll
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using System.Data.SQLite;
namespace Pulse.Client.irc
{
    //eventually incorporate chatlogs too
    public class IrcClient
    {        // Irc server to connect
        public string SERVER = "p.ulse.net";
        // Irc server's port (6667 is default port)
        public int PORT = 6667;
        // User information defined in RFC 2812 (Internet Relay Chat: Client Protocol) is sent to irc server
        public string USER = "USER guest pulse pulse :pulse user";
        // Bot's nickname
        public string NICK = "Alex|Pulse";
        // Channel to join
        public string CHANNEL = "#pulse";
        public StreamWriter writer;
        public string realNick;
        NetworkStream stream;
        TcpClient irc;
        string inputLine;
        StreamReader reader;
        Thread pingSender;
        Thread listener;
        Thread waiter;
        public List<string> HL = new List<string>(); //highlight terms
        //part for leaving channel, quit for quitting entirely
        //  public List<IrcUser> users = new List<IrcUser>();
        public Queue<IrcMessage> recieved = new Queue<IrcMessage>();
        public Dictionary<string, Dictionary<string, UserLevel>> users = new Dictionary<string, Dictionary<string, UserLevel>>();
        public List<String> pulseUsers = new List<string>();
        public IrcClient(string nick, params string[] hl)
        {
            try
            {
                this.NICK = nick;

                foreach (string s in hl)
                {
                    HL.Add(s);
                }
                irc = new TcpClient(SERVER, PORT);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                pingSender = new Thread(this.sendPings);
                pingSender.IsBackground = true;
                //pingSender.Start();              
                listener = new Thread(Listen);
                listener.IsBackground = true;
                listener.Start();
                waiter = new Thread(wait);
                waiter.IsBackground = true;
                waiter.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in connecting to irc server " + e);
            }
        }
        public void sendPart(string channel, string reason)
        {
            writer.WriteLine("PART " + channel + " :" + reason);
        }
        public void sendMsg(string line)
        {
            // writer.WriteLine("PRIVMSG #pulse :Please enter more arguments. Type 'help' to see a list");
            //do  command parsing in text box instead
            //    if (!line.StartsWith("/")) //not a command
            //  {
            writer.WriteLine("PRIVMSG " + CHANNEL + " :" + line);
            /*  }
              else
              {
                  string[] splitted = line.Split(' ');
                  if (splitted[0].Equals("/pm"))
                  {

                  }
              }*/
        }

        public void joinChan(string channel)
        {
            writer.WriteLine("JOIN " + channel);
        }

        public void sendPm(string user, string message)
        {
            writer.WriteLine("PRIVMSG " + user + " :" + message);
        }
        public void sendRaw(string data)
        {
            writer.WriteLine(data);
        }
        void wait()
        {
            try
            {
                Thread.Sleep(1000);
                writer.WriteLine(USER);
                writer.Flush();
                writer.WriteLine("NICK " + NICK);
                writer.Flush();
                Thread.Sleep(5000); //wait for server to accept
                if (!donotsend)
                {
                    Console.WriteLine("Sent nick and now joining channel");
                    writer.WriteLine("JOIN " + CHANNEL);
                    sendPm("nickserv", "register " + this.NICK + Config.SongLibraryHash + " " + Account.currentAccount.AccountName + "@pulse.net");
                    sendPm("nickserv", "identify " + this.NICK + Config.SongLibraryHash);
                    hbs = true;
                }
            }
            catch
            {
                //fucked but ignore
            }
        }
        bool hbs;
        bool firstmessage = true;
        public void Listen()
        {
            recieved.Enqueue(new IrcMessage("", "connecting to irc..."));
            try
            {
                while (true)
                {
                    while ((inputLine = reader.ReadLine()) != null)
                    {
                        if (firstmessage)
                        {
                            recieved.Enqueue(new IrcMessage("", "Connected to " + SERVER + "!"));
                            firstmessage = false;
                        }
#if showAll
                        Console.WriteLine(inputLine);
#endif
                        IrcMessage processed = processMessage(inputLine);
                        if (processed != null)
                        {
                            recieved.Enqueue(processed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                recieved.Enqueue(new IrcMessage("", "Disconnected from irc, try logging in again."));
                Console.WriteLine(e);
                //terminate();
            }
        }
        bool showmotd;
        bool isMOTD(string s)
        {
            if (!showmotd) //if want to show motd, don't bother to evaluate just return false, then won't be filtered out
            {
                string[] splitted = s.Split(' ');
                if (splitted.Length > 1)
                {
                    int code;
                    if (Int32.TryParse(splitted[1], out code))
                    {
                        if (code == 375 || code == 372 || code == 376)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        IrcMessage processMessage(string input)
        {

            string[] splitted = input.Split(' ');
            if (splitted[1].Equals("PRIVMSG"))
            {
                string temp = input.Substring(splitted[0].Length + 1 + splitted[1].Length + 1 + splitted[2].Length + 1).Substring(1); //get rid of first ':' and return rest of text
                string nick = splitted[0].Substring(1).Split('!')[0]; //get rid of ':' and split to get actual nick
                string target = splitted[2];
                if (splitted[2].Equals(NICK)) //private message
                {
                    //nick += "|pm";
                    target = nick;
                }

                /*
                else if (splitted[2] != CHANNEL) //splitted 2 is the channel
                {
                    return "";
                }*/
                char soh = (char)1;
                string tosend = "<" + nick + "> " + temp;
                bool isme = false;
                if (temp[0] == soh) // /me message, soh is start of header or something
                {
                    string temp2 = temp.Substring(temp.IndexOf("ACTION") + 6);
                    tosend = "*" + nick + temp2.Remove(temp2.Length - 1);
                    isme = true;
                }
                IrcMessage msg = new IrcMessage(target, tosend);
                msg.sender = !isme? "<" + nick + ">" : "*"+nick;
                msg.sendercolor = System.Drawing.Color.SteelBlue;
                if (users.ContainsKey(nick))
                {
                    if (users[nick].ContainsKey(target))
                    {
                        if (users[nick][target] > UserLevel.VOICE)
                        {
                            msg.sendercolor = System.Drawing.Color.Orange;
                        }
                    }
                }
                msg.timestamp = true;
                foreach (string s in HL)
                {
                    if (temp.ToLower(Config.cultureEnglish).Contains(s.ToLower(Config.cultureEnglish)))
                    {
                        msg.HLed = true;
                        // Game.game.
                        //IntPtr hw = Game.game.getHandle();
                        //Console.WriteLine(hw.ToString());

                        //   Console.WriteLine(FlashWindow.Flash(Game.game.getHandle()));
                        //FlashWindow.Flash()
                    }
                }
                if (temp.ToLower(Config.cultureEnglish).Contains(NICK.ToLower(Config.cultureEnglish)))
                {
                    msg.HLed = true;
                }
                return msg;
            }
            else if (splitted[1].Equals("353"))
            {
                parseUsers(splitted[4], input);
                /*string[] s2 = input.Split(':');
                s2 = s2[2].Split(' ');
                string n;
                string r;
                for (int x = 1; x < s2.Length - 1; x++)
                {
                    r = "";
                    if (!char.IsLetter(s2[x][0]))
                    {
                        n = s2[x].Substring(1);
                        r = "" + s2[x][0];
                    }
                    else
                    {
                        n = s2[x];
                    }
                    IrcUser temp = new IrcUser()
                    {
                        name = n,
                        channel = splitted[4],
                        role = r
                    };
                    //users.Add(temp);
                }*/
            }
            else if (splitted[1].Equals("433")) //nick in use, sent if next nick change fails so random is safe            
            {
                Console.WriteLine("this had 433 " + input);

                if (hbs)
                {
                    Console.WriteLine("now attempting to ghost");
                    writer.WriteLine("NICK " + (this.NICK + new Random().Next()));
                    sendPm("nickserv", "ghost " + this.NICK + " " + this.NICK + Config.SongLibraryHash);
                }
                writer.WriteLine("NICK " + this.NICK);
                sendPm("nickserv", "identify " + this.NICK + Config.SongLibraryHash);
                writer.WriteLine("JOIN " + CHANNEL);
            }
            else if (splitted[0].Equals("PING"))
            {
                writer.WriteLine("PONG " + splitted[1]);
            }
            else if (splitted[1] == "319") //whois channel list
            {
                string user = splitted[3];
                string noinit = input.Substring(1);
                string text = noinit.Substring(noinit.IndexOf(':') + 1);
                string[] channels = text.Split(' ');
                foreach (string s in from i in channels where !string.IsNullOrWhiteSpace(i) select i)
                {
                    UserLevel toAdd = UserLevel.NORMAL;
                    switch (s[0])
                    {
                        case '+':
                            toAdd = UserLevel.VOICE;
                            break;
                        case '%':
                            toAdd = UserLevel.HOP;
                            break;
                        case '@':
                            toAdd = UserLevel.OP;
                            break;
                        case '~':
                            toAdd = UserLevel.OWNER;
                            break;
                        case '&':
                            toAdd = UserLevel.ADMIN;
                            break;
                    }
                    string channame = s;
                    if (toAdd != UserLevel.NORMAL)
                    {
                        channame = channame.Substring(1); //getrid of symbol if  special status
                        if (users.ContainsKey(user))
                        {
                            if (users[user].ContainsKey(channame))
                            {
                                users[user][channame] = toAdd;
                            }
                            else
                            {
                                users[user].Add(channame, toAdd);
                            }
                        }
                        else
                        {
                            users.Add(user, new Dictionary<string, UserLevel>());
                            users[user].Add(channame, toAdd);
                        }
                    }
                    else
                    {
                        if (users.ContainsKey(user))
                        {
                            if (users[user].ContainsKey(channame))
                            {
                                users[user].Remove(channame);
                                if (users[user].Count == 0)
                                {
                                    users.Remove(user);
                                }
                            }
                        }
                    }
                }
            }
            else if (splitted.Length > 4 && splitted[1] == "MODE")
            {
                writer.WriteLine("WHOIS " + splitted[4]);
            }
            else if (splitted[1].Equals("JOIN"))
            {
                string channelname = splitted[2].Substring(1);
                string nick = splitted[0].Substring(1).Split('!')[0]; //get rid of ':' and split to get actual nick
                if (!nick.Equals(NICK)) //only respond if is the current nick, otherwise is someone else joining
                {
                    if (channelname == CHANNEL && nick.StartsWith("pulse|") && !pulseUsers.Contains(nick))
                    {
                        pulseUsers.Add(nick);
                    }
                    return new IrcMessage(UI.PTextBox.baseChannel, input);
                }
                else
                {
                    return new IrcMessage(channelname, "Now talking on " + channelname + "!");
                }
                /* if (!Game.pbox.tabs.ContainsKey(channelname)) //GLcontexts, doesn't work
                 {
                     Game.pbox.addTab(channelname);
                     Game.pbox.addLine("Now talking on " + channelname + "!", System.Drawing.Color.SteelBlue, channelname);
                 }*/

            }
            else if (splitted[1].Equals("PART"))
            {
                string channelname = splitted[2];
                string nick = splitted[0].Substring(1).Split('!')[0]; //get rid of ':' and split to get actual nick
                if (!nick.Equals(NICK))
                {
                    if (channelname == CHANNEL && nick.StartsWith("pulse|") && pulseUsers.Contains(nick))
                    {

                        pulseUsers.Remove(nick);
                        lock (User.users)
                        User.users.Remove(nick.Split('|')[1].ToLower());
                    }
                }
            }
            else if (splitted[1].Equals("QUIT"))
            {
                string nick = splitted[0].Substring(1).Split('!')[0]; //get rid of ':' and split to get actual nick
                if (nick.StartsWith("pulse|") && pulseUsers.Contains(nick))
                {
                    pulseUsers.Remove(nick);
                    lock (User.users)
                    User.users.Remove(nick.Split('|')[1].ToLower());
                }

            }
            else if (splitted[1].Equals("KICK"))
            {
                if (splitted[3].Equals(NICK))
                {
                    string channel = splitted[2];
                    if (Game.pbox.tabs.ContainsKey(channel))
                    {
                        Game.pbox.closeTab(channel);
                    }

                    IrcMessage msg = new IrcMessage(UI.PTextBox.baseChannel, "You have been kicked from " + channel);
                    msg.customColor = true;
                    msg.col = System.Drawing.Color.Red;
                    return msg;
                }
                else
                {
                    string nick = splitted[3];
                    if (nick.StartsWith("pulse|"))
                    {
                        pulseUsers.Remove(nick);
                        lock (User.users)
                        User.users.Remove(nick.Split('|')[1].ToLower());
                    }
                }
            }
            else
            {
                if (Game.pbox.ircverbose && !isMOTD(input))
                {
                    return new IrcMessage(UI.PTextBox.baseChannel, input);
                }

                // meh flood
            }
            return null;
        }
        static string PING = "PING :";

        public void sendPings()
        {
            try
            {
                while (true)
                {
                    writer.WriteLine(PING + SERVER);
                    writer.WriteLine("PONG : " + SERVER);
                    writer.Flush();
                    Console.WriteLine("pinged");
                    Thread.Sleep(15000);
                }
            }
            catch (Exception)
            {

            }
        }
        bool donotsend = false;
        public void terminate()
        {
            try
            {
                writer.WriteLine("QUIT :Terminating");
                //writer.Close();
                writer.Dispose();
                //   reader.Close();
                reader.Dispose();
                //   stream.Close();
                stream.Dispose();
                pingSender.Abort();
                listener.Abort();
                waiter.Abort();
                donotsend = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void parseUsers(string channel, string inputLine)
        {
            //      string channel = param[4];//.Equals(CHANNEL)

            string[] nicks = inputLine.Substring(1).Split(':')[1].Split(' ');
            foreach (string nick in nicks)
            {
                if (!string.IsNullOrWhiteSpace(nick))
                {
                    if (nick.StartsWith("pulse|"))
                    {
                        if (!pulseUsers.Contains(nick))
                        {
                            pulseUsers.Add(nick);
                        }
                    }
                    string rnick = nick;
                    UserLevel toAdd = UserLevel.NORMAL;
                    switch (nick[0])
                    {
                        case '+':
                            toAdd = UserLevel.VOICE;
                            break;
                        case '%':
                            toAdd = UserLevel.HOP;
                            break;
                        case '@':
                            toAdd = UserLevel.OP;
                            break;
                        case '~':
                            toAdd = UserLevel.OWNER;
                            break;
                        case '&':
                            toAdd = UserLevel.ADMIN;
                            break;
                    }

                    if (toAdd != UserLevel.NORMAL)
                    {
                        rnick = nick.Substring(1);

                        if (users.ContainsKey(rnick))
                        {
                            if (users[rnick].ContainsKey(channel))
                            {
                                users[rnick][channel] = toAdd;
                            }
                            else
                            {
                                users[rnick].Add(channel, toAdd);
                            }
                        }
                        else
                        {
                            users.Add(rnick, new Dictionary<string, UserLevel>());
                            users[rnick].Add(channel, toAdd);
                        }
                    }
                    else
                    {
                        if (users.ContainsKey(rnick))
                        {
                            if (users[rnick].ContainsKey(channel))
                            {
                                users[rnick].Remove(channel);
                                if (users[rnick].Count == 0)
                                {
                                    users.Remove(rnick);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public class IrcMessage
    {
        string target;

        public string Target
        {
            get { return target; }
            //  set { target = value; }
        }
        string msg;
        //So, if you want a custom color, set customColor in the IrcMessage to true, and set the col, otherwise col will be ignored.
        //set hl'ed to true for automatic hl handling
        public bool HLed; //false by default
        public bool customColor;
        public System.Drawing.Color col; //= System.Drawing.Color.FromArgb(255, 255, 150);
        public bool timestamp; //add timestamp when adding to chatbox
        public string sender;
        public System.Drawing.Color sendercolor;
        public string Msg
        {
            get { return msg; }
            // set { msg = value; }
        }
        public IrcMessage(string tar, string message)
        {
            target = tar;
            msg = message;
        }
    }
    public class IrcUser
    {
        public string channel;
        public string name;
        public string role;
    }

    public enum UserLevel : int
    {
        NORMAL = 0,
        VOICE = 1,
        HOP = 2,
        OP = 3,
        OWNER = 4,
        ADMIN = 5
    }
}