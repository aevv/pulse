using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Pulse.UI;

namespace Pulse
{
    public class Rect
    {
        private double layer = 0;

        public double Layer
        {
            get { return layer; }
            set { layer = value; }
        }
        private RectangleF sourceBounds;

        public RectangleF SourceBounds
        {
            get { return sourceBounds; }
            set { sourceBounds = value; }
        }
        private SizeF bMaskSize = new SizeF(0, 0);

        public SizeF BMaskSize
        {
            get { return bMaskSize; }
            set { bMaskSize = value; }
        }
        private SizeF bSize = new SizeF(0, 0);

        public SizeF BSize
        {
            get { return bSize; }
            set { bSize = value; }
        }
        private float rotation = 0.0f;

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        private bool masked = false;

        public bool Masked
        {
            get { return masked; }
            set { masked = value; }
        }
        private bool fading, scaling, moving;

        public bool Moving
        {
            get { return moving; }
            set { moving = value; }
        }

        public bool Scaling
        {
            get { return scaling; }
            set { scaling = value; }
        }

        public bool Fading
        {
            get { return fading; }
            set { fading = value; }
        }
        float targetAlpha = 0.0f;
        double fadeTime, scaleTime, moveTime;
        double moveXTime, moveYTime;
        double fadeTimeLeft, scaleTimeLeft, moveTimeLeft;
        double moveXTimeLeft, moveYTimeLeft;
        int startX, startY, targetX, targetY;
        float startAlpha = 0.0f;
        Point startLocation;
        Size startSize;
        Point targetLocation;
        Size targetSize;
        private string imagePath;
        bool textured = false;
        public bool Textured
        {
            get
            {
                return textured;
            }
            set
            {
                textured = value;
            }
        }
        private double lifespan = 0;

        public double Lifespan
        {
            get
            {
                return lifespan;
            }
            set
            {
                lifespan = value;
            }
        }
        private Rectangle bounds;
        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
            set
            {
                bounds = value;
            }
        }

        private int textureID;

        public int TextureID
        {
            get
            {
                return textureID;
            }
            set
            {
                textureID = value;
            }
        }
        private int maskID;

        public int MaskID
        {
            get { return maskID; }
            set { maskID = value; }
        }
        private Color4 colour = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

