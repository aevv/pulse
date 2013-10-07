using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Mechanics
{
    class Mods
    {
        private uint flags = 0;

        public uint Flags
        {
            get { return flags; }
            set { flags = value; }
        }
        private double scroll = 1.0;

        public double Scroll
        {
            get { return scroll; }
            set { scroll = value; }
        }
        private double speed = 1.0;

        public double Speed
        {
            get { return speed; }
            set { speed = value; }
        }
    }
}
