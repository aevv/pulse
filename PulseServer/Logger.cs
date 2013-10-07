using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PulseServer
{
    class Logger
    {
        public static void log(string err)
        {
            using (StreamWriter sr = new StreamWriter("errorlog.txt", true))
            {
                sr.WriteLine(System.DateTime.Now + ": " + err);
                sr.Close();
            }            
        }
    }
}
