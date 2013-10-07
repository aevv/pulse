using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pulse.UI
{
    public class Control : InterfaceComponent
    {
        protected double layer;

        public double Layer
        {
            get { return layer; }
            set
            {
                layer = value;
                if (texture != null)
                    texture.Layer = value;
                if (textTexture != null)
                    textTexture.Layer = value;
            }
        }
        int otherData = 0;

        public int OtherData
        {
            get { return otherData; }
            set { otherData = value; }
        }
        protected Game game;

        protected Rect texture;
        public Rect Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        protected Text textTexture;

        public Text TextTexture
        {
            get { return textTexture; }
            set { textTexture = value; }
        }

        protected string text;
        private void updatesize(string newText)
        {
            SizeF temp = TextTexture.getStringSize();
            if (temp.IsEmpty)
            {
                temp = new SizeF(1, 1);
            }
            TextTexture.TextureSize = new Size((int)temp.Width, (int)temp.Height);
        }
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                if (textTexture != null)
                {
                    textTexture.Update(value);
                    updatesize(value);
                }
            }
        }

        protected Rectangle bounds;

        public Rectangle Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public Control(Game game)
        {
            this.game = game;
            textTexture = new Text(game.ClientSize, new Size(0, 0), new Point(0, 0));
            textTexture.Update("");
            this.text = "";
        }

        public Control(Game game, string text)
        {
            this.game = game;
            textTexture = new Text(game.ClientSize, new Size(0, 0), new Point(0, 0));
            textTexture.Update(text);
            this.text = text;
        }

        public Control(Game game, Rectangle bounds)
        {
            this.game = game;
            this.bounds = bounds;
            textTexture = new Text(game.ClientSize, new Size(bounds.Width, bounds.Height), new Point(bounds.X, bounds.Y));
            textTexture.Update("");
            this.text = "";
        }

        public Control(Game game, Rectangle bounds, string text)
        {
            this.game = game;
            this.bounds = bounds;
            textTexture = new Text(game.ClientSize, new Size(bounds.Width, bounds.Height), new Point(bounds.X, bounds.Y));
            textTexture.Update(text);
            this.Text = text;
        }
        protected bool intersects(Point p)
        {
            return this.bounds.Contains(p);
        }
        public override void OnLoad(EventArgs e)
        {

        }

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {

        }

        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {

        }
    }
}
