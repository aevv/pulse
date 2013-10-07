using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Pulse.Networking
{
    class ChatHandler : Handler
    {
        public void handleData(BinaryReader br)
        {
            /* Sample write would be
             * write((uint) 13123) id
             * write("noob") user
             * write(255) color rgb
             * write(255)
             * write(255)
             * 
             * write("hello world") message
             */
            uint id = br.ReadUInt32(); //perform some session validation checking
            string user = br.ReadString();
            int r = br.ReadInt32();
            int g = br.ReadInt32();
            int b = br.ReadInt32();
            Color chatColor = Color.FromArgb(r, g, b);
            string message = br.ReadString();
            //do something cool, like url parsing :(
            try
            {
                Regex regexObj = new Regex(@"\b((?#protocol)https?|ftp)://((?#domain)[-A-Z0-9.]+)((?#file)/[-A-Z0-9+&@#/%=~_|!:,.;]*)?((?#parameters)\?[A-Z0-9+&@#/%=~_|!:,.;]*)?", RegexOptions.IgnoreCase);
                Match matchResults = regexObj.Match(message);
                while (matchResults.Success)
                {
                    // matched text: matchResults.Value
                    // match start: matchResults.Index
                    // match length: matchResults.Length
                    Console.WriteLine(matchResults.Value);
                    matchResults = matchResults.NextMatch();
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
    }
}
