using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Input;
using System.Diagnostics;

namespace Pulse.UI
{
    class Dragbar : Control
    {
        new public double Layer
        {
            set
            {
                layer = value;
                if (texture != null)
                    texture.Layer = value;
                if (textTexture != null)
                    textTexture.Layer = value;
                if (scrollTexture != null)
                    scrollTexture.Layer = value + 0.2;
            }
        }
        public dragged del;
        public int Length;
        private bool vertical = true;
        public bool Vertical
        {
            get { return vertical; }
            set { vertical = value; }
        }
        public Dragbar(Game game, Point pos, int length, bool vert, dragged d)
            : base(game, new Rectangle(pos.X, pos.Y, 13, length))
        {
            OnLoad(null);
            vertical = vert;
            if (!vertical)
            {
                bounds = new Rectangle(Bounds.Location, new Size(length, 13));
                texture = new Rect(new Rectangle(bounds.X, bounds.Y + 8, bounds.Width, bounds.Height), Skin.skindict["hbar"]);
            }
            else
            {
                texture = new Rect(new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), Skin.skindict["bar"]);
            }
            del = d;
            this.Length = length;
        }
        Rect scrollTexture;

        public Rect ScrollTexture
        {
            get { return scrollTexture; }
            set { scrollTexture = value; }
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //texture = new Rect(bounds, Skin.skindict["bar"]);
            if (vertical)
            {
                scrollTexture = new Rect(new Rectangle(bounds.X - 8, bounds.Y, 30, 30), Skin.skindict["scroll"]);
            }
            else
            {
                scrollTexture = new Rect(new Rectangle(bounds.X - 8, bounds.Y - 15, 30, 30), Skin.skindict["scroll"]);
            }
        }
        Point? dragStart;
        bool toDrag;
        int realY;
        public void setPos(int pos)
        {
            if (vertical)
            {
                scrollTexture.Bounds = new Rectangle(scrollTexture.Bounds.X, pos, 30, 30);
            }
            else
            {
                scrollTexture.Bounds = new Rectangle(pos, scrollTexture.Bounds.Y, 30, 30);
            }
        }
        public double getPercentScrolled()
        {
            double temp = 0.0;
            if (vertical)
            {
                temp = ((double)(scrollTexture.Bounds.Y - texture.Bounds.Y) / Length) * 100;
            }
            else
            {
                temp = ((double)(scrollTexture.Bounds.X - texture.Bounds.X) / Length) * 100;
            }
            temp = Math.Round(temp, 2);
            return temp;
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            if (this.texture.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)) && Game.MouseState.LeftButton && game.Focused)
            {
                toDrag = true;
                if (vertical)
                {
                    dragStart = new Point(Game.MouseState.X, Game.MouseState.Y - texture.Bounds.Y);
                }
                else
                {
                    dragStart = new Point(Game.MouseState.X - texture.Bounds.X, Game.MouseState.Y);
                }
            }
            if (toDrag)
            {
                if (vertical)
                {
                    int diff = Game.MouseState.Y - scrollTexture.Bounds.Y - (scrollTexture.ModifiedBounds.Height / 2) + texture.Bounds.Y;
                    float percentToMove = (float)(diff) / Length;
                    float currentPercent = (float)(scrollTexture.Bounds.Y - texture.Bounds.Y) / Length;
                    if (currentPercent < 0.0f)
                    {
                        scrollTexture.Bounds = new Rectangle(scrollTexture.Bounds.X, texture.ModifiedBounds.Y, 30, 30);
                    }
                    else if (currentPercent > 1.0f)
                    {
                        scrollTexture.Bounds = new Rectangle(scrollTexture.Bounds.X, texture.ModifiedBounds.Height + texture.ModifiedBounds.Y, 30, 30);
                    }
                    if ((int)((currentPercent + percentToMove) * Length * (1 / Rect.ScaleY())) > texture.Bounds.Y &&
                        (int)((currentPercent + percentToMove) * Length * (1 / Rect.ScaleY())) < texture.Bounds.Y + texture.Bounds.Height)
                    {
                        scrollTexture.Bounds = new Rectangle(scrollTexture.Bounds.X, (int)((currentPercent + percentToMove) * Length * (1 / Rect.ScaleY())), 30, 30);
                    }
                    del(diff);
                }
                else
                {
                    int diff = Game.MouseState.X - scrollTexture.Bounds.X - (scrollTexture.ModifiedBounds.Width / 2) + texture.Bounds.X;
                    float percentToMove = (float)(diff) / Length;
                    float currentPercent = (float)(scrollTexture.Bounds.X - texture.Bounds.X) / Length;
                    if (currentPercent < 0.0f)
                    {
                        scrollTexture.Bounds = new Rectangle(texture.ModifiedBounds.X, scrollTexture.Bounds.Y, 30, 30);
                    }
                    else if (currentPercent > 1.0f)
                    {
                        scrollTexture.Bounds = new Rectangle(texture.ModifiedBounds.Width + texture.ModifiedBounds.X, scrollTexture.Bounds.Y, 30, 30);
                    }
                    if ((int)((currentPercent + percentToMove) * Length * (1 / Rect.scaleX())) > texture.Bounds.X &&
                        (int)((currentPercent + percentToMove) * Length * (1 / Rect.scaleX())) < texture.Bounds.X + texture.Bounds.Width)
                    {
                        scrollTexture.Bounds = new Rectangle((int)((currentPercent + percentToMove) * Length * (1 / Rect.scaleX())), scrollTexture.Bounds.Y, 30, 30);
                    }
                    del(diff);
                }
            }
            if (!Game.MouseState.LeftButton)
            {
                toDrag = false;
            }
            texture.OnRenderFrame(e);
            scrollTexture.OnRenderFrame(e);
        }
        public delegate void dragged(int delta);
    }
}
