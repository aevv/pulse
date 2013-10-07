using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Updater
{
    class Worker
    {
        public static TcpListener listener;
        public void start(object port)
        {
            listener = new TcpListener((int)port);
            listener.Start();
            try
            {
                while (true)
                {
                    
                    if (Form1.stop)
                    {
                        Console.WriteLine("broke");
                        listener.Stop();
                        break;
                    }
                    TcpClient client = listener.AcceptTcpClient();
                    ClientHandler ch = new ClientHandler(client);
                    Form1.clients.Add(ch);
                    Form1.form.Invoke(new Form1.changeText(Form1.form.changeLabel), "Connected:" + Form1.clients.Count);
                    ch.clientDC += Form1.clientDCed;
                    Thread t = new Thread(new ThreadStart(ch.start));
                    t.Start();

                }
            }
            catch (ThreadAbortException t)
            {
                //this shit isn't called because of the block of accepting tcp
                Console.WriteLine("got the abort");
                listener.Stop();
            }
           
        }
    }
}
