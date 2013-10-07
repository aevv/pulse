using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Pulse.UI
{
    public class Frame
    {
        List<Rect> keyReleased = new List<Rect>();
        Rect left;
        Point location;
        Rect hitBar;
        int[] laneLoc = new int[8];

        public int[] LaneLoc
        {
            get { return laneLoc; }
            set { laneLoc = value; }
        }
        int[] laneWidth = new int[8];

        public int[] LaneWidth
        {
            get { return laneWidth; }
            set { laneWidth = value; }
        }
        double hitHeight = 600;
        public void setHeight(int i)
        {
            hitHeight = i;
        }
        public double HitHeight
        {
            get { return hitHeight; }
            set { hitHeight = value; }
        }
        double width = 0;

        public double Width
        {
            get { return width; }
            set { width = value; }
        }
        List<Rect> frameSides = new List<Rect>();

        public List<Rect> FrameSides
        {
            get { return frameSides; }
            set { frameSides = value; }
        }
        List<Rect> frameBacks = new List<Rect>();

        public List<Rect> FrameBacks
        {
            get { return frameBacks; }
            set { frameBacks = value; }
        }
        List<Rect> frameDivides = new List<Rect>();

        public List<Rect> FrameDivides
        {
            get { return frameDivides; }
            set { frameDivides = value; }
        }

        public Point Location
        {
            get { return location; }
            set { location = value; }
        }
        private bool editor = false;
        public Frame(Point location, bool middleEdit, bool editor)
            : this(location, 8)
        {
            this.editor = editor;
            if (editor)
            {
                bottomFrame = null;
                keyReleased.Clear();
                left = null;
            }
            if (middleEdit)
            {
                hitBar = new Rect(new Rectangle(location.X, location.Y + (((int)hitHeight - 4) / 2), (int)width, 8), Skin.skindict["hitBar"]);
            }
        }
        public Frame(Point location, int keys)
        {
            this.editor = false;            
            if (Skin.BottomSpace)
            {
                hitHeight = 570;
            }
            else
            {
                hitHeight = 600;
            }
            for (int x = 0; x < 8; x++)
            {
                laneLoc[x] = Skin.LaneLoc[x];
                laneWidth[x] = Skin.LaneWidth[x];
            }
            Rect temp = new Rect(new Rectangle(location.X, location.Y, 5, 600), Skin.skindict["frameSide"]);
            frameSides.Add(temp);
            float bgAlpha = 1.0f;
            if (Skin.ClearFrame)
            {
                bgAlpha = 0.5f;
            }
            this.width += (2 * 5) + (1 * (keys - 2));
            for (int x = 0; x < keys; x++)
            {
                this.width += Skin.LaneWidth[x];
            }
            for (int x = keys; x < 8; x++)
            {
                laneLoc[x] = laneLoc[keys - 1];
                laneWidth[x] = 0;
            }
            int count = 0;
            for (int x = location.X + 5; x < location.X + width - 10; x += laneWidth[count++] + 1)
            {
                temp = new Rect(new Rectangle(x, location.Y, laneWidth[count], 600), Skin.skindict["frameBack"]);
                temp.Color = new Color4(Skin.BarColours[frameBacks.Count].R, Skin.BarColours[frameBacks.Count].G, Skin.BarColours[frameBacks.Count].B, bgAlpha);
                frameBacks.Add(temp);
            }

            temp = new Rect(new Rectangle(location.X + (int)width - 5, location.Y, 5, 600), Skin.skindict["frameSide"]);
            frameSides.Add(temp);
            this.location = location;
            count = 0;
            for (int x = location.X + laneWidth[count] + 5; x < location.X + width - 10; x += laneWidth[++count] + 1)
            {
                temp = new Rect(new Rectangle(x, location.Y, 1, 600), Skin.skindict["frameDivide"]);
                if (frameDivides.Count == 2 || frameDivides.Count == 3)
                {
                    temp.Color = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
                }
                else
                {
                    temp.Color = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
                }
                frameDivides.Add(temp);
            }
            hitBar = new Rect(new Rectangle(location.X, location.Y + (int)hitHeight - 4, (int)width, 8), Skin.skindict["hitBar"]);
            if (!editor)
            {
                count = 0;
                bottomFrame = new Rect(new Rectangle(0, Location.Y + 600, Config.ResWidth, 170), Skin.skindict["scoreBottom"]);
                for (int x = location.X + 5; x < location.X + width - 10; x += laneWidth[count++] + 1)
                {
                    keyReleased.Add(new Rect(new Rectangle(x, location.Y + 600, laneWidth[count] + 1, 50), Skin.skindict["key" + (count + 1)]));
                }
                left = new Rect(new Rectangle(location.X - 55, Location.Y, 55, 600), Skin.skindict["frameLeft"]);
            }            
        }
        public Rect bottomFrame;
        public void draw(FrameEventArgs e)
        {
            if (bottomFrame != null)
            {
                bottomFrame.OnRenderFrame(e);
            }
            if (left != null)
            {
                left.OnRenderFrame(e);
            }
            foreach (Rect r in frameSides)
            {
                r.OnRenderFrame(e);
            }
            foreach (Rect r in frameBacks)
            {
                r.OnRenderFrame(e);
            }
            foreach (Rect r in frameDivides)
            {
                r.OnRenderFrame(e);
            }
            foreach (Rect r in keyReleased)
            {
                r.OnRenderFrame(e);
            }
            hitBar.OnRenderFrame(e);            
        }
    }
}
