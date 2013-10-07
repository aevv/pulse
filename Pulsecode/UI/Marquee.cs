using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Pulse;
namespace Pulse.UI
{
    class Marquee : Control
    {
        new public double Layer
        {
            set
            {
                layer = value;
                if (bg != null)
                    bg.Layer = value;
                if (txt != null)
                    txt.Layer = value + 0.2;
            }
        }
        public string message = "";
        Text txt;
        Rect bg;
        int sr;
        public Marquee(Game game, Rectangle bounds, string text, int scrollrate)
            : base(game, bounds, text)
        {
            bg = new Rect(bounds);
            bg.Color = new OpenTK.Graphics.Color4(0, 0, 0, .5f);
            message = text;
            txt = new Text(Config.ClientSize, new Size((int)Pulse.Text.getStringSize(text, Pulse.Text.defaultFont).Width + bounds.Size.Width, bounds.Size.Height), new Point(bounds.X + bounds.Width, bounds.Y));
            txt.Line = message;
             sr = scrollrate;
            txt.Shadow = false;
        }
        double counter; //retarded
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            counter+=(e.Time * sr);
            if (counter >= 1)
            {
                txt.Location = new Point(txt.Location.X - (int) counter, txt.Location.Y);
                if (txt.Location.X + txt.getStringSize().Width < bg.Bounds.X)
                {
                    txt.Location = new Point(bg.Bounds.X + bg.Bounds.Width, txt.Location.Y);
                }

                counter = counter % 1;
            }
          /*  if(!txt.Moving)
            txt.move(new Point(txt.Location.X - sr, txt.Location.Y), 1);
            if (txt.Location.X + txt.getStringSize().Width < bg.Bounds.X)
            {
                txt.Location = new Point(bg.Bounds.X + bg.Bounds.Width, txt.Location.Y);
            }*/
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
