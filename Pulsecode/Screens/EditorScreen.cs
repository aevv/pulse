using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using clip = System.Windows.Forms;
using OpenTK.Input;
using Lua511;
using LuaInterface;
using Pulse.Audio;

using Pulse.UI;
using Pulse.Mechanics;
using OpenTK;
using OpenTK.Graphics;

namespace Pulse.Screens
{
    public class EditorScreen : Screen
    {
        EditorOptions options;
        public bool opOpen = false;
        private double offsetUpdateTime = 0;
        private class SnapLine : Line
        {
            public bool visible = false;
            private double offset = 0;
            public double Offset
            {
                get { return offset; }
                set { offset = value; }
            }
            public int vertical
            {
                set
                {
                    v1.Y = value + 54;
                    v2.Y = value + 54;
                }
            }
            public SnapLine(Point v1, Point v2, double off)
                : base(v1, v2)
            {
                Offset = off;
            }
        }
        private void openEditOptions()
        {
            options = new EditorOptions(this);
            options.ShowDialog();
            Game.resetStates();
        }
        private void openHelp()
        {
            new EditorHelp().ShowDialog();
            Game.resetStates();
        }
        List<Note> highlightedNotes = new List<Note>();
        List<Note> highlightedHolds = new List<Note>();

        Rect bg, overlay;
        Frame frame;
        int snapping = 4;
        Pulse.UI.Button optionsButton;
        Song currentSong;
        Pulse.UI.Label currentTimeLabel, musicRateLabel, snapLabel;
        public Song CurrentSong
        {
            get { return currentSong; }
            set { currentSong = value; }
        }
        int difficulty, endOffset = 0;
        double sectionOffset;
        public int EndOffset
        {
            get { return endOffset; }
            set { endOffset = value; }
        }
        Chart chart;

        public Chart Chart
        {
            get { return chart; }
            set { chart = value; }
        }
        bool paused = true;
        double noteSnap;
        double currentOffset = 0;

        public double CurrentOffset
        {
            get { return currentOffset; }
            set { currentOffset = value; }
        }
        double moveTime = 1200, moveRate = 0;

        public double MoveTime
        {
            get { return moveTime; }
            set
            {
                moveTime = value;
                if (Config.EditMiddle)
                {
                    moveRate = frame.HitHeight / 2 / moveTime;
                }
                else
                    moveRate = frame.HitHeight / moveTime;
            }
        }
        bool[] hold = new bool[8];
        List<SnapLine> frameSnaps = new List<SnapLine>();
        List<SnapLine> bookmarks = new List<SnapLine>();
        List<SnapLine> timingLines = new List<SnapLine>();

