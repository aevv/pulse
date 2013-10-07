using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Un4seen.Bass;
using OpenTK.Input;

using System.Collections.Specialized;
using Pulse.Audio;

using Pulse.UI;
using Pulse.Mechanics;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Pulse.Screens
{
    /// <summary>
    /// Does displayScores need a lock for thread safety
    /// </summary>
    class SelectScreen : Screen
    {
        int index = 0, diffIndex = 0;
        bool play = true;
        bool calib = false;

        public bool Calib
        {
            get { return calib; }
            set { calib = value; }
        }

        public bool Play
        {
            get
            {
                return play;
            }
            set
            {
                play = value;
            }
        }
        Rect selectionTexture, sel2, searchBox, scorebg;
        List<DiffPair> difficultyTexts = new List<DiffPair>();
        Rect background;
        List<SongPair> songNameList = new List<SongPair>();

        Label searchLabel;
        Label searchInfoL;
        Button backLabel, help, playButton, onlineToggle, prevPage, nextPage;
        public Song currentSong;

        bool changed = false;

        float targetVolume = 0.0f;

        public SelectScreen(Game game, string name)
            : base(game, name)
        {

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
            if (play)
            {
                game.Title = "Pulse | Play Selection";
                playButton.Text = "Play";
                newSongb.Enabled = false;
                newSongb.Visible = false;
                help.Enabled = false;
                help.Visible = false;
            }
            else
            {
                game.Title = "Pulse | Edit Selection";
                playButton.Text = "Edit";
                newSongb.Enabled = true;
                newSongb.Visible = true;
                help.Enabled = false;
                help.Visible = false;
            }
            SongLibrary.cacheSongInfo();
            index = 0;
            for (int x = 0; x < songNameList.Count; x++)
            {
                if (songNameList[x].Info.SongName.Equals(Game.M.CurrentSong.SongName) && songNameList[x].Info.Dir.Equals(Game.M.CurrentSong.Dir))
                {
                    index = x;
                }
            }
            //currentSong = SongLibrary.loadSong(songNameList[index].Info);
            string bgString = "";
            if (currentSong.FileVersion == 0)
            {
                bgString = currentSong.BgName;
            }
            else
            {
                bgString = currentSong.Charts[0].BgName;
            }
            background.useTexture("songs\\" + currentSong.Dir + "\\" + bgString);
            music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
            Music.Volume = 0.0f;
            targetVolume = Config.Volume / 100.0f;
            music.Speed = 0.0f;
            changed = true;
            Music.play(false, true);            
            updateDiffs();
            updateScoreLabels(0);
            changeSong();
            for (int x = 0; x < songNameList.Count; x++)
            {
                songNameList[x].select.Bounds = new Rectangle(0, 246 + ((x - index) * 90), songNameList[x].select.Bounds.Width, songNameList[x].select.Bounds.Height);
            }
        }
        public Button newSongb;
        Button calibrateButton;
        static FileSystemWatcher fsw = new FileSystemWatcher("songs");
        void fsw_Created(object sender, FileSystemEventArgs e)
        {
            if (Game.toasts.Count > 0)
            { //random crash if no if statement
                if (!Game.toasts.Peek().text.Equals("Detected change in skins folder"))
                {
                    Game.addToast("Detected change in skins folder");
                }
            }
            else
            {
                Game.addToast("Detected change in skins folder");
            }
            //refresh();
            //changeSong();
        }
        Rect cover;
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            panel.Color = new OpenTK.Graphics.Color4(0.3f, 0.3f, 0.3f, 0.0f);
            panel.Layer = 8;
            dim.Color = new OpenTK.Graphics.Color4(0.0f, 0.0f, 0.0f, 0.0f);
            dim.Layer = 9.5;
            fsw.EnableRaisingEvents = true;
            fsw.IncludeSubdirectories = true;
            fsw.Created += new FileSystemEventHandler(fsw_Created);
            fsw.Changed += new FileSystemEventHandler(fsw_Created);
            fsw.Deleted += new FileSystemEventHandler(fsw_Deleted);
            fsw.Error += new ErrorEventHandler(fsw_Error);
            fsw.InternalBufferSize = 61440; //lets handle large changes ;D
            SongLibrary.cacheSongInfo();
            game.KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(game_KeyPress);
            bgoverlay.Color = new OpenTK.Graphics.Color4(0.0f, 0.0f, 0.0f, 0.5f);
            bgoverlay.Layer = 5;
            searchLabel = new Label(game, new Point(0, 700), "");
            searchLabel.TextTexture.Shadow = true;
            searchLabel.Layer = 4;
            searchInfoL = new Label(game, new Point(0, 700), "type to begin search..");
            searchInfoL.Layer = 4;
            //searchInfoL.TextTexture.Shadow = true;
            searchInfoL.TextTexture.Colour = Color.White;
            
            onlineToggle = new ToggleButton(game, new Rectangle(Utils.getMX(780), 130, 50, 40), "O", delegate(int data)
            {
                Config.LocalScores = !Config.LocalScores;
                updateScoreLabels(0);
            });
            onlineToggle.Layer = 4;

            prevPage = new Button(game, new Rectangle(Utils.getMX(835), 130, 50, 40), "<<", delegate(int data)
            {                
                if(displayScores != null && scoreDisplayPage > 0) {
                    scoreDisplayPage--;
                    updateScoreLabels(scoreDisplayPage);
                }
            });
            prevPage.Layer = 4;
            nextPage = new Button(game, new Rectangle(Utils.getMX(890), 130, 50, 40), ">>", delegate(int data)
            {
                if (displayScores != null && displayScores.Count > 0) //if count was 0 maxpage would be -1
                {
                    int maxpage = displayScores.Count / 8;
                    if (displayScores.Count % 8 > 0) //if there's a remainder that means theres some extra scores to display so need new page
                    {
                        maxpage++;
                    }
                    maxpage--; //remember, scoredisplaypage is 0 based!
                    if (scoreDisplayPage < maxpage)
                    {
                        scoreDisplayPage++;
                        updateScoreLabels(scoreDisplayPage);
                        /*if (scoreDisplayPage == maxpage) //finish later, faulty to have in a place like this
                        {
                            nextPage.Visible = false;
                            nextPage.Enabled = false;
                        }*/
                    }
                }
            });
            nextPage.Layer = 4;
            if (!Config.LocalScores)
            {
                ((ToggleButton)onlineToggle).toggle();
            }
            UIComponents.Add(prevPage);
            UIComponents.Add(nextPage);
            UIComponents.Add(onlineToggle);
            calibrateButton = new Button(game, new Rectangle(70, 768 - 170, 200, 50), "Calibrate offset", delegate(int data)
            {
                int tIndex = 0;
                for (int x = 0; x < currentSong.Charts.Count; x++)
                {
                    if (currentSong.Charts[x].Name.Equals(diffs.BaseText.Line))
                    {
                        tIndex = x;
                    }
                }
                IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                try
                {
                    uint flags = 0;
                    flags = flags | (uint)(nfB.Selected ? 1 : 0);
                    flags = flags | (uint)(autoB.Selected ? 2 : 0);
                    flags = flags | (uint)(mirB.Selected ? 4 : 0);
                    flags = flags | (uint)(hdB.Selected ? 8 : 0);
                    temp.loadSong(SongLibrary.loadSong(songNameList[index].Info), tIndex, new Mods()
                            {
                                Speed = dtB.Selected ? 1.5 : 1.0,
                                Flags = flags,
                                Scroll = Config.PlaySpeed
                            }, null, IngameScreen.PlayType.PLAY);
                    Music.stop();
                    Game.setScreen(game.screens["ingameScreen"]);
                    temp.Calibrate = true;
                    game.Title = "Pulse | " + currentSong.Artist + " - " + currentSong.SongName + " [" + currentSong.Charts[tIndex].Name + "]";
                }
                catch (Exception ex)
                {
                    ErrorLog.log(ex);
                }
            });
            calibrateButton.Layer = 4;
            calibrateButton.Visible = false;
            calibrateButton.Enabled = false;
         //   int backpos = Config.ResWidth - 244;//Config.WideScreen? (int)((780d/1024d) * game.Width) : 780;
            backLabel = new Button(game, new Rectangle(Utils.getMX(780), 10, 220, 50), "Back", delegate(int data)
            {
                Game.setScreen(game.screens["menuScreen"]);
            });
            backLabel.Layer = 4;
            help = new Button(game, new Rectangle(Utils.getMX(780), 130, 220, 50), "Help", delegate(int data)
            {
                new EditorHelp().ShowDialog();
                Game.resetStates();
            });
            backLabel.Layer = 4;
            UIComponents.Add(help);

            newSongb = new Button(game, new Rectangle(Utils.getMX(780), 70, 220, 50), "New Song", delegate(int data)
            {
                new NewSong().ShowDialog();
                Game.resetStates();
            });
            newSongb.Layer = 4;
            //   UIComponents.Add(searchLabel);
            UIComponents.Add(backLabel);
            UIComponents.Add(newSongb);
            #region obsolete
            /*difficultyTexts[0] = new Text(Config.ClientSize, new Size(300, 33), new Point(0, 0));
            difficultyTexts[0].Update("3key");
            difficultyTexts[0].Shadow = true;
            difficultyTexts[1] = new Text(Config.ClientSize, new Size(300, 33), new Point(0, 0));
            difficultyTexts[1].Update("4key");
            difficultyTexts[1].Shadow = true;
            difficultyTexts[2] = new Text(Config.ClientSize, new Size(300, 33), new Point(0, 0));
            difficultyTexts[2].Update("6key");
            difficultyTexts[2].Shadow = true;
            difficultyTexts[3] = new Text(Config.ClientSize, new Size(300, 33), new Point(0, 0));
            difficultyTexts[3].Update("7key");
            difficultyTexts[3].Shadow = true;*/
            #endregion
            refresh();
            index = 0;
            for (int x = 0; x < songNameList.Count; x++)
            {
                if (songNameList[x].Info.SongName.Equals(Game.M.CurrentSong.SongName) && songNameList[x].Info.Dir.Equals(Game.M.CurrentSong.Dir))
                {
                    index = x;
                } 
            }
            changeSong();
            selectionTexture = new Rect(new Rectangle(2, 250, 548, 28));
            selectionTexture.Layer = 4;
            sel2 = new Rect(new Rectangle(0, 249, 552, 30));
            sel2.Layer = 4;
            searchBox = new Rect(new Rectangle(0, 700, Config.ResWidth, 30));
            searchBox.Color = new OpenTK.Graphics.Color4(0.27f, 0.509f, 0.705f, .8f);
            searchBox.Layer = 4;
            cover = new Rect(new Rectangle(Config.ResWidth - 300, 0, 300, 768));
            cover.Layer = 4;
            cover.Color = new OpenTK.Graphics.Color4(1f, 1f, 1f, .7f);
            scorebg = new Rect(new Rectangle((int)(Config.ResWidth - 324), 180, 295, 600), Skin.skindict["scoreback"]);
            scorebg.Layer = 2;
            sel2.Color = new OpenTK.Graphics.Color4(0.0f, 0.0f, 0.0f, 1.0f);
            selectionTexture.Color = new OpenTK.Graphics.Color4(0.25f, 0.0f, 1.0f, 1.0f);
            background = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            background.Layer = -9;

            currentSong = SongLibrary.loadSong(songNameList[index].Info);
            string bgString = "";
            if (currentSong.FileVersion == 0)
            {
                bgString = currentSong.BgName;
            }
            else
            {
                bgString = currentSong.Charts[0].BgName;
            }
            background.useTexture("songs\\" + currentSong.Dir + "\\" + bgString);

            music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
            Music.Volume = 0.0f;
            targetVolume = Config.Volume / 100.0f;
            changed = true;
            Music.Position = (long)Game.M.Music.Position;
            Music.play(false, true);
            if (play)
            {
                game.Title = "Pulse | Play Selection";
                newSongb.Enabled = false;
                newSongb.Visible = false;
                help.Enabled = false;
                help.Visible = false;
            }
            else
            {
                game.Title = "Pulse | Edit Selection";
                newSongb.Enabled = true;
                newSongb.Visible = true;
            }
            updateDiffs();
            updateScoreLabels(0);
            //   changeSong();
            //   index = 0;
            for (int x = 0; x < songNameList.Count; x++)
            {
                // if (songNameList[x].textData.Location.Y != 246 + ((x - index) * 33) && !songNameList[x].textData.Moving)
                //  {
                songNameList[x].select.Bounds = new Rectangle(0, 246 + ((x - index) * 90), songNameList[x].select.Bounds.Width, songNameList[x].select.Bounds.Height);
                //  Console.WriteLine(songNameList[x].textData.Position.
                //   
            }
            //Console.WriteLine("why");
            List<string> t = new List<String>();
            t.Add(" ");
            diffs = new DropDownBox(game, t, new Rectangle(70, 120, 150, 35));
            diffs.selected += new Action<int>(diffs_selected);
            string tempS = "";
            if (play)
            {
                tempS = "Play";
            }
            else
            {
                tempS = "Edit";
            }
            playButton = new Button(game, new Rectangle(290 + 220, 768 - 170, 200, 50), tempS, delegate(int data)
            {
                if (pickDiff)
                {
                    int tIndex = 0;
                    for (int x = 0; x < currentSong.Charts.Count; x++)
                    {
                        if (currentSong.Charts[x].Name.Equals(diffs.BaseText.Line))
                        {
                            tIndex = x;
                        }
                    }
                    /*if (Config.AutoPlay && play)
                    {
                        ReplayScreen temp = (ReplayScreen)game.screens["replayScreen"];
                        try
                        {
                            temp.loadSong(SongLibrary.loadSong(songNameList[index].Info), tIndex);
                            Game.M.setSong(songNameList[index].Info);
                            Game.M.play();
                            Music.stop();
                            Game.setScreen(game.screens["replayScreen"]);
                            game.Title = "Pulse | Watch replay | " + currentSong.Artist + " - " + currentSong.SongName + " [" + currentSong.Charts[tIndex].Name + "]";
                        }
                        catch { }
                    }
                    else */if (play)
                    {
                        IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                        try
                        {
                            IngameScreen.PlayType te = IngameScreen.PlayType.PLAY;
                            if (autoB.Selected)
                                te = IngameScreen.PlayType.AUTO;
                            uint flags = 0;
                            flags = flags | (uint)(nfB.Selected ? 1 : 0);
                            flags = flags | (uint)(autoB.Selected ? 2 : 0);
                            flags = flags | (uint)(mirB.Selected ? 4 : 0);
                            flags = flags | (uint)(hdB.Selected ? 8 : 0);
                            temp.loadSong(SongLibrary.loadSong(songNameList[index].Info), tIndex, new Mods()
                            {
                                Speed = dtB.Selected ? 1.5 : 1.0,
                                Flags = flags,
                                Scroll = Config.PlaySpeed
                            }, null, te);
                            Game.M.setSong(ref songNameList[index].Info);
                            Game.M.play();
                            Music.stop();
                            temp.Calibrate = false;
                            Game.setScreen(game.screens["ingameScreen"]);
                            game.Title = "Pulse | " + currentSong.Artist + " - " + currentSong.SongName + " [" + currentSong.Charts[tIndex].Name + "]";
                            scoreDisplayText.Clear();
                            scoreDisplayPage = 1;
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.log(ex);
                        }
                    }
                    else if (!play)
                    {
                        EditorScreen temp = (EditorScreen)game.screens["editScreen"];
                        try
                        {
                            temp.loadSong(SongLibrary.loadSong(songNameList[index].Info), tIndex);
                            Game.M.setSong(ref songNameList[index].Info);
                            Game.M.play();
                            Music.stop();
                            Game.setScreen(game.screens["editScreen"]);
                        }
                        catch (KeyNotFoundException)
                        {
                        }
                        game.Title = "Pulse|Editor|" + currentSong.Artist + "-" + currentSong.SongName + "[" + currentSong.Charts[tIndex].Name + "]";
                    }
                }
            });
            playButton.Layer = 7;
            autoB = new ToggleButton(game, new Rectangle(874 - 480, 120, 100, 40), "Auto", delegate(int data)
            {
                Config.AutoPlay = autoB.Selected;
            });
            autoB.Layer = 7;
            dtB = new ToggleButton(game, new Rectangle(874 - 370, 120, 100, 40), "DT", delegate(int data)
            {
                Config.Dt = dtB.Selected;
                if (Config.Dt)
                {
                    Config.Ht = false;
                    if (htB.Selected)
                    {
                        htB.toggle();
                    }
                }
            });
            dtB.Layer = 7;
            htB = new ToggleButton(game, new Rectangle(874 - 260, 120, 100, 40), "HT", delegate(int data)
            {
                Config.Ht = htB.Selected;
                if (Config.Ht)
                {
                    Config.Dt = false;
                    if (dtB.Selected)
                    {
                        dtB.toggle();
                    }
                }
            });
            htB.Layer = 7;
            mirB = new ToggleButton(game, new Rectangle(874 - 480, 170, 100, 40), "Mirror", delegate(int data)
            {
                Config.Mirror = mirB.Selected;
            });
            mirB.Layer = 7;
            hdB = new ToggleButton(game, new Rectangle(874 - 370, 170, 100, 40), "HD", delegate(int data)
            {
                Config.Hidden = hdB.Selected;
            });
            hdB.Layer = 7;
            nfB = new ToggleButton(game, new Rectangle(874 - 260, 170, 100, 40), "No fail", delegate(int data)
            {
                Config.NoFail = nfB.Selected;
            });
            nfB.Layer = 7;
            closeSel = new Button(game, new Rectangle(290, 768 - 170, 200, 50), "Close", delegate(int data)
                {
                    notPickDiffs();
                });
            closeSel.Layer = 7;
            rateLabel = new Label(game, new Point(874 - 460, 370), "Scroll speed: " + Config.PlaySpeed);
            rateLabel.Layer = 7;
            rateDrag = new Dragbar(game, new Point(874-460, 400), 300, false, delegate(int d)
                {
                    double temp = rateDrag.getPercentScrolled();
                    int x = (int)(temp / 10);
                    Config.PlaySpeed = ((float)x / 2) + 0.5f;
                    rateLabel.Text = "Scroll speed: " + Config.PlaySpeed;
                });
            rateDrag.Layer = 7;
            rateDrag.setPos(rateDrag.Bounds.X + (int)(Config.PlaySpeed * 20) + rateDrag.Bounds.Width / 10);            
            game.Mouse.Move += new EventHandler<MouseMoveEventArgs>(Mouse_Move);
        }

        void diffs_selected(int obj)
        {
            updateScoreLabels(0);
        }
        int ydelta = 0;
        void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if (game.Focused && game.Active == this)
            {
               
                int midpt = game.Bounds.Height / 2;
                int diff = midpt - e.Y;
               
                if (Math.Abs(diff) < .30f * game.Bounds.Height) //.40f <- increase to increase area of non-scrolling
                {
                    ydelta = 0;
                   return;
                }
                diff = diff / 8;
              //  Console.WriteLine(diff);
                ydelta = diff;
            /*    for (int x = 0; x < songNameList.Count; x++)
                {
                    songNameList[x].select.move(new Point(0, songNameList[x].select.Bounds.Y+diff), 0.1);
                }*/
            }
        }
      //  int lastloc;
        void scroll()
        {
            try
            {
                if (game.Mouse.X > game.Width / 2)
                {
                    ydelta = 0;
                    return;
                }
                if (!songNameList[0].select.Texture.yMoving && ydelta != 0 && !pickDiff)
                {
                    //  lastloc = ydelta;
                    if (songNameList[0].select.Bounds.Y > game.Height)
                    {
                        if (ydelta >= 1)
                        {
                            ydelta = 0;
                        }
                    }
                    if (songNameList[songNameList.Count - 1].select.Bounds.Y < 0)
                    {
                        if (ydelta < 0)
                        {
                            ydelta = 0;
                        }
                    }
                    for (int x = 0; x < songNameList.Count; x++)
                    {
                        //if (songNameList[x].select.Bounds.Y + ydelta)
                        songNameList[x].select.moveY(new Point(songNameList[x].select.Bounds.X, songNameList[x].select.Bounds.Y + ydelta), 0.1);
                    }
                }
            }
            catch { }
        }
        int searchCountdown;
        bool resetSearch = true;
        Label rateLabel;
        ToggleButton autoB, dtB, htB, mirB, hdB, nfB;
        Dragbar rateDrag;
        Button closeSel;
        void game_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            if (game.Active == this && !pickDiff && !Game.pbox.expanded)
            {
                if ((int)e.KeyChar == 8)
                { //backspace
                    if (resetSearch)
                    { 
                        resetSearch = false;
                    }
                    try
                    {
                        searchLabel.Text = searchLabel.Text.Remove(searchLabel.Text.Length - 1);
                    }
                    catch { }
                }
                else
                {
                    if (!char.IsControl(e.KeyChar))
                    {
                        if (resetSearch)
                        {
                            resetSearch = false;
                        }
                        searchLabel.Text += e.KeyChar;
                    }
                }
                searchCountdown = 500;
            }
        }

        void fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            //refresh(); 
        }

        private void refresh()
        { //todo show some animation during this            
            transitioning = true;
            songNameList.Clear();
            SongLibrary.cacheSongInfo(); //prob should make separate method since searching forces cache refresh lol. we are good coders dont worry
            foreach (var pair in SongLibrary.songInfos)
            {
                Text temp = new Text(Config.ClientSize, new Size(550, 35), new Point(0, 0));
                /*SizeF temp1 = temp.getStringSize(pair.Value.Artist + " - " + pair.Value.SongName);
                temp.TextureSize = new Size((int)temp1.Width, (int)temp1.Height);*/
                temp.Update(pair.Value.Artist + " - " + pair.Value.SongName);
                temp.Shadow = true;
                SelectComponent sc = new SelectComponent(game, new Rectangle(-50, 0, 550, 90), pair.Value.Artist, pair.Value.SongName);
                sc.clickEvent += new Action<int, SelectComponent>(sc_clickEvent);
                SongPair sp = new SongPair(sc, pair.Value);
                songNameList.Add(sp);
                sc.index = songNameList.IndexOf(sp);
            }
            songNameList.Sort(new Comparison<SongPair>(delegate(SongPair f, SongPair j)
           {
               return -f.Info.SongName.CompareTo(j.Info.SongName);
           }));
            foreach (SongPair sp in songNameList)
            {
                sp.select.index = songNameList.IndexOf(sp);
            }
        }
        SelectComponent currentlySelected;
        bool doIt = true;
        Rect panel = new Rect(new Rectangle(50, 100, Config.ResWidth - 330, 768 - 200));
        void sc_clickEvent(int obj, SelectComponent obj2)
        {
            //    Console.WriteLine(obj);
            if (doIt && !pickDiff) //stupid but required
            {
                if (obj2.selected)
                {
                    pickDiffs();
                }
                else
                {
                    notPickDiffs();
                    index = obj;
                    changeSong();
                    /*   if (currentlySelected != null)
                       {
                           currentlySelected.selected = false;
                       }
                       obj2.selected = true;
                       currentlySelected = obj2;*/
                }
                doIt = false;
            }
        }

        void fsw_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("filesystemwatcher buffer overflowed, increase it?");
        }
        double time = 0.0;
        bool accept = false, pickSong = true, pickDiff = false;
        int count;
        float prevMouse = 0;
        float totalscroll;
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            scroll();
            float mwdif = 0;
            if (!Game.MouseState.LeftButton)
            {
                doIt = true;
            }
            float currentmw = game.Mouse.WheelPrecise;
            base.OnUpdateFrame(e);
            if (searchCountdown > 0 && !resetSearch)
            {
                searchCountdown -= (int)(e.Time * 1000);
            }
            if (searchCountdown <= 0 && !resetSearch)
            {
                //   if(!searchLabel.Text.Equals("")) {
                search(searchLabel.Text);
                //}
                resetSearch = true;
                searchCountdown = 500;
            }
            if (updateScore) {
                if (displayScores != null && displayScores.Count > 0)
                {
                    int upperlimit = displayScores.Count > scoreDisplayPage * 8 + 8 ? scoreDisplayPage * 8 + 8 : displayScores.Count;
                    if (upperlimit > displayScores.Count)
                    {
                        upperlimit = displayScores.Count;
                    }
                    for (int i = scoreDisplayPage * 8; i < upperlimit; i++)
                    {
                        int toset = i;
                        int loc = (i == 0 ? 0 : (i % 8));
                        Button toAdd = new Button(game, new Rectangle(Config.ResWidth, 200 + (loc * 60) + (loc * 1) /*1 px padding*/, 230, 60), "#" + (toset + 1) + " " + 
                            displayScores[toset].Player + " " + displayScores[toset].TotalScore.ToString("D8"), delegate(int data)
                        {
                            ScoreSelectScreen temp = (ScoreSelectScreen)game.screens["sSScreen"];
                            Game.setScreen(temp);
                            temp.Music = this.Music;
                            temp.setScore(displayScores[toset], currentSong, currtIndex, data);
                        }, Skin.skindict["scoreentry"]);
                        toAdd.OtherData = i + 1;
                        scoreDisplayText.Add(toAdd);
                    }
                }
                updateScore = false;
            }
            for (int i = 0; i < scoreDisplayText.Count; i++)
            {
                //orig 770
                int recalc = Config.ResWidth - 254;
                Button b = scoreDisplayText[i];
                if (b.Bounds.X == (int)(Config.ResWidth - 254))
                {
                    continue; //nothing to do here
                }
                if (i != 0)
                {
                    if (scoreDisplayText[i - 1].Bounds.X > (int)(Config.ResWidth - 104))
                    { //stagger it
                        continue;
                    }
                }
                //770
                //total of 254 pixels to travel
                double percentage = (b.Bounds.X - recalc) / 254d;
              //  double percentage = (b.Bounds.X - 1034) / 254d;
                double squared = Math.Pow(percentage, 0.7);
                /// Console.WriteLine("{0} {1}", percentage, squared);
                if (b.Bounds.X > recalc)
                {
                    //         Console.WriteLine(b.Bounds.X);
                    int tomove = b.Bounds.X - ((int)Math.Round((20 * squared)) + 1);// ((int)(1600 * e.Time)); //with *time wasnt being staggered correctly because if the update took too long in the first update it would step over 920
                    if (tomove < recalc)
                    {
                        tomove = recalc;
                    }
                    b.Bounds = new Rectangle(tomove, b.Bounds.Y, b.Bounds.Width, b.Bounds.Height);
                }
                else
                {
                    b.Bounds = new Rectangle(recalc, b.Bounds.Y, b.Bounds.Width, b.Bounds.Height);
                }
            }
            if (transitioning)
            {
                if (transIn)
                {
                    bgoverlay.Alpha += 0.15f;
                    if (bgoverlay.Alpha >= 1f)
                    {
                        transIn = false;
                    }
                }
                else
                {
                    bgoverlay.Alpha -= 0.15f;
                    if (bgoverlay.Alpha <= 0f)
                    {
                        transitioning = false;
                        transIn = true;
                    }
                }
            }
            if (changed)
            {
                time += e.Time;
                if (time > 0.1)
                {
                    Music.Volume += targetVolume / 10.0f;
                    if (Music.Volume >= targetVolume)
                    {
                        Music.Volume = targetVolume;
                        changed = false;
                    }
                    time = 0.0;
                }
            }
            if (accept && !Game.pbox.expanded)
            {
                //   searchLabel.Text += toadd;
                if (keyPress(Key.Down))
                {
                    if (pickSong)
                    {
                        if (index + 1 != songNameList.Count)
                        {
                            index++;
                            changeSong();
                        }
                    }
                }
                else if (keyPress(Key.Up))
                {
                    if (pickSong)
                    {
                        if (index - 1 > -1)
                        {
                            index--;
                            changeSong();
                        }
                    }
                }
                else if (keyPress(Key.Enter))
                {
                    if (pickSong)
                    {
                        pickDiffs();
                        updateScoreLabels(0);
                        if (play)
                        {
                            calibrateButton.Visible = true;
                            calibrateButton.Enabled = true;
                        }
                    }
                    else if (pickDiff)
                    {
                        playButton.del.Invoke(0);
                    }
                }
                else if (keyPress(Key.Escape))
                {
                  
                    if (pickDiff)
                    {
                        notPickDiffs();
                        updateScoreLabels(0);
                        diffIndex = 0;
                        if (play)
                        {
                            calibrateButton.Visible = false;
                            calibrateButton.Enabled = false;
                        }
                    }
                    else if (searchLabel.Text.Length > 0)
                    {
                        resetSearch = false;
                        searchCountdown = 0; 
                        searchLabel.Text = "";
                    }
                    else
                    {
                        //Music.stop();
                        Game.setScreen(game.screens["menuScreen"]);
                    }
                }
                else if (keyPress(Key.F1))
                {
                    refresh();
                    changeSong();
                }
                else if ((mwdif = currentmw - prevMouse) != 0)
                {
                    prevMouse = currentmw;
                    totalscroll += mwdif;
                    //  Console.WriteLine("{0},{1},{2}", mwdif, prevMouse, "lol");
                    if (totalscroll < -5f)
                    {
                        if (pickSong)
                        {
                            if (index + 1 != songNameList.Count)
                            {
                                index++;
                                changeSong();
                            }
                        }
                        totalscroll = 0;
                        // Console.WriteLine("total scroll reset");
                    }
                    else if (totalscroll > 5f)
                    {
                        if (pickSong)
                        {
                            if (index - 1 > -1)
                            {
                                index--;
                                changeSong();
                            }
                        }
                        totalscroll = 0;
                        //Console.WriteLine("total scroll reset");
                    }
                }
                else
                {
                }

            }
            else
            {
                count++;
                if (count == 10)
                    accept = true;
            }
            for (int x = 0; x < songNameList.Count; x++)
            {
                songNameList[x].select.OnUpdateFrame(e);
            }
            if (dim.Color.A > 0 || pickDiff)
            {
                dim.OnRenderFrame(e);
                panel.OnRenderFrame(e);
                playButton.OnUpdateFrame(e);
                hdB.OnUpdateFrame(e);
                dtB.OnUpdateFrame(e);
                nfB.OnUpdateFrame(e);
                mirB.OnUpdateFrame(e);
                autoB.OnUpdateFrame(e);
                htB.OnUpdateFrame(e);
                closeSel.OnUpdateFrame(e);
                calibrateButton.OnUpdateFrame(e);
                rateDrag.OnUpdateFrame(e);
            } 
            if (displayScores != null)
            {
                if (scoreDisplayText.Count > 0)
                {
                    foreach (Button t in scoreDisplayText)
                    {
                        t.OnUpdateFrame(null);
                    }
                }
            }
        }
        List<Score> displayScores;
        List<Button> scoreDisplayText = new List<Button>();
        private void updateScoreLabels(int e)
        {
            scoreDisplayText.Clear();
            //gotScore = false;
            if (!pickDiff)
            {
                displayScores = null;
                return;
            }
            SongInfo ss = songNameList[index].Info;
            scoreDisplayPage = e;
            //Console.WriteLine("updating score labels");
            int tIndex = 0;
            for (int x = 0; x < currentSong.Charts.Count; x++)
            {
                if (currentSong.Charts[x].Name.Equals(diffs.BaseText.Line))
                {
                    tIndex = x;
                }
            }
            String diffName = currentSong.Charts[tIndex].Name;
            if (currentSong.FileVersion == 1)
            {
                if (Config.LocalScores)
                {
                    string tempFileName = ScoreLibrary.getFileFromDiff(ss, diffName);
                    string hash = Utils.calcHash(tempFileName);
                    //  List<Score> tempList = null;
                    if (File.Exists("replay\\" + hash + ".psf"))
                    {
                        if (currentUpload != null)
                        {
                            currentUpload.abort();
                        }
                        displayScores = ScoreLibrary.reconstruct("replay\\" + hash + ".psf");
                        displayScores.Sort(scoreCompare);
                        scoreDisplayText.Clear();
                        //if calculated upper bound exceeds displayscores.count, use count as upper bound instead
                        int upperlimit = displayScores.Count > scoreDisplayPage * 8 + 8 ? scoreDisplayPage * 8 + 8 : displayScores.Count;

                        for (int i = scoreDisplayPage * 8; i < upperlimit; i++)
                        {
                            int toset = i;
                            int loc = (i == 0 ? 0 : (i % 8));
                            Button toAdd = new Button(game, new Rectangle(Config.ResWidth, 200 + (loc * 60) + (loc * 1) /*1 px padding*/, 230, 60), "#" + (toset + 1) + " " + displayScores[toset].Player + " " + displayScores[toset].TotalScore.ToString("D8"), delegate(int data)
                            {
                                ScoreSelectScreen temp = (ScoreSelectScreen)game.screens["sSScreen"];
                                Game.setScreen(temp);
                                temp.Music = this.Music;
                                temp.setScore(displayScores[toset], currentSong, tIndex, data);
                            }, Skin.skindict["scoreentry"]);
                            toAdd.Layer = 5;
                            toAdd.OtherData = i + 1;
                            scoreDisplayText.Add(toAdd);
                        }
                        return;
                    }
                    else
                    {
                        displayScores = null;
                        return;
                    }
                }
                else
                {
                    if (Account.currentAccount != null)
                    {
                        //online scores TODO
                        if (!gotScore)
                        {
                            gotScore = true;
                            string hash = Utils.calcHash(currentSong.Charts[tIndex].Path);
                            NameValueCollection n = new NameValueCollection();
                            n.Add("n", Account.currentAccount.AccountName);
                            n.Add("c", hash);

                            scoreDisplayText.Clear();
                            if (currentUpload != null)
                            {
                                //currentUpload.abort();
                            }
                            UploadClass uc = new UploadClass();
                            currentUpload = uc;
                            uc.additionalArg = tIndex; //hacky
                            uc.uploadDone += new Action<string, int>(uc_uploadDone);
                            uc.HttpUploadFileAsync("http://p.ulse.net/getscores", n);
                        }
                        //Game.game.Context.MakeCurrent(null); not gonna fly ;x
                        /*  I feel like I am flying so free across the sky
                            Like i'm diving in the air
                            I fly high the way you want it

                            Just gotta the way you want it.

                            Don't trip on the people
                            And things around you now
                            Keep lookin' ahead cousin
                            So glide on until you find it

                            Just gotta until you find it
                            In this limitless sky

                            Full speed ahead and I get high
                            Open the window (Look up the sky)
                            (That's the floating island in the legendary world)
                            Future is eternal, (Light, early-morning sunlight)
                            (The greatest, open up, look now with your left eye)

                            It's all a weird-ass place i'm in so weird a place
                            I'm tripping-out passing time
                            It's a wonderful way to free my mind
                            Up in the sky
                            I'm content to keep on flying.*/
                    }
                    else
                    {
                        Config.LocalScores = true;
                        ((ToggleButton)onlineToggle).toggle();
                    }
                }
            }            
        }
        bool gotScore = false;
        bool updateScore = false;
        int currtIndex;
        int scoreDisplayPage;
        UploadClass currentUpload;
        //gl context problems... again!
        void uc_uploadDone(string obj, int tIndex)
        {
          //  Game.game.MakeCurrent();
            updateScore = true;
            Score me = null;
            displayScores = ScoreLibrary.parseOnline(obj, ref me, currentSong, tIndex);
            currtIndex = tIndex;
            gotScore = false;
        }

        private void changeSong()
        {
            if (background != null)
            {
                Game.M.setSong(ref songNameList[index].Info);
                currentSong = Game.M.CurrentSong;                
                targetVolume = Config.Volume / 100.0f;
                Music.stop();
                string bgString = "";
                if (currentSong.FileVersion == 0)
                {
                    bgString = currentSong.BgName;
                }
                else
                {
                    bgString = currentSong.Charts[0].BgName;
                }
                background.useTexture("songs\\" + currentSong.Dir + "\\" + bgString);
                background.Layer = -9;
                updateScoreLabels(0);
                music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
                Music.Volume = 0.0f;
                Music.Position = (long)currentSong.Preview;
                Music.play(false, true);
                changed = true;
                transitioning = true;
                difficultyTexts.Clear();
                updateDiffs();
            }
            for (int x = 0; x < songNameList.Count; x++)
            {
                songNameList[x].select.moveY(new Point(songNameList[x].select.Bounds.X, 246 + ((x - index) * 90)), 0.3);
            }
            if (currentlySelected != null)
            {
                currentlySelected.selected = false;
            }
            songNameList[index].select.selected = true;
            currentlySelected = songNameList[index].select;
        }
        public void search(String query)
        {
            List<SongPair> toAdd = new List<SongPair>();
            refresh(); //show all to clear results of last search
            foreach (var i in songNameList)
            {
                if (i.Info.Artist.ToLower(Config.cultureEnglish).Contains(query.ToLower(Config.cultureEnglish)) || i.Info.SongName.ToLower(Config.cultureEnglish).Contains(query.ToLower(Config.cultureEnglish)))
                {
                    toAdd.Add(i);
                }
            }
            if (toAdd.Count == 0)
            {
                Game.addToast("No search results found");
                searchLabel.Text = "";
                changeSong(); //in case indexes don't match up (search "solar" -> index = 0 on solar, then search "asdasdasdasd", no results so it shows all but index still = 0 
                return;
            }
            songNameList.Clear();

            songNameList.AddRange(toAdd);
            foreach (var sp in songNameList)
            {
                sp.select.index = songNameList.IndexOf(sp);
            }
            index = 0; //important
            changeSong();

        }
        private static Comparison<DiffPair> comparer = new Comparison<DiffPair>(textComparer);
        private static Comparison<Score> scoreCompare = new Comparison<Score>(ScoreLibrary.CompareScores);
        public void updateDiffs()
        {
            List<string> temp = new List<string>();
            foreach (KeyValuePair<int, Chart> c in currentSong.Charts)
            {
                temp.Add(c.Value.Name);
            }
            diffs = new DropDownBox(game, temp, new Rectangle(100, 150, 150, 35));
            updateScoreLabels(0);
            diffs.selected += new Action<int>(diffs_selected);
        }
        public static int textComparer(DiffPair s, DiffPair s1)
        {
            return s.textData.Line.CompareTo(s1.textData.Line);
        }

        public static bool transitioning = true;
        public static bool transIn = true;
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            background.OnRenderFrame(e);
            cover.OnRenderFrame(e);
            bgoverlay.OnRenderFrame(e);
            base.OnRenderFrame(e);
            //  sel2.draw(e);
            // selectionTexture.draw(e);
            //for (int x = 0; x < songNameList.Count
            for (int x = 0; x < songNameList.Count; x++)
            {
                songNameList[x].draw(e);
            }
            
            scorebg.OnRenderFrame(e);
            if (displayScores != null)
            {
                if (scoreDisplayText.Count > 0)
                {
                    foreach (Button t in scoreDisplayText)
                    {
                        t.Layer = 5;
                        t.OnRenderFrame(null);
                    }
                }
            }

            searchBox.OnRenderFrame(e);

            if (string.IsNullOrWhiteSpace(searchLabel.Text))
            {
                searchInfoL.TextTexture.OnRenderFrame(e);
            }
            else
            {
                searchLabel.TextTexture.OnRenderFrame(e);
            }
            if (dim.Color.A > 0 || pickDiff)
            {
                dim.OnRenderFrame(e);
                panel.OnRenderFrame(e);
                diffs.OnRenderFrame(e);
                playButton.OnRenderFrame(e);
                hdB.OnRenderFrame(e);
                dtB.OnRenderFrame(e);
                nfB.OnRenderFrame(e);
                mirB.OnRenderFrame(e);
                autoB.OnRenderFrame(e);
                htB.OnRenderFrame(e);
                closeSel.OnRenderFrame(e);
                calibrateButton.OnRenderFrame(e);
                rateLabel.OnRenderFrame(e);
                rateDrag.OnRenderFrame(e);
            }
        }
        private void pickDiffs()
        {
            pickDiff = true;            
            panel.fade(0.8f, 0.2);
            dim.fade(0.5f, 0.2);
            nfB.fade(1.0f, 0.2);
            htB.fade(1.0f, 0.2);
            dtB.fade(1.0f, 0.2);
            mirB.fade(1.0f, 0.2);
            autoB.fade(1.0f, 0.2);
            hdB.fade(1.0f, 0.2);
            playButton.fade(1.0f, 0.2);
            calibrateButton.fade(1.0f, 0.2);
            closeSel.fade(1.0f, 0.2);
            pickSong = false;
            updateDiffs();
        }
        private void notPickDiffs()
        {
            pickDiff = false;
            panel.fade(0.0f, 0.2);
            dim.fade(0.0f, 0.2);
            nfB.fade(0.0f, 0.2);
            htB.fade(0.0f, 0.2);
            dtB.fade(0.0f, 0.2);
            mirB.fade(0.0f, 0.2);
            autoB.fade(0.0f, 0.2);
            hdB.fade(0.0f, 0.2);
            playButton.fade(0.0f, 0.2);
            calibrateButton.fade(0.0f, 0.2);
            closeSel.fade(0.0f, 0.2);
            pickSong = true;
        }
        Rect bgoverlay = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
        Rect dim = new Rect(new Rectangle(0, 0, 755, 768));
        DropDownBox diffs;
    }
}
