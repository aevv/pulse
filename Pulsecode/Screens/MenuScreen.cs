using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using OpenTK.Input;
using System.Net;
using Pulse.UI;
using Pulse.Mechanics;
using Pulse.Audio;
using Un4seen.Bass.Misc;
using OpenTK.Graphics.OpenGL;
using System.Threading;
using System.Collections.Specialized;
//using AviFile;
//using JockerSoft.Media;
//using JockerSoft;
using System.Diagnostics;
namespace Pulse.Screens
{
    class MenuScreen : Screen
    {
        Rect staticText, pulsingText, backGround, backGroundOverlay;
        Label startText;
        Rect logo;
        public MenuScreen(Game game, string name)
            : base(game, name)
        {

        }

        public void play(bool play)
        {
            Game.M.Enabled = false;
            Game.M.Visible = false;
           // Game.M.Music.stop();
            SelectScreen tempScreen = (SelectScreen)game.screens["selectScreen"];
            tempScreen.Play = true;
            Game.setScreen(game.screens["selectScreen"]);
        }
        [STAThread]
        private void openOptions()
        {
            (new Options()).ShowDialog();
            Game.resetStates();
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
        }

        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine("done downloading avatar");
            //Image i = Image.FromFile(Path.GetTempPath() + "pulsetempavi.jpg");
            avatar = new Rect(new Rectangle(Utils.getMX(800), 10, 128, 128), Path.GetTempPath() + "pulsetempavi.jpg");
            //.//   i.Dispose();
        }
        bool done;
        /* public static void print(this string s)
         {
             Console.WriteLine(s);
         }*/
        void downloadThread()
        {
            wc.Proxy = null;
            wc.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
            //wc.Headers.Add("User-Agent", "other");

            //wc.Headers.Add("Accept", "*/*");
            //wc.Headers.Add("Connection", "Keep-Alive");

            try
            {
                string dlstring = ((Account.currentAccount.AvatarUrl == "http://p.ulse.net/forum/download/file.php?avatar=") ? "http://p.ulse.net/generic_avatar.jpg" : Account.currentAccount.AvatarUrl);
                Console.WriteLine(dlstring);

                wc.DownloadFile(dlstring, Path.GetTempPath() + "pulsetempavi.jpg");
            }
            catch (Exception e)
            {
                Console.WriteLine("downloading avi " + e);
            }//   lock (this)
            //  {
            Console.WriteLine("done downloading avatar");
            //Image i = Image.FromFile(Path.GetTempPath() + "pulsetempavi.jpg");
            done = true;//
            //   Console.WriteLine("...");
            //  }
        }
        WebClient wc = new WebClient();
        Rect avatar;
        //first downloadasync didn't work until window was moved for no reason, then threading context issues... ;_;
        /// <summary>
        /// temporarily disabled until avatars are sorted
        /// </summary>
        public void downloadAvatar()
        {
            if (Account.currentAccount != null)
            {
                //if (!string.IsNullOrWhiteSpace(Account.currentAccount.AvatarUrl))
                //{
                avatar = null;
                done = false;
                Thread t = new Thread(new ThreadStart(downloadThread));
                t.IsBackground = true;
                t.Start();
                //   }
            }
            #region obsolete

            /* wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                
                Console.WriteLine("starting d/l");
              //  HttpWebRequest req = HttpWebRequest.Create("");
                
                wc.DownloadFileAsync(new Uri("http://rep.ulse.net/uploads/profile/photo-323.gif"), Path.GetTempPath() + "pulsetempavi.jpg");

                Console.WriteLine("started async," + wc.IsBusy);
                //        Console.WriteLine("done downloading avatar");
                //          Image i = Image.FromFile(Path.GetTempPath() + "pulsetempavi.jpg");
                //            avatar = new Rect(new Rectangle(Utils.getMX(800), 10, i.Width, i.Height), Path.GetTempPath() + "pulsetempavi.jpg");
              */
            #endregion
        }

