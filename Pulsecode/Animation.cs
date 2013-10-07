using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Pulse
{
    public class Animation
    {
        private float rotation = 0.0f;
        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value; 
                foreach (Rect r in frames)
                {
                    r.Rotation = value;
                }
            }
        }
        private string texturePrefix = "";
        private List<Rect> frames = new List<Rect>();

        public List<Rect> Frames
        {
            get { return frames; }
            set { frames = value; }
        }
        private double frameRate = 60;

        public double FrameRate
        {
            get { return frameRate; }
            set { frameRate = value; }
        }
        private Rectangle bounds;

        public Rectangle Bounds
        {
            get { return bounds; }
            set
            {
                bounds = value;
                foreach (Rect r in frames)
                {
                    r.Bounds = bounds;
                }
            }
        }
        private bool loop;

        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }
        private int frameCount = 0;

        public int FrameCount
        {
            get { return frameCount; }
            set { frameCount = value; }
        }
        private bool mask = false;

        public bool Mask
        {
            get { return mask; }
            set { mask = value; }
        }
        public Animation(Rectangle bounds, int frameRate, string prefix, int frameCount, bool loop, bool mask)
        {
            this.mask = mask;
            this.bounds = bounds;
            this.frameCount = frameCount;
            this.frameRate = frameRate;
            this.texturePrefix = prefix;
            this.loop = loop;
            makeFrames();
        }
        private void makeFrames()
        {
            for (int x = 0; x < frameCount; x++)
            {
                frames.Add(new Rect(bounds, Skin.skindict[texturePrefix + x], mask));
            }
        }
        bool fading = false;

        public bool Fading
        {
            get { return fading; }
            set { fading = value; }
        }
        float startAlpha;
        float targetAlpha = 0.0f;
        double fadeTime = 0, fadeTimeLeft = 0;
        public void fade(float alpha, double time)
        {
            targetAlpha = alpha;
            fadeTime = time;
            startAlpha = Colour.A;
            fadeTimeLeft = time;
            fading = true;
        }
        bool active = true;

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }
        double time = 0;
        int currentFrame = 0;
        public void draw(FrameEventArgs e)
        {
            if (active)
            {
                if (fading)
                {
                    float change = 0;
                    if (fadeTimeLeft <= 0)
                    {
                        fading = false;
                    }
                    else
                    {
                        double progress = (fadeTimeLeft / fadeTime) * 100;
                        progress = 100 - progress;
                        float difference = startAlpha - targetAlpha;
                        change = (difference / 100) * (float)progress;
                        change = startAlpha - change;
                        fadeTimeLeft -= e.Time;
                    }
                    foreach (Rect r in frames)
                    {
                        r.Alpha = change;
                        if (!fading)
                        {
                            r.Alpha = targetAlpha;
                        }
                    }
                }
                time += e.Time;
                if (time > 1 / frameRate)
                {
                    time = 0;
                    if (currentFrame >= frames.Count - 1)
                    {
                        if (loop)
                        {
                            currentFrame = 0;
                        }
                        else
                        {
                            active = false;
                        }
                    }
                    else
                    {
                        currentFrame++;
                    }
                }
                frames[currentFrame].OnRenderFrame(e);
            }
        }
        public void dispose()
        {
            foreach (Rect r in frames)
            {
                r.disposeTexture();
            }
            frames.Clear();
        }
        public Color4 Colour
        {
            get { return frames[0].Color; }
            set
            {
                foreach (Rect r in frames)
                {
                    r.Color = value;
                }
            }
        }
    }
}
