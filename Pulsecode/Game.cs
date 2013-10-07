#undef DEBUG
#undef SENDFATAL
#undef DEPTHTESTING
//#define DEPTHTESTING
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Ionic.Zip;
using OpenTK.Input;
using Un4seen.Bass;
using Pulse.Screens;
using Pulse.Mechanics;
using System.Diagnostics;
using Pulse.UI;
using Pulse.Audio;
using System.Threading;
using System.Drawing.Imaging;
using Pulse.Networking;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Reflection;
using LuaInterface;
using OpenTK.Platform;
namespace Pulse
{
    public class Game : GameWindow
    {        
        bool displayUsers = false;
        static bool leftButton = false;
        public static bool setMarquee = false;
        static bool rightButton = false;
        static MediaPlayer m;
        static List<OpenTK.Input.Key> pressedKey = new List<OpenTK.Input.Key>();
        public static Lua lua;
        public static PTextBox pbox;
        public static void resetStates()
        {
            leftButton = false;
            rightButton = false;

        }
        public static List<OpenTK.Input.Key> KeyboardState
        {
            get
            {
                List<OpenTK.Input.Key> newList = new List<OpenTK.Input.Key>();
                foreach (Key k in pressedKey)
                {
                    newList.Add(k);
                }
                return newList;
            }
        }
        internal static MediaPlayer M
        {
            get { return m; }
            set { m = value; }
        }
        public static MouseInfo MouseState
        {
            get { return new MouseInfo(leftButton, rightButton, game.Mouse.X, game.Mouse.Y); }
        }
        public static bool lClickFrame = false;
        Text fpsText;
        Screen active = null;
        public static Game game;
        public Screen Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        private void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs mbe)
        {
            if (mbe.Button == OpenTK.Input.MouseButton.Left)
            {
                Game.leftButton = true;
            }
            else if (mbe.Button == OpenTK.Input.MouseButton.Right)
            {
                Game.rightButton = true;
            }
        }
        private void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs mbe)
        {
            if (mbe.Button == OpenTK.Input.MouseButton.Left)
            {
                Game.leftButton = false;
            }
            else if (mbe.Button == OpenTK.Input.MouseButton.Right)
            {
                Game.rightButton = false;
            }
        }
        private void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs kbe)
        {
            pressedKey.Add(kbe.Key);
        }
        private void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs kbe)
        {
            pressedKey.Remove(kbe.Key);
        }
        static public Screen tempScreen;
        static bool screenChange = false;

        public Dictionary<string, Screen> screens = new Dictionary<string, Screen>();
        public Game()
            : base(Config.ClientWidth, Config.ClientHeight, GraphicsMode.Default, "g")
        {
            game = this;
            this.Title = "Pulse";
            this.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            this.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            this.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            this.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
            if (Config.Vsync)
            {
                this.VSync = VSyncMode.On;
            }
            else
                this.VSync = VSyncMode.Off;
            if (Config.Fullscreen)
            {
                this.WindowState = WindowState.Fullscreen;
            }
            else
                this.WindowState = WindowState.Normal;
        }
        public static void setVSync(bool vsync)
        {
            if (game != null)
            {
                if (vsync)
                {
                    game.VSync = VSyncMode.On;
                }
                else
                {
                    game.VSync = VSyncMode.Off;
                }
            }
        }
        public static void setFullscreen(bool fs)
        {
            if (game != null)
            {
                if (fs)
                {
                    OpenTK.DisplayDevice.Default.ChangeResolution(Config.ClientWidth, Config.ClientHeight, OpenTK.DisplayDevice.Default.BitsPerPixel, OpenTK.DisplayDevice.Default.RefreshRate);
                    game.WindowState = WindowState.Fullscreen;
                }
                else
                {
                    game.WindowState = WindowState.Normal;
                    OpenTK.DisplayDevice.Default.ChangeResolution(Config.oldRes.Width, Config.oldRes.Height, OpenTK.DisplayDevice.Default.BitsPerPixel, OpenTK.DisplayDevice.Default.RefreshRate);
                }
            }
        }
        /// <summary>
        /// thx http://www.opentk.com/node/1221
        /// </summary>
        /// <returns></returns>
        public IntPtr getHandle()
        {
            IWindowInfo ii = ((OpenTK.NativeWindow)this).WindowInfo;
            object inf = ((OpenTK.NativeWindow)this).WindowInfo;
            PropertyInfo pi = (inf.GetType()).GetProperty("WindowHandle");
            IntPtr hnd = ((IntPtr)pi.GetValue(ii, null));
            return hnd;
        }
        public static void setVolume(int volume)
        {
            if (game != null)
            {
                if (game.active != null)
                {
                    if (game.active.Music != null)
                    {
                        game.active.Music.Volume = Config.Volume / 100.0f;
                    }
                    if (m.Music != null)
                    {
                        m.Music.Volume = Config.Volume / 100.0f;
                    }
                }
            }
        }
        public static Queue<toast> toasts;
        static toast activeToast;
        static Text t;
        public static bool toastExists()
        {
            return activeToast != null;
        }
        public static void addToast(String t)
        {
            toasts.Enqueue(new toast(100, t));
        }
        static Rect toasttexture;
        static Rect volumeTexture;
        static Rect volumeRect;
        public static object ircLock = new object();
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            Config.saveConfig();
            SongLibrary.saveSongInfo();
            if (consoleAlloc)
            {
                FreeConsole();
            }
            OpenTK.DisplayDevice.Default.ChangeResolution(Config.oldRes.Width, Config.oldRes.Height, OpenTK.DisplayDevice.Default.BitsPerPixel, OpenTK.DisplayDevice.Default.RefreshRate);
        }
        protected override void OnLoad(EventArgs e)
        {
            lua = new Lua();

            lua.DoString("luanet=nil");
            Skin.luaInit();
            /*  Type ty = typeof(Config);
            foreach (MethodInfo mi in ty.GetMethods())
             {
                 lua.RegisterFunction(mi.Name,this , mi);
                 Console.WriteLine(mi.Name);
             }*/
            conn = new Client.Connection();
            conn.recvPacket += new Action<short, Client.RecievePacket>(conn_recvPacket);
            Pulse.Client.PacketWriter.sendCheckVersion(conn.Bw);
            this.Closing += new EventHandler<System.ComponentModel.CancelEventArgs>(Game_Closing);
            base.OnLoad(e);
            AudioManager.initBass();
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
#if DEPTHTESTING
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);
#endif
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            if (Skin.MaskBursts)
            {
                Animation preloadBursts = new Animation(new Rectangle(1, 1, 1, 1), 1, "burst", 10, false, true);
            }
            blackOverlay.Color = new Color4(0.0f, 0.0f, 0.0f, 0.2f);
            blackOverlay.Layer = 9.99;
            toasts = new Queue<toast>();
            toasttexture = new Rect(new Rectangle(0, 0, Config.ResWidth, 50), Skin.skindict["toast"]);
            volumeTexture = new Rect(new Rectangle((int)(Config.ResWidth * 0.26), 727, (int)(Config.ResWidth * 0.5), 43), Skin.skindict["volumecontrol"]);
            int toSet = (int)((Config.Volume / 100d) * (Config.ResWidth * 0.5));
            volumeRect = new Rect(new Rectangle((int)(Config.ResWidth * 0.37), 739, toSet, 25));
            volumeRect.Color = Color4.Orange;
            t = new Text(new Size(Width, Height), new Size(Config.ResWidth, 50), new Point(50, 0));
            this.Icon = DefaultSkin.pulseicon;

            SongLibrary.loadSongInfo();
            SongLibrary.cacheSongInfo(); //for newly added songs while game wasn't onint diff = 0;
            m = new MediaPlayer(this);
            screens.Add("menuScreen", new MenuScreen(this, "Pulse Menu"));
            screens.Add("selectScreen", new SelectScreen(this, "Song Select"));
            screens.Add("sSScreen", new ScoreSelectScreen(this, "Score Select Screen"));
            screens.Add("editScreen", new EditorScreen(this, "Edit Mode"));
            screens.Add("timingScreen", new TimingScreen(this, "Timing Screen"));
            screens.Add("ingameScreen", new IngameScreen(this, "ingame"));
            screens["ingameScreen"].Hide();
            screens["timingScreen"].Hide();
            screens["editScreen"].Hide();
            screens["selectScreen"].Hide();
            screens["sSScreen"].Hide();
            screens["menuScreen"].Show();
            Active = screens["menuScreen"];

            GL.ClearColor(Color4.SlateGray);

            fpsText = new Text(new Size(Width, Height), new Size(150, 35), new Point(Config.ResWidth - 120, 733));
            fpsText.Layer = 9.999;
            fpsText.Update("fps: " + this.TargetRenderFrequency);
            fpsText.Colour = Color.Yellow;
            fpsText.Shadow = true;
            int tipIndex = new Random().Next(Tips.tips.Length);
            addToast(Tips.tips[tipIndex]);
            if (!Directory.Exists("skin"))
            {
                Directory.CreateDirectory("skin");
            }
            fsw = new FileSystemWatcher("skin");
            fsw.EnableRaisingEvents = true;
            fsw.Created += new FileSystemEventHandler(fsw_Created);
            pbox = new PTextBox(game, new Rectangle(0, 768 - 300, Utils.getMX(1024), 290), "", ircl);
            pbox.minimize(0);
            // Thread check = new Thread(new ThreadStart(game.checkVersions));
            //   check.IsBackground = true;
            //   check.Start();
            Account.tryLoadAcc();
            int x = 5;
            SongLibrary.findByMD5("  ", ref x);
            userScreen = new UserDisplayScreen(this);
          //  n = new Notice(new Point(0, 200), 5000, "ASHASHASHASHASHASHASHASHASHASHASHASHASHASHASHASDFK;J");
        }
        public static NoticeManager nm = new NoticeManager();
        public static Pulse.Client.irc.IrcClient ircl;
        void conn_recvPacket(short arg1, Client.RecievePacket arg2)
        {
            
            switch (arg1)
            {
                case (short)Pulse.Client.RecvHeaders.VERSION_CHECK:
                    double serverversion = (double)arg2.info[0];
                    if (serverversion > Config.Version)
                    {
                        serverVer = serverversion;
                        verRemind = true;
                    }
                    else
                    {
                        Console.WriteLine("running latest ver of pulse");
                    }
                    break;
                case (short)Pulse.Client.RecvHeaders.LOGIN_AUTH:
                    byte login = (byte)arg2.info[0];
                    if (login == 0) //successful
                    {
                        Account.currentAccount = new Account((string)arg2.info[2], (string)arg2.info[1], (string)arg2.info[3]);
                        //going to assume i can't set text from here because of gl context crap
                        //Game.game.Context.Ma
                        ((MenuScreen)screens["menuScreen"]).downloadAvatar();
                        lock (ircLock)
                        {
                            if (Game.ircl != null)
                            {
                                Game.ircl.terminate();
                                Game.ircl = null;
                            }
                            setMarquee = true;
                            Game.ircl = new Client.irc.IrcClient("pulse|" + Account.currentAccount.AccountName, Account.currentAccount.AccountName);
                            Game.ircl.realNick = Account.currentAccount.AccountName;
                            Game.pbox.setIrc(Game.ircl);
                        }
                    }
                    else
                    {
                        Game.addToast("Login failed");
                    }
                    break;
                case (short)Pulse.Client.RecvHeaders.SPECTATE_RECORD:
                    Config.Spectated = true;
                    break;
                case (short)Pulse.Client.RecvHeaders.SPECTATE_END:
                    Config.Spectated = false;
                    break;
                case (short)Pulse.Client.RecvHeaders.SPECTATE_START:
                    Config.SpectatedUser = (string)arg2.info[0];
                    Config.Spectating = true;
                    int diff = 0;
                    SongInfo song = SongLibrary.findByMD5((string)arg2.info[1], ref diff);
                    if (song != null)
                    {                        
                        stopMedia = true;
                        IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                        try
                        {
                            if (active.Music != null)
                            {
                                active.Music.pause();
                            }
                            Mods m = new Mods()
                            {
                                Flags = (uint)((int)arg2.info[2]),
                                Scroll = (double)arg2.info[3]
                            };
                            temp.loadSong(SongLibrary.loadSong(song), diff, m, new Replay(), IngameScreen.PlayType.SPECTATE);
                            Game.setScreen(game.screens["ingameScreen"]);
                            Game.M.Music.pause();
                            Game.M.setSong(ref song);
                            Game.M.Visible = false;
                            Game.M.Enabled = false;
                            Client.PacketWriter.sendSpectateGotChart(Game.conn.Bw);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message + e.StackTrace);
                        }
                    }
                    break;
                case (short)Pulse.Client.RecvHeaders.SPECTATE_CANCEL:
                    if (active is IngameScreen)
                    {
                        if (active.Music != null)
                        {
                            active.Music.stop();
                        }
                        Config.Spectating = false;
                        Config.SpectatedUser = "";
                        Config.Specs = "";
                        Game.setScreen(game.screens["menuScreen"]);
                    }
                    break;
                case (short)Pulse.Client.RecvHeaders.SPECTATE_USERS:
                    Config.Specs = (string)arg2.info[0];
                    Console.WriteLine(arg2.info[0]);
                    break;
                case (short)Pulse.Client.RecvHeaders.SPECTATE_USERS_ME:
                    Config.SpecsOnMe = (string)arg2.info[0];
                    Console.WriteLine(arg2.info[0]);
                    break;

            }
        }
        static public bool stopMedia = false;
        void Game_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Config.ConfirmClose)
            {
                if (System.Windows.Forms.MessageBox.Show("Are you sure?", "Confirmation", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        void fsw_Created(object sender, FileSystemEventArgs e)
        {
            addToast("Detected change in skins folder");
        }
        bool verRemind = false;
        double serverVer = Config.Version;
        static FileSystemWatcher fsw;
        double time = 0;
        float previousMouse;
        bool canpresss = false, userToggle = false;
        UserDisplayScreen userScreen;
        double heartbeatTime = 0;
        Notice n;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            heartbeatTime += e.Time;
            //n.OnUpdateFrame(e);
            nm.update(e);
            if (heartbeatTime > 30 && conn.Bw != null)
            {
                try
                {
                    Client.PacketWriter.sendClientHeartbeat(conn.Bw);
                    heartbeatTime = 0;
                }
                catch
                {

                }
            }
            if (!screenChange)
            {
                if (!active.loaded)
                {
                    active.OnLoad(null);
                    active.loaded = true;
                    active.switched = true;
                }
                else if (!active.switched && active.loaded)
                {
                    active.onSwitched();
                    active.switched = true;
                }
            }
            if (displayUsers)
            {
                userScreen.OnUpdateFrame(e);
            }
            pbox.OnUpdateFrame(e);

            sleepCount++;
            if (sleepCount >= 10)
            {
                //need to work out a good way to use this to limit the cpu usage, or find another way to do it
                //skipFrame = true;
                sleepCount = 0;
            }
            if (verRemind)
            {
                System.Windows.Forms.MessageBox.Show("Client version (" + Config.Version + ") out of date! Latest (" + serverVer + ") available on the updater.");
                verRemind = false;
            }
            if (pressedKey.Contains(Key.F3) && userToggle)
            {
                userToggle = false;
                displayUsers = !displayUsers;
                if (displayUsers)
                {
                    if (ircl != null)
                    {
                        string userss = "";
                        foreach (string i in ircl.pulseUsers)
                        {
                            userss += i.Split('|')[1] + ";";
                        }
                        //   Console.WriteLine(userss);
                        Client.PacketWriter.sendUserRequest(Game.conn.Bw, userss);
                    }
                }
            }
            else if (!pressedKey.Contains(Key.F3))
            {
                userToggle = true;
            }
            if (Keyboard[OpenTK.Input.Key.AltLeft] && Keyboard[OpenTK.Input.Key.F4])
            {
                this.Exit();
            }
            if (toasts.Count > 0 && activeToast == null)
            {
                activeToast = toasts.Dequeue();
                t.Update(activeToast.text);
            }
            if (activeToast != null)
            {
                if (activeToast.y < 0 && activeToast.DisplayTime > 0)
                {
                    activeToast.y += 2;
                    toasttexture.Bounds = new Rectangle(0, activeToast.y, Config.ResWidth, 50);
                    t.Location = new Point(50, activeToast.y);
                }
                if (activeToast.y == 0)
                {
                    activeToast.DisplayTime--;
                }
                if (activeToast.DisplayTime <= 0)
                {
                    activeToast.y -= 2;
                    toasttexture.Bounds = new Rectangle(0, activeToast.y, Config.ResWidth, 50);
                    t.Location = new Point(50, activeToast.y);
                }
                if (activeToast.y <= -50)
                {
                    activeToast = null;
                    toasttexture.Bounds = new Rectangle(0, 0, Config.ResWidth, 50);
                    t.Location = new Point(50, 0);
                }
            }
            base.OnUpdateFrame(e);
            time += e.Time;
            if (Config.DisplayFps)
            {
                if (time > 0.25)
                {
                    time = 0;
                    fpsText.Update("fps: " + frameCount * 4);
                    frameCount = 0;
                }
            }
            if (volumeLife > 0)
            {
                volumeLife -= (int)(e.Time * 1000);
                if (volumeTexture.Bounds.Y > 730)
                {
                    volumeTexture.Bounds = new Rectangle(volumeTexture.Bounds.X, volumeTexture.Bounds.Y - 5, volumeTexture.Bounds.Width, volumeTexture.Bounds.Height);
                    volumeRect.Bounds = new Rectangle(volumeRect.Bounds.X, volumeRect.Bounds.Y - 5, volumeRect.Bounds.Width, volumeRect.Bounds.Height);
                }
            }
            if (volumeLife <= 0 && volumeTexture.Bounds.Y < 768)
            {
                volumeTexture.Bounds = new Rectangle(volumeTexture.Bounds.X, volumeTexture.Bounds.Y + 5, volumeTexture.Bounds.Width, volumeTexture.Bounds.Height);
                volumeRect.Bounds = new Rectangle(volumeRect.Bounds.X, volumeRect.Bounds.Y + 5, volumeRect.Bounds.Width, volumeRect.Bounds.Height);
            }
            lock (ircLock)
            {
                if (ircl != null)
                {

                    while (ircl.recieved.Count > 0)
                    {
                        Client.irc.IrcMessage msg = ircl.recieved.Dequeue();
                        string now = DateTime.Now.Hour.ToString("D2") + ":" + DateTime.Now.Minute.ToString("D2") + " ";
                        if (msg.HLed)
                        {
                            if (!pbox.expanded)
                            {
                                addToast("You have received a highlight, press F2 to open chat");
                            }
                            pbox.addLine((msg.timestamp ? now : "") + msg.Msg, Color.Green, msg.Target);
                            if (Config.ChatSounds)
                            {
                                Skin.ChatSound.play(true);
                            }
                            if (pbox.tabs.ContainsKey(" " + msg.Target + " "))
                            {
                                pbox.tabs[msg.Target].Alert = true;
                            }
                            if (!pbox.tabs.ContainsKey("#highlight"))
                            {
                                pbox.addTab("#highlight");
                            }
                            pbox.addLine((msg.timestamp ? now : "") + "(" + msg.Target + ") " + msg.Msg, col, "#highlight");
                            pbox.tabs["#highlight"].Alert = true;
                        }
                        else
                        {

                            if (msg.Target.Equals(ircl.realNick) && !game.Focused && Config.ChatSounds)
                            {
                                if (!pbox.expanded)
                                {
                                    addToast("You have recieved a private message, press F2 to open chat");
                                }
                                Skin.ChatSound.play(true);
                            }
                            if (msg.customColor)
                            {
                                pbox.addLine((msg.timestamp ? now : "") + msg.Msg, msg.col, msg.Target);
                            }
                            else
                            {
                                string toadd = (msg.timestamp ? now : "") + msg.Msg;
                                if (!string.IsNullOrEmpty(msg.sender))
                                {
                                    Dictionary<Pair<int, int>, Color> colors = new Dictionary<Pair<int, int>, Color>();
                                    int index = toadd.IndexOf(msg.sender);
                                    colors.Add(new Pair<int, int>(0, index), col);
                                    colors.Add(new Pair<int, int>(index, index + msg.sender.Length), msg.sendercolor);
                                    colors.Add(new Pair<int, int>(index + msg.sender.Length, toadd.Length), col);
                                    pbox.addLine(toadd, colors, msg.Target);
                                }
                                else
                                {
                                    /* if (msg.Msg.Length > 115)
                                     {
                                         Dictionary<Pair<int, int>, Color> colors = new Dictionary<Pair<int, int>, Color>();
                                         colors.Add(new Pair<int, int>(0, 100), Color.White);
                                         colors.Add(new Pair<int, int>(100, 110), Color.Blue);
                                         colors.Add(new Pair<int, int>(110, msg.Msg.Length), Color.White);
                                         pbox.addLine(toadd, colors, msg.Target);
                                     }
                                     else
                                     {*/
                                    pbox.addLine(toadd, col, msg.Target);
                                    //}
                                }
                            }

                            if (pbox.activeTab.Title != msg.Target && pbox.tabs.ContainsKey(msg.Target))
                            {
                                pbox.tabs[msg.Target].Alert = true;

                            }
                        }
                    }
                }
            }
            if (!Config.DisableMousewheel || Mouse.Y > pbox.Bounds.Y)
            {
                float nowMouse = Mouse.WheelPrecise;
                float delta = nowMouse - previousMouse;
                if (delta != 0)
                {
                    previousMouse = nowMouse;
                    if (Active != screens["selectScreen"] && active != screens["editScreen"] && !pbox.expanded)
                    {
                        delta *= 15;
                        int result = volumeRect.Bounds.Width + (int)delta;
                        if (result < 0)
                        {
                            result = 0;
                        }
                        else if (result > 370)
                        {
                            result = 370;
                        }
                        volumeRect.Bounds = new Rectangle(volumeRect.Bounds.X, volumeRect.Bounds.Y, result, volumeRect.Bounds.Height);
                        int percent = (int)((result / 370d) * 100);
                        Config.Volume = percent;
                        volumeLife = 1000;
                    }
                }
            }


            if (active != null && active.Enabled)
            {
                active.OnUpdateFrame(e);
            }
            if (screenChange && !transIn)
            {
                screenChange = false;
                active.Hide();
                if (active.Music != null && !(tempScreen is MenuScreen))
                {
                    active.Music.stop();
                }
                Active = tempScreen;
                tempScreen = null;
                active.Show();
            }
            if (transitioning)
            {
                if (transIn)
                {
                    blackOverlay.Alpha += 0.1f;
                    if (blackOverlay.Alpha >= 1f)
                    {
                        transIn = false;
                    }
                }
                else
                {
                    blackOverlay.Alpha -= 0.1f;
                    if (blackOverlay.Alpha <= 0f)
                    {
                        transitioning = false;
                        transIn = true;
                    }
                }
            }
            if (m.Enabled)
            {
                m.onUpdateFrame(e);
            }
            lClickFrame = false;
        }
        Color col = Color.FromArgb(255, 255, 210);
        Rect blackOverlay = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
        public static bool transitioning = true;
        public static bool transIn = true;
        static int volumeLife;
        bool skipFrame = false;
        int sleepCount = 0;
        int frameCount = 0;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            
            if (!screenChange)
            {
                if (!active.loaded)
                {
                    active.OnLoad(null);
                    active.loaded = true;
                    active.switched = true;
                }
                else if (!active.switched && active.loaded)
                {
                    active.onSwitched();
                    active.switched = true;
                }
            }
            frameCount++;

            if (skipFrame)
            {
                Thread.Sleep(1);
                skipFrame = false;
            }
            else if (!Focused)
            {
                Thread.Sleep(1);
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 ortho_projection = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, -10f, 10);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadMatrix(ref ortho_projection);

            if (active.Visible)
            {
                active.OnRenderFrame(e);
            }

            if (Config.DisplayFps)
            {
                fpsText.OnRenderFrame(e);
            }

            if (m.Visible)
            {
                m.onRenderFrame(e);
            }
            if (activeToast != null)
            {
                toasttexture.OnRenderFrame(e);
                t.OnRenderFrame(e);
            }
            volumeTexture.Layer = 9.9;
            volumeRect.Layer = 9.95;
            volumeTexture.OnRenderFrame(e);
            volumeRect.OnRenderFrame(e);
            if (blackOverlay.Color.A > 0)
            {
                blackOverlay.OnRenderFrame(e);
            }
            pbox.OnRenderFrame(e);
            if (displayUsers)
            {
                userScreen.OnRenderFrame(e);
            }
           // n.OnRenderFrame(e);
            nm.render(e);
            GL.PopMatrix();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            Config.ClientHeight = Height;
            Config.ClientWidth = Width;
            GL.Viewport(new System.Drawing.Size(Width, Height));
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, 0, Height, -1, 1);
        }

        public static void setScreen(Screen s)
        {
            tempScreen = s;
            game.active.Enabled = false;
            game.active.switched = false;
            transitioning = true;
            screenChange = true;
        }
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(uint dwProcessId);
        const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;
        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;
        [DllImport("kernel32", SetLastError = true)]
        static extern bool FreeConsole();
        public static bool consoleAlloc = false;
        public static Pulse.Client.Connection conn;
        [STAThread] //for drag and drop :|
        static void Main(string[] args)
        {
#if DEBUG
            //   IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            //    SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            //   FileStream fs = new FileStream(stdHandle, FileAccess.Write);
            //   StreamWriter sw = new StreamWriter(fs);
            //   Console.SetOut(sw);           
            //AttachConsole(ATTACH_PARENT_PROCESS);
            consoleAlloc = AllocConsole(); //allocate anyway if DEBUG defined
            Console.Title = "Pulse|Debug";
            Console.WriteLine("Running in Debug mode! All console output will be shown");
#endif

            if (args.Length > 0)
            {
                if (args[0].Equals("/debug") && !consoleAlloc)
                {
                    consoleAlloc = AllocConsole();
                    Console.Title = "Pulse|Debug";
                    Console.WriteLine("Running in Debug mode! All console output will be shown");
                }
                else
                {
                    try
                    {
                        string location = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Parent.FullName;
                        Console.WriteLine(location);
                        Console.ReadKey();
                        string songFolder = location + "\\songs";
                        ZipFile zf = new ZipFile(args[0]);
                        if (!Directory.Exists(songFolder))
                        {
                            Directory.CreateDirectory(songFolder);
                        }

                        foreach (ZipEntry ze in zf)
                        {
                            ze.Extract(songFolder, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show("Error extracting " + e);
                    }
                    Environment.Exit(0);
                }
            }
            if (!Directory.Exists("replay"))
            {
                Directory.CreateDirectory("replay");
            }
            Config.load();
            //  SongLibrary.loadSongInfo();
            using (Game game = new Game())
            {
#if SENDFATAL

                try
                {
                    game.Run(120.0, Config.Fps);
                }
                catch (Exception e)
                {
                    if (game != null && game.active != null)
                    {
                        if (game.active is EditorScreen)
                        {
                            EditorScreen temp = (EditorScreen)game.active;
                            try
                            {
                                temp.saveMap();
                            }
                            catch
                            {

                            }
                        }
                    }
                    ErrorLog.log("User : " + (Account.currentAccount != null ? Account.currentAccount.AccountName : "n/a") + " Fatal error: " + e.Message + "\n" + e.StackTrace);
                    System.Windows.Forms.MessageBox.Show("A fatal exception: \"" + e.Message + "\" has occured and been reported to the dev team");
                }
#endif

#if !SENDFATAL
                game.Run(120.0, Config.Fps); //want to show where exceptions are
#endif
            }
        }
        /*   public void checkVersions()
           {
               try
               {
                   TcpClient tcp = new TcpClient("92.232.66.53", 7777);
                   tcp.ReceiveTimeout = 10000;
                   BinaryWriter br = new BinaryWriter(tcp.GetStream());
                   br.Write((short)6);
                   br.Write((double)Config.Version);
                   BinaryReader b = new BinaryReader(tcp.GetStream());
                   double temp = b.ReadDouble();
                   if (temp > (double)Config.Version)
                   {
                       serverVer = temp;
                       verRemind = true;
                   }
                   tcp.Close();
               }
               catch (Exception e)
               {
                   Console.WriteLine("Could not connect to server to verify version, Pulse may be out of date!");
               }
           }*/
    }
}
