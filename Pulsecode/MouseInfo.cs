using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse
{
    public class MouseInfo
    {
        private bool leftButton;

        public bool LeftButton
        {
            get { return leftButton; }
            set { leftButton = value; }
        }
        private bool rightButton;

        public bool RightButton
        {
            get { return rightButton; }
            set { rightButton = value; }
        }
        private int x;

        public int X
        {
            get { return x; }
            set { x = value; }
        }
        private int y;

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        public MouseInfo(bool l, bool r, int x, int y)
        {
            leftButton = l;
            rightButton = r;
            this.x = x;
            this.y = y;
        }

    }
}
