using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pulse.UI
{
    class Panel : Control
    {
        public Panel(Game game, Rectangle bounds)
            : base(game, bounds)
        {
            texture = new Rect(bounds);
            texture.Color = OpenTK.Graphics.Color4.GreenYellow;
        }
        List<InterfaceComponent> components = new List<InterfaceComponent>();
        public override void OnLoad(EventArgs e)
        {

        }

        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            foreach (InterfaceComponent i in components)
            {
                if (i.Enabled)
                    i.OnUpdateFrame(e);
            }
        }

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            texture.OnRenderFrame(e);
            foreach (InterfaceComponent i in components)
            {
                if (i.Visible)
                    i.OnRenderFrame(e);
            }
        }
        
    }
}
