using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Input;
using System.Diagnostics;
using Un4seen.Bass.Misc;
using OpenTK.Graphics.OpenGL;
namespace Pulse.UI
{
    class Button : Control
    {
        Rect end1, end2;
        new public double Layer
        {
            set
            {
                layer = value;
                if (texture != null)
                    texture.Layer = value;
                if (textTexture != null)
                    textTexture.Layer = value;
                if (stretchable)
                {
                    if (end1 != null) end1.Layer = value;
                    if (end2 != null) end2.Layer = value;
                }
            }
        }
        public bool manualColour = false;
        private bool stretchable = false;
        public bool Stretchable
        {
            get { return stretchable; }
        }
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
                this.textTexture.Location = new Point(value.X, value.Y);
            }
        }
        public Color Color
        {
            set
            {
                texture.Color = new OpenTK.Graphics.Color4(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, 1.0f);
                if (stretchable)
                {
                    end1.Color = new OpenTK.Graphics.Color4(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, 1.0f);
                    end2.Color = new OpenTK.Graphics.Color4(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, 1.0f);
                }
            }
        }
        public Button(Game game, Rectangle bounds, string text, clicked c)
            : base(game, bounds, text)
        {
            OnLoad(null);
            del = c;
        }
        public Button(Game game, Rectangle bounds, string text, clicked c, bool stretch, bool custom, Color col)
            : this(game, bounds, text, c)
        {
            if (stretchable = stretch) //rofl n1 matt
            {
                end1 = new Rect(new Rectangle(bounds.X, bounds.Y, 65, bounds.Height), Skin.skindict["buttonLeft"]);
                end2 = new Rect(new Rectangle(bounds.X + bounds.Width - 63, bounds.Y, 66, bounds.Height), Skin.skindict["buttonRight"]);
                texture = new Rect(new Rectangle(bounds.X + 65, bounds.Y, bounds.Width - 128, bounds.Height), Skin.skindict["buttonMid"]);
                TextTexture.Location = new Point((bounds.X + bounds.Width / 2) - ((int)Pulse.Text.getStringSize(text, textTexture.textFont).Width / 2), bounds.Y + (bounds.Height / 2) - 20);
            }
            customcol = custom;
            this.custom = col;
        }
        private string customTexturePath = "";
        public Button(Game game, Rectangle bounds, string text, clicked c, string customTexture)
            : this(game, bounds, text, c)
        {
            customTexturePath = customTexture;
            texture = new Rect(bounds, customTexturePath);
        }
        public clicked del;
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            texture = new Rect(bounds, Skin.skindict["menuButton"]);
            TextTexture.Location = new Point((bounds.X + bounds.Width / 2) - ((int)Pulse.Text.getStringSize(text, textTexture.textFont).Width / 2), bounds.Y + (bounds.Height / 2) - 20);
            textTexture.Shadow = true;
        }
        public bool canpress = false;
        bool customcol;
        Color custom;
        int subamt = 30;
    //    Color customtemp;
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);  
            texture.OnRenderFrame(e);
            TextTexture.OnRenderFrame(e);     
            if (stretchable)
            {
                end1.OnRenderFrame(e);
                end2.OnRenderFrame(e);
            }
        }
        public virtual void onpress()
        {
            del(OtherData);
            canpress = false;
        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Rectangle clickBounds;
            bool lc = Game.MouseState.LeftButton;
            if (!stretchable)
            {
                clickBounds = texture.ModifiedBounds;
            }
            else
            {
                clickBounds = new Rectangle(end1.ModifiedBounds.X, end1.ModifiedBounds.Y, end2.ModifiedBounds.X + end2.ModifiedBounds.Width - end1.ModifiedBounds.X, end1.ModifiedBounds.Height);
            }
            if (clickBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)) && lc && canpress && game.Focused && !Game.lClickFrame)
            {
                onpress();
                Game.lClickFrame = true;
            }

            if (clickBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)))
            {
                if (customcol)
                {
                    this.Color = Color.FromArgb(custom.A, (custom.R - subamt > 0 ? custom.R - subamt : 0), (custom.G - subamt > 0 ? custom.G - subamt : 0), (custom.B - subamt > 0 ? custom.B - subamt : 0));
                }
                else
                {
                    if (!manualColour)
                    {
                        this.Color = Color.FromArgb((int)(texture.Color.A * 255), 255, 165, 0);
                    }
                }
            }
            else
            {
                if (customcol)
                {
                    this.Color = custom;
                }
                else
                {
                    if (!manualColour)
                    {
                        this.Color = Color.FromArgb((int)(texture.Color.A * 255), 70, 130, 180);
                    }
                }
            }
            if (!lc)
            {
                canpress = true;
            }
            else
            {
                canpress = false;
            }
        }
        public void move(Point p, double time)
        {
            texture.move(p, time);
            textTexture.move(p, time);
            if (stretchable)
            {
                end1.move(p, time);
                end2.move(p, time);
            }
        }
        public void fade(float alpha, double time)
        {
            texture.fade(alpha, time);
            textTexture.fade(alpha, time);
            if (stretchable)
            {
                end1.fade(alpha, time);
                end2.fade(alpha, time);
            }
        }
        public delegate void clicked(int data);
    }
}
