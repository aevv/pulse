using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pulse.UI
{
    class Label : Control
    {
        public Label(Game game, Rectangle bounds, string text)
            : base(game, bounds, text)
        {
            OnLoad(null);
        }
        public Label(Game game, Point location, string text)
            : base(game, text)
        {
            bounds = new Rectangle(location, new Size(0, 0));
            OnLoad(null);
            SizeF temp = TextTexture.getStringSize();
            if(temp.IsEmpty) {
            temp = new SizeF(1, 1);
                }
            TextTexture.TextureSize = new Size((int)temp.Width, (int)temp.Height);
            bounds = new Rectangle(location, new Size((int)temp.Width, (int)temp.Height));
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TextTexture.Location = new Point(bounds.X, bounds.Y);
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            TextTexture.OnRenderFrame(e);
        }
    }
}
