using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Input;
using Pulse.UI;
using Pulse.Mechanics;
using Pulse.Audio;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Specialized;
namespace Pulse
{
    class MediaPlayer
    {
        bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        bool enabled = true;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        Song currentSong;
        public Song CurrentSong
        {
            get { return currentSong; }
            set { currentSong = value; }
        }
        AudioFX music;
        public AudioFX Music
        {
            get { return music; }
            set { music = value; }
        }
        Button forward, backward, pause, stop;
        Label title;
        Dragbar position;
        int currentID;
        Game game;
        bool stopped = false;
        List<int> previousSongs = new List<int>();

        public MediaPlayer(Game game)
        {
            this.game = game;
            stop = new Button(game, new Rectangle(155, 5, 45, 45), "", delegate(int data)
            {
                stopped = true;
                pause.Texture.useTexture(Skin.skindict["mediaPY"]);
                music.pause();
            }, Skin.skindict["mediaSP"]);
            stop.Layer = 6;
            stop.manualColour = true;
            forward = new Button(game, new Rectangle(new Point(105, 5), new Size(45, 45)), "", delegate(int data)
            {
                if (!music.Finished)
                {
                    music.stop();
                }
                pause.Texture.useTexture(Skin.skindict["mediaPS"]);
            }, Skin.skindict["mediaFF"]);
            forward.Layer = 6;
            forward.manualColour = true;
            pause = new Button(game, new Rectangle(new Point(55, 5), new Size(45, 45)), "", delegate(int data)
            {
                if (!music.Paused)
                {
                    music.pause();
                    pause.Texture.useTexture(Skin.skindict["mediaPY"]);
                }
                else
                {
                    if (stopped)
                    {
                        music.play(true);
                    }
                    else
                    {
                        music.play(false);
                    }
                    stopped = false;
                    pause.Texture.useTexture(Skin.skindict["mediaPS"]);
                }
            }, Skin.skindict["mediaPS"]);
            pause.Layer = 6;
            pause.manualColour = true;
            backward = new Button(game, new Rectangle(new Point(5, 5), new Size(45, 45)), "", delegate(int data)
            {

                pause.Texture.useTexture(Skin.skindict["mediaPS"]);
                if (previousSongs.Count > 1)
                {
                    music.stop();
                    if (currentID - 1 == -1)
                    {
                        currentID = 0;
                    }
                    else
                    {
                        currentID--;
                    }
                    setSong(previousSongs[currentID]);
                    play();
                }
                else
                {
                    music.Position = 0;
                    currentID = 0;
                }
            }, Skin.skindict["mediaFB"]);
            backward.Layer = 6;
            backward.manualColour = true;
            title = new Label(game, new Point(0, 50), "");
            title.Layer = 6;
            Random r = new Random();
            if (SongLibrary.Songs.Count > 0)
            {
                int id;
                id = r.Next(0, SongLibrary.Songs.Count);
                setSong(id);
                play();
                previousSongs.Add(id);
                currentID = 0;
            }
            position = new Dragbar(game, new Point(10, 85), 230, false, delegate(int d)
                {
                    music.Position = (long)((music.Length / 100) * position.getPercentScrolled());
                });
            position.Layer = 6;
        }
        public void onUpdateFrame(FrameEventArgs e)
        {
            if (updateText)
            {
                title.Text = (currentSong.Artist + " - " + currentSong.SongName);
                updateText = false;
            }
            if (!music.Paused)
            {
                double percentPlayed = music.Position / music.Length;
                position.setPos((int)((position.Length * percentPlayed) + position.Texture.ModifiedBounds.X));
            }
            if (music.Position >= Math.Floor(music.Length))
            {
                music.Finished = true;
            }
            if (music.Finished && currentID + 1 != previousSongs.Count)
            {
                currentID++;
                setSong(previousSongs[currentID]);
                play();
            }
            else if (music.Finished && currentID + 1 == previousSongs.Count)
            {
                int id;
                Random r = new Random();
                id = r.Next(0, SongLibrary.Songs.Count);
                if (previousSongs.Count == SongLibrary.Songs.Count)
                {
                    previousSongs.Clear();
                    setSong(id);
                    play();
                    previousSongs.Add(id);
                    currentID = 0;
                }
                else
                {
                    while (previousSongs.Contains(id))
                    {
                        id = r.Next(0, SongLibrary.Songs.Count);
                    }
                    setSong(id);
                    if (!Game.stopMedia)
                    {
                        play();
                    }
                    previousSongs.Add(id);
                    currentID = previousSongs.Count - 1;
                }
            }
            forward.OnUpdateFrame(e);
            backward.OnUpdateFrame(e);
            pause.OnUpdateFrame(e);
            stop.OnUpdateFrame(e);
        }
        public void onRenderFrame(FrameEventArgs e)
        {
            if (enabled)
            {
                forward.OnRenderFrame(e);
                backward.OnRenderFrame(e);
                pause.OnRenderFrame(e);
                stop.OnRenderFrame(e);
                title.OnRenderFrame(e);
                position.OnRenderFrame(e);
            }
        }
        bool updateText = false;
        public void setSong(int id)
        {
            currentSong = SongLibrary.loadSong(SongLibrary.Songs[id]);
            music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
            music.Volume = Config.Volume / 100.0f;
            updateText = true;
         //  try
            {
                if (currentSong.FileVersion > 0)
                {
                    Console.WriteLine(currentSong.Dir + " " + currentSong.ID);
                    if (currentSong.ID == -1)
                    {
                        NameValueCollection nvc = new NameValueCollection();
                        nvc.Add("c", Utils.calcHash(currentSong.Charts[0].Path));
                        int id2 = Convert.ToInt32(Utils.HttpUploadFile("http://p.ulse.net/idchart", nvc));
                        if (id2 == -1)
                        {
                            id2 = -2;
                        }
                        currentSong.ID = id2;
                        SongLibrary.songInfos[currentSong.Dir].ID = id2;
                        SongLibrary.Songs[id].ID = id2;
                    }
                }
                else
                {
                    Console.WriteLine("Cannot get song id, pncv0");
                }
            }
         //   catch { }
        }
        public void setSong(ref SongInfo s)
        {
            currentSong = SongLibrary.loadSong(s);
            if (music != null)
            {
                music.stop();
            }
            music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
            music.Volume = Config.Volume / 100.0f;
            updateText = true;
            if (currentSong.FileVersion > 0)
            {
                try
                {
                    if (currentSong.ID == -1)
                    {
                        NameValueCollection nvc = new NameValueCollection();
                        nvc.Add("c", Utils.calcHash(currentSong.Charts[0].Path));
                        int id = Convert.ToInt32(Utils.HttpUploadFile("http://p.ulse.net/idchart", nvc));
                        if (id == -1)
                        {
                            id = -2;
                        }
                        currentSong.ID = id;

                        SongLibrary.Songs[SongLibrary.Songs.IndexOf(s)].ID = id;
                        SongLibrary.songInfos[currentSong.Dir].ID = id;
                    }
                }
                catch
                {
                    //no connection
                }
            }
            else
            {
                Console.WriteLine("Cannot get song id, pncv0");
            }       
        }
        public void play()
        {
            play(true);
        }
        public void play(bool restart)
        {
            music.play(restart);
            pause.Texture.useTexture(Skin.skindict["mediaPS"]);
        }
    }
}
