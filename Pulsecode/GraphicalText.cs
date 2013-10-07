using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Pulse
{
    class GraphicalText
    {
        private bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        private int charWidth = 50;

        public int CharWidth
        {
            get { return charWidth; }
            set { charWidth = value; }
        }
        private int charHeight = 50;

        public int CharHeight
        {
            get { return charHeight; }
            set { charHeight = value; }
        }
        private string text;

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                for (int x = 0; x < text.Length; x++)
                {
                    if (x < rects.Count)
                    {
                        rects[x].useTexture(Skin.skindict[text.Substring(x, 1)]);
                        rects[x].Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                    }
                    else
                    {
                        Rect temp;
                        if (text.Substring(x, 1).Equals(" "))
                        {
                            temp = new Rect(new Rectangle(location.X + (x * (charWidth - Skin.TextOverlap)), location.Y, charWidth, charHeight));
                            temp.Color = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
                        }
                        else
                        {
                            temp = new Rect(new Rectangle(location.X + (x * (charWidth - Skin.TextOverlap)), location.Y, charWidth, charHeight), Skin.skindict[text.Substring(x, 1)]);
                        }
                        rects.Add(temp);
                    }
                }
                List<Rect> toRemove = new List<Rect>();
                if (text.Length < rects.Count)
                {
                    for (int x = text.Length; x < rects.Count; x++)
                    {
                        toRemove.Add(rects[x]);
                    }
                }
                for (int i = toRemove.Count - 1; i > -1; i--)
                {
                    rects.Remove(toRemove[i]);
                }
            }
        }
        private List<Rect> rects = new List<Rect>();

        public List<Rect> Rects
        {
            get { return rects; }
            set { rects = value; }
        }
        private Point location;

        public Point Location
        {
            get { return location; }
            set
            {
                location = value;
                for (int x = 0; x < rects.Count; x++)
                {
                    rects[x].Bounds = new Rectangle(location.X + x * (charWidth - Skin.TextOverlap), location.Y, charWidth, charHeight);
                }
            }
        }
        Rect[] holders = new Rect[11];
        public GraphicalText(string text, Point location)
        {
            this.text = text;
            this.location = location;
            for (int x = 0; x < 10; x++)
            {
                holders[x] = new Rect(new Rectangle(0, 0, 0, 0), Skin.skindict["" + x]);
            }
            for (int x = 0; x < text.Length; x++)
            {
                Rect temp;
                if (text.Substring(x, 1).Equals(" "))
                {
                    temp = new Rect(new Rectangle(location.X + (x * (charWidth - Skin.TextOverlap)), location.Y, charWidth, charHeight));
                    temp.Color = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
                }
                else
                {
                    temp = new Rect(new Rectangle(location.X + (x * (charWidth - Skin.TextOverlap)), location.Y, charWidth, charHeight), Skin.skindict[text.Substring(x, 1)]);
                }
                rects.Add(temp);
            }
        }
        public void draw(FrameEventArgs e)
        {
            if (scaling)
            {
                if (scaleTimeLeft <= 0)
                {
                    charHeight = targetSize.Height;
                    charWidth = targetSize.Width;
                    scaling = false;
                    for (int x = 0; x < rects.Count; x++)
                    {
                        rects[x].Bounds = new Rectangle(new Point(location.X + (x * (charWidth - Skin.TextOverlap)), location.Y), new Size(charWidth, charHeight));
                    }
                }
                else
                {
                    double progress = (scaleTimeLeft / scaleTime) * 100;
                    progress = 100 - progress;
                    double xDiff = startSize.Width - targetSize.Width;
                    double xChange = (xDiff / 100) * progress;
                    double yDiff = startSize.Height - targetSize.Height;
                    double yChange = (yDiff / 100) * progress;
                    xChange = startSize.Width - xChange;
                    yChange = startSize.Height - yChange;
                    charWidth = (int)xChange;
                    charHeight = (int)yChange;
                    scaleTimeLeft -= e.Time;
                    for (int x = 0; x < rects.Count; x++)
                    {
                        rects[x].Bounds = new Rectangle(new Point(location.X + (x * (charWidth - Skin.TextOverlap)), location.Y), new Size(charWidth, charHeight));
                    }
                }
            }
            if (moving)
            {
                if (moveTimeLeft <= 0)
                {
                    Location = targetLocation;
                    moving = false;
                }
                else
                {
                    double progress = (moveTimeLeft / moveTime) * 100;
                    progress = 100 - progress;
                    double xDiff = startLocation.X - targetLocation.X;
                    double xChange = (xDiff / 100) * progress;
                    double yDiff = startLocation.Y - targetLocation.Y;
                    double yChange = (yDiff / 100) * progress;
                    xChange = startLocation.X - xChange;
                    yChange = startLocation.Y - yChange;
                    Location = new Point((int)xChange, (int)yChange);
                    moveTimeLeft -= e.Time;
                }
                for (int x = 0; x < rects.Count; x++)
                {
                    rects[x].Bounds = new Rectangle(new Point(Location.X + (x * (charWidth - Skin.TextOverlap)), Location.Y), new Size(charWidth, charHeight));
                }
            }
            foreach (Rect r in rects)
            {
                r.OnRenderFrame(e);
            }
        }
        public void scale(Size s, double timespan)
        {
            targetSize = s;
            scaleTime = timespan;
            scaleTimeLeft = timespan;
            startSize = new Size(charWidth, charHeight);
            scaling = true;
        }
        double scaleTime, scaleTimeLeft, moveTime, moveTimeLeft;
        Size startSize, targetSize;
        Point targetLocation, startLocation;
        public bool scaling, moving;
        public void move(Point p, double timespan)
        {
            targetLocation = p;
            moveTime = timespan;
            moveTimeLeft = timespan;
            startLocation = location;
            moving = true;
        }
        public static int measureString(string text, int width)
        {
            int total = text.Length * (width - Skin.TextOverlap);
            return total;
        }
    }
}
