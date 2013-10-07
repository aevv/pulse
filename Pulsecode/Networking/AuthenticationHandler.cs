using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.SqlServer.Server;
namespace Pulse.Networking
{
    class AuthenticationHandler : Handler
    {
        public void handleData(BinaryReader br)
        {
            string username = br.ReadString();
            string pwhash = br.ReadString();
            //SOME SQL CHECK GG i hate psuedo implmentations
           
        }
    }
}
