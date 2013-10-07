using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Screens
{
    class UserDisplayScreen : Screen
    {
        public UserDisplayScreen(Game game)
            : base(game, "users")
        {
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            int count = 0;
            int ycount = 0;
            lock (Client.User.users)
            {
                foreach (var pair in Client.User.users)
                {
                    if (pair.Value.UpdateGraphics)
                    {
                        pair.Value.Disp = new UI.UserDisplay(pair.Value, new System.Drawing.Point(0, 0));
                        pair.Value.UpdateGraphics = false;
                    }
                    if (pair.Value != null && pair.Value.Disp != null)
                    {
                        pair.Value.Disp.update(e);
                    }
                    pair.Value.Disp.move(new System.Drawing.Point(count * 350, ycount * 104), 0);
                    count++;
                    if (count * 350  + 350 > Config.ResWidth)
                    {
                        ycount++;
                        count = 0;
                    }
                }
            }
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            lock (Client.User.users)
            {
                foreach (var pair in Client.User.users)
                {
                    if (pair.Value != null && pair.Value.Disp != null)
                    {
                        pair.Value.Disp.draw(e);
                    }
                }
            }
        }
    }
}