        public Color4 Color
        {
            get
            {
                return colour;
            }
            set
            {
                colour = value; alpha = colour.A;
            }
        }
        private float alpha;
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                colour = new Color4(colour.R, colour.G, colour.B, value);
            }
        }
        public Rectangle ModifiedBounds
        {
            get
            {
                return new Rectangle(scaledToX(bounds.X), scaledToY(bounds.Y), scaledToX(bounds.Width), scaledToY(bounds.Height));
            }
        }
        public Rect(Rectangle r, string path)
            : this(r)
        {
            Alpha = 1.0f;
            this.textureID = TextureManager.loadImage(path, out bSize);
            imagePath = path;
            textured = true;
        }
        public Rect(Rectangle r, string path, bool mask, bool cache) : this(r)
        {
            Alpha = 1.0f;
            this.textureID = TextureManager.loadImage(path, cache, out bSize);
            imagePath = path;
            textured = true;
            if (mask)
            {
                masked = true;
                maskID = TextureManager.loadMask(path, out bMaskSize);
            }
        }
        public Rect(Rectangle r, string path, bool mask)
            : this(r, path)
        {
            if (mask)
            {
                masked = true;
                maskID = TextureManager.loadMask(path, out bSize);
            }
        }
        public bool disposeOnFree = false;

        public Rect(Rectangle r, Bitmap b, String key)
            : this(r)
        {
            this.textureID = TextureManager.loadFromBitmap(b, key, out bSize);
            textured = true;
            imagePath = "";
        }
        public Rect(Rectangle r, Bitmap b)
            : this(r)
        {
            this.textureID = TextureManager.loadFromBitmap(b, out bSize);
            textured = true;
            imagePath = "";
        }
        public Rect(Rectangle r)
        {
            this.bounds = r;
        }
        public void fade(float alpha, double timeSpan)
        {
            targetAlpha = alpha;
            fadeTime = timeSpan;
            startAlpha = Color.A;
            fadeTimeLeft = timeSpan;
            fading = true;
        }
        /// <summary>
        /// Convenience method of move(Point p, double timeSpan). Exact same function.
        /// </summary>
        /// <param name="x">x coord of the point</param>
        /// <param name="y">y coord of the point</param>
        /// <param name="timeSpan">the time</param>
        public void move(int x, int y, double timeSpan)
        {
            move(new Point(x, y), timeSpan);
        }
        /// <summary>
        /// Moves the Rect object to the specified point, taking timeSpan amount of seconds.
        /// </summary>
        /// <param name="p">The destination of the Rect</param>
        /// <param name="timeSpan">How long (in seconds) it should take for the Rect to move; the smaller this number is the faster the transition.</param>
        public void move(Point p, double timeSpan)
        {
            targetLocation = p;
            moveTime = timeSpan;
            moveTimeLeft = timeSpan;
            startLocation = bounds.Location;
            moving = true;
        }
        public bool xMoving;
        public void moveX(int newX, double timeSpan)
        {
            targetX = newX;
            moveXTime = timeSpan;
            moveXTimeLeft = timeSpan;
            startX = bounds.X;
            xMoving = true;
        }
        public void moveY(int newY, double timeSpan)
        {
            targetY = newY;
            moveYTime = timeSpan;
            moveYTimeLeft = timeSpan;
            startY = bounds.Y;
            yMoving = true;
        }
        public bool yMoving;
        public void scale(int height, int width, double timeSpan)
        {
            scale(new Size(height, width), timeSpan);
        }
        /// <summary>
        /// Scale (resizes) the object to the specified in timeSpan seconds. The origin is the top left.
        /// </summary>
        /// <param name="s">the new target size</param>
        /// <param name="timeSpan">time length of the resize (in seconds)</param>
        public void scale(Size s, double timeSpan)
        {
            targetSize = s;
            scaleTime = timeSpan;
            scaleTimeLeft = timeSpan;
            startSize = bounds.Size;
            scaling = true;
        }
        public void useTexture(string path)
        {
            disposeTexture();
            if (File.Exists(path))
            {
                this.textureID = TextureManager.loadImage(path, out bSize);
                imagePath = path;
                textured = true;
            }
            else
            {
                this.textureID = TextureManager.loadFromBitmap(DefaultSkin.defaultbg, true, "defaultbg", out bSize);
                imagePath = "";
                textured = true;
            }
        }
        public void disposeTexture()
        {
            if (textured && textureID > 0)
            {
                TextureManager.removeImage(imagePath);
            }
        }

        public void OnRenderFrame(FrameEventArgs e)
        {
            if (fading)
            {
                if (fadeTimeLeft <= 0)
                {
                    colour.A = targetAlpha;
                    //Alpha = targetAlpha;
                    fading = false;
                }
                else
                {
                    double progress = (fadeTimeLeft / fadeTime) * 100;
                    progress = 100 - progress;
                    float difference = startAlpha - targetAlpha;
                    float change = (difference / 100) * (float)progress;
                    change = startAlpha - change;
                    colour.A = change;
                    //Alpha = change;
                    fadeTimeLeft -= e.Time;
                }
            }
            if (moving)
            {
                if (moveTimeLeft <= 0)
                {
                    bounds.Location = targetLocation;
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
                    bounds.Location = new Point((int)xChange, (int)yChange);
                    moveTimeLeft -= e.Time;
                }
            }
            if (xMoving)
            {
                if (moveXTimeLeft <= 0)
                {
                    bounds.Location = new Point(targetX, bounds.Location.Y);
                    xMoving = false;
                }
                else
                {
                    double progress = (moveXTimeLeft / moveXTime) * 100;
                    progress = 100 - progress;
                    double xDiff = startX - targetX;
                    double xChange = (xDiff / 100) * progress;
                    xChange = startX - xChange;
                    bounds.Location = new Point((int)xChange, bounds.Location.Y);
                    moveXTimeLeft -= e.Time;
                }
            }
            if (yMoving)
            {
                if (moveYTimeLeft <= 0)
                {
                    bounds.Location = new Point(bounds.Location.X, targetY);
                    yMoving = false;
                }
                else
                {
                    double progress = (moveYTimeLeft / moveYTime) * 100;
                    progress = 100 - progress;
                    double yDiff = startY - targetY;
                    double yChange = (yDiff / 100) * progress;
                    yChange = startY - yChange;
                    bounds.Location = new Point(bounds.Location.X, (int)yChange);
                    moveYTimeLeft -= e.Time;
                }
            }
            if (scaling)
            {
                if (scaleTimeLeft <= 0)
                {
                    bounds.Size = targetSize;
                    scaling = false;
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
                    bounds.Size = new Size((int)xChange, (int)yChange);
                    scaleTimeLeft -= e.Time;
                }
            }
            double tempscaleX = scaledToX(bounds.X);
            double tempscaleY = scaledToY(bounds.Y);
            double tempscaleW = scaledToX(bounds.Width);
            double tempscaleH = scaledToY(bounds.Height);
            GL.PushMatrix();
            GL.Translate(tempscaleX + (tempscaleW / 2), tempscaleY + (tempscaleH / 2), 0);
            GL.Rotate(rotation, 0, 0, 1);
            GL.Translate(-tempscaleX - (tempscaleW / 2), -tempscaleY - (tempscaleH / 2), 0);
            if (masked)
            {
                GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);
                if (Config.BoundTexture != maskID)
                {
                    GL.BindTexture(TextureTarget.Texture2D, maskID);
                    Config.BoundTexture = maskID;
                }
                GL.Begin(BeginMode.Quads);
                GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
                GL.TexCoord2(0, 0);
                GL.Vertex3(tempscaleX, tempscaleY, layer);
                GL.TexCoord2(1, 0);
                GL.Vertex3(tempscaleX + tempscaleW, tempscaleY, layer);
                GL.TexCoord2(1, 1);
                GL.Vertex3(tempscaleX + tempscaleW, tempscaleY + tempscaleH, layer);
                GL.TexCoord2(0, 1);
                GL.Vertex3(tempscaleX, tempscaleY + tempscaleH, layer);
                GL.End();
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            }
            if (textured)
            {
                if (Config.BoundTexture != textureID)
                {
                    GL.BindTexture(TextureTarget.Texture2D, textureID);
                    Config.BoundTexture = textureID;
                }
                GL.Begin(BeginMode.Quads);
                GL.Color4(colour);
                GL.TexCoord2(0, 0);
                GL.Vertex3(tempscaleX, tempscaleY, layer);
                GL.TexCoord2(1, 0);
                GL.Vertex3(tempscaleX + tempscaleW, tempscaleY, layer);
                GL.TexCoord2(1, 1);
                GL.Vertex3(tempscaleX + tempscaleW, tempscaleY + tempscaleH, layer);
                GL.TexCoord2(0, 1);
                GL.Vertex3(tempscaleX, tempscaleY + tempscaleH, layer);
                GL.End();
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
                GL.Begin(BeginMode.Quads);
                GL.Color4(colour);
                GL.Vertex3(tempscaleX, tempscaleY, layer);
                GL.Vertex3(tempscaleX + tempscaleW, tempscaleY, layer);
                GL.Vertex3(tempscaleX + tempscaleW, tempscaleY + tempscaleH, layer);
                GL.Vertex3(tempscaleX, tempscaleY + tempscaleH, layer);
                GL.End();
                GL.Enable(EnableCap.Texture2D);
            }
            if (masked)
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Basic premise behind scaling in pulse : set everything in terms of 1024x768; then scale appropriately with opengl
        /// </summary>
        /// <returns></returns>

        public static float scaleX()
        {
            return Config.ClientWidth / (float)(768f * Config.AspectRatio);
        }
        public static float ScaleY()
        {
            return Config.ClientHeight / 768f;
        }
        public static int scaledToX(int toscale)
        {
            return (int)Math.Ceiling(toscale * scaleX());
        }
        public static int scaledToY(int toscale)
        {
            return (int)Math.Ceiling((toscale * ScaleY()));
        }
    }
}
