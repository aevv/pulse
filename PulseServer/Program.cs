using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulseServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
               // Console.WriteLine(System.IO.Path.GetTempPath());
               // Database.getInstance().test();
                Server.listen();
            //   Database c = Database.getInstance();
              //  Session s =  c.login("nop", "nop");
              //  Console.WriteLine(s.getAvatarUrl());
              //  Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
