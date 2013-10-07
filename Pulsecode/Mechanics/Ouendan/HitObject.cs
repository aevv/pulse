using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;

namespace Pulse.Mechanics.Ouendan
{
    public enum HitObjectType
    {
        CIRCLE = 1,
        SLIDER = 2,
        SPINNER = 3
    }
    public class HitObject
    {
        private Rect approachTexture;
        private Rect texture;

        public Rect Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        private Point location;

        public Point Location
        {
            get { return location; }
            set { location = value; }
        }
        private double offset;

        public double Offset
        {
            get { return offset; }
            set { offset = value; }
        }
        private int number = 0;

        public int Number
        {
            get { return number; }
            set { number = value; }
        }
        private double approach;

        public double Approach
        {
            get { return approach; }
            set { approach = value; }
        }

        public HitObject(Game game, int offset, Point location)
        {
            this.location = location;
            this.approach = offset - (1200 - (approach * 100));
            texture = new Rect(new Rectangle(location.X - 100, location.Y - 100, 200, 200), Skin.skindict["circle"]);
            approachTexture = new Rect(new Rectangle(location.X - 200, location.Y - 200, 400, 400), Skin.skindict["app"]);
        }

        public void setApproach(int currentTime)
        {
            double temp = offset - currentTime;
            double percent = (temp / (offset - approach));
            approachTexture.Bounds = new Rectangle((int)(location.X) - (int)((200 / 2) * percent),
                (int)(location.Y) - (int)((200 / 2) * percent),
                100 + (int)(200 * percent), 100 + (int)(200 * percent));
        }
        public void draw(FrameEventArgs e)
        {

        }
    }
}
