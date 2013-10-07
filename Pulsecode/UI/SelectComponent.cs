using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Un4seen.Bass.Misc;

namespace Pulse.UI
{
    class SelectComponent : Control
    {
        public Text lineTwo;
        public string line;
        public SelectComponent(Game game, Rectangle bounds, string text, string line2)
            : base(game, bounds, text)
        {

            line = line2;
            lineTwo = new Text(Config.ClientSize, new Size(bounds.Width, bounds.Height), new Point(bounds.X, bounds.Y));
            lineTwo.Update(line);
            OnLoad(null);
            baseX = bounds.X;
        }
        public void move(Point p, double span)
        {
            Texture.move(p, span);
            TextTexture.move(new Point(p.X + 50, p.Y), span);
            lineTwo.move(new Point(p.X + 50, p.Y + 30), span);
            Bounds = new Rectangle(p.X, p.Y, Bounds.Width, Bounds.Height);
        }
        public void moveX(Point p, double span)
        {
            Texture.moveX(p.X, span);
            TextTexture.moveX(p.X + 50, span);
            lineTwo.moveX(p.X + 50, span);
            Bounds = new Rectangle(p.X, p.Y, Bounds.Width, Bounds.Height);
        }
        public void moveY(Point p, double span)
        {
            Texture.moveY(p.Y, span);
            TextTexture.moveY(p.Y, span);
            lineTwo.moveY(p.Y + 30, span);
            Bounds = new Rectangle(p.X, p.Y, Bounds.Width, Bounds.Height);
        }
        public bool selected;
        public new Rectangle Bounds
        {
            get
            {
                return bounds;
            }
            set
            {
                bounds = value;
                texture.Bounds = bounds;
                this.textTexture.Location = new Point(value.X + 50, value.Y);
                this.lineTwo.Location = new Point(value.X + 50, value.Y + 30);
            }
        }
        public Color Colour
        {
            set
            {
                texture.Color = new OpenTK.Graphics.Color4(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, 1.0f);
            }
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            texture = new Rect(bounds, Skin.skindict["selecttexture"]);
            TextTexture.Location = new Point(bounds.X + 50, bounds.Y + 10);
            lineTwo.Location = new Point(bounds.X + 50, bounds.Y + 40);
            textTexture.Shadow = true;
            lineTwo.Shadow = true;
        }
        Bitmap visualizer;
        public event Action<int, SelectComponent> clickEvent;
        public bool canpress = false;
        public int index;
        int baseX;
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);            
            if (this.texture.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)) && Game.MouseState.LeftButton && canpress && game.Focused && !Game.lClickFrame)
            {
                Console.WriteLine("2");
                if (clickEvent != null && canpress)
                {
                    //  Console.WriteLine("invoke");
                    clickEvent.Invoke(index, this);
                }
                canpress = false; 
            }

            if (this.texture.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)))
            {
                this.Colour = Color.FromArgb(255, 255, 165, 0);
                if (!this.texture.xMoving && baseX == this.Bounds.X)
                {
                    this.moveX(new Point(Bounds.X + 50, Bounds.Y), .2);
                }

            }
            else
            {
                if (!this.texture.xMoving && baseX == this.Bounds.X - 50)
                {
                    this.moveX(new Point(Bounds.X - 50, Bounds.Y), .2);
                }
                if (!selected)
                {
                    this.Colour = Color.Purple;
                }
                else
                {
                    this.Colour = Color.OrangeRed;
                }
            }
            if (!Game.MouseState.LeftButton && !Game.lClickFrame)// && this.texture.ModifiedBounds.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)))
            {
                canpress = true;
            }
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            texture.OnRenderFrame(e);
            TextTexture.OnRenderFrame(e);
            lineTwo.OnRenderFrame(e);
        }
    }
}
