using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Drawing2D;
using System.Linq;
namespace Pulse
{
    public class Text
    {
        private double layer;

        public double Layer
        {
            get { return layer; }
            set { layer = value; }
        }
        private bool fading, moving;

        public bool Fading
        {
            get { return fading; }
            set { fading = value; }
        }

        public bool Moving
        {
            get { return moving; }
            set { moving = value; }
        }
        float targetAlpha = 0.0f;
        double fadeTime, moveTime;
        double fadeTimeLeft, moveTimeLeft;
        double moveXTimeLeft, moveYTimeLeft;
        double moveXTime, moveYTime;
        int startX, startY, targetX, targetY;
        public bool yMoving;
        float startAlpha = 0.0f;
        Point startLocation;
        Point targetLocation;
        private Graphics gfx;
        private bool shadow = true;
        public bool Shadow
        {
            get { return shadow; }
            set { shadow = value; }
        }
        public static readonly Font defaultFont = new Font("Myriad Pro", 20);
        public Font textFont = new Font("Myriad Pro", 20);
        public Font TextFont
        {
            get { return textFont; }
            set
            {
                textFont = value;
                updatesize(Line);
            }
        }
        public Bitmap TextBitmap;
        private PointF position;
        public PointF Position
        {
            get { return position; }
            set { position = value; }
        }
        private string line;
        public string Line
        {
            get { return line; }
            set { line = value; Update(value); }
        }
        private Color colour;
        public Color Colour
        {
            get { return colour; }
            set { colour = value; alpha = colour.A; }
        }
        private float alpha = 1.0f;
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }
        private int _textureId;
        private Point location;
        public virtual Point Location
        {
            get { return location; }
            set { location = value; }
        }
        private Size textureSize;
        public Size TextureSize
        {
            get { return textureSize; }
            set { textureSize = value; TextBitmap = new Bitmap(TextBitmap, value); Update(line); }
        }

        public void changeSize(Size window)
        {
        }
        public virtual void Update(string newText)
        {

            line = newText;
            _textureId = CreateTexture();

        }

        public SizeF getStringSize()
        {
            Bitmap temp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(temp);
            SizeF temp2 = g.MeasureString(Line, textFont);

            return temp2;
        }
        public static SizeF getStringSize(String text, Font f)
        {
            using (Bitmap temp = new Bitmap(1, 1))
            {
                Graphics g = Graphics.FromImage(temp);
                SizeF temp2 = g.MeasureString(text, f);
                return temp2;
            }
        }
        public void autoResize(int width, int height)
        {
            int best = 5;
            for (int x = 1; x < 100; x++)
            {
                SizeF temp = getStringSize(line, new Font("Myriad Pro", x));
                if (temp.Width < width && temp.Height < height)
                {
                    best = x;
                }
                else
                {
                    break;
                }
            }
            textFont = new Font("Myriad Pro", best);
            Update(line);
        }
        public SizeF testSize(string s)
        {
            using (Bitmap temp = new Bitmap(1, 1))
            {
                Graphics g = Graphics.FromImage(temp);
                StringFormat format = StringFormat.GenericDefault;
                format.Trimming = StringTrimming.None;
                format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
                SizeF sz = new SizeF();
                foreach (Region r in g.MeasureCharacterRanges(s, textFont, new RectangleF(this.Location, this.TextBitmap.Size), format))
                {
                    sz += r.GetBounds(g).Size;
                }
                return sz;
            }
        }
        public static SizeF getStringSizePBox(String text, Font f)
        {
            using (Bitmap temp = new Bitmap(1, 1))
            {

                Graphics g = Graphics.FromImage(temp);

                StringFormat format = StringFormat.GenericDefault;
                format.Trimming = StringTrimming.None;
                format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

                SizeF temp2 = g.MeasureString(text, f, Int32.MaxValue, format); //unlimited width, measure trailing
                //StringFormat.GenericDefault.Trimming.
                //  g.MeasureString(text,f,5, StringFormat.GenericDefault.Trimming)
                //  Console.WriteLine(g.MeasureString(" ", f).Width);
                /*if (text.Length > 0)
                {
                    if (char.IsWhiteSpace(text[text.Length - 1]))
                    {
                        float counter = 0;
                        for (int i = text.Length - 1; i > 0; i--)
                        {
                            char j = text[i];
                            if (char.IsWhiteSpace(j))
                            {
                                //  counter += 6.6f;//g.MeasureString(" ", f).Width;
                            }
                            else
                            {
                                break; //nonespace character encountered
                            }
                        }
                        temp2.Width += counter;
                    }
                }*/

                return temp2;
            }
        }
        public void updatesize(string newText)
        {
            SizeF temp = getStringSize();
            if (temp.IsEmpty)
            {
                temp = new SizeF(1, 1);
            }
            TextureSize = new Size((int)temp.Width, (int)temp.Height);
        }
        public Text(Size a, Point p)
            : this(Config.ClientSize, a, p)
        {

        }

