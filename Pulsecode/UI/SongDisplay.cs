using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Mechanics;
using System.Drawing;

namespace Pulse.UI
{
    /// <summary>
    /// UI Component for Song Selection screen, displaying each song as a bar in the scroller
    /// </summary>
    public class SongDisplay : Control
    {
        new public double Layer
        {
            set
            {
                base.Layer = value;
            }
        }

        public bool Selected
        {
            set;
            get;
        }

        public SongInfo SongInfo;

        public SongDisplay(Game game, SongInfo song, Rectangle bounds)
            : base(game, bounds, song.Artist + " - " + song.SongName)
        {
            this.SongInfo = song;
            Texture = new Rect(bounds, Skin.skindict["songdisplay"]);
            textTexture.Location = new Point(textTexture.Location.X, textTexture.Location.Y + bounds.Height / 4 + 4);
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            texture.OnRenderFrame(e);
            textTexture.OnRenderFrame(e);
        }
    }
}
