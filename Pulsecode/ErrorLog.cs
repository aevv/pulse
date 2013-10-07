using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace Pulse
{
    public class ErrorLog
    {
        private static void sendErrorLog(object e, BinaryWriter bw)
        {
            string temp = (string)e;
            Console.WriteLine(temp);
            saveLog(temp);
            Client.PacketWriter.sendError(bw, temp);
            /*try
            {
                //IIRC can just use e-mail instead, so you don't have to run a server
                //btw, i think the client should be closed ye
                TcpClient tcp = new TcpClient("92.232.66.53", 7777);
                BinaryWriter br = new BinaryWriter(tcp.GetStream());
                br.Write((short)5);
                br.Write(temp);
                br.Close();
                tcp.Close();
            }
            catch
            {
                Console.WriteLine("Could not establish a connection to server to send error report:");
                Console.WriteLine(temp);
            }*/
        }
        public static void log(Exception e)
        {
            log(e.Message + "\n" + e.StackTrace);
        }
        public static void log(string s)
        {
          //  sendErrorLog();
            if (Game.conn != null)
            {
                sendErrorLog(s, Game.conn.Bw);
            }
        }
        public static void saveLog(string err)
        {
            using (StreamWriter sr = new StreamWriter("errorlog.txt", true))
            {
                sr.WriteLine(System.DateTime.Now + ": " + err);
                sr.Close();
            }      
        }
    }
}