        public Text(Size ClientSize, Size areaSize, Point location)
        {
            if (areaSize.Width <= 1)
                areaSize.Width = 1;
            if (areaSize.Height <= 1)
                areaSize.Height = 1;
            TextBitmap = new Bitmap(areaSize.Width, areaSize.Height);
            gfx = Graphics.FromImage(TextBitmap);
            this.location = location;
            position = new PointF(0, 0);
            colour = Color.White;
            line = "";
            _textureId = CreateTexture();
        }
        public Dictionary<Pair<int, int>, Color> colors = new Dictionary<Pair<int, int>, Color>();
        public bool customcol;
        private int CreateTexture()
        {
            if (_textureId > 0)
                GL.DeleteTexture(_textureId);
            int textureId;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            BitmapData data = TextBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.Finish();
            TextBitmap.UnlockBits(data);
            if (line != null)
            {
                gfx = Graphics.FromImage(TextBitmap);
                gfx.Clear(Color.Transparent);
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                if (!customcol)
                {
                    gfx.DrawString(line, textFont, new SolidBrush(colour), new RectangleF(position.X, position.Y, getStringSize().Width + 10, getStringSize().Height));
                }
                else
                {
                    //try //disabled for testing
                    {
                        float pos = 0;
                        //SortedDictionary<Pair<int, int>, Color> sd = new SortedDictionary<Pair<int, int>, Color>(new PairComparer());
                        var sortedDict = (from entry in colors orderby entry.Key.key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value); //just in case, thanks http://stackoverflow.com/questions/289/how-do-you-sort-a-c-sharp-dictionary-by-value
                        //Console.WriteLine();
                        foreach (var i in sortedDict)
                        {
                            //     Console.WriteLine(i.Key.key + " " + i.Key.value + " is the pair");
                            string portion = line.Substring(i.Key.key, i.Key.value - i.Key.key);
                            gfx.DrawString(portion, textFont, new SolidBrush(i.Value), new RectangleF(pos, position.Y, getStringSize().Width + 10, getStringSize().Height));
                            pos += Text.getStringSizePBox(portion, textFont).Width - 7.5f; //nfi why 7.5 needed

                        }
                    }
                    /*  catch
                      {
                          Console.WriteLine("Coloring failed resorting to default string was : " + line);
                          gfx.Clear(Color.Transparent);
                          gfx.DrawString(line, textFont, new SolidBrush(colour), new RectangleF(position.X, position.Y, getStringSize().Width + 10, getStringSize().Height));
                      }*/
                }
                System.Drawing.Imaging.BitmapData data2 = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data2.Scan0);
                TextBitmap.UnlockBits(data2);
                gfx.Dispose();
            }
            return textureId;
        }
        /*
        private int CreateTexture()
        {
            if (_textureId > 0)
                GL.DeleteTexture(_textureId);
            int textureId;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            BitmapData data = TextBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.Finish();
            TextBitmap.UnlockBits(data);
            if (!line.Equals(""))
            {
                gfx = Graphics.FromImage(TextBitmap);
                gfx.Clear(Color.Transparent);
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                GraphicsPath gp = new GraphicsPath();
              
                gp.AddString(line, textFont.FontFamily, 0, (float)textFont.Size * 1.4f, new Point(0, 0), StringFormat.GenericDefault);
    
                gfx.FillPath(Brushes.White, gp);
                if(shadow) {
                    gfx.DrawPath(new Pen(Color.Black, .1f), gp);
                    }
               gp.Dispose();           
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
               // gfx.DrawString(line, textFont, new SolidBrush(colour), position);
                System.Drawing.Imaging.BitmapData data2 = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data2.Scan0);
                TextBitmap.UnlockBits(data2);
                gfx.Dispose();
            }
            return textureId;
        }
        */
        public void Dispose()
        {
            if (_textureId > 0)
                GL.DeleteTexture(_textureId);
        }
        public virtual void fade(float alpha, double timeSpan)
        {
            targetAlpha = alpha;
            fadeTime = timeSpan;

            startAlpha = this.Alpha; //had to use this.Alpha because this.Colour is normal system.drawing color meaning alpha is at 255
            fadeTimeLeft = timeSpan;
            fading = true;
        }
        public void move(Point p, double timeSpan)
        {
            targetLocation = p;
            moveTime = timeSpan;
            moveTimeLeft = timeSpan;
            startLocation = Location;
            moving = true;
        }
        public bool xMoving;
        /*
        public void scale(Size s, double timeSpan)
        {
            targetSize = s;
            scaleTime = timeSpan;
            scaleTimeLeft = timeSpan;
            startSize = bounds.Size;
            scaling = true;
        }*/
        public void moveX(int newX, double timeSpan)
        {
            targetX = newX;
            moveXTime = timeSpan;
            moveXTimeLeft = timeSpan;
            startX = Location.X;
            xMoving = true;
        }
        public void moveY(int newY, double timeSpan)
        {
            targetY = newY;
            moveYTime = timeSpan;
            moveYTimeLeft = timeSpan;
            startY = Location.Y;
            yMoving = true;
        }
        public virtual void OnRenderFrame(FrameEventArgs e)
        {

            if (fading)
            {
                if (fadeTimeLeft <= 0)
                {
                    alpha = targetAlpha;
                    fading = false;
                }
                else
                {
                    double progress = (fadeTimeLeft / fadeTime) * 100;
                    progress = 100 - progress;
                    float difference = startAlpha - targetAlpha;
                    float change = (difference / 100) * (float)progress;
                    change = startAlpha - change;
                    alpha = change;
                    fadeTimeLeft -= e.Time;
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
            }
            if (xMoving)
            {
                if (moveXTimeLeft <= 0)
                {
                    Location = new Point(targetX, Location.Y);
                    xMoving = false;
                }
                else
                {
                    double progress = (moveXTimeLeft / moveXTime) * 100;
                    progress = 100 - progress;
                    double xDiff = startX - targetX;
                    double xChange = (xDiff / 100) * progress;
                    xChange = startX - xChange;
                    Location = new Point((int)xChange, Location.Y);
                    moveXTimeLeft -= e.Time;
                }
            }
            if (yMoving)
            {
                if (moveYTimeLeft <= 0)
                {
                    Location = new Point(Location.X, targetY);
                    yMoving = false;
                }
                else
                {
                    double progress = (moveYTimeLeft / moveYTime) * 100;
                    progress = 100 - progress;
                    double yDiff = startY - targetY;
                    double yChange = (yDiff / 100) * progress;
                    yChange = startY - yChange;
                    Location = new Point(Location.X, (int)yChange);
                    moveYTimeLeft -= e.Time;
                }
            }/*
            if (scaling)
            {
                if (scaleTimeLeft <= 0)
                {
                    Size = targetSize;
                }
                double progress = (scaleTimeLeft / scaleTime) * 100;
                progress = 100 - progress;
                double xDiff = startSize.Width - targetSize.Width;
                double xChange = (xDiff / 100) * progress;
                double yDiff = startSize.Height - targetSize.Height;
                double yChange = (yDiff / 100) * progress;
                xChange = startSize.Width - xChange;
                yChange = startSize.Height - yChange;
                TextBitmap.Size = new Size((int)xChange, (int)yChange);
                scaleTimeLeft -= e.Time;
            }*/
            if (Config.BoundTexture != _textureId)
            {
                GL.BindTexture(TextureTarget.Texture2D, _textureId);
                Config.BoundTexture = _textureId;
            }

            double tempscaleX = Rect.scaledToX(location.X);
            double tempscaleY = Rect.scaledToY(location.Y);
            double tempscaleW = Rect.scaledToX(TextBitmap.Width);
            double tempscaleH = Rect.scaledToY(TextBitmap.Height);
            GL.LineWidth(2f);
            if (shadow)
            {
                GL.Color4(0.0, 0.0, 0.0, alpha);
                GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0, 0);
                GL.Vertex3(tempscaleX + 1, tempscaleY + 1, layer - 0.1);
                GL.TexCoord2(1, 0);
                GL.Vertex3(tempscaleX + 1 + tempscaleW, tempscaleY + 1, layer - 0.1);
                GL.TexCoord2(1, 1);
                GL.Vertex3(tempscaleX + 1 + tempscaleW, tempscaleY + 1 + tempscaleH, layer - 0.1);
                GL.TexCoord2(0, 1);
                GL.Vertex3(tempscaleX + 1, tempscaleY + 1 + tempscaleH, layer - 0.1);
                GL.End();
            }
            GL.Color4(colour.R, colour.G, colour.B, alpha);
            GL.Begin(BeginMode.Quads);
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
    }
}