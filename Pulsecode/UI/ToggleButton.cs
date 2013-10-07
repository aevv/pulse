using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Input;
using System.Diagnostics;

namespace Pulse.UI
{
    class ToggleButton : Button
    {
        public Boolean Selected
        {
            get;
            set;
        }
        public ToggleButton(Game game, Rectangle bounds, string text, clicked c)
            : base(game, bounds, text, c)
        {
            OnLoad(null);
            del = c;
            Selected = false;
        }
        public override void onpress()
        {
            toggle();
            base.onpress();
        }
        public void toggle()
        {
            Selected = !Selected;
            if (Selected)
            {
                manualColour = true;
                texture.Color = new OpenTK.Graphics.Color4(34 / 255.0f, 139 / 255.0f, 34 / 255.0f, 1.0f);
            }
            else
            {
                texture.Color = new OpenTK.Graphics.Color4(Color.White.R, Color.White.G, Color.White.B, 1.0f);
                manualColour = false;
            }
        }
    }
}