        AudioFX sfx;
        public EditorScreen(Game game, string name)
            : base(game, name)
        {
            moveTime = 1200;
            moveRate = 100 / moveTime;
        }
        byte[] hitsound;
        public void loadSong(Song song, int difficulty)
        {
            hitsound = File.ReadAllBytes(Skin.skindict["normal-hitnormal"]);
            currentSong = song;
            this.difficulty = difficulty;
            chart = song.Charts[difficulty];
            music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
            endOffset = (int)music.Length;
            music.Position = 0;
            music.Volume = Config.Volume / 100.0f;
            if (song.TimingsList.Count > 0)
            {
                for (int x = 0; x < song.TimingsList.Count; x++)
                {
                    if (song.TimingsList[x].Snap > 0 && song.TimingsList[x].ChangeSnap)
                    {
                        currentOffset = song.TimingsList[x].Offset;
                        noteSnap = song.TimingsList[x].Snap;
                        sectionOffset = song.TimingsList[x].Offset;
                        break;
                    }
                }
            }
            else
            {
                song.Timings.Add(0, new TimingSection(0, 1000, true));
                song.remakeLists(chart);
                currentOffset = song.TimingsList[0].Offset;
                noteSnap = song.TimingsList[0].Snap;
                sectionOffset = song.TimingsList[0].Offset;
            }
            bg = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            bg.useTexture("songs\\" + currentSong.Dir + "\\" + chart.BgName);
            setNoteOffsets();
            refreshGraphics();
        }
        Dragbar offsetBar;
        Button time, zoomPlus, zoomMinus, helpButton;
        Dragbar snapBar;
        public void rate(double f)
        {
            this.music.Speed = (float)f;
        }
        public override void OnLoad(EventArgs e)
        {
            Game.lua.RegisterFunction("rate", this, this.GetType().GetMethod("rate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
            base.OnLoad(e);
            try
            {
                Client.PacketWriter.sendSongStart(Game.conn.Bw, Account.currentAccount.AccountName, "", currentSong.Artist + " - " + currentSong.SongName, (short)8, 0, 0, 0);
            }
            catch
            {
            }
            Config.Editing = true;
            if (Config.EditMiddle)
            {
                moveTime = 600;
                frame = new Frame(new Point(317, 54), true, true);
            }
            else
            {
                frame = new Frame(new Point(317, 54), false, true);
            }
            currentTimeLabel = new Label(game, new Point(0, (int)frame.HitHeight + 48), "0");
            UIComponents.Add(currentTimeLabel);
            MoveTime = moveTime;
            overlay = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            zoomMinus = new Button(game, new Rectangle(new Point(280, 150), new Size(30, 30)), "-", delegate(int data)
            {
                if (moveTime + 100 < 3400)
                {
                    MoveTime += 100;
                }
                else
                {
                    MoveTime = 3400;
                }
            });
            zoomPlus = new Button(game, new Rectangle(new Point(280, 190), new Size(30, 30)), "+", delegate(int data)
            {
                if (moveTime - 100 > 300)
                {
                    MoveTime -= 100;
                }
                else
                {
                    MoveTime = 300;
                }
            });
            UIComponents.Add(zoomMinus);
            UIComponents.Add(zoomPlus);
            offsetBar = new Dragbar(game, new Point(0, (int)frame.HitHeight + 84), 300, false, delegate(int d)
            {
                currentOffset = (endOffset / 100) * offsetBar.getPercentScrolled();
                snap(ref currentOffset, snapping, true);
                music.Position = currentOffset;
            });

            double percentPlayed = currentOffset / endOffset;
            offsetBar.setPos((int)((offsetBar.Length * percentPlayed) + offsetBar.Texture.ModifiedBounds.X));
            snapLabel = new Label(game, new Point(730, 460), "Snap: 1/" + snapping);
            UIComponents.Add(snapLabel);
            snapBar = new Dragbar(game, new Point(730, 500), 200, false, delegate(int d)
                {
                    int temp = 4;
                    if (snapBar.getPercentScrolled() < (100 / 6))
                    {
                        temp = 1;
                    }
                    else if (snapBar.getPercentScrolled() < (100 / 6) * 2)
                    {
                        temp = 2;
                    }
                    else if (snapBar.getPercentScrolled() < (100 / 6) * 3)
                    {
                        temp = 3;
                    }
                    else if (snapBar.getPercentScrolled() < (100 / 6) * 4)
                    {
                        temp = 4;
                    }
                    else if (snapBar.getPercentScrolled() < (100 / 6) * 5)
                    {
                        temp = 6;
                    }
                    else
                    {
                        temp = 8;
                    }
                    if (temp != snapping)
                    {
                        snapping = temp;
                        snapLabel.Text = "Snap: 1/" + snapping;
                        snap(ref currentOffset, 1, true);
                        resnap();
                        setLines();
                    }
                });
            snapBar.setPos(snapBar.Bounds.X + snapBar.Length - (snapBar.Length / 2));
            UIComponents.Add(snapBar);
            overlay.Color = new Color4(0.0f, 0.0f, 0.0f, 0.5f);
            time = new Button(game, new Rectangle(850, 90, 150, 50), "Timing", delegate(int data)
            {
                TimingScreen ts = (TimingScreen)game.screens["timingScreen"];
                ts.loadSong(Music, currentSong);
                music.Speed = 0.0f;
                Game.setScreen(game.screens["timingScreen"]);
            });
            helpButton = new Button(game, new Rectangle(850, 160, 150, 50), "Help", delegate(int data)
            {
                music.pause();
                paused = true;
                new EditorHelp().ShowDialog();
                Game.resetStates();
            });
            UIComponents.Add(helpButton);
            UIComponents.Add(time);
            optionsButton = new Button(game, new Rectangle(850, 20, 150, 50), "Options", delegate(int data)
            {
                openEditOptions();
                setLines();
                resnap();
            });
            UIComponents.Add(offsetBar);
            UIComponents.Add(optionsButton);
            musicRateLabel = new Label(game, new Point(707, 650), "Rate: " + String.Format("{0:0.00}%", music.Speed + 100.0f));
            UIComponents.Add(musicRateLabel);
        }
        public override void onSwitched()
        {
            base.onSwitched();
            try
            {
                Client.PacketWriter.sendSongStart(Game.conn.Bw, Account.currentAccount.AccountName, "", currentSong.Artist + " - " + currentSong.SongName, (short)8, 0, 0, 0);
            }
            catch
            {
            }
            Config.Editing = true;
            if (Config.EditMiddle)
            {
                moveTime = 600;
                frame = new Frame(new Point(317, 54), true, true);
            }
            else
            {
                frame = new Frame(new Point(317, 54), false, true);
            }
            MoveTime = moveTime;
            paused = true;
            canpress = true;
            lc = false;
            offsetDifference = 0;
            distancedMoved = 0;
            musicRateLabel.Text = "Rate: " + String.Format("{0:0.00}%", music.Speed + 100.0f);
            snapping = 4;
            tempVertical = 0;
            double percentPlayed = currentOffset / endOffset;
            if (offsetBar != null)
            {
                offsetBar.setPos((int)((offsetBar.Length * percentPlayed) + offsetBar.Texture.ModifiedBounds.X));
                currentTimeLabel.Text = "" + currentOffset;
            };
            if (snapLabel != null)
            {
                snapLabel.Text = "Snap: 1/" + snapping;
            }
            currentSong.remakeLists(chart);
        }
        bool canpress = true, lc = false;
        double mult = 1;
        double offsetDifference = 0;
        double distancedMoved = 0;
        double tempVertical = 0;
        double lastFrameOffset = 0;
        private void resnap()
        {
            resnap(snapping);
        }
        private void resnap(int amount)
        {
            int id = 0;
            for (int x = 0; x < currentSong.TimingsList.Count; x++)
            {
                if (currentOffset >= currentSong.TimingsList[x].Offset)
                {
                    id = x;
                }
            }
            if (currentSong.TimingsList[id].ChangeSnap)
            {
                noteSnap = currentSong.TimingsList[id].Snap;
                if (sectionOffset != currentSong.TimingsList[id].Offset)
                {
                    if (sectionOffset < currentSong.TimingsList[id].Offset)
                    {
                        sectionOffset = currentSong.TimingsList[id].Offset;
                        //music.PositionAsMilli = sectionOffset;
                        //currentOffset = music.PositionAsMilli;
                        snap(ref currentOffset, amount, true);
                    }
                    else
                    {
                        sectionOffset = currentSong.TimingsList[id].Offset;
                        //music.PositionAsMilli = sectionOffset;
                        //currentOffset = music.PositionAsMilli;
                        snap(ref currentOffset, amount, true);
                    }
                    setLines();
                }
            }
        }
        float previousMouse = 0.0f;
        double leftCount = 0, rightCount = 0, lAmount = 0.5, rAmount = 0.5;
        bool test = false;
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            if (keyPress(Key.F5))
            {
                //test = true;
            }
            if (!test)
            {
                if (oldMouse == null)
                {
                    oldMouse = new MouseInfo(false, false, 0, 0);
                }
                int usableHeight = (int)frame.HitHeight;
                int clearOffset = -200;
                if (Config.EditMiddle)
                {
                    usableHeight = (int)frame.HitHeight / 2;
                    clearOffset = 0 - (int)moveTime;
                }
                offsetUpdateTime += e.Time;
                if (offsetUpdateTime > 0.25)
                {
                    if (!currentTimeLabel.Text.Equals(TimeSpan.FromMilliseconds(currentOffset).ToString()))
                    {
                        try
                        {
                            Regex r = null;
                            if (currentOffset > 3600000)
                            {
                                r = new Regex(@"\d*:\d\d:\d\d[.]\d\d\d");
                            }
                            else
                            {
                                r = new Regex(@"\d\d:\d\d[.]\d\d\d");
                            }
                            string temp = "" + TimeSpan.FromMilliseconds(currentOffset).ToString();
                            Match c = r.Match(temp);
                            currentTimeLabel.Text = c.Value;
                        }
                        catch
                        {
                            currentTimeLabel.Text = "" + TimeSpan.FromMilliseconds(currentOffset).ToString();
                        }
                    }
                    offsetUpdateTime = 0;
                }
                if (music.Finished)
                {
                    music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
                    music.Volume = Config.Volume / 100.0f;
                    music.Position = currentOffset;
                    paused = true;
                }
                if (lastFrameOffset != currentOffset)
                {
                    resnap();
                    lastFrameOffset = currentOffset;
                }
                else
                    lastFrameOffset = currentOffset;
                base.OnUpdateFrame(e);
                if (!paused)
                {
                    float percentPlayed = (float)Music.Position / (float)Music.Length;
                    offsetBar.setPos((int)((offsetBar.Length * percentPlayed) + offsetBar.Texture.ModifiedBounds.X));
                    currentOffset = music.Position + (-7 * (music.Speed / 10));
                }
                #region Key Input
                if (!Game.pbox.expanded)
                {
                    if (keyPress(Key.F1))
                    {
                        music.pause();
                        paused = true;
                        new EditorHelp().ShowDialog();
                        Game.resetStates();
                    }
                    if (keyPress(Key.Escape))
                    {
                        saveMap();
                        highlightedHolds.Clear();
                        highlightedNotes.Clear();
                        resetNoteOffsets();
                        music.stop();
                        music.Speed = 0.0f;
                        SongLibrary.cacheSongInfo();
                        Game.setScreen(game.screens["selectScreen"]);
                    }
                    if (keyHold(Key.ShiftLeft) || keyHold(Key.ShiftRight))
                    {
                        mult = 4;
                    }
                    else if (keyPress(Key.Y))
                    {
                        offsetBar.setPos(50);
                    }
                    else if (keyHold(Key.ControlLeft) || keyHold(Key.ControlRight))
                    {
                        mult = 0.5;
                    }
                    else mult = 1;
                    float nowMouse = game.Mouse.WheelPrecise;
                    float delta = nowMouse - previousMouse;
                    if (delta != 0)
                    {
                        previousMouse = nowMouse;
                        if (delta < 0)
                        {
                            delta = -1;
                        }
                        else
                        {
                            delta = 1;
                        }
                        if (music.Position + (delta * (noteSnap / (snapping)) * mult) < endOffset && music.Position + (delta * (noteSnap / (snapping)) * mult) > 0)
                        {
                            currentOffset += delta * (noteSnap / snapping * mult);
                            snap(ref currentOffset, (int)(snapping * 2), true);
                            music.Position = currentOffset;
                            Console.WriteLine("Change: " + currentOffset + " music: " + music.Position);
                        }
                    }
                    if (!keyHold(Key.Up))
                    {
                        rAmount = 0.5;
                        rightCount = 0;
                    }
                    if (!keyHold(Key.Down))
                    {
                        lAmount = 0.5;
                        leftCount = 0;
                    }
                    if (keyPress(Key.Up))
                    {
                        music.pause();
                        paused = true;
                        if (keyHold(Key.B))
                        {
                            int id = -1;
                            for (int x = 0; x < currentSong.Bookmarks.Count; x++)
                            {
                                if (currentOffset >= currentSong.Bookmarks[x])
                                {
                                    id = x;
                                }
                            }
                            if (id != currentSong.Bookmarks.Count - 1)
                            {
                                music.Position = currentSong.Bookmarks[id + 1];
                                currentOffset = music.Position;
                            }
                        }
                        else if (music.Position + (noteSnap / (snapping)) * mult < endOffset)
                        {
                            currentOffset += (long)((noteSnap / (snapping)) * mult);
                            if (mult == 0.5)
                            {
                                snap(ref currentOffset, snapping * 2, false);
                            }
                            else
                            {
                                snap(ref currentOffset, snapping * 2, false);
                            }
                            music.Position = currentOffset;
                        }
                        float percentPlayed = (float)Music.Position / (float)Music.Length;
                        offsetBar.setPos((int)((offsetBar.Length * percentPlayed) + offsetBar.Texture.ModifiedBounds.X));
                    }
                    else if (keyHold(Key.Up))
                    {
                        rightCount += e.Time;
                        if (rightCount > rAmount)
                        {
                            rAmount = 0.075;
                            rightCount = 0;
                            if (keyHold(Key.B))
                            {
                                int id = -1;
                                for (int x = 0; x < currentSong.Bookmarks.Count; x++)
                                {
                                    if (currentOffset >= currentSong.Bookmarks[x])
                                    {
                                        id = x;
                                    }
                                }
                                if (id != currentSong.Bookmarks.Count - 1)
                                {
                                    music.Position = currentSong.Bookmarks[id + 1];
                                    currentOffset = music.Position;
                                }
                            }
                            else if (music.Position + (noteSnap / (snapping)) * mult < endOffset)
                            {
                                currentOffset += (long)((noteSnap / (snapping)) * mult);
                                if (mult == 0.5)
                                {
                                    snap(ref currentOffset, snapping * 2, false);
                                }
                                else
                                {
                                    snap(ref currentOffset, snapping * 2, false);
                                }
                                music.Position = currentOffset;
                            }
                            float percentPlayed = (float)Music.Position / (float)Music.Length;
                            offsetBar.setPos((int)((offsetBar.Length * percentPlayed) + offsetBar.Texture.ModifiedBounds.X));
                        }
                    }
                    else if (keyPress(Key.Down))
                    {
                        music.pause();
                        paused = true;
                        if (keyHold(Key.B))
                        {
                            int id = 0;
                            for (int x = 0; x < currentSong.Bookmarks.Count; x++)
                            {
                                if (currentOffset >= currentSong.Bookmarks[x])
                                {
                                    id = x;
                                }
                            }
                            if (id != 0)
                            {
                                music.Position = currentSong.Bookmarks[id - 1];
                                currentOffset = music.Position;
                            }
                        }
                        else if (music.Position - (noteSnap / (snapping)) * mult > 0)
                        {
                            currentOffset -= (long)((noteSnap / (snapping)) * mult);
                            if (mult == 0.5)
                            {
                                snap(ref currentOffset, snapping * 2, true);
                            }
                            else
                            {
                                snap(ref currentOffset, snapping * 2, true);
                            }
                            music.Position = currentOffset;
                            //  Console.WriteLine("{0} {1}", music.PlayPosition, currentOffset);
                        }
                        float percentPlayed = (float)Music.Position / (float)Music.Length;
                        offsetBar.setPos((int)((offsetBar.Length * percentPlayed) + offsetBar.Texture.ModifiedBounds.X));
                    }
                    else if (keyHold(Key.Down))
                    {
                        leftCount += e.Time;
                        if (leftCount > lAmount)
                        {
                            lAmount = 0.075;
                            leftCount = 0;
                            if (keyHold(Key.B))
                            {
                                int id = 0;
                                for (int x = 0; x < currentSong.Bookmarks.Count; x++)
                                {
                                    if (currentOffset >= currentSong.Bookmarks[x])
                                    {
                                        id = x;
                                    }
                                }
                                if (id != 0)
                                {
                                    music.Position = currentSong.Bookmarks[id - 1];
                                    currentOffset = music.Position;
                                }
                            }
                            else if (music.Position - (noteSnap / (snapping)) * mult > 0)
                            {
                                currentOffset -= (long)((noteSnap / (snapping)) * mult);
                                if (mult == 0.5)
                                {
                                    snap(ref currentOffset, snapping * 2, true);
                                }
                                else
                                {
                                    snap(ref currentOffset, snapping * 2, true);
                                }
                                music.Position = currentOffset;
                                //  Console.WriteLine("{0} {1}", music.PlayPosition, currentOffset);
                            }
                            float percentPlayed = (float)Music.Position / (float)Music.Length;
                            offsetBar.setPos((int)((offsetBar.Length * percentPlayed) + offsetBar.Texture.ModifiedBounds.X));
                        }
                    }
                    else if ((keyHold(Key.ControlLeft) | keyHold(Key.ControlRight) && keyPress(Key.C)))
                    {
                        string copy = "";
                        highlightedNotes.Sort(delegate(Note a, Note b)
                        {
                            return a.Offset.CompareTo(b.Offset);
                        });
                        foreach (Note n in highlightedNotes)
                        {
                            Regex r = null;
                            if (n.Offset > 3600000)
                            {
                                r = new Regex(@"\d*:\d\d:\d\d[.]\d\d\d");
                            }
                            else
                            {
                                r = new Regex(@"\d\d:\d\d[.]\d\d\d");
                            }
                            string temp = "" + TimeSpan.FromMilliseconds(n.Offset).ToString();
                            Match c = r.Match(temp);
                            string loc = "" + n.Location;
                            copy += c.Value + " " + loc;
                            if (n.Hold)
                            {
                                temp = "" + TimeSpan.FromMilliseconds(n.HoldOffset).ToString();
                                c = r.Match(temp);
                                copy += " " + c.Value + "\r\n";
                            }
                            else
                            {
                                copy += "\r\n";
                            }
                        }
                        clip.Clipboard.SetText(copy);
                    }
                    else if ((keyHold(Key.ControlLeft) | keyHold(Key.ControlRight) && keyPress(Key.V)))
                    {
                        Regex r = new Regex(@"\d\d:\d\d[.]\d* \d \d\d:\d\d[.]\d*");
                        string temp = clip.Clipboard.GetText();
                        int zero = int.MaxValue;
                        for (int y = 0; y < 2; y++)
                        {
                            MatchCollection c = r.Matches(temp);
                            for (int q = 0; q < c.Count; q++)
                            {
                                string[] split = c[q].Value.Split(' ');
                                int location = int.Parse(split[1]);
                                double offset2 = 0;
                                double min;
                                string time;
                                if (y == 0)
                                {
                                    string[] split2 = split[2].Split(':');
                                    min = double.Parse(split2[0]) * 60;
                                    time = "" + min + split2[1];
                                    offset2 = double.Parse(time) * 1000;
                                }
                                split = split[0].Split(':');
                                min = double.Parse(split[0]) * 60;
                                time = "" + min + split[1];
                                double offset = double.Parse(time) * 1000;
                                if (offset < zero)
                                {
                                    zero = (int)offset;
                                }
                            }
                            r = new Regex(@"\d\d:\d\d[.]\d* \d");
                        }
                        Console.WriteLine(temp);
                        r = new Regex(@"\d\d:\d\d[.]\d* \d \d\d:\d\d[.]\d*");
                        for (int y = 0; y < 2; y++)
                        {
                            MatchCollection c = r.Matches(temp);
                            for (int q = 0; q < c.Count; q++)
                            {
                                temp.Replace(c[q].Value, "");
                                string[] split = c[q].Value.Split(' ');
                                int location = int.Parse(split[1]);
                                double offset2 = 0;
                                double min;
                                string time;
                                if (y == 0)
                                {
                                    string[] split2 = split[2].Split(':');
                                    min = double.Parse(split2[0]) * 60;
                                    time = "" + min + split2[1];
                                    offset2 = double.Parse(time) * 1000;
                                }
                                split = split[0].Split(':');
                                min = double.Parse(split[0]) * 60;
                                time = "" + min + split[1];
                                double offset = double.Parse(time) * 1000;
                                Note note = chart.Notes.Find(delegate(Note n)
                                {
                                    return n.Offset == (int)(currentOffset + offset - zero) && n.Location == location;
                                });
                                if (note == null)
                                {
                                    string temp2 = "a";
                                    int tempInt = 0;
                                    if (Skin.NoteStyle == 1)
                                    {
                                        if ((location - 1) % 2 == 1)
                                        {
                                            tempInt++;
                                        }
                                    }
                                    else if (Skin.NoteStyle == 2)
                                    {
                                        if ((location + 1) % 2 == 0)
                                        {
                                            tempInt++;
                                        }
                                        if (location + 1 == 4)
                                        {
                                            tempInt++;
                                        }
                                    }
                                    switch (tempInt)
                                    {
                                        case 0:
                                            temp2 = "a";
                                            break;
                                        case 1:
                                            temp2 = "b";
                                            break;
                                        case 2:
                                            temp2 = "c";
                                            break;
                                    }
                                    note = new Note((int)(currentOffset + offset - zero), location, temp2);
                                    if (y == 0)
                                    {
                                        note.Hold = true;
                                        note.HoldOffset = currentOffset + offset2 - zero;
                                    }
                                    note.XOffset = 317;
                                    note.YOffset = 54;
                                    chart.Notes.Add(note);
                                }
                            }
                            r = new Regex(@"\d\d:\d\d[.]\d* \d");
                        }
                    }
                    else if (keyPress(Key.A))
                    {
                        if (keyHold(Key.ControlLeft) || keyHold(Key.ControlRight))
                        {
                            highlightedHolds.Clear();
                            highlightedNotes.Clear();
                            foreach (Note n in chart.Notes)
                            {
                                highlightedNotes.Add(n);
                                n.Highlight = true;
                                if (n.Hold)
                                {
                                    highlightedHolds.Add(n);
                                    n.HoldHighlight = true;
                                }
                            }
                        }
                    }
                    else if (keyPress(Key.B))
                    {
                        if (keyHold(Key.ControlLeft) || keyHold(Key.ControlRight))
                        {
                            currentSong.Bookmarks.Add(currentOffset);
                            setLines();
                        }
                    }
                    else if (keyPress(Key.Right))
                    {
                        if (keyHold(Key.ControlLeft) | keyHold(Key.ControlRight))
                        {
                            foreach (Note n in highlightedNotes)
                            {
                                double startOffset = n.Offset;
                                n.Offset += (int)(noteSnap / (snapping * 2));
                                double tempDouble = n.Offset;
                                snap(ref tempDouble, (int)(snapping * 2), false);
                                double offsetChange = tempDouble - startOffset;
                                n.Offset = (int)tempDouble;
                                if (n.Hold && !n.HoldHighlight)
                                {
                                    n.HoldOffset += offsetChange;
                                }
                            }
                            foreach (Note n in highlightedHolds)
                            {
                                double startOffset = n.HoldOffset;
                                n.HoldOffset += noteSnap / (snapping * 2);
                                double tempDouble = n.HoldOffset;
                                snap(ref tempDouble, (int)(snapping * 2), true);
                                n.HoldOffset = (int)tempDouble;
                            }
                        }
                        else
                        {
                            if (music.Speed + 10.0f > 100.0f)
                                music.Speed = 100.0f;
                            else
                            {
                                music.Speed += 10.0f;
                            }
                            musicRateLabel.Text = "Rate: " + String.Format("{0:0.00}%", music.Speed + 100.0f);
                        }
                    }
                    else if (keyPress(Key.Left))
                    {
                        if (keyHold(Key.ControlLeft) || keyHold(Key.ControlRight))
                        {
                            foreach (Note n in highlightedNotes)
                            {
                                double startOffset = n.Offset;
                                n.Offset -= noteSnap / (snapping * 2);
                                double tempDouble = n.Offset;
                                snap(ref tempDouble, (int)(snapping * 2), true);
                                double offsetChange = tempDouble - startOffset;
                                n.Offset = tempDouble;
                                if (n.Hold && !n.HoldHighlight)
                                {
                                    n.HoldOffset += offsetChange;
                                }
                            }
                            foreach (Note n in highlightedHolds)
                            {
                                double startOffset = n.HoldOffset;
                                n.HoldOffset -= noteSnap / (snapping * 2);
                                double tempDouble = n.HoldOffset;
                                snap(ref tempDouble, (int)(snapping * 2), true);
                                n.HoldOffset = tempDouble;
                            }
                        }
                        else
                        {
                            if (music.Speed - 10.0f < -70.0f)
                                music.Speed = -80.0f;
                            else
                            {
                                music.Speed -= 10.0f;
                            }
                            musicRateLabel.Text = "Rate: " + String.Format("{0:0.00}%", music.Speed + 100.0f);
                        }
                    }
                    else if (keyPress(Key.Space))
                    {
                        if (paused)
                        {
                            music.Position = currentOffset;
                            music.play(false);
                            paused = false;
                        }
                        else
                        {
                            paused = true;
                            music.pause();
                            snap(ref currentOffset, snapping, true);
                            music.Position = currentOffset;
                        }
                    }
                    else if ((keyHold(Key.ControlLeft) || keyHold(Key.ControlRight)) && keyPress(Key.S))
                    {
                        saveMap();
                    }
                    else if (keyPress(Key.Delete))
                    {
                        List<int> toRemove = new List<int>();
                        for (int x = 0; x < highlightedNotes.Count; x++)
                        {
                            chart.Notes.Remove(highlightedNotes[x]);
                            toRemove.Add(x);
                        }
                        for (int i = toRemove.Count - 1; i > -1; i--)
                        {
                            highlightedNotes.Remove(highlightedNotes[toRemove[i]]);
                        }
                        toRemove.Clear();
                    }
                    else if (keyPress(Key.H))
                    {
                        if (highlightedNotes.Count != 0)
                        {
                            foreach (Note n in highlightedNotes)
                            {
                                if (!n.Hold)
                                {
                                    n.Hold = true;
                                    n.HoldOffset = n.Offset + (noteSnap / snapping);
                                    n.setColor();
                                }
                                else
                                {
                                    n.Hold = false;
                                    n.HoldOffset = 0;
                                    highlightedHolds.Remove(n);
                                }
                            }
                        }
                        if (highlightedHolds.Count != 0)
                        {
                            foreach (Note n in highlightedHolds)
                            {
                                n.Hold = false;
                                n.HoldOffset = 0;
                                n.HoldHighlight = false;
                                highlightedHolds.Remove(n);
                                break;
                            }
                        }
                    }
                    #region note key input
                    for (int x = 0; x < 8; x++)
                    {
                        if (keyPress((Key)(110 + x)))
                        {
                            Note note = chart.Notes.Find(delegate(Note n)
                            {
                                return n.Offset == (int)currentOffset && n.Location == x + 1;
                            });
                            if (note == null)
                            {
                                string temp2 = "a";
                                int tempInt = 0;
                                if (Skin.NoteStyle == 1)
                                {
                                    if (x % 2 == 1)
                                    {
                                        tempInt++;
                                    }
                                }
                                else if (Skin.NoteStyle == 2)
                                {
                                    if ((x + 1) % 2 == 0)
                                    {
                                        tempInt++;
                                    }
                                    if (x + 1 == 4)
                                    {
                                        tempInt++;
                                    }
                                }
                                switch (tempInt)
                                {
                                    case 0:
                                        temp2 = "a";
                                        break;
                                    case 1:
                                        temp2 = "b";
                                        break;
                                    case 2:
                                        temp2 = "c";
                                        break;
                                }
                                note = new Note((int)currentOffset, x + 1, temp2);
                                note.XOffset = 317;
                                note.YOffset = 54;
                                chart.Notes.Add(note);
                            }
                            else
                            {
                                chart.Notes.Remove(note);
                            }
                        }
                        if (keyHold((Key)(110 + x)) && oldKeyState.Contains((Key)(110 + x)))
                        {
                            hold[x] = true;
                        }
                        else
                        {
                            hold[x] = false;
                        }
                    }
                    #endregion
                }
                #endregion
                lc = false;
                if (newMouse != null && oldMouse != null && newMouse.LeftButton && oldMouse.LeftButton)
                {
                    if (highlightedNotes.Count != 0)
                    {
                        foreach (Note n in highlightedNotes)
                        {
                            double moved = oldMouse.Y - newMouse.Y;
                            n.Offset = n.Offset + (moved * (1 / (usableHeight / moveTime)));
                            if (n.Hold)
                            {
                                n.HoldOffset = n.HoldOffset + (moved * (1 / (usableHeight / moveTime)));
                            }
                            if (keyHold(Key.ShiftLeft) || keyHold(Key.ShiftRight))
                            {
                                double startOffset = n.Offset;
                                double tempDouble = n.Offset;
                                snap(ref tempDouble, (int)(snapping * 2), true);
                                double offsetChange = tempDouble - startOffset;
                                n.Offset = Math.Floor(tempDouble);
                                if (n.Hold)
                                {
                                    n.HoldOffset += offsetChange;
                                }
                            }
                        }
                    }
                    if (highlightedHolds.Count != 0)
                    {
                        bool noteDrag = false;
                        foreach (Note n in highlightedHolds)
                        {
                            if (n.Holdbar.ModifiedBounds.Contains(new Point(newMouse.X, newMouse.Y)) || n.HoldEnd.ModifiedBounds.Contains(new Point(newMouse.X, newMouse.Y)) || n.HoldStart.ModifiedBounds.Contains(new Point(newMouse.X, newMouse.Y)))
                            {
                                noteDrag = true;
                            }
                        }
                        if (noteDrag)
                        {
                            foreach (Note n in highlightedHolds)
                            {
                                double moved = oldMouse.Y - newMouse.Y;
                                double newOff = n.HoldOffset + (moved * (1 / (usableHeight / moveTime)));
                                if (newOff > n.Offset)
                                {
                                    n.HoldOffset = newOff;
                                }
                                if (keyHold(Key.ShiftLeft) || keyHold(Key.ShiftRight))
                                {
                                    double tempDouble = n.HoldOffset;
                                    snap(ref tempDouble, (int)(snapping * 2), true);
                                    n.HoldOffset = tempDouble;
                                }
                                if (n.HoldOffset == n.Offset || n.HoldOffset < n.Offset)
                                {
                                    n.HoldOffset = n.Offset + 20;
                                }
                            }
                        }
                    }
                }
                bool canAdd = true;
                if (newMouse != null && oldMouse != null && newMouse.LeftButton && canpress && !oldMouse.LeftButton)
                {
                    lc = true;
                    canpress = false;
                    if (!keyHold(Key.ControlLeft) && !keyHold(Key.ControlRight))
                    {
                        foreach (Note h in highlightedHolds)
                        {
                            h.HoldHighlight = false;
                        }
                        foreach (Note h in highlightedNotes)
                        {
                            h.Highlight = false;
                        }
                        highlightedHolds.Clear();
                        highlightedNotes.Clear();
                    }
                }
                foreach (Note n in chart.Notes)
                {
                    if (((n.Offset - currentOffset < moveTime && n.Offset - currentOffset > clearOffset)) || (n.Hold && n.Offset - currentOffset < moveTime && n.HoldOffset - currentOffset > clearOffset))
                    {
                        offsetDifference = n.Offset - currentOffset;
                        distancedMoved = (moveTime - offsetDifference) * moveRate;
                        tempVertical = n.Vertical;
                        n.Vertical = distancedMoved;
                        if ((tempVertical < usableHeight && n.Vertical > usableHeight || n.Vertical == usableHeight) && !paused)
                        {
                            if (Config.HitVolume > 0)
                            {
                                sfx = AudioManager.loadFromMemory(hitsound);
                                double change = currentOffset - music.Position - (-7 * (music.Speed / 10));
                                Console.WriteLine(change);
                                if (change > 10 || change < -10)
                                {
                                    currentOffset += change;
                                    music.Position = currentOffset;
                                }
                                sfx.Volume = Config.HitVolume / 100f;
                                sfx.play(false);

                            }
                        }
                        if (n.Hold)
                        {
                            offsetDifference = n.HoldOffset - currentOffset;
                            distancedMoved = (moveTime - offsetDifference) * moveRate;
                            tempVertical = n.HoldVertical;
                            n.HoldVertical = distancedMoved;
                            if ((tempVertical < usableHeight && n.HoldVertical > usableHeight || n.HoldVertical == usableHeight) && !paused)
                            {
                                if (Config.HitVolume > 0)
                                {
                                    sfx = AudioManager.loadFromMemory(hitsound);
                                    sfx.Volume = Config.HitVolume / 100f;
                                    sfx.play(false);
                                }
                            }
                        }
                        if (lc)
                        {
                            if (n.Hold)
                            {
                                if ((n.Holdbar.ModifiedBounds.Contains(new Point(newMouse.X, newMouse.Y)) || n.HoldEnd.ModifiedBounds.Contains(new Point(newMouse.X, newMouse.Y))) && game.Focused && canAdd)
                                {
                                    if (!n.HoldHighlight)
                                    {
                                        n.HoldHighlight = true;
                                        if (!highlightedHolds.Contains(n))
                                        {
                                            highlightedHolds.Add(n);
                                        }
                                        canAdd = false;
                                    }
                                }
                                else if (!keyHold(Key.ControlLeft) || !keyHold(Key.ControlRight))
                                {
                                    n.HoldHighlight = false;
                                    highlightedHolds.Remove(n);
                                }
                            }
                            if (n.Texture.Frames[0].ModifiedBounds.Contains(new Point(newMouse.X, newMouse.Y)) && game.Focused && canAdd)
                            {
                                if (!n.Highlight)
                                {
                                    n.Highlight = true;
                                    if (!highlightedNotes.Contains(n))
                                    {
                                        highlightedNotes.Add(n);
                                    }
                                    canAdd = false;
                                }
                            }
                            else if (!keyHold(Key.ControlLeft) || keyHold(Key.ControlRight))
                            {
                                n.Highlight = false;
                                highlightedNotes.Remove(n);
                            }

                        }
                    }
                }
                foreach (SnapLine s in frameSnaps)
                {
                    if (s.Offset - currentOffset < moveTime && s.Offset - currentOffset > clearOffset)
                    {
                        s.visible = true;
                        offsetDifference = s.Offset - currentOffset;
                        distancedMoved = (moveTime - offsetDifference) * moveRate;
                        s.vertical = (int)distancedMoved;
                    }
                    else
                    {
                        s.visible = false;
                    }
                }
                foreach (SnapLine s in bookmarks)
                {
                    if (s.Offset - currentOffset < moveTime && s.Offset - currentOffset > clearOffset)
                    {
                        s.visible = true;
                        offsetDifference = s.Offset - currentOffset;
                        distancedMoved = (moveTime - offsetDifference) * moveRate;
                        s.vertical = (int)distancedMoved;
                    }
                    else
                    {
                        s.visible = false;
                    }
                }
                foreach (SnapLine s in timingLines)
                {
                    if (s.Offset - currentOffset < moveTime && s.Offset - currentOffset > clearOffset)
                    {
                        s.visible = true;
                        offsetDifference = s.Offset - currentOffset;
                        distancedMoved = (moveTime - offsetDifference) * moveRate;
                        s.vertical = (int)distancedMoved;
                    }
                    else
                    {
                        s.visible = false;
                    }
                }
                if (newMouse.LeftButton)
                {
                    canpress = true;
                }
                if (refresh)
                {
                    snap(ref currentOffset, 1, true);
                    setLines();
                    resnap();
                    refresh = false;
                }
            }
            else
            {
                //test
            }
        }
        private void setNoteOffsets()
        {
            foreach (Note n in chart.Notes)
            {
                n.XOffset = 317;
                n.YOffset = 54;
            }
        }
        private void resetNoteOffsets()
        {
            foreach (Note n in chart.Notes)
            {
                n.XOffset = 0;
                n.YOffset = 0;
            }
        }
        public bool saveMap()
        {
            if (System.Windows.Forms.MessageBox.Show("Do you wish to save?", "Save map", System.Windows.Forms.MessageBoxButtons.YesNoCancel) == System.Windows.Forms.DialogResult.Yes)
            {
                try
                {
                    DirectoryInfo d = new DirectoryInfo("songs\\" + currentSong.Dir);
                    foreach (FileInfo f in d.GetFiles())
                    {
                        if (f.Extension.Equals(".pnc"))
                        {
                            File.Delete(f.FullName);
                        }
                    }
                    if (currentSong.FileVersion == 0)
                    {
                        SongLibrary.songInfos[currentSong.Dir].FileVersion = 1;
                    }
                    currentSong.FileVersion = 1;
                    int diff = -1; 
                    foreach (var c in currentSong.Charts)
                    {
                        using (StreamWriter sr = new StreamWriter("songs\\" + currentSong.Dir + "\\" + currentSong.Artist + " - " + currentSong.SongName + " - " + c.Value.Name + " (" + currentSong.Creator + ").pnc", false))
                        {
                            sr.AutoFlush = false;
                            sr.WriteLine("pncv1");
                            sr.WriteLine("[song]");
                            sr.WriteLine("filename=" + currentSong.FileName);
                            sr.WriteLine("songname=" + currentSong.SongName);
                            sr.WriteLine("artist=" + currentSong.Artist);
                            sr.WriteLine("bg=" + c.Value.BgName);
                            sr.WriteLine("difficulty=" + c.Value.Name);
                            sr.WriteLine("leadin=" + c.Value.LeadInTime * 1000);
                            sr.WriteLine("preview=" + currentSong.Preview);
                            sr.WriteLine("creator=" + currentSong.Creator);
                            sr.WriteLine("judge=" + c.Value.Judgement);
                            string tags = "";
                            foreach (string s in c.Value.Tags)
                            {
                                if (!s.Equals(""))
                                {
                                    tags += s + ",";
                                }
                            }
                            sr.WriteLine("tags=" + tags);
                            int keys = 5;
                            foreach (Note n in c.Value.Notes)
                            {
                                if (n.Location > keys)
                                {
                                    keys = n.Location;
                                }
                            }
                            c.Value.Keys = keys;
                            sr.WriteLine("keys=" + keys);
                            sr.WriteLine("[timing]");
                            foreach (var pair in currentSong.Timings)
                            {
                                sr.WriteLine(pair.Key + "," + pair.Value.Snap.ToString(Config.cultureEnglish) + "," + pair.Value.ChangeSnap + "," + pair.Value.MoveTime);
                            }
                            sr.WriteLine("[editor]");
                            foreach (double o in currentSong.Bookmarks)
                            {
                                sr.WriteLine("bm," + o);
                            }
                            sr.WriteLine("[objects]");
                            c.Value.Notes.Sort(delegate(Note a, Note b)
                            {
                                return a.Offset.CompareTo(b.Offset);
                            });
                            foreach (TimingSection t in c.Value.Sections)
                            {
                                sr.WriteLine("t," + t.Offset + "," + t.MoveTime);
                            }
                            foreach (Note n in c.Value.Notes)
                            {
                                if (!n.Hold)
                                {
                                    sr.WriteLine("n," + Math.Round(n.Offset, 0) + "," + n.Location);
                                }
                                else
                                {
                                    sr.WriteLine("n," + Math.Round(n.Offset, 0) + "," + n.Location + ",true," + Math.Round(n.HoldOffset, 0));
                                }
                            }
                            sr.Flush();
                            sr.Close();
                        }
                        diff++;
                        SongLibrary.songInfos[currentSong.Dir].ChartMd5s[diff] = Utils.calcHash("songs\\" + currentSong.Dir + "\\" + currentSong.Artist + " - " + currentSong.SongName + " - " + c.Value.Name + " (" + currentSong.Creator + ").pnc");
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void snap(ref double off, int division, bool prev)
        {
            double offsetToUse = 0;
            int id = 0;
            for (int x = 0; x < currentSong.TimingsList.Count; x++)
            {
                if ((off > currentSong.TimingsList[x].Offset || off == currentSong.TimingsList[x].Offset) && currentSong.TimingsList[x].Offset > offsetToUse && currentSong.TimingsList[x].ChangeSnap)
                {
                    offsetToUse = currentSong.TimingsList[id].Offset;
                    id = x;
                }
            }
            if (offsetToUse == 0)
            {
                for (int s = 0; s < currentSong.TimingsList.Count; s++)
                {
                    if (currentSong.TimingsList[s].ChangeSnap)
                    {
                        id = s;
                    }
                }
            }
            offsetToUse = currentSong.TimingsList[id].Offset;
            double snap = currentSong.TimingsList[id].Snap / division;
            double prevOff = offsetToUse, nextOff = offsetToUse + snap;
            bool loop = true;
            if (off < prevOff && off < nextOff)
            {
                while (loop)
                {
                    if (prevOff > off && nextOff < off || off == nextOff || off == prevOff)
                    {
                        if (prev)
                        {
                            if (off - nextOff < snap / 2)
                            {
                                off = nextOff;
                            }
                            else
                            {
                                off = prevOff;
                            }
                        }
                        else
                        {
                            if (prevOff - off < snap / 2)
                            {
                                off = prevOff;
                            }
                            else
                            {
                                off = nextOff;
                            }
                        }
                        loop = false;
                    }
                    prevOff = nextOff;
                    nextOff = nextOff -= snap;
                }
            }
            else
            {
                while (loop)
                {
                    if (prevOff < off && nextOff > off || off == nextOff || off == prevOff)
                    {
                        if (prev)
                        {
                            if (nextOff - off < snap / 2)
                            {
                                off = nextOff;
                            }
                            else
                            {
                                off = prevOff;
                            }
                        }
                        else
                        {
                            if (prevOff - off > snap / 2)
                            {
                                off = prevOff;
                            }
                            else
                            {
                                off = nextOff;
                            }
                        }
                        loop = false;
                    }
                    prevOff = nextOff;
                    nextOff = nextOff += snap;
                }
            }
        }

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            bg.OnRenderFrame(e);
            overlay.OnRenderFrame(e);
            frame.draw(e);
            foreach (SnapLine l in frameSnaps)
            {
                if (l.visible)
                {
                    l.draw(e);
                }
            }
            foreach (SnapLine l in timingLines)
            {
                if (l.visible)
                {
                    l.draw(e);
                }
            }
            foreach (SnapLine l in bookmarks)
            {
                if (l.visible)
                {
                    l.draw(e);
                }
            }
            int lowerBound = 0;
            if (Config.EditMiddle)
            {
                lowerBound = 0 - (int)moveTime;
            }
            else
            {
                lowerBound = 0 - (int)(moveTime / 5);
            }
            foreach (Note n in chart.Notes)
            {
                if (((n.Offset - currentOffset < moveTime && n.Offset - currentOffset > lowerBound)) || (n.Hold && n.Offset - currentOffset < moveTime && n.HoldOffset - currentOffset > lowerBound))
                {
                    n.draw(e);
                }
            }
            base.OnRenderFrame(e);
        }

        private void setLines()
        {
            frameSnaps.Clear();
            timingLines.Clear();
            currentSong.remakeLists(chart);
            foreach (TimingSection s in currentSong.TimingsList)
            {
                SnapLine temp = new SnapLine(new Point(322, 0), new Point(322 + (int)frame.Width - 10, 0), s.Offset);
                if (s.ChangeSnap)
                {
                    temp.Colour = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
                }
                else
                {
                    temp.Colour = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
                }
                timingLines.Add(temp);
            }
            foreach (double d in currentSong.Bookmarks)
            {
                SnapLine temp = new SnapLine(new Point(322, 0), new Point(322 + (int)frame.Width - 10, 0), d);
                temp.Colour = new Color4(0.3f, 0.6f, 0.7f, 0.85f);
                bookmarks.Add(temp);
            }
            int divide = 0;
            double tempOff = currentOffset;
            snap(ref tempOff, 1, true);
            double offsetToUse = (tempOff - (sectionOffset - Math.Round(noteSnap, 6, MidpointRounding.ToEven)));
            double snapAmount = Math.Round(offsetToUse % noteSnap, 6, MidpointRounding.ToEven);
            double quart = (noteSnap / snapping);
            double tQuart = quart * 3;
            double half = quart * 2;
            quart = Math.Round(quart, 6, MidpointRounding.ToEven);
            tQuart = Math.Round(tQuart, 6, MidpointRounding.ToEven);
            half = Math.Round(half, 6, MidpointRounding.ToEven);
            snapAmount = Math.Round(snapAmount, 6);
            double snapToUse = Math.Round(noteSnap, 6);
            if (Math.Round(snapAmount, 6) != snapToUse && Math.Round(snapAmount, 6) != 0)
            {
                if ((snapAmount == quart && snapAmount != Math.Round(snapToUse / 2, 6)))
                {
                    divide = 4;
                }
                else if (snapAmount == tQuart)
                {
                    divide = 3;
                }
                else if (snapAmount == half)
                {
                    divide = 2;
                }
            }
            else if (snapAmount == 0 || snapAmount == snapToUse)
            {
                divide = 1;
            }
            double offset = sectionOffset;
            int reuse = divide;
            do
            {
                SnapLine temp = new SnapLine(new Point(322, 0), new Point(322 + (int)frame.Width - 10, 0), offset);
                switch (reuse)
                {
                    case 1:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        temp.thickness = 3;
                        if (snapping == 1)
                            reuse = 1;
                        else
                            reuse = 2;
                        break;
                    case 3:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 3)
                            reuse = 1;
                        else
                            reuse = 4;
                        break;
                    case 2:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 2)
                            reuse = 1;
                        else
                            reuse = 3;
                        break;
                    case 4:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 4)
                            reuse = 1;
                        else
                            reuse = 5;
                        break;
                    case 5:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        reuse = 6;
                        break;
                    case 6:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 6)
                            reuse = 1;
                        else
                            reuse = 7;
                        break;
                    case 7:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        reuse = 8;
                        break;
                    case 8:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        reuse = 1;
                        break;
                }
                frameSnaps.Add(temp);
            } while ((offset += quart) < endOffset);
            offset = sectionOffset;
            reuse = divide;
            do
            {
                SnapLine temp = new SnapLine(new Point(322, 0), new Point(322 + (int)frame.Width - 10, 0), offset);
                switch (reuse)
                {
                    case 1:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        temp.thickness = 3;
                        if (snapping == 1)
                            reuse = 1;
                        else
                            reuse = 2;
                        break;
                    case 3:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 3)
                            reuse = 1;
                        else
                            reuse = 4;
                        break;
                    case 2:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 2)
                            reuse = 1;
                        else
                            reuse = 3;
                        break;
                    case 4:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 4)
                            reuse = 1;
                        else
                            reuse = 5;
                        break;
                    case 5:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        reuse = 6;
                        break;
                    case 6:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        if (snapping == 6)
                            reuse = 1;
                        else
                            reuse = 7;
                        break;
                    case 7:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        reuse = 8;
                        break;
                    case 8:
                        temp.Colour = lineColors[snapping - 1][reuse - 1];
                        reuse = 1;
                        break;
                }
                frameSnaps.Add(temp);
            } while ((offset -= quart) > 0);
        }
        private Color4[][] lineColors = new Color4[][]
        {
            new Color4[]{Color4.White},
            new Color4[]{Color4.White, Color4.Blue},
            new Color4[]{Color4.White, Color4.LightGreen, Color4.Blue},
            new Color4[]{Color4.White, Color4.LightGreen, Color4.Blue, Color4.LightGreen},
            new Color4[0],
            new Color4[]{Color4.White, Color4.PapayaWhip, Color4.LightGreen, Color4.PapayaWhip, Color4.Blue, Color4.PapayaWhip},
            new Color4[0],
            new Color4[]{Color4.White, Color4.MintCream, Color4.LightGreen, Color4.MintCream, Color4.Blue, Color4.MintCream, Color4.LightGreen, Color4.MintCream}
        };
        private bool refresh = false;
        public void refreshGraphics()
        {
            refresh = true;
        }
    }
}