        Button avatarbutton, website;
    //    Rect test;
        public override void OnLoad(EventArgs e)
        {
            Game.stopMedia = false;
            base.OnLoad(e);
           // AviManager am = new AviManager("vid1.avi", true);
          //  VideoStream vs = am.GetVideoStream();
         //   test = new Rect(new Rectangle(0, 0, 300, 300), FrameGrabber.GetFrameFromVideo("vid1.avi", .2));
            //vs.GetFrameOpen();
            //vs.GetBitmap()
          //  Console.WriteLine(vs.StreamInfo.dwLength);
         //   vs.GetFrameClose();
            Animation preload = new Animation(new Rectangle(0, 0, 0, 0), 100, "holdBurst", 3, true, true);
            // ircl = new Client.irc.IrcClient("lol");
            logo = new Rect(new Rectangle(Config.ResWidth / 2 - (710 / 2), 20, 710, 519), "skin\\Pulse-LogoText.png");
            startText = new Label(game, new Rectangle(new Point(0, 0),
                new Size(0, 0)), "pulse alpha version " + Config.Version);
            startText.Layer = 1;
            SizeF temp = startText.TextTexture.getStringSize();
            Button optionsButton = new Button(game, new Rectangle((Config.ResWidth / 2) - 150, 768 / 2 + 240, 300, 60), "Options", delegate(int data)
            {
                //  new Thread(new ThreadStart(openOptions)).Start();
                new Options().ShowDialog();
            }, true, true, Color.LawnGreen);
            optionsButton.Layer = 1;
            Button exitButton = new Button(game, new Rectangle((Config.ResWidth / 2) - 150, 768 / 2 + 305, 300, 60), "Exit", delegate(int data)
            {
                game.Exit();
            }, true, true, Color.Cyan);
            exitButton.Layer = 1;
            Button playButton = new Button(game, new Rectangle(Config.ResWidth / 2 - 150, 768 / 2 + 110, 300, 60), "Play", delegate(int data)
            {
                play(true);
            }, true, true, Color.Red);
            playButton.Layer = 1;
            Button editButton = new Button(game, new Rectangle(Config.ResWidth / 2 - 150, 768 / 2 + 175, 300, 60), "Edit", delegate(int data)
            {
                play(false);
            }, true, true, Color.Blue);
            editButton.Layer = 1;
            avatarbutton = new Button(game, new Rectangle(Utils.getMX(700), 0, 300, 100), "\tLogin", delegate(int data)
            {
                new Pulse.UI.LoginDialog().ShowDialog();
                // downloadAvatar();
            });
            avatarbutton.Layer = 1;
            UIComponents.Add(avatarbutton);
            startText.TextTexture.Location = new Point(Config.ResWidth / 2 - ((int)temp.Width / 2), 0);
            startText.TextTexture.TextureSize = new Size((int)temp.Width, (int)temp.Height);
            staticText = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            staticText.Color = new OpenTK.Graphics.Color4(0.4f, 0.4f, 0.4f, 1.0f);
            staticText.useTexture("skin\\Pulse-LogoText.png");
            staticText.Layer = -8;
            pulsingText = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            pulsingText.Color = new OpenTK.Graphics.Color4(0.4f, 0.4f, 0.4f, 1.0f);
            pulsingText.useTexture("skin\\Pulse-LogoText.png");
            pulsingText.Layer = -7;
            pulsingText.Alpha = 0.3f;
            backGround = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            backGround.Color = new OpenTK.Graphics.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            backGround.useTexture("skin\\Pulse-MenuBG.png");
            backGround.Layer = -9;
            backGroundOverlay = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            backGroundOverlay.Color = new OpenTK.Graphics.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            backGroundOverlay.useTexture("skin\\Pulse-MenuOverlay.png");
            backGroundOverlay.Layer = -6;
            website = new Button(game, new Rectangle(0, 768 - 53, 360, 53), "", delegate(int data)
                {
                    if (!Game.pbox.expanded)
                    {
                        System.Diagnostics.Process.Start("http://p.ulse.net");
                    }
                }, Skin.skindict["websiteText"]);
            website.Layer = 1;
            setMarquee();

            UIComponents.Add(startText);
            UIComponents.Add(optionsButton);
            UIComponents.Add(playButton);
            UIComponents.Add(editButton);
            //UIComponents.Add(nowp);
            UIComponents.Add(exitButton);
            // pbox = new PTextBox(game, new Rectangle(0, 768-300, Utils.getMX(1024), 290), "", ircl);
            //UIComponents.Add(pbox);
            //UIComponents.Add(skipForwardButton);
            //UIComponents.Add(togglePauseButton);
            Game.conn.recvPacket += new Action<short, Client.RecievePacket>(conn_recvPacket);
            //downloadAvatar();
        }
        public void setMarquee()
        {
            string mt = "";
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Proxy = null;
                    wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    string dl = "http://p.ulse.net/marquee.php";
                    if (Account.currentAccount != null)
                    {
                        dl += "?n=" + Account.currentAccount.AccountName + "&p=" + Account.currentAccount.passHash;
                    }
                    mt = wc.DownloadString(dl);
                }
                catch (Exception) { }
            }
            mq = new Marquee(Game.game, new Rectangle(0, 120, Utils.getMX(1024), 35), mt, 150);
            mq.Layer = 5;
        }
        Marquee mq;
        //public PTextBox pbox;
        void conn_recvPacket(short arg1, Client.RecievePacket arg2)
        {
            /*if (arg1 == (short)Pulse.Client.RecvHeaders.LOGIN_AUTH) //mo
            {
                byte login = (byte)arg2.info[0];
                if (login == 0) //successful
                {
                    Account.currentAccount = new Account((string)arg2.info[2], (string)arg2.info[1]);
                    //going to assume i can't set text from here because of gl context crap
                    //Game.game.Context.Ma
                    downloadAvatar();
                    if (Game.ircl != null)
                    {
                        Game.ircl.terminate();
                        Game.ircl = null;
                    }
                    Game.ircl = new Client.irc.IrcClient(Account.currentAccount.AccountName + "|pulse", Account.currentAccount.AccountName);
                    Game.ircl.realNick = Account.currentAccount.AccountName;
                    Game.pbox.setIrc(Game.ircl);
                }
                else
                {
                    Game.addToast("Login failed");                 
                }
            }*/
        }
        public override void onSwitched()
        {
            try
            {
                Client.PacketWriter.sendSongStart(Game.conn.Bw, Account.currentAccount.AccountName, "", "", (short)2, 0, 0, 0);
            }
            catch
            {
            }
            base.onSwitched();
            Game.stopMedia = false;
            Game.M.Visible = true;
            Game.M.Enabled = true;
            Game.game.Title = "Pulse";
        }
        public static bool isOptions = false;
        double time = 0;
        public Visuals vs = new Visuals();
        Rectangle vbounds = new Rectangle(0, 250, Config.ResWidth, 200);
        int counter = 0;
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            if (torender != null)
            {
                GL.DeleteTexture(torender.TextureID); //VERY IMPORTANT OR ELSE MASSIVE LEAKS
                
            }
         /*   if (test != null)
            {
                GL.DeleteTexture(test.TextureID);
            }*/
            float temp = Game.M.CurrentSong.getPulsePercent((int)Game.M.Music.Position);
            pulsingText.Bounds = new Rectangle((int)(0 - (20 * (temp + 2))), (int)(0 - ((768 / 20) * (temp + 2))), (int)(Config.ResWidth + (40 * (temp + 2))), (int)(768 + ((768 / 10) * (temp + 2))));
            pulsingText.Alpha = 0.6f - (temp / 10f);
            base.OnUpdateFrame(e);
            if (!Game.M.Music.Paused)
            {
                counter++;
                if (counter % 5 == 0)
                {
                    
                  /*  Stopwatch sw = new Stopwatch();
                    sw.Start();
                    FrameGrabber.GetFrameFromVideo("vid.avi", Game.M.Music.PositionAsMilli / Game.M.Music.Length);
                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds);*/
                }
                if (Config.Waveform)
                {
                    visualizer = vs.CreateWaveForm(Game.M.Music.handle, vbounds.Width, vbounds.Height, Color.SteelBlue, Color.Orange, Color.Transparent, Color.Transparent, 1, true, false, false);
                }
                if (visualizer != null && Config.Waveform)
                {
                    torender = new Rect(vbounds, visualizer);
                }
                if (!Config.Waveform)
                {
                    visualizer = null;
                    torender = null;
                }
            }
            else
            {
                torender = null;
            }
            if (Game.M.Music.Paused)
            {
                time += e.Time;
            }
            else
                time = 0;
            // if (!bgPath.Equals("songs\\" + Game.M.CurrentSong.Dir + "\\" + Game.M.CurrentSong.BgName))
            // {
            //     bgPath = "songs\\" + Game.M.CurrentSong.Dir + "\\" + Game.M.CurrentSong.BgName;
            //bg.useTexture("songs\\" + Game.M.CurrentSong.Dir + "\\" + Game.M.CurrentSong.BgName);
            //}
            if (keyPress(Key.O) && !Game.pbox.expanded)
            {
                new Options().ShowDialog();
                //new Thread(new ThreadStart(openOptions)).Start();
            }
            else if (keyPress(Key.P) && !Game.pbox.expanded)
            {
                play(true);
            }
            else if (keyPress(Key.E) && !Game.pbox.expanded)
            {
                play(false);
            }
            else if (keyPress(Key.C) && !Game.pbox.expanded && true) //make true if debugging shit
            {
                rotate = true;
            }
            website.OnUpdateFrame(e);
            if (Game.setMarquee)
            {
                setMarquee();
                Game.setMarquee = false;
            }
            if (mq != null)
            {
                mq.OnUpdateFrame(e);
            }
        }
        bool rotate = false;
        Bitmap visualizer;
        Rect torender;

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            backGround.OnRenderFrame(e);
            if (torender != null)
            {
                torender.OnRenderFrame(e);
            }
            
            staticText.OnRenderFrame(e);
            pulsingText.OnRenderFrame(e);
            backGroundOverlay.OnRenderFrame(e);
            base.OnRenderFrame(e);
            if (avatar == null && done)
            {
                avatar = new Rect(new Rectangle(Utils.getMX(705), 5, 90, 90), Path.GetTempPath() + "pulsetempavi.jpg", false, false);
                avatarbutton.Text = "\t" + Account.currentAccount.AccountName;
                done = false;
            }
            if (avatar != null)
            {
                avatar.Layer = 2;
                avatar.OnRenderFrame(e);
            }
            if (rotate)
            {
            }
            website.OnRenderFrame(e);
            if (mq != null)
            {
                mq.OnRenderFrame(e);
            }
          //  test.draw(e);
        }

        public void logOut()
        {
            avatarbutton.Text = "\tLogin";
            avatar = null;

        }
    }
}
