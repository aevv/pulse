using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Un4seen.Bass;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
    /// Song selection screen, used to pick a song when dealing with multiplayer, edit, or play modes 
    /// </summary>
    class SelectScreen2 : Screen
    {
        #region misc variables
        string titleText;
        bool play;
        int songIndex = 0;
        int diffIndex = 0;
        Song currentSong;
        double keyHoldDuration = 0;
        double scrollTime = 0.2;
        double currentSongTimer = 0;
        bool songLoaded = false;
        #endregion

        #region UI
        Rect songInfoArea;
        Rect videoBgArea;
        Rect songsArea;
        Rect difficultiesStatsArea;
        Rect videoInfoBreaker;
        Rect songsVideoBreaker;
        Rect songsInfoBreaker;
        Rect activeBackground;
        Rect pulseBackground;

        Rect artistBackground;
        Text artistTitleText;

        Rect titleBackground;
        Text songTitleText;
        List<Pair<Rect, Text>> difficulties = new List<Pair<Rect, Text>>();
        List<SongDisplay> songDisplays = new List<SongDisplay>();
        #endregion

        #region properties
        public bool IsPlaySelection
        {
            set { play = value; }
            get { return play; }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game">Instance of the game the screen is a part of</param>
        /// <param name="title">Screen title</param>
        public SelectScreen2(Game game, string title)
            : base(game, title)
        {
            this.titleText = title;
        }
        /// <summary>
        /// Needed until Game.cs is refactored
        /// </summary>
        public override void onSwitched()
        {
            base.onSwitched();
            this.OnLoad(null);
        }
        /// <summary>
        /// Called whenever the screen is switched to as the current active screen
        /// </summary>
        /// <param name="e">Unused?</param>
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            game.Title = this.titleText + (play ? " | Play Selection" : " | Edit Selection");

            //is this needed? is it a problem with the song library?
            SongLibrary.cacheSongInfo();

            initUI();
            refreshSongDisplay();
            for (int x = 0; x < songDisplays.Count; x++)
            {
                if (songDisplays[x].SongInfo.SongName.Equals(Game.M.CurrentSong.SongName) && songDisplays[x].SongInfo.Dir.Equals(Game.M.CurrentSong.Dir))
                {
                    songIndex = x;
                }
            }
            moveSongDisplay(songIndex);

            useSong(true);
            
            songLoaded = true;
        }

        /// <summary>
        /// Moves the list of songs to the specified index. Does not sort list, add/refresh should be sorting only (no need to sort on just a move)
        /// </summary>
        /// <param name="index">Song ID of SongLibrary.songInfos to move to</param>
        private void moveSongDisplay(int index)
        {
            currentSongTimer = 0;
            songLoaded = false;
            bool afterIndex = false;
            for (int x = 0; x < songDisplays.Count; x++)
            {
                int height = 768 / 10;
                int yPos = ((index - x) * 50 + 768 / 2) - height / 2;
                if (afterIndex)
                    yPos += 15;
                double curveAdd = Math.Abs(index - x);
                int xPos = ((Config.ResWidth / 3) * 2 + 15) + ((int)curveAdd * (int)curveAdd);
                if (x == index)
                {
                    xPos -= 25;
                    afterIndex = true;
                    height += 15;
                    double layerChange = (double)Math.Abs(index - x) / 1.5;
                    songDisplays[x].Layer = 6 - layerChange;
                    songDisplays[x].TextTexture.textFont = new Font("Myriad Pro", 25);
                    songDisplays[x].TextTexture.updatesize(songDisplays[x].Text);
                    songDisplays[x].Selected = true;
                }
                else
                {
                    double layerChange = (double)Math.Abs(index - x);
                    songDisplays[x].Layer = 5 - layerChange / 10;

                    if (songDisplays[x].Selected)
                    {
                        songDisplays[x].Selected = false;
                        songDisplays[x].TextTexture.textFont = new Font("Myriad Pro", 20);
                        songDisplays[x].TextTexture.updatesize(songDisplays[x].Text);
                    }
                }
                songDisplays[x].Texture.scale(new Size(Config.ResWidth / 3 + 75, height), 0.2);
                songDisplays[x].Texture.move(new Point(xPos, yPos), 0.2);// = new Rectangle(xPos, yPos, Config.ResWidth / 3 + 25, height);
                songDisplays[x].TextTexture.move(new Point(xPos + 10, yPos + height / 4 + 4), 0.2);
            }            
        }

        /// <summary>
        /// Refreshes the song display list, should be used in conjunction with moveSongDisplay as this does not position the displays, but only generates and orders them from the current song library.
        /// </summary>
        private void refreshSongDisplay()
        {
            songDisplays.Clear();
            foreach (var pair in SongLibrary.songInfos)
            {
                Random n = new Random((int)DateTime.Now.Ticks);
                Color4 r = new Color4(0, (byte)n.Next(0, 255),0, 255);
                SongDisplay temp = new SongDisplay(game, pair.Value, new Rectangle(0, 0, 0, 0));
                temp.Texture.Color = r;
                songDisplays.Add(temp);
            }
            songDisplays.Sort(new Comparison<SongDisplay>(delegate(SongDisplay a, SongDisplay b)
                {
                    return -a.SongInfo.Artist.CompareTo(b.SongInfo.Artist);
                }));
        }
        /// <summary>
        /// Initialises all UI objects
        /// </summary>
        private void initUI()
        {
            if (songInfoArea == null)
            {
                songInfoArea = new Rect(new Rectangle(0, 0, Config.ResWidth / 3, 768 / 2));
                songInfoArea.Layer = -8;
                songInfoArea.Color = new Color4(0, 0, 0, 100);
            }
            if (videoBgArea == null)
            {
                videoBgArea = new Rect(new Rectangle(0, 768 / 2, Config.ResWidth / 3, 768 / 2));
                videoBgArea.Layer = -8;
                videoBgArea.Color = new Color4(0, 0, 0, 100);
            }
            if (videoInfoBreaker == null)
            {
                videoInfoBreaker = new Rect(new Rectangle(0, 768 / 2 - 10, Config.ResWidth / 3, 20));
                videoInfoBreaker.Layer = 0;
                videoInfoBreaker.Color = new Color4(255, 255, 0, 255);
            }
            if (songsVideoBreaker == null)
            {
                songsVideoBreaker = new Rect(new Rectangle(Config.ResWidth / 3 - 10, 768 / 2 - 10, 20, 768 / 2 + 10));
                songsVideoBreaker.Layer = 0;
                songsVideoBreaker.Color = new Color4(255, 255, 0, 255);
            }
            if (difficultiesStatsArea == null)
            {
                difficultiesStatsArea = new Rect(new Rectangle(Config.ResWidth / 3, 0, Config.ResWidth / 3, 768));
                difficultiesStatsArea.Layer = -8;
                difficultiesStatsArea.Color = new Color4(0, 0, 0, 100);
            }
            if (songsArea == null)
            {
                songsArea = new Rect(new Rectangle((Config.ResWidth / 3) * 2, 0, Config.ResWidth, 768), "skin\\carbon.png");
                songsArea.Layer = -8;
                songsArea.Color = new Color4(255, 255, 255, 150);
            }
            if (songsInfoBreaker == null)
            {
                songsInfoBreaker = new Rect(new Rectangle((Config.ResWidth / 3) * 2 - 10, 0, 20, 768));
                songsInfoBreaker.Layer = 0;
                songsInfoBreaker.Color = new Color4(255, 255, 0, 255);
            }
            if (pulseBackground == null)
            {
                pulseBackground = new Rect(new Rectangle(0, 0, Config.ResWidth, 768), "skin\\defaultbg.png");
                pulseBackground.Layer = -9.5;
                pulseBackground.Color = new Color4(150, 150, 150, 255);
            }
            if (titleBackground == null)
            {
                titleBackground = new Rect(new Rectangle(Config.ResWidth / 16, 17, Config.ResWidth / 5 * 2, 50));                
                titleBackground.Color = Color.Red;
                titleBackground.Layer = -4;
            }
            if (songTitleText == null)
            {
                songTitleText = new Text(new Size(Config.ResWidth / 5 * 2, 50), new Point(Config.ResWidth / 16, 21));
                songTitleText.textFont = new Font("Myriad Pro", 25);
                songTitleText.Layer = -3.8;
            }
            if (artistBackground == null)
            {
                artistBackground = new Rect(new Rectangle(titleBackground.Bounds.X + titleBackground.Bounds.Width / 20, 80, titleBackground.Bounds.Width - (titleBackground.Bounds.Width / 10), 40));
                artistBackground.Color = Color.Yellow;
                artistBackground.Layer = -4;
            }
            if (artistTitleText == null)
            {
                artistTitleText = new Text(new Size(Config.ResWidth / 6 * 2, 40), new Point(Config.ResWidth / 12, 80));
                artistTitleText.Layer = -3.8;
            }
        }

        /// <summary>
        /// Sets the song specific UI elements to the currently chosen song
        /// </summary>
        /// <param name="init">
        /// only true if switching from menu screen to maintain current position in song
        /// </param>
        private void useSong(bool init)
        {

            if (!init)
            {
                Game.M.setSong(ref songDisplays[songIndex].SongInfo);

                Game.M.play(!init);
            }
            currentSong = Game.M.CurrentSong;
            songTitleText.textFont = new Font("Myriad Pro", 25);
            songTitleText.Update(currentSong.SongName);
            if ((int)songTitleText.getStringSize().Width > titleBackground.Bounds.Width)
            {
                songTitleText.autoResize(Config.ResWidth / 5 * 2, 50);
            }
            songTitleText.Location = new Point(Config.ResWidth / 16 + (titleBackground.Bounds.Width / 2) - ((int)songTitleText.getStringSize().Width / 2), songTitleText.Location.Y);
            artistTitleText.textFont = new Font("Myriad Pro", 23);
            artistTitleText.Update(currentSong.Artist);
            if ((int)artistTitleText.getStringSize().Width > artistBackground.Bounds.Width)
            {
                artistTitleText.autoResize(Config.ResWidth / 6 * 2, 40);
            }
            artistTitleText.Location = new Point(Config.ResWidth / 12 + (artistBackground.Bounds.Width / 2) - ((int)artistTitleText.getStringSize().Width / 2), artistTitleText.Location.Y);

            diffIndex = 0;
            difficulties.Clear();
            for (int x = 0; x < currentSong.Charts.Count; x++)
            {
                Rect r = new Rect(new Rectangle(Config.ResWidth / 16 + 140 * x, 129, 125, 40));
                r.Layer = 5;
                r.Color = Color.Blue;
                if (x == 0)
                    r.Color = Color.Cyan;
                Text t = new Text(new Size(125, 40), new Point(Config.ResWidth / 16 + 140 * x, 129));
                t.Layer = 5.5;
                t.Update(currentSong.Charts[x].Name);
                t.autoResize(125, 40);
                difficulties.Add(new Pair<Rect, Text>(r, t));
            }

            if (activeBackground == null)
            {
                activeBackground = new Rect(new Rectangle(0, 768 / 2, Config.ResWidth / 3, 768 / 2), "songs\\" + currentSong.Dir + "\\" + currentSong.Charts[0].BgName);
            }
            else
            {
                activeBackground.useTexture("songs\\" + currentSong.Dir + "\\" + currentSong.Charts[0].BgName);
            }
           
            
        }
        /// <summary>
        /// Called whenever the screen is active during the games update loop
        /// </summary>
        /// <param name="e">Contains time since last frame</param>
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (currentSongTimer > 0.3 && !songLoaded)
            {
                useSong(false);
                songLoaded = true;
            }
            currentSongTimer += e.Time;

            if (keyHold(Key.Down))
            {
                if (keyHoldDuration > scrollTime)
                {
                    keyHoldDuration = 0;
                    scrollTime -= 0.01;
                    scrollTime = (scrollTime > 0.05 ? scrollTime : 0.05);
                    songIndex--;
                    if (songIndex < 0)
                        songIndex = 0;
                    moveSongDisplay(songIndex);
                }
                else
                {
                    keyHoldDuration += e.Time;
                }
            }
            else if (keyHold(Key.Up))
            {
                if (keyHoldDuration > scrollTime)
                {                    
                    keyHoldDuration = 0;
                    scrollTime -= 0.01;
                    scrollTime = (scrollTime > 0.05 ? scrollTime : 0.05);
                    songIndex++;
                    if (songIndex > SongLibrary.songInfos.Count - 1)
                        songIndex = SongLibrary.songInfos.Count - 1;
                    moveSongDisplay(songIndex);
                }
                else
                {
                    keyHoldDuration += e.Time;
                }
            }
            else
            {
                keyHoldDuration = 0.3;
                scrollTime = 0.2;
            }
            if (keyPress(Key.Left))
            {
                difficulties[diffIndex].key.Color = Color.Blue;
                if (--diffIndex < 0)
                {
                    diffIndex = 0;
                }
                difficulties[diffIndex].key.Color = Color.Cyan;
            }
            else if (keyPress(Key.Right))
            {
                difficulties[diffIndex].key.Color = Color.Blue;
                if (++diffIndex > currentSong.Charts.Count - 1)
                {
                    diffIndex = currentSong.Charts.Count - 1;
                }
                difficulties[diffIndex].key.Color = Color.Cyan;
            }
            if (keyPress(Key.Escape))
            {
                Game.setScreen(game.screens["menuScreen"]);
            }
            if (keyPress(Key.Enter))
            {
                IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                try
                {
                    IngameScreen.PlayType te = IngameScreen.PlayType.PLAY;
                   /* if (autoB.Selected)
                        te = IngameScreen.PlayType.AUTO;
                    uint flags = 0;
                    flags = flags | (uint)(nfB.Selected ? 1 : 0);
                    flags = flags | (uint)(autoB.Selected ? 2 : 0);
                    flags = flags | (uint)(mirB.Selected ? 4 : 0);
                    flags = flags | (uint)(hdB.Selected ? 8 : 0);*/
                    
                    temp.loadSong(SongLibrary.loadSong(songDisplays[songIndex].SongInfo), diffIndex, new Mods()
                    {
                        Speed = 1.0,
                        Flags = 0,
                        Scroll = Config.PlaySpeed
                    }, null, te);
                    Game.M.setSong(ref songDisplays[songIndex].SongInfo);
                    Game.M.play();
                    
                    temp.Calibrate = false;
                    Game.setScreen(game.screens["ingameScreen"]);
                    game.Title = "Pulse | " + currentSong.Artist + " - " + currentSong.SongName + " [" + currentSong.Charts[diffIndex].Name + "]";
                }
                catch (Exception ex)
                {
                    ErrorLog.log(ex);
                }
            }
        }

        /// <summary>
        /// Called whenever the screen is active during the games render loop
        /// </summary>
        /// <param name="e">Contains time since last frame</param>
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            pulseBackground.OnRenderFrame(e);
            songInfoArea.OnRenderFrame(e);
            videoBgArea.OnRenderFrame(e);
            difficultiesStatsArea.OnRenderFrame(e);
            songsArea.OnRenderFrame(e);
            songsInfoBreaker.OnRenderFrame(e);
            videoInfoBreaker.OnRenderFrame(e);
            songsVideoBreaker.OnRenderFrame(e);
            titleBackground.OnRenderFrame(e);
            songTitleText.OnRenderFrame(e);
            artistBackground.OnRenderFrame(e);
            artistTitleText.OnRenderFrame(e);

            //ensures that the song displays cascade in the correct order
            for (int x = songDisplays.Count - 1; x > songIndex; x--)
            {
                songDisplays[x].OnRenderFrame(e);
            }
            for (int x = 0; x < songIndex; x++)
            {
                songDisplays[x].OnRenderFrame(e);
            }
            songDisplays[songIndex].OnRenderFrame(e);

            if (activeBackground != null)
            {
                activeBackground.OnRenderFrame(e);
            }
            foreach (var p in difficulties)
            {
                p.key.OnRenderFrame(e);
                p.value.OnRenderFrame(e);
            }
        }
    }
}
