using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using Pulse.Client;
using System.IO;
using System.Net;
using System.Threading;

namespace Pulse.UI
{
    public class UserDisplay
    {
        Rect back = new Rect(new Rectangle(0, 0, 350, 104));
        Rect front = new Rect(new Rectangle(0, 0, 298, 128));
        Rect avatar;
        Text name;
        Text playCount;
        WebClient wc;
        bool done = false;
        User u;
        Button b;
        Button b2;
        public UserDisplay(User u, Point location)
        {
            this.u = u;
            back.Bounds = new Rectangle(location, new Size(back.Bounds.Width, back.Bounds.Height));
            back.Layer = 9.3;
            front.Bounds = new Rectangle(location.X + 2, location.Y + 2, front.Bounds.Width, front.Bounds.Height);
            front.Layer = 9.4;
            back.Color = new Color4(0.6f, 0.6f, 0.6f, 0.9f);
            front.Color = new Color4(0.6f, 0.6f, 0.6f, 0.8f);
            Thread t = new Thread(new ParameterizedThreadStart(downloadThread));
            wc = new WebClient();
            t.IsBackground = true;
            t.Start(u.Avatar);
            name = new Text(new Size(118, 200), new Point(location.X + 90, location.Y + 2));
            name.Layer = 9.5;
            name.Shadow = false;
            name.Update(u.RealName);
            playCount = new Text(new Size(300, 300), new Point(location.X + 90, location.Y + 35));
            playCount.Layer = 9.5;
            playCount.Shadow = false;
            playCount.textFont = new Font("Myriad Pro", 14);
            playCount.Line = ("PC: " + u.Playcount + "\nScore: " + u.TotalScore + "\nLevel: " + u.Level);
            b = new Button(Game.game, new Rectangle(playCount.Location, new Size(100, 30)), "PM", delegate(int e)
            {
                Game.pbox.addTab("pulse|" + u.RealName);
            });
            b.Layer = 9.5;
            b2 = new Button(Game.game, new Rectangle(playCount.Location, new Size(100, 30)), "Spec", delegate(int e)
            {
                if (!Config.Spectating)
                {
                    if (!u.Name.ToLower().Equals(Account.currentAccount.AccountName.ToLower()))
                    {
                        Client.PacketWriter.sendSpectateHook(Game.conn.Bw, u.Name);
                        Game.pbox.addLine("Spectating " + u.Name + " (if they are online)");
                    }
                    else
                    {
                        Game.pbox.addLine("You can't spectate yourself");
                    }
                }
                else
                {
                    if (!Config.SpectatedUser.Equals(""))
                    {
                        Game.pbox.addLine("Canceling spectate on " + Config.SpectatedUser);
                    }
                    Client.PacketWriter.sendSpectateCancel(Game.conn.Bw, Config.SpectatedUser);
                    Config.SpectatedUser = "";
                    Config.Spectating = false;
                }
            });
            b2.Layer = 9.5;
            if (u.avatarRect != null)
            {
                avatar = u.avatarRect;
            }
        }
        bool enabled = false;
        bool canpress = false;
        void downloadThread(object avatar)
        {
            if (u.downloadAvatar)
            {
                string av = (string)avatar;
                wc.Proxy = null;
                wc.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
                try
                {
                    string dlstring = av.Equals("") ? "http://p.ulse.net/generic_avatar.jpg" : "http://p.ulse.net/forum/download/file.php?avatar=" + av;
                    Console.WriteLine(dlstring);
                    wc.DownloadFile(dlstring, Path.GetTempPath() + u.Name + "av.jpg");
                    done = true;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                u.downloadAvatar = false;
            }
           
        }
        bool set = false;
        bool set1 = false;
        
        public void update(FrameEventArgs e)
        {
            if (enabled)
            {
                b.OnUpdateFrame(e);
                b2.OnUpdateFrame(e);
            }
            if (avatar == null && done)
            {
                avatar = new Rect(new Rectangle(back.Bounds.X + 2, back.Bounds.Y+2, 100, 100), Path.GetTempPath() + u.Name + "av.jpg", false, false);
                avatar.Alpha = 0;
                avatar.fade(1, .3);
                avatar.Layer = 9.5;
                u.avatarRect = avatar;
                done = false;
            }
            if (Utils.contains(back.ModifiedBounds) && Game.MouseState.LeftButton && canpress)
            {
                enabled = !enabled;
                if (enabled)
                {
                    playCount.Alpha = 0;
                }
                else
                {
                    playCount.Alpha = 1;
                }
                canpress = false;
                Game.lClickFrame = true;
            }
            else
            {
                if (!Game.MouseState.LeftButton)
                {
                    canpress = true;
                }
            }
            if (Utils.contains(back.ModifiedBounds) && !set)
            {
                playCount.Line = "Accuracy: " + u.Accuracy.ToString("N2")+"%";//("Song: " + u.CurrentSong + "\n" + u.CurrentChart);
                set = true;
                set1 = true;
            } else {
                if (set1 && !Utils.contains(back.ModifiedBounds))
                {
                    playCount.Line = ("PC: " + u.Playcount + "\nScore: " + u.TotalScore + "\nLevel: " + u.Level);
                    set1 = false;
                    set = false;
                }
                    

            }
        }
        public void draw(FrameEventArgs e)
        {

            back.OnRenderFrame(e);
          //  front.draw(e);
            if (avatar != null)
            {
                avatar.OnRenderFrame(e);
            }
            name.OnRenderFrame(e);
            playCount.OnRenderFrame(e);

            if (enabled)
            {
                b.OnRenderFrame(e);
                b2.OnRenderFrame(e);
            }
        }
        public void move(Point loc, double time)
        {
            if (back != null)
                back.move(loc, time);
            if (front != null)
                front.move(new Point(loc.X + 2, loc.Y + 2), time);
            if (avatar != null)
                avatar.move(new Point(loc.X + 2, loc.Y +2), time);
            if (name != null)
                name.move(new Point(loc.X + 110, loc.Y + 2), time);
            if (playCount != null)
                playCount.move(new Point(loc.X + 110, loc.Y + 30), time);
            b.move(new Point(loc.X + 110, loc.Y + 30), time);
            b2.move(new Point(loc.X + 110, loc.Y + 65), time);
        }
    }
}
