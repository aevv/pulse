using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Pulse;
namespace Pulse.UI
{
    class UserList : Control
    {
        public string message = "";
        Text txt;
        Rect bg;
        int sr;
        public UserList(Game game, Rectangle bounds, string text)
            : base(game, bounds, text)
        {
            bg = new Rect(bounds);
            bg.Layer = 9;
            bg.Color = new OpenTK.Graphics.Color4(0, 0, 0, .5f);
            message = text;
            txt = new Text(Config.ClientSize, new Size((int)Pulse.Text.getStringSize(text, Pulse.Text.defaultFont).Width + bounds.Size.Width, bounds.Size.Height), new Point(bounds.X + bounds.Width, bounds.Y));
            txt.Layer = 9.2;
            txt.Line = message;
            txt.Shadow = false;
        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {

            base.OnUpdateFrame(e);
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {

            bg.OnRenderFrame(e);
            txt.OnRenderFrame(e);

            base.OnRenderFrame(e);

        }
    }
}
