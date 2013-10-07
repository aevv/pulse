using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Pulse
{
    class Line
    {
        protected Point v1, v2;

        public Point V2
        {
            get { return v2; }
            set { v2 = value; }
        }
        public float thickness
        {
            get;
            set;
        }
        public Point V1
        {
            get { return v1; }
            set { v1 = value; }
        }
        private Color4 colour = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

        public Color4 Colour
        {
            get { return colour; }
            set { colour = value; }
        }
        public Line(Point v1, Point v2)
        {
            this.v1 = v1;
            this.v2 = v2;
            thickness = 1;
        }
        float targetAlpha = 0.0f;
        double fadeTime, moveTime;
        double fadeTimeLeft, moveTimeLeft;
        float startAlpha = 0.0f;
        Point startLocation,startLocation2;
        Point targetLocation;
        private bool fading, moving;
        Point targetLocation2;
        public void move(Point p, Point p2, double timeSpan)
        {
            targetLocation = p;
            targetLocation2 = p2;
            moveTime = timeSpan;
            moveTimeLeft = timeSpan;
            startLocation = v1;
            startLocation2 = v2;
            moving = true;
        }
        public void draw(FrameEventArgs e)
        {
            if (moving)
            {
                if (moveTimeLeft <= 0)
                {
                    v1 = targetLocation;
                    v2 = targetLocation2;
                    moving = false;
                }
                else
                {
                    double progress = (moveTimeLeft / moveTime) * 100;
                    progress = 100 - progress;
                    double xDiff = startLocation.X - targetLocation.X;
                    double xDiff2 = startLocation2.X - targetLocation2.X;
                    double xChange = (xDiff / 100) * progress;
                    double xChange2 = (xDiff2 / 100) * progress;
                    double yDiff = startLocation.Y - targetLocation.Y;
                    double yDiff2 = startLocation2.Y - targetLocation2.Y;
                    double yChange = (yDiff / 100) * progress;
                    double yChange2 = (yDiff2 / 100) * progress;
                    xChange = startLocation.X - xChange;
                    xChange2 = startLocation2.X - xChange2;
                    yChange = startLocation.Y - yChange;
                    yChange2 = startLocation2.Y - yChange2;
                    v1 = new Point((int)xChange, (int)yChange);
                    v2 = new Point((int)xChange2, (int)yChange2);
                    moveTimeLeft -= e.Time;
                }
            }
            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(thickness);
            GL.Begin(BeginMode.Lines);

            GL.Color4(colour);

            GL.Vertex2(v1.X * (float)((float)Config.ClientWidth / (float)Config.ResWidth), v1.Y * (Config.ClientHeight / 768f));
            GL.Vertex2(v2.X * (float)((float)Config.ClientWidth / (float)Config.ResWidth), v2.Y * (Config.ClientHeight / 768f));
            GL.End();
            GL.Enable(EnableCap.Texture2D);
        }
    }
}
