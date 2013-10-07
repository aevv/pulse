using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Pulse.UI;
using Pulse.Mechanics;
using System.IO;
using Pulse.Audio;
using System.Xml;
using System.Collections.Specialized;
using Pulse.Client;

namespace Pulse.Screens
{
    class IngameScreen : Screen
    {
        ScoreScreen scoreScreen;
        Animation burst;
        public enum PlayType
        {
            TEST = 9,
            PLAY = 3,
            REPLAY = 5,
            AUTO = 10,
            SPECTATE = 4
        }
        AudioFX sfx;
        private PlayType playType;
        public PlayType PlayType1
        {
            get { return playType; }
            set { playType = value; }
        }
        private bool[] keys = new bool[8];
        private bool[] prevKey = new bool[8];
        private bool[] holding = new bool[8];
        private bool[] canHit = new bool[8];
        PauseScreen pauseScreen;
        Rect[] presses = new Rect[8];
        Rect[] lights = new Rect[8];
        Rect[] glows = new Rect[8];
        GraphicalText scoreLabel;
        GraphicalText accuracyLabel;
        GraphicalText comboText;
        Label offsetLabel;
        Label songtitle;
        Label songartist;
        Label countDown;
        Button skip;
        private int hp = 50;
        double scoreMod = 1;
        double hitWindow = 250;
        double endOffset = 0;
        double moveRate = 0;
        double startOffset = 0;
        string[] accsID = new string[4];
        Text userList;
        Score score;
        Replay currentReplay;
        public Replay CurrentReplay
        {
            get { return currentReplay; }
            set { currentReplay = value; }
        }
        bool recordingReplay;
        bool replay;
        bool running;
        bool paused;
        bool comboMove;
        bool scores;
        bool failed;
        bool notAdded;
        bool invert;
        bool forceScores;
        public bool Invert
        {
            get { return invert; }
            set { invert = value; }
        }
        bool pulse;
        public byte[] hitsound;
        public byte[] breaksound;
        List<Rect> burstRem = new List<Rect>();
        List<Animation> accRem = new List<Animation>();
        List<Animation> holdRem = new List<Animation>();
        List<Animation> holdBursts = new List<Animation>();
        List<int> offsets = new List<int>();
        List<Note> activeNotes = new List<Note>();
        List<Animation> bursts = new List<Animation>();
        double time1;
        double time2;
        double time3;
        double sectionSnap;
        double currentOffset;
        double scoreFadeTime;
        int difficulty;
        int keygroup = 3;
        public int Difficulty
        {
            get { return difficulty; }
            set { difficulty = value; }
        }
        private Mods mods;
        public Mods Mods
        {
            get { return mods; }
            set { mods = value; }
        }
        Frame frame;
        Label bufferingLabel;
        Rect bufferOverlay;
        Rect tlContainer;
        Rect hpBar;
        Rect overlay;
        Rect glow;
        Rect bg;
        Rect accBurst;
        private bool calibrate = false;
        public bool Calibrate
        {
            get { return calibrate; }
            set
            {
                calibrate = value;
            }
        }
        private Song currentSong;
        public Song CurrentSong
        {
            get { return currentSong; }
            set { currentSong = value; }
        }
        private Chart chart;
        public Chart Chart
        {
            get
            {
                return chart;
            }
            set
            {
                chart = value;
            }
        }

        public IngameScreen(Game game, string name)
            : base(game, name)
        {
        }
        private void setNoteOffsets(Point loc)
        {
            foreach (Note n in chart.Notes)
            {
                n.XOffset = loc.X;
                n.YOffset = loc.Y;
            }
        }
        static public bool first = true;
        public void loadSong(Song song, int difficulty, Mods m, Replay replay, PlayType t)
        {
            for (int i = 0; i < 8; i++)
            {
                holding[i] = false;
                canHit[i] = true;
            }
            first = true;
            specFailed = false;
            scoreMod = 1;
            hp = 100;
            notAdded = true;
            mods = m;
            currentReplay = null;
            playType = t;
            currentSong = song;
            this.difficulty = difficulty;
            chart = song.Charts[difficulty];
            hitWindow = 50;
            hitWindow = hitWindow + (chart.Judgement * 20);
            keygroup = chart.Keys - 5;
            switch (t)
            {
                case PlayType.PLAY:
                    recordingReplay = true;
                    this.replay = false;
                    currentReplay = new Replay();
                    this.currentReplay.Mods = m;
                    currentReplay.Mods = mods;
                    if (Config.Spectating)
                    {
                        Config.Spectating = false;
                        try
                        {
                            PacketWriter.sendSpectateCancel(Game.conn.Bw, Config.SpectatedUser);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case PlayType.REPLAY:
                    recordingReplay = false;
                    currentReplay = replay;
                    this.replay = true;
                    this.mods = replay.Mods;
                    foreach (var pair in currentReplay.PressTimings)
                    {
                        pair.value.value = false;
                    }
                    foreach (var pair in currentReplay.ReleaseTimings)
                    {
                        pair.value.value = false;
                    }
                    foreach (ReplayHit h in currentReplay.HitTimings)
                    {
                        h.Used = false;
                    }
                    break;
                case PlayType.TEST:
                    recordingReplay = false;
                    this.replay = false;
                    generateAutoReplay();
                    break;
                case PlayType.AUTO:
                    this.replay = true;
                    this.recordingReplay = false;
                    generateAutoReplay();
                    break;
                case PlayType.SPECTATE:
                    this.replay = true;
                    currentReplay = new Replay();
                    break;
            }

            Game.M.Music.stop();
            try
            {
                PacketWriter.sendSongStart(Game.conn.Bw, Account.currentAccount.AccountName, Utils.calcHash(chart.Path), song.Artist + " - " + song.SongName, (short)playType, mods.Flags, Convert.ToDouble(mods.Scroll, Config.cultureEnglish), mods.Speed);
            }
            catch
            {
            }
            try
            {
                breaksound = File.ReadAllBytes(Skin.skindict["combobreak"]);
                hitsound = File.ReadAllBytes(Skin.skindict["normal-hitnormal"]);
            }
            catch
            {
                Console.WriteLine("Error loading hitsounds, probably missing file");
            }
            if (currentSong.FileVersion == 0)
            {
                music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
            }
            else
            {
                music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName); //fix in future to load per chart
            }
            Music.Position = 0;
            Music.Volume = Config.Volume / 100.0f;
            currentOffset = -(chart.LeadInTime * 1000);
            endOffset = 0;
            startOffset = -1;
            foreach (Note n in chart.Notes)
            {
                n.Vertical = -20;
                if (n.Hold)
                {
                    n.HoldVertical = -20;
                }
                n.Enabled = true;
                n.Visible = true;
                n.setAlpha(1.0f);
                n.setColor();
                n.Texture.Fading = false;
                n.Holdbar.Fading = false;
                n.HoldEnd.Fading = false;
                if (startOffset < 0)
                    startOffset = n.Offset;
                if (n.Hold)
                {
                    if (n.HoldOffset > endOffset)
                    {
                        endOffset = n.HoldOffset;
                    }
                }
                else if (n.Offset > endOffset)
                {
                    endOffset = n.Offset;
                }
                if (n.Offset < startOffset)
                {
                    startOffset = n.Offset;
                }
            }
            endOffset += 1500;
            Dictionary<int, int> moves = new Dictionary<int, int>();
            foreach (TimingSection s in currentSong.TimingsList)
            {
                if (s.ChangeSnap)
                {
                    double move = 60 / (s.Snap / 1000);
                    if (move > 400)
                    {
                        move = 400;
                    }
                    double perc = (move / 400) * 100;
                    move = 3000 - (27 * perc);
                    try
                    {
                        moves.Add((int)s.Offset, (int)move);
                    }
                    catch { }
                }
            }
            List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>();
            list.Sort((f, s) =>
            {
                return f.Key.CompareTo(s.Key);
            });
            invert = false;
            if ((mods.Flags & 4) == 4)
            {
                invert = true;
            }
            if ((mods.Flags & 8) == 8)
            {
                scoreMod += 0.05;
            }
            List<int> keys = new List<int>(moves.Keys);
            List<int> offs = new List<int>(moves.Values);
            foreach (Note n in chart.Notes)
            {
                if (n.Offset < keys[0])
                {
                    n.MoveTime = (int)(offs[0] / (mods.Scroll > 5 ? mods.Scroll / 10 : mods.Scroll));
                }
            }
            foreach (Note n in chart.Notes)
            {
                foreach (var pair in moves)
                {
                    if (n.Offset >= pair.Key)
                    {
                        n.MoveTime = (int)(pair.Value / (mods.Scroll > 5 ? mods.Scroll / 10 : mods.Scroll));
                    }
                }
                if (invert)
                {
                    n.Location = 9 - n.Location;
                }
            }
            time1 = 0;
            time2 = 0;
            time3 = 0;
            finalScore = new Score();
        }
        private void generateAutoReplay()
        {
            currentReplay = new Replay();
            currentReplay.Mods = this.mods;
            foreach (Note n in Chart.Notes)
            {
                try
                {
                    currentReplay.HitTimings.Add(new ReplayHit((int)(n.Offset * (1 / mods.Speed)), 0, n.Location - 1, Replay.HitType.PERFECT));
                    currentReplay.PressTimings.Add(new Pair<int, Pair<int, bool>>((int)(n.Offset * (1 / mods.Speed)) - 15, new Pair<int, bool>(n.Location - 1, false)));
                    if (!n.Hold)
                    {
                        currentReplay.ReleaseTimings.Add(new Pair<int, Pair<int, bool>>((int)(n.Offset * (1 / mods.Speed)) + 75, new Pair<int, bool>(n.Location - 1, false)));
                    }
                    else
                    {
                        currentReplay.HitTimings.Add(new ReplayHit((int)(n.HoldOffset * (1 / mods.Speed)), 0, n.Location - 1, Replay.HitType.PERFECT));
                        currentReplay.ReleaseTimings.Add(new Pair<int, Pair<int, bool>>((int)(n.HoldOffset * (1 / mods.Speed)) - 15, new Pair<int, bool>(n.Location - 1, false)));
                    }
                }
                catch { }
            }
        }
        private void recPacket(short arg1, RecievePacket packet)
        {
            switch ((RecvHeaders)arg1)
            {
                case RecvHeaders.SPECTATE_HIT:
                    int off = (int)packet.info[0];
                    int diff = (int)packet.info[1];
                    int lane = (int)packet.info[2];
                    Replay.HitType t = (Replay.HitType)packet.info[3];
                    hitTimingsToAdd.Add(new ReplayHit(off, diff, lane, t));
                    break;
                case RecvHeaders.SPECTATE_PRESS:
                    int off2 = (int)packet.info[0];
                    int lane2 = (int)packet.info[1];
                    pressTimingsToAdd.Add(new Pair<int, Pair<int, bool>>(off2, new Pair<int, bool>(lane2, false)));
                    break;
                case RecvHeaders.SPECTATE_RELEASE:
                    int off3 = (int)packet.info[0];
                    int lane3 = (int)packet.info[1];
                    releaseTimingsToAdd.Add(new Pair<int, Pair<int, bool>>(off3, new Pair<int, bool>(lane3, false)));
                    break;
                case RecvHeaders.SPECTATE_HEARTBEAT:
                    int off4 = (int)packet.info[0];
                    int score = (int)packet.info[1];
                    int combo = (int)packet.info[2];
                    int hp = (int)packet.info[3];
                    float accu = (float)((double)packet.info[4]);
                    if (updateTime == -1234)
                    {
                        scoreUpdate = score;
                        comboUpdate = combo;
                        updateTime = off4;
                        hpUpdate = hp;
                        accUpdate = accu;
                    }
                    if (first)
                    {
                        running = true;
                        unpause();
                        music.Position = off4;
                        currentOffset = music.Position;
                        this.score.TotalScore = score;
                        this.score.Combo = combo;
                        this.hp = hp;
                        this.score.Accuracy = accu;
                        foreach (Note n in chart.Notes)
                        {
                            if (n.Offset < off4)
                            {
                                n.Enabled = false;
                            }
                        }
                        first = false;
                    }
                    bufferOffset = off4;
                    if (buffering)
                    {
                        if (bufferOffset - currentOffset > bufferAmount)
                        {
                            buffering = false;
                            unpause();
                        }
                    }
                    break;
                case RecvHeaders.SPECTATE_FINISH:
                    //set score
                    int sc = (int)packet.info[0];
                    int co = (int)packet.info[1];
                    int pr = (int)packet.info[2];
                    int gr = (int)packet.info[3];
                    int ok = (int)packet.info[4];
                    int ms = (int)packet.info[5];
                    int flags = (int)packet.info[6];
                    float acc = (float)((double)packet.info[7]);
                    bufferGap = 0;
                    bufferOffset = endOffset + 2000;
                    this.finalScore.TotalScore = sc;
                    this.finalScore.Combo = co;
                    this.finalScore.Perfects = pr;
                    this.finalScore.Goods = gr;
                    this.finalScore.Oks = ok;
                    this.finalScore.Misses = ms;
                    this.finalScore.Accuracy = acc;
                    break;
                case RecvHeaders.SPECTATE_FAIL:
                    bufferGap = 0;
                    bufferOffset = endOffset + 2000;
                    specFailed = true;
                    break;
            }
        }
        int scoreUpdate = 0, comboUpdate = 0, updateTime = -1234, hpUpdate = 0;
        float accUpdate = 0;
        bool specFailed = false;
        string oldSpecs = "";
        Score finalScore = new Score();
        int bufferGap = 1500;
        int bufferAmount = 1500;
        List<Pair<int, Pair<int, bool>>> releaseTimingsToAdd = new List<Pair<int, Pair<int, bool>>>();
        List<Pair<int, Pair<int, bool>>> pressTimingsToAdd = new List<Pair<int, Pair<int, bool>>>();
        List<ReplayHit> hitTimingsToAdd = new List<ReplayHit>();
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Game.conn.recvPacket += new Action<short, Client.RecievePacket>(recPacket);
            accsID[0] = Skin.skindict["miss"];
            accsID[1] = Skin.skindict["ok"];
            accsID[2] = Skin.skindict["great"];
            accsID[3] = Skin.skindict["perfect"];
            initUI();
        }
        public override void onSwitched()
        {
            base.onSwitched();
            initUI();
            holdBursts.Clear();
            bursts.Clear();
        }
        private void initUI()
        {
            if (skip != null)
            {
                UIComponents.Remove(skip);
            }
            skip = new Button(game, new Rectangle(400, 350, 200, 50), "Skip", delegate(int data)
            {
                running = true;
                unpause();
                music.Position = (long)startOffset - 2000;
                currentOffset = music.Position;
                skip.Visible = false;
                skip.Enabled = false;
            });
            UIComponents.Add(skip);
            if (startOffset < 2000)
            {
                skip.Visible = false;
                skip.Enabled = false;
            }
            else
            {
                skip.Visible = true;
                skip.Enabled = true;
            }
            bg = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            if (currentSong.FileVersion == 0)
            {
                bg.useTexture("songs\\" + currentSong.Dir + "\\" + currentSong.BgName);
                music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName);
            }
            else
            {
                bg.useTexture("songs\\" + currentSong.Dir + "\\" + chart.BgName);
                music = AudioManager.loadFromFile("songs\\" + currentSong.Dir + "\\" + currentSong.FileName); //fix in future to load per chart
            }
            forceScores = false;
            failed = false;
            scores = false;
            running = false;
            paused = false;
            UIComponents.Clear();
            score = new Score();
            score.Flags = (int)mods.Flags;
            scoreFadeTime = 0;
            pulse = false;
            Config.Editing = false;
            Skin.PlayFrame = new Frame(Skin.FrameLoc, chart.Keys);
            frame = Skin.PlayFrame;
            setNoteOffsets(Skin.FrameLoc);
            tlContainer = new Rect(new Rectangle(Config.ResWidth - 605, 0, 605, 150), Skin.skindict["tlContainer"]);
            hpBar = new Rect(new Rectangle((int)frame.Location.X + (int)frame.Width, 0, 10, (int)frame.HitHeight));
            adjustHp(0);
            overlay = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            overlay.Color = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            glow = new Rect(new Rectangle((int)frame.Location.X, (int)frame.HitHeight - 43, (int)frame.Width, 40), Skin.skindict["keyLight"]);
            glow.Color = Skin.PulseColour;
            for (int x = 0; x < lights.Length; x++)
            {
                presses[x] = new Rect(new Rectangle(frame.Location.X + frame.LaneLoc[x] + 5 + (1 * x), frame.Location.Y + 600, frame.LaneWidth[x] + 1, 50), Skin.skindict["press" + (x + 1)]);
                presses[x].fade(0.0f, 0.075);
                lights[x] = new Rect(new Rectangle(((frame.Location.X) + frame.LaneLoc[x]) + (1 * x) + 5 + 25, (int)frame.HitHeight - 53, 0, frame.LaneWidth[x]), Skin.skindict["keyLight"]);
                glows[x] = new Rect(new Rectangle(((frame.Location.X) + frame.LaneLoc[x]) + (1 * x) + 5, (int)frame.HitHeight - 203, frame.LaneWidth[x], 200), Skin.skindict["keyLight"]);
                glows[x].Color = new Color4(1.0f, 1.0f, 1.0f, 0.5f);
                lights[x].Color = Skin.LightColours[x];
                lights[x].fade(0.0f, 0.075);
                glows[x].fade(0.0f, 0.075);
            }
            songtitle = new Label(game, new Point(Config.ResWidth - 490, 10), currentSong.SongName + " - " + chart.Name);
            songartist = new Label(game, new Point(Config.ResWidth - 485, 60), currentSong.Artist);
            songtitle.TextTexture.TextFont = new Font("Myriad Pro", 35);
            songtitle.TextTexture.Shadow = false;
            songartist.TextTexture.Shadow = false;
            UIComponents.Add(songartist);
            UIComponents.Add(songtitle);
            if (calibrate)
            {
                offsetLabel = new Label(game, new Point(450, 85), "Average mistime: ");
                UIComponents.Add(offsetLabel);
                offsetLabel.Visible = true;
                offsetLabel.Enabled = true;
                offsets.Clear();
            }
            scoreLabel = new GraphicalText(score.TotalScore.ToString("D9"), Skin.ScoreLocation);
            scoreLabel.scale(new Size(30, 30), 0.1);
            comboText = new GraphicalText("0x", (new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString("0x", frame.LaneWidth[0])) / 2, frame.Location.Y + 200)));
            comboText.Visible = false;
            accuracyLabel = new GraphicalText("00.00%", Skin.AccLocation);
            pauseScreen = new PauseScreen(game, "pause", this);
            pauseScreen.OnLoad(null);
            countDown = new Label(game, new Point((int)frame.Location.X + (int)frame.Width + 20, (int)frame.HitHeight - 20), "00:00");
            countDown.TextTexture.TextFont = new Font("Myriad pro", 30);
            UIComponents.Add(countDown);
            scoreLabel.scale(new Size(Skin.ScoreSize.Width, Skin.ScoreSize.Height), 0.0);
            accuracyLabel.scale(new Size(Skin.AccSize.Width, Skin.AccSize.Height), 0.0);
            if (skip != null)
            {
                UIComponents.Add(skip);
            }
            bufferOverlay = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            bufferOverlay.Color = new Color4(0, 0, 0, 0.5f);
            bufferingLabel = new Label(game, new Point(Config.ResWidth / 2 - 100, 768 / 2 - 20), "Buffering...");
            specFailedLabel = new Label(game, new Point(Config.ResWidth / 2 - 100, 768 / 2 - 20), "User failed");
            userList = new Text(new Size(700, 100), new Point((int)frame.Width + 50, 70));
            oldSpecs = "";
        }
        private void burstAcc(int acc)
        {
            Rect tempTexture;
            tempTexture = new Rect(new Rectangle(frame.Location.X + (int)(frame.Width / 2) - 15, (int)frame.HitHeight - 250, 30, 10), accsID[acc]);
            tempTexture.Color = Skin.AccColours[acc];
            tempTexture.Lifespan = 0.20;
            tempTexture.Alpha = 0.0f;
            tempTexture.fade(1.0f, 0.07);
            tempTexture.scale(new Size((int)frame.Width - 20, 100), 0.07);
            tempTexture.move(new Point(frame.Location.X + 10, (int)frame.HitHeight - 300), 0.07);
            accBurst = tempTexture;
        }
        private void miss()
        {
            if (Config.HitVolume > 0 && score.Combo >= 30 && breaksound != null)
            {
                AudioFX sfx1 = AudioManager.loadFromMemory(breaksound);
                sfx1.Volume = Config.HitVolume / 100.0f; //add separate hitsound config setting later?
                sfx1.play(false);
            }
            score.Combo = 0;
            comboText.Visible = false;
            score.AccuracyTotal.Add(0);
        }
        private void adjustHp(int adjust)
        {
            hp += adjust;
            if (hp < 0)
            {
                if ((mods.Flags & 1) != 1)
                {
                    failed = true;
                }
                hp = 0;
            }
            else if (hp > 200)
            {
                hp = 200;
            }
            hpBar.Color = new Color4(1.0f - (hp / 200.0f), 0.0f, hp / 200.0f, 1.0f);
            hpBar.move(new Point((int)frame.Location.X + (int)frame.Width, (int)(0 + ((int)frame.HitHeight / 200.0) * (200.0 - hp))), 0.1);
            hpBar.scale(new Size(10, (int)frame.HitHeight - (0 + (int)(((int)frame.HitHeight / 200.0) * (200.0 - hp)))), 0.1);
        }
        public void pause()
        {
            if (running)
            {
                Music.pause();
            }
            paused = true;
            pauseScreen.onSwitched();
        }
        public void unpause()
        {
            if (running)
            {
                Music.play(false);
            }
            paused = false;
        }
        public void togglePause()
        {
            if (running)
            {
                if (music.Paused)
                    music.play(false);
                else
                    Music.pause();
            }
            paused = !paused;
            if (paused)
                pauseScreen.onSwitched();
        }
        double heartbeatTime = 0;
        public override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (currentOffset > updateTime && updateTime != -1234)
            {
                score.TotalScore = scoreUpdate;
                score.Combo = comboUpdate;
                hp = hpUpdate;
                score.Accuracy = accUpdate;
                updateTime = -1234;
            }
            if (skip.Visible == true && keyPress(Config.SkipKey))
            {
                running = true;
                unpause();
                music.Position = (long)startOffset - 2000;
                currentOffset = music.Position;
                skip.Visible = false;
                skip.Enabled = false;
            }
            if (!Config.Specs.Equals(oldSpecs) && Config.Spectating)
            {
                userList.Update(Config.Specs);
                oldSpecs = Config.Specs;
            }
            else if (!Config.SpecsOnMe.Equals(oldSpecs) && Config.Spectated)
            {
                userList.Update(Config.SpecsOnMe);
                oldSpecs = Config.SpecsOnMe;
            }
            if (startOffset - currentOffset < 2000)
            {
                skip.Visible = false;
                skip.Enabled = false;
            }
            if (!paused)
            {
                heartbeatTime += e.Time;
                time1 += e.Time;
                time2 += e.Time;
                time3 += e.Time;
                if (time3 > 0.5)
                {
                    time3 = 0;
                    double min = Math.Floor((endOffset - music.Position) / 60000);
                    double sec = Math.Floor((endOffset - music.Position) % 60000);
                    sec = Math.Floor(sec / 1000);
                    countDown.Text = min.ToString("0#") + ":" + sec.ToString("0#");
                }
            }
            if (time2 > 0.05 && !comboText.moving && comboMove)
            {
                comboMove = false;
                comboText.scale(new Size((int)(frame.LaneWidth[0] * 1.5), (int)(frame.LaneWidth[0] * 1.5)), 0.12);
                comboText.move(new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, (int)(frame.LaneWidth[0] * 1.5))) / 2, frame.Location.Y + 200), 0.12);
            }
            if (!running && !paused && !scores)
            {
                currentOffset += e.Time * 1000 * mods.Speed;
                if (time1 > chart.LeadInTime / mods.Speed)
                {
                    running = true;
                    unpause();
                    time1 = 0;
                }
            }
            if (failed)
            {
                pause();
                try
                {
                    if (running == true)
                    {
                        if (Config.Spectated)
                        {
                            PacketWriter.sendSpectateFail(Game.conn.Bw);
                        }
                        PacketWriter.sendSongStart(Game.conn.Bw, Account.currentAccount.AccountName, "", "", (short)2, 0, 0, 0);
                    }
                }
                catch
                {
                }
                running = false;
                scores = false;
                pauseScreen.Failed = true;
            }
            if (scores)
            {
                if (invert)
                {
                    invert = false;
                    foreach (Note n in chart.Notes)
                    {
                        if (Config.Mirror)
                        {
                            n.Location = 9 - n.Location;
                        }
                    }
                }
                music.Speed = 0.0f;
                scoreScreen.OnUpdateFrame(e);
                if (scoreScreen.Score == null)
                {
                    scoreScreen.Score = score;
                }
                scoreFadeTime += e.Time;
                if (scoreFadeTime > 0.025 && overlay.Color.A < 1.0f)
                {
                    float newAlpha = overlay.Color.A + 0.2f;
                    overlay.Color = new Color4(overlay.Color.R, overlay.Color.G, overlay.Color.B, newAlpha);
                    scoreFadeTime = 0;
                }
            }
            if (keyPress(Key.Escape))
            {
                if (buffering || specFailed)
                {
                    Game.setScreen(game.screens["selectScreen"]);
                    game.Title = "Pulse";
                    try
                    {
                        PacketWriter.sendSpectateCancel(Game.conn.Bw, Config.SpectatedUser);
                        Config.Spectating = false;
                        Config.SpectatedUser = "";
                        buffering = false;
                    }
                    catch
                    {

                    }
                }
                else if (!running && paused && !scores && !failed)
                {
                    unpause();
                    //time = 0;
                    //running = true;
                }
                else if (!running && !scores && !failed)
                {
                    pause();
                }
                else if (!scores && !failed)
                {
                    if (currentOffset > endOffset - 1510)
                    {
                        forceScores = true;
                    }
                    else
                    {
                        togglePause();
                    }
                }
                else if (playType == PlayType.PLAY && !scores && failed)
                {
                    foreach (Note n in Chart.Notes)
                    {
                        if (Config.Mirror)
                        {
                            n.Location = 9 - n.Location;
                        }
                    }
                    loadSong(CurrentSong, Difficulty, mods, null, playType);
                    onSwitched();
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (!holding[i])
                {
                    canHit[i] = true;
                }
            }
            if (!paused && running)
            {
                currentOffset = music.Position + Config.Offset + (-7 * (music.Speed / 10));
            }
            if (replay)
            {
                #region replay
                if (!paused)
                {
                    if (playType == PlayType.SPECTATE)
                    {
                        if (bufferOffset - currentOffset < bufferGap)
                        {
                            pause();
                            buffering = true;
                            bufferAmount += 250;
                        }
                    }
                    pulse = true;
                    foreach (TimingSection t in currentSong.TimingsList)
                    {
                        if (currentOffset > t.Offset)
                        {
                            if (sectionSnap != t.Snap)
                            {
                                sectionSnap = t.Snap;
                                glow.Bounds = new Rectangle(new Point((int)frame.Location.X, (int)frame.HitHeight - 43), new Size((int)frame.Width, 40));
                            }
                        }
                    }
                    foreach (var pair in currentReplay.PressTimings)
                    {
                        if (currentOffset > pair.key && !pair.value.value)
                        {
                            pair.value.value = true;
                            keys[pair.value.key] = true;
                            int temp = 0;
                            for (int x = 0; x < pair.value.key; x++)
                            {
                                temp += frame.LaneWidth[x];
                            }
                            lights[pair.value.key].fade(0.65f, 0.075);
                            lights[pair.value.key].move(new Point((int)frame.Location.X + (frame.LaneLoc[pair.value.key]) + (1 * pair.value.key) + 5, 96), 0.1);
                            lights[pair.value.key].scale(new Size(frame.LaneWidth[pair.value.key], (int)frame.HitHeight - 99), 0.1);
                            glows[pair.value.key].fade(0.65f, 0.075);
                            presses[pair.value.key].fade(1.0f, 0.05);
                        }
                    }
                    foreach (var pair in currentReplay.ReleaseTimings)
                    {
                        if (currentOffset > pair.key && !pair.value.value)
                        {
                            pair.value.value = true;
                            keys[pair.value.key] = false;
                            int temp = 0;
                            for (int x = 0; x < pair.value.key; x++)
                            {
                                temp += frame.LaneWidth[x];
                            }
                            lights[pair.value.key].fade(0.0f, 0.075);
                            lights[pair.value.key].move(new Point((int)frame.Location.X + (frame.LaneLoc[pair.value.key] + (1 * pair.value.key)) + 5 + 25, 596), 0.075);
                            lights[pair.value.key].scale(new Size(0, (int)frame.HitHeight - 599), 0.05);
                            glows[pair.value.key].fade(0.0f, 0.075);
                            presses[pair.value.key].fade(0.0f, 0.05);
                        }
                    }
                }
                else
                {
                    pulse = false;
                }
                if (playType != PlayType.TEST && (currentOffset > endOffset && !scores) || (music.Position == music.Length && !scores))
                {
                    scores = true;
                    if (playType == PlayType.SPECTATE)
                    {
                        this.score = finalScore;
                    }
                    try
                    {
                        PacketWriter.sendSongStart(Game.conn.Bw, Account.currentAccount.AccountName, "", "", (short)2, 0, 0, 0);
                    }
                    catch
                    {
                    }
                    scoreScreen = new ScoreScreen(game, "scores", currentReplay, currentSong, difficulty, false);
                    scoreScreen.OnLoad(null);
                    paused = false;
                    running = false;
                    SongInfo curr = SongLibrary.songInfos[CurrentSong.PncName];
                    String cstring = chart.Name;
                    score.chartName = chart.Name;
                    score.ArtistTitle = currentSong.Artist + " - " + currentSong.SongName;
                    score.dateString = DateTime.Now.ToString();
                    scoreScreen.Score = score;
                }
                if (!scores)
                {
                    foreach (ReplayHit r in currentReplay.HitTimings)
                    {
                        if (r.Used == false && currentOffset > r.NoteOffset - r.OffsetDifference)
                        {
                            Note temp = null;
                            bool hold = false;
                            foreach (Note n in chart.Notes)
                            {
                                if (n.Hold && n.HoldOffset + hitWindow < currentOffset)
                                {
                                    n.Enabled = false;
                                    n.setAlpha(0.0f);
                                    foreach (Animation a in holdBursts)
                                    {
                                        if (a.Bounds.X == ((int)frame.Location.X + (frame.LaneLoc[n.Location - 1]) + (1 * r.Lane)) + 5 - frame.LaneWidth[n.Location - 1] * 2)
                                        {
                                            a.Active = false;
                                            holdRem.Add(a);
                                        }
                                    }
                                }
                                else if (!n.Hold && n.Offset + hitWindow < currentOffset)
                                {
                                    n.Enabled = false;
                                    n.setAlpha(0.0f);
                                }
                                if (n.Offset == r.NoteOffset && n.Location == r.Lane + 1)
                                {
                                    temp = n;
                                    break;
                                }
                                else if (n.HoldOffset == r.NoteOffset && n.Location == r.Lane + 1)
                                {
                                    temp = n;
                                    hold = true;
                                    break;
                                }
                            }
                            r.Used = true;
                            if (r.Hit != Replay.HitType.MISS && r.Hit != Replay.HitType.HOLDMISS && temp != null)
                            {
                                score.Combo++;
                                //comboText.Location = new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, 50)) / 2, frame.Location.Y + 200);
                                comboText.move(new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0] * 2)) / 2, frame.Location.Y + 185), 0.02);
                                comboText.scale(new Size(frame.LaneWidth[0] * 2, frame.LaneWidth[0] * 2), 0.02);
                                if (score.Combo > 2)
                                {
                                    comboText.Visible = true;
                                }
                                comboMove = true;
                                time2 = 0;
                                if (Config.HitVolume > 0)
                                {
                                    sfx = AudioManager.loadFromMemory(hitsound);//.loadFromFile(Skin.skindict["normal-hitnormal"],true);
                                    sfx.Volume = Config.HitVolume / 100f; //add separate hitsound config setting later?
                                    sfx.play(false);
                                }
                                temp.Texture.fade(0.0f, 0.05);
                                if (!temp.Hold)
                                {
                                    temp.Enabled = false;
                                }
                                else if (temp.Hold && hold)
                                {
                                    temp.Enabled = false;
                                    temp.Texture.fade(0.0f, 0.05);
                                    temp.Holdbar.fade(0.0f, 0.05);
                                    temp.HoldStart.fade(0.0f, 0.05);
                                    temp.HoldEnd.fade(0.0f, 0.05);
                                    foreach (Animation a in holdBursts)
                                    {
                                        if (a.Bounds.X == ((int)frame.Location.X + (frame.LaneLoc[temp.Location - 1]) + (1 * r.Lane)) + 5 - frame.LaneWidth[temp.Location - 1] * 2)
                                        {
                                            a.Active = false;
                                            holdRem.Add(a);
                                        }
                                    }
                                }
                                else if (temp.Hold)
                                {
                                    holdBursts.Add(new Animation(new Rectangle(((int)frame.Location.X + (frame.LaneLoc[temp.Location - 1]) + (1 * r.Lane)) + 5 - frame.LaneWidth[temp.Location - 1] * 2, (int)frame.HitHeight - 25 - frame.LaneWidth[temp.Location - 1] * 2, frame.LaneWidth[temp.Location - 1] + frame.LaneWidth[temp.Location - 1] * 4, frame.LaneWidth[temp.Location - 1] + frame.LaneWidth[temp.Location - 1] * 4), Skin.HoldFrameRate, "holdBurst", 3, true, Skin.MaskHolds));
                                }
                                if (r.Hit == Replay.HitType.PERFECT)
                                {
                                    adjustHp(2);
                                    burstAcc(3);
                                    score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 3);
                                    score.AccuracyTotal.Add(3);
                                }
                                else if (r.Hit == Replay.HitType.GREAT)
                                {
                                    adjustHp(1);
                                    burstAcc(2);
                                    score.TotalScore += (score.Combo * 45) / 6;
                                    score.AccuracyTotal.Add(3.5f);
                                }
                                else if (r.Hit == Replay.HitType.GOOD)
                                {
                                    adjustHp(1);
                                    burstAcc(2);
                                    score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 2);
                                    score.AccuracyTotal.Add(2);
                                }
                                else if (r.Hit == Replay.HitType.OK)
                                {
                                    adjustHp(-3);
                                    burstAcc(1);
                                    score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 1);
                                    score.AccuracyTotal.Add(1);
                                }
                                if (!hold)
                                {
                                    burst = new Animation(new Rectangle(((int)frame.Location.X + (frame.LaneLoc[temp.Location - 1]) + (1 * r.Lane)) + 5 - frame.LaneWidth[temp.Location - 1] * 2, (int)frame.HitHeight - 25 - frame.LaneWidth[temp.Location - 1] * 2, frame.LaneWidth[temp.Location - 1] + frame.LaneWidth[temp.Location - 1] * 4, frame.LaneWidth[temp.Location - 1] + frame.LaneWidth[temp.Location - 1] * 4), Skin.BurstFrameRate, "burst", Skin.BurstFrameCount, false, Skin.MaskBursts);
                                    bursts.Add(burst);
                                }
                            }
                            else if (temp != null)
                            {
                                temp.Enabled = false;
                                temp.Texture.fade(0.0f, 0.1);
                                if (temp.Hold)
                                {
                                    temp.Holdbar.fade(0.0f, 0.1);
                                    temp.HoldEnd.fade(0.0f, 0.1);
                                    temp.HoldStart.fade(0.0f, 0.1);
                                    foreach (Animation a in holdBursts)
                                    {
                                        if (a.Bounds.X == ((int)frame.Location.X + (frame.LaneLoc[temp.Location - 1]) + (1 * r.Lane)) + 5 - frame.LaneWidth[temp.Location - 1] * 2)
                                        {
                                            a.Active = false;
                                            holdRem.Add(a);
                                        }
                                    }
                                }
                                burstAcc(0);
                                if (r.Hit == Replay.HitType.HOLDMISS)
                                {
                                    adjustHp(-24);
                                }
                                else
                                {
                                    adjustHp(-12);
                                }
                                miss();
                            }
                            //comboText.Location = new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, comboText.CharWidth)) / 2, frame.Location.Y + 200);
                        }
                    }
                    foreach (Note n in chart.Notes)
                    {
                        if (n.Offset - currentOffset > 20000)
                            break;
                        if (n.Texture.Colour.A <= 0 && running && n.Holdbar.Color.A <= 0)
                        {
                            n.Visible = false;
                        }
                        if (n.Offset - currentOffset < n.MoveTime && n.Visible)
                        {
                            moveRate = (double)(int)frame.HitHeight / n.MoveTime;
                            double temp = n.Offset - currentOffset;
                            double moved = (n.MoveTime - temp) * moveRate;
                            n.Vertical = moved;
                            if (n.Hold)
                            {
                                temp = n.HoldOffset - currentOffset;
                                moved = (n.MoveTime - temp) * moveRate;
                                n.HoldVertical = moved;
                            }
                            if (((Mods.Flags & 8) == 8) && n.Offset - currentOffset < n.MoveTime - (n.MoveTime / 3))
                            {
                                if (!n.Texture.Fading)
                                {
                                    n.Texture.fade(0.0f, 0.2 / (mods.Scroll > 5 ? mods.Scroll / 10 : mods.Scroll));
                                }
                                if (n.Hold && n.HoldOffset - currentOffset < n.MoveTime - (n.MoveTime / 3) && !n.Holdbar.Fading)
                                {
                                    n.Holdbar.fade(0.0f, 0.2);
                                    n.HoldEnd.fade(0.0f, 0.2);
                                    n.HoldStart.fade(0.0f, 0.2);
                                    foreach (Animation a in holdBursts)
                                    {
                                        if (a.Bounds.X == ((int)frame.Location.X + (frame.LaneLoc[n.Location - 1]) + (1 * n.Location - 1)) + 5 - frame.LaneWidth[n.Location - 1] * 2)
                                        {
                                            a.Active = false;
                                            holdRem.Add(a);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {

                }
                #endregion
            }
            else if (!replay)
            {
                if (Config.Spectated && heartbeatTime > 1 && !paused && !scores)
                {
                    try
                    {
                        PacketWriter.sendSpectateHeartbeat(Game.conn.Bw, (int)currentOffset, score.TotalScore, score.Combo, hp, score.Accuracy);
                        heartbeatTime = 0;
                    }
                    catch
                    {
                    }
                }
                #region play
                if (!paused)
                {
                    pulse = true;
                    foreach (TimingSection t in currentSong.TimingsList)
                    {
                        if (currentOffset > t.Offset)
                        {
                            if (sectionSnap != t.Snap)
                            {
                                sectionSnap = t.Snap;
                                glow.Bounds = new Rectangle(new Point((int)frame.Location.X + 5, (int)frame.HitHeight - 43), new Size((int)frame.Width - 3, 40));
                            }
                        }
                    }
                    if (recordingReplay)
                    {
                        for (int x = 0; x < chart.Keys; x++)
                        {
                            if (newKeyState.Contains(Config.keys[keygroup][x]) && !oldKeyState.Contains(Config.keys[keygroup][x]))
                            {
                                currentReplay.PressTimings.Add(new Pair<int, Pair<int, bool>>((int)currentOffset - 8, new Pair<int, bool>(x, false)));
                                if (Config.Spectated)
                                {
                                    PacketWriter.sendSpectatePress(Game.conn.Bw, (int)currentOffset, x);
                                }
                            }
                            else if (!newKeyState.Contains(Config.keys[keygroup][x]) && oldKeyState.Contains(Config.keys[keygroup][x]))
                            {
                                currentReplay.ReleaseTimings.Add(new Pair<int, Pair<int, bool>>((int)currentOffset - 8, new Pair<int, bool>(x, false)));
                                if (Config.Spectated)
                                {
                                    PacketWriter.sendSpectateRelease(Game.conn.Bw, (int)currentOffset, x);
                                }
                            }
                            if (newKeyState.Contains(Config.keys[keygroup][x]))
                            {
                                keys[x] = true;
                                if (lights[x].Color.A == 0.0f && !lights[x].Fading)
                                {
                                    lights[x].fade(0.65f, 0.075);
                                    lights[x].move(new Point(((frame.Location.X) + frame.LaneLoc[x]) + (1 * x) + 5, 96), 0.1);
                                    lights[x].scale(new Size(frame.LaneWidth[x], (int)frame.HitHeight - 99), 0.1);
                                    glows[x].fade(0.65f, 0.075);
                                }
                                if (!presses[x].Fading && presses[x].Color.A == 0.0f)
                                {
                                    presses[x].fade(1.0f, 0.05);
                                }
                            }
                            else
                            {
                                keys[x] = false;
                                if (!lights[x].Fading && lights[x].Color.A > 0.0f)
                                {
                                    lights[x].fade(0.0f, 0.075);
                                    lights[x].move(new Point(((frame.Location.X) + frame.LaneLoc[x]) + (1 * x) + 5 + 25, 596), 0.075);
                                    lights[x].scale(new Size(0, (int)frame.HitHeight - 599), 0.05);
                                    glows[x].fade(0.0f, 0.075);
                                }
                                if (!presses[x].Fading && presses[x].Color.A > 0.0f)
                                {
                                    presses[x].fade(0.0f, 0.05);
                                }
                            }
                        }
                    }
                }
                else
                {
                    pulse = false;
                }
                if ((playType == PlayType.PLAY && !scores && (currentOffset > endOffset || music.Position == music.Length)) || forceScores)
                {
                    scores = true;
                    try
                    {
                        PacketWriter.sendSongStart(Game.conn.Bw, Account.currentAccount.AccountName, "", "", (short)2, 0, 0, 0);
                    }
                    catch
                    {
                    }
                    paused = false;
                    running = false;
                    if (notAdded)
                    {
                        try
                        {
                            if (Config.Spectated)
                            {
                                PacketWriter.sendSpectateFinish(Game.conn.Bw, score.TotalScore, score.MaxCombo, score.Perfects, score.Goods, score.Oks, score.Misses, (int)mods.Flags, (double)score.Accuracy);
                            }
                        }
                        catch { }
                        if (!calibrate)
                        {
                            SongInfo curr = SongLibrary.songInfos[CurrentSong.PncName];
                            String cstring = chart.Name;
                            score.chartName = chart.Name;
                            score.ArtistTitle = currentSong.Artist + " - " + currentSong.SongName;
                            score.dateString = DateTime.Now.ToString();
                            try
                            {
                                if (CurrentSong.FileVersion == 1)
                                {
                                    string fileName = ScoreLibrary.getFileFromDiff(curr, cstring);
                                    string hash = Utils.calcHash(fileName);
                                    string sName = "replay\\" + hash + ".psf";
                                    string rName;
                                    XmlDocument rep = new XmlDocument();
                                    XmlDeclaration d = rep.CreateXmlDeclaration("1.0", null, "yes");
                                    rep.AppendChild(d);
                                    XmlElement r = rep.CreateElement("replay");
                                    r.SetAttribute("Version", "1");
                                    XmlElement m = rep.CreateElement("m");
                                    XmlElement scr = rep.CreateElement("c");
                                    scr.InnerText = "" + Convert.ToDouble(mods.Scroll, Config.cultureEnglish);
                                    m.AppendChild(scr);
                                    XmlElement speed = rep.CreateElement("s");
                                    speed.InnerText = "" + mods.Speed;
                                    m.AppendChild(speed);
                                    XmlElement flags = rep.CreateElement("f");
                                    flags.InnerText = "" + mods.Flags;
                                    m.AppendChild(flags);
                                    r.AppendChild(m);
                                    foreach (ReplayHit h in currentReplay.HitTimings)
                                    {
                                        XmlElement t = rep.CreateElement("h");
                                        XmlElement hit = rep.CreateElement("t");
                                        hit.InnerText = "" + (int)h.Hit;
                                        t.AppendChild(hit);
                                        XmlElement lane = rep.CreateElement("l");
                                        lane.InnerText = "" + h.Lane;
                                        t.AppendChild(lane);
                                        XmlElement o = rep.CreateElement("o");
                                        o.InnerText = "" + h.NoteOffset;
                                        t.AppendChild(o);
                                        XmlElement di = rep.CreateElement("d");
                                        di.InnerText = "" + h.OffsetDifference;
                                        t.AppendChild(di);
                                        r.AppendChild(t);
                                    }
                                    foreach (Pair<int, Pair<int, bool>> h in currentReplay.PressTimings)
                                    {
                                        XmlElement p = rep.CreateElement("p");
                                        XmlElement o = rep.CreateElement("o");
                                        o.InnerText = "" + h.key;
                                        p.AppendChild(o);
                                        XmlElement l = rep.CreateElement("l");
                                        l.InnerText = "" + h.value.key;
                                        p.AppendChild(l);
                                        r.AppendChild(p);
                                    }
                                    foreach (Pair<int, Pair<int, bool>> h in currentReplay.ReleaseTimings)
                                    {
                                        XmlElement p = rep.CreateElement("r");
                                        XmlElement o = rep.CreateElement("o");
                                        o.InnerText = "" + h.key;
                                        p.AppendChild(o);
                                        XmlElement l = rep.CreateElement("l");
                                        l.InnerText = "" + h.value.key;
                                        p.AppendChild(l);
                                        r.AppendChild(p);
                                    }
                                    rep.AppendChild(r);
                                    if (File.Exists(sName))
                                    {
                                        Console.WriteLine("1237");
                                        rName = "replay\\r\\" + Config.UnixTime() + " " + hash + ".pcr";
                                        if (!Directory.Exists("replay\\r"))
                                        {
                                            Directory.CreateDirectory("replay\\r");
                                        }
                                        rep.Save(rName);
                                        score.ReplayName = Config.UnixTime() + " " + hash;
                                        List<Score> reconstructed = ScoreLibrary.reconstruct(sName);
                                        if (reconstructed.Count < 8)
                                        {
                                            reconstructed.Add(score);
                                            ScoreLibrary.serializeScores(reconstructed, sName);
                                        }
                                        else
                                        {
                                            Comparison<Score> scoreCompare = new Comparison<Score>(ScoreLibrary.CompareScores);
                                            reconstructed.Sort(scoreCompare);
                                            if (reconstructed[reconstructed.Count - 1].TotalScore < score.TotalScore)
                                            {
                                                File.Delete("replay\\r\\" + reconstructed[reconstructed.Count - 1].ReplayName);
                                                reconstructed.RemoveAt(reconstructed.Count - 1);
                                                reconstructed.Add(score);
                                                ScoreLibrary.serializeScores(reconstructed, sName);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        List<Score> newL = new List<Score>();
                                        newL.Add(score);
                                        ScoreLibrary.serializeScores(newL, sName);
                                        rName = "replay\\r\\" + Config.UnixTime() + " " + hash + ".pcr";
                                        Directory.CreateDirectory("replay\\r");
                                        rep.Save(rName);
                                        score.ReplayName = Config.UnixTime() + " " + hash;
                                        newL = new List<Score>();
                                        newL.Add(score);
                                        ScoreLibrary.serializeScores(newL, sName);
                                    }
                                    if (Account.currentAccount != null)
                                    {
                                        NameValueCollection nvc = new NameValueCollection();
                                        nvc.Add("u", Account.currentAccount.AccountName);
                                        nvc.Add("p", Account.currentAccount.passHash);
                                        nvc.Add("c", hash);
                                        XmlDocument doc = new XmlDocument();
                                        XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, "yes");
                                        doc.AppendChild(dec);
                                        XmlElement root = doc.CreateElement("score");
                                        root.SetAttribute("Version", "1");
                                        XmlElement oks = doc.CreateElement("oks");
                                        oks.InnerText = score.Oks.ToString();
                                        root.AppendChild(oks);
                                        XmlElement perfects = doc.CreateElement("perfects");
                                        perfects.InnerText = score.Perfects.ToString();
                                        root.AppendChild(perfects);
                                        XmlElement goods = doc.CreateElement("goods");
                                        goods.InnerText = score.Goods.ToString();
                                        root.AppendChild(goods);
                                        XmlElement misses = doc.CreateElement("misses");
                                        misses.InnerText = score.Misses.ToString();
                                        root.AppendChild(misses);
                                        XmlElement maxC = doc.CreateElement("maxc");
                                        maxC.InnerText = score.MaxCombo.ToString();
                                        root.AppendChild(maxC);
                                        XmlElement totalScore = doc.CreateElement("totalscore");
                                        totalScore.InnerText = score.TotalScore.ToString();
                                        root.AppendChild(totalScore);
                                        XmlElement acc = doc.CreateElement("acc");
                                        acc.InnerText = score.Accuracy.ToString(Config.cultureEnglish);
                                        root.AppendChild(acc);
                                        XmlElement flag = doc.CreateElement("flags");
                                        flag.InnerText = "" + score.Flags;
                                        root.AppendChild(flag);
                                        doc.AppendChild(root);
                                        doc.Save("tmp.xml");
                                        UploadClass uc = new UploadClass();
                                        uc.uploadDone += new Action<string, int>(uc_uploadDone);
                                        try
                                        {
                                            uc.HttpUploadFileAsync("http://p.ulse.net/scoresubmit", nvc, rName, "replay", "text/xml", "tmp.xml", "score", "text/xml");
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) { Console.WriteLine("Score submit failed: " + ex.Message); }
                        }
                        notAdded = false;
                        scoreScreen = new ScoreScreen(game, "scores", currentReplay, currentSong, difficulty, true);
                        scoreScreen.OnLoad(null);
                    }
                }
                if (!scores)
                {
                    foreach (Note n in chart.Notes)
                    {
                        if (n.Offset - currentOffset > 20000)
                            break;
                        if (n.Texture.Colour.A <= 0 && running && n.Offset - currentOffset > -hitWindow && n.Holdbar.Color.A <= 0 && !holding[n.Location - 1])
                        {
                            n.Visible = false;
                        }
                        if (n.Offset - currentOffset < n.MoveTime && n.Visible && n.Enabled)
                        {
                            moveRate = (double)(int)frame.HitHeight / n.MoveTime;
                            double temp = n.Offset - currentOffset;
                            double moved = (n.MoveTime - temp) * moveRate;
                            n.Vertical = moved;
                            if (n.Hold)
                            {
                                temp = n.HoldOffset - currentOffset;
                                moved = (n.MoveTime - temp) * moveRate;
                                n.HoldVertical = moved;
                            }
                            if (((Mods.Flags & 8) == 8) && n.Offset - currentOffset < n.MoveTime - (n.MoveTime / 3))
                            {
                                if (!n.Texture.Fading)
                                {
                                    n.Texture.fade(0.0f, 0.2 / (mods.Scroll > 5 ? mods.Scroll / 10 : mods.Scroll));
                                }
                                if (n.Hold && n.HoldOffset - currentOffset < n.MoveTime - (n.MoveTime / 3) && !n.Holdbar.Fading)
                                {
                                    n.Holdbar.fade(0.0f, 0.2);
                                    n.HoldEnd.fade(0.0f, 0.2);
                                    n.HoldStart.fade(0.0f, 0.2);
                                }
                            }
                        }
                        if (n.Enabled)
                        {
                            bool added = false;
                            if ((n.Offset - currentOffset < n.MoveTime && n.Offset - currentOffset > -hitWindow) || holding[n.Location - 1])
                            {
                                if (!Music.Paused)
                                {
                                    if (canHit[n.Location - 1])
                                    {
                                        if (keys[n.Location - 1] && !prevKey[n.Location - 1])
                                        {
                                            if (n.Offset - currentOffset < hitWindow && n.Offset - currentOffset > -hitWindow)
                                            {
                                                if (calibrate)
                                                {
                                                    offsets.Add((int)currentOffset - (int)n.Offset);
                                                    offsetLabel.Text = "Average mistime: " + Math.Round(offsets.Average(), 3);
                                                }
                                                comboText.move(new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0] * 2)) / 2, frame.Location.Y + 185), 0.02);
                                                comboText.scale(new Size(frame.LaneWidth[0] * 2, frame.LaneWidth[0] * 2), 0.02);
                                                comboMove = true;
                                                canHit[n.Location - 1] = false;
                                                if (Config.HitVolume > 0 && hitsound != null)
                                                {
                                                    sfx = AudioManager.loadFromMemory(hitsound);//.loadFromFile(Skin.skindict["normal-hitnormal"],true);
                                                    sfx.Volume = Config.HitVolume / 100f; //add separate hitsound config setting later?
                                                    sfx.play(false);
                                                }
                                                if (!added)
                                                {
                                                    Replay.HitType hit = Replay.HitType.MISS;
                                                    added = true;

                                                    score.Combo++;
                                                    if (n.Offset - currentOffset < (hitWindow - ((hitWindow / 100) * 70)) && n.Offset - currentOffset > -(hitWindow - ((hitWindow / 100) * 70)))
                                                    {
                                                        adjustHp(2);
                                                        burstAcc(3);
                                                        score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 3);
                                                        score.AccuracyTotal.Add(3);
                                                        hit = Replay.HitType.PERFECT;
                                                    }
                                                    else if (n.Offset - currentOffset < (hitWindow - ((hitWindow / 100) * 40)) && n.Offset - currentOffset > -(hitWindow - ((hitWindow / 100) * 40)))
                                                    {
                                                        adjustHp(1);
                                                        burstAcc(2);
                                                        score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 2);
                                                        score.AccuracyTotal.Add(2);
                                                        hit = Replay.HitType.GOOD;
                                                    }
                                                    else if (n.Offset - currentOffset < (hitWindow - ((hitWindow / 100) * 10)) && n.Offset - currentOffset > -(hitWindow - ((hitWindow / 100) * 10)))
                                                    {
                                                        adjustHp(-3);
                                                        burstAcc(1);
                                                        score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 1);
                                                        score.AccuracyTotal.Add(1f);
                                                        hit = Replay.HitType.OK;
                                                    }
                                                    if (hit != Replay.HitType.MISS)
                                                    {
                                                        currentReplay.HitTimings.Add(new ReplayHit((int)n.Offset, (int)n.Offset - (int)currentOffset, n.Location - 1, hit));
                                                        if (Config.Spectated)
                                                        {
                                                            PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.Offset, (int)(n.Offset - currentOffset), n.Location - 1, (int)hit);
                                                        }
                                                        burst = new Animation(new Rectangle(((int)frame.Location.X + (frame.LaneLoc[n.Location - 1]) + (1 * n.Location - 1)) + 5 - frame.LaneWidth[n.Location - 1] * 2, (int)frame.HitHeight - 25 - frame.LaneWidth[n.Location - 1] * 2, frame.LaneWidth[n.Location - 1] + frame.LaneWidth[n.Location - 1] * 4, frame.LaneWidth[n.Location - 1] + frame.LaneWidth[n.Location - 1] * 4), Skin.BurstFrameRate, "burst", Skin.BurstFrameCount, false, Skin.MaskBursts);
                                                        bursts.Add(burst);
                                                    }
                                                }
                                                if (!n.Hold)
                                                {
                                                    n.Enabled = false;
                                                    n.Texture.fade(0.0f, 0.05);
                                                }
                                                else
                                                {
                                                    holding[n.Location - 1] = true;
                                                    n.Texture.fade(0.0f, 0.05);
                                                    holdBursts.Add(new Animation(new Rectangle(((int)frame.Location.X + (frame.LaneLoc[n.Location - 1]) + (1 * n.Location)) + 5 - frame.LaneWidth[n.Location - 1] * 2, (int)frame.HitHeight - 25 - frame.LaneWidth[n.Location - 1] * 2,
                                                        frame.LaneWidth[n.Location - 1] + frame.LaneWidth[n.Location - 1] * 4, frame.LaneWidth[n.Location - 1] + frame.LaneWidth[n.Location - 1] * 4), Skin.HoldFrameRate, "holdBurst", 3, true, Skin.MaskHolds));
                                                }
                                            }
                                            else if (n.Offset - currentOffset < ((double)hitWindow * 1.1) && n.Offset - currentOffset > (double)(hitWindow * 0.9))
                                            {
                                                n.Enabled = false;
                                                n.Texture.fade(0.0f, 0.1);
                                                if (n.Hold)
                                                {
                                                    n.Holdbar.fade(0.0f, 0.1);
                                                    n.HoldEnd.fade(0.0f, 0.1);
                                                    n.HoldStart.fade(0.0f, 0.1);
                                                }
                                                burstAcc(0);
                                                if (!added)
                                                {
                                                    if (!n.Hold)
                                                    {
                                                        currentReplay.HitTimings.Add(new ReplayHit((int)n.Offset, (int)n.Offset - (int)currentOffset, n.Location - 1, Replay.HitType.MISS));
                                                        if (Config.Spectated)
                                                        {
                                                            PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.Offset, (int)(n.Offset - currentOffset), n.Location - 1, (int)Replay.HitType.MISS);
                                                        }
                                                        adjustHp(-12);
                                                    }
                                                    else
                                                    {
                                                        currentReplay.HitTimings.Add(new ReplayHit((int)n.Offset, (int)n.Offset - (int)currentOffset, n.Location - 1, Replay.HitType.HOLDMISS));
                                                        if (Config.Spectated)
                                                        {
                                                            PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.Offset, (int)(n.Offset - currentOffset), n.Location - 1, (int)Replay.HitType.HOLDMISS);
                                                        }
                                                        adjustHp(-24);
                                                    }
                                                    miss();
                                                    added = true;
                                                }
                                            }
                                            //Console.WriteLine("Current: " + currentOffset + ", Note: " + n.Offset + ", Diff: " + (currentOffset - n.Offset));
                                            comboText.Location = new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0])) / 2, frame.Location.Y + 200);
                                            if (score.Combo > 2)
                                            {
                                                comboText.Visible = true;
                                            }
                                        }
                                    }
                                    else if (!canHit[n.Location - 1] && n.Hold && holding[n.Location - 1])
                                    {
                                        //display hold success texture/effect when implemented                                    
                                        if (!keys[n.Location - 1] && prevKey[n.Location - 1])
                                        {
                                            foreach (Animation a in holdBursts)
                                            {
                                                if (a.Bounds.X == ((int)frame.Location.X + (frame.LaneLoc[n.Location - 1]) + (1 * n.Location)) + 5 - frame.LaneWidth[n.Location - 1] * 2)
                                                {
                                                    a.Active = false;
                                                    holdRem.Add(a);
                                                }
                                            }
                                            if (n.HoldOffset - currentOffset < hitWindow * 2 && n.HoldOffset - currentOffset > -hitWindow * 2)
                                            {
                                                comboText.move(new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0] * 2)) / 2, frame.Location.Y + 185), 0.03);
                                                comboText.scale(new Size(frame.LaneWidth[0] * 2, frame.LaneWidth[0] * 2), 0.03);
                                                comboMove = true;
                                                if (Config.HoldHitsounds && Config.HitVolume > 0 && hitsound != null)
                                                {
                                                    sfx = AudioManager.loadFromMemory(hitsound);//.loadFromFile(Skin.skindict["normal-hitnormal"],true);
                                                    sfx.Volume = Config.HitVolume / 100f; //add separate hitsound config setting later?
                                                    sfx.play(false);
                                                }
                                                if (!added)
                                                {
                                                    Replay.HitType hit = Replay.HitType.MISS;
                                                    if (n.HoldOffset - currentOffset < (hitWindow - ((hitWindow / 100) * 70)) * 2 && n.HoldOffset - currentOffset > -(hitWindow - ((hitWindow / 100) * 70)) * 2)
                                                    {
                                                        adjustHp(2);
                                                        burstAcc(3);
                                                        score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 3);
                                                        score.AccuracyTotal.Add(3);
                                                        hit = Replay.HitType.PERFECT;
                                                    }
                                                    else if (n.HoldOffset - currentOffset < (hitWindow - ((hitWindow / 100) * 40)) * 2 && n.HoldOffset - currentOffset > -(hitWindow - ((hitWindow / 100) * 40)) * 2)
                                                    {
                                                        adjustHp(1);
                                                        burstAcc(2);
                                                        score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 2);
                                                        score.AccuracyTotal.Add(2);
                                                        hit = Replay.HitType.GOOD;
                                                    }
                                                    else if (n.HoldOffset - currentOffset < (hitWindow - ((hitWindow / 100) * 10)) * 2 && n.HoldOffset - currentOffset > -(hitWindow - ((hitWindow / 100) * 10)) * 2)
                                                    {
                                                        adjustHp(-3);
                                                        burstAcc(1);
                                                        score.TotalScore += (int)(scoreMod * ((score.Combo * 9) / 6.5) * 3);
                                                        score.AccuracyTotal.Add(1);
                                                        hit = Replay.HitType.OK;
                                                    }
                                                    if (hit != Replay.HitType.MISS)
                                                    {
                                                        currentReplay.HitTimings.Add(new ReplayHit((int)n.HoldOffset, (int)n.HoldOffset - (int)currentOffset, n.Location - 1, hit));
                                                        if (Config.Spectated)
                                                        {
                                                            PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.HoldOffset, (int)(n.HoldOffset - currentOffset), n.Location - 1, (int)hit);
                                                        }
                                                        score.Combo++;
                                                        added = true;
                                                        if (score.Combo > 2)
                                                        {
                                                            comboText.Visible = true;
                                                        }
                                                    }
                                                }
                                                n.Enabled = false;
                                                n.Texture.fade(0.0f, 0.05);
                                                n.Holdbar.fade(0.0f, 0.05);
                                                n.HoldStart.fade(0.0f, 0.05);
                                                n.HoldEnd.fade(0.0f, 0.05);
                                                holding[n.Location - 1] = false;
                                            }
                                            else if (n.Offset - currentOffset < 0)
                                            {
                                                burstAcc(0);
                                                adjustHp(-12);
                                                currentReplay.HitTimings.Add(new ReplayHit((int)n.Offset, (int)n.Offset - (int)currentOffset, n.Location - 1, Replay.HitType.MISS));
                                                if (Config.Spectated)
                                                {
                                                    PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.Offset, (int)(n.Offset - currentOffset), n.Location - 1, (int)Replay.HitType.MISS);
                                                }
                                                if (!added)
                                                {
                                                    miss();
                                                }
                                                n.Enabled = false;
                                                n.Texture.fade(0.0f, 0.1);
                                                n.Holdbar.fade(0.0f, 0.1);
                                                n.HoldEnd.fade(0.0f, 0.1);
                                                n.HoldStart.fade(0.0f, 0.1);
                                                holding[n.Location - 1] = false;
                                            }
                                            comboText.Location = new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0])) / 2, frame.Location.Y + 200);
                                        }
                                        else if (n.HoldOffset - currentOffset < -hitWindow * 2)
                                        {
                                            foreach (Animation a in holdBursts)
                                            {
                                                if (a.Bounds.X == ((int)frame.Location.X + (frame.LaneLoc[n.Location - 1]) + (1 * n.Location)) + 5 - frame.LaneWidth[n.Location - 1] * 2)
                                                {
                                                    a.Active = false;
                                                    holdRem.Add(a);
                                                }
                                            }
                                            burstAcc(0);
                                            adjustHp(-12);
                                            currentReplay.HitTimings.Add(new ReplayHit((int)n.HoldOffset, (int)n.HoldOffset - (int)currentOffset, n.Location - 1, Replay.HitType.MISS));
                                            if (Config.Spectated)
                                            {
                                                PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.HoldOffset, (int)(n.HoldOffset - currentOffset), n.Location - 1, (int)Replay.HitType.MISS);
                                            }
                                            if (!added)
                                            {
                                                added = true;
                                                miss();
                                            }
                                            n.Enabled = false;
                                            n.Texture.fade(0.0f, 0.1);
                                            n.Holdbar.fade(0.0f, 0.1);
                                            n.HoldEnd.fade(0.0f, 0.1);
                                            n.HoldStart.fade(0.0f, 0.1);
                                            holding[n.Location - 1] = false;
                                            comboText.Location = new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0])) / 2, frame.Location.Y + 200);
                                        }
                                    }
                                    if (n.Enabled && n.Offset - currentOffset < -hitWindow && !holding[n.Location - 1])
                                    {
                                        burstAcc(0);
                                        adjustHp(-12);
                                        currentReplay.HitTimings.Add(new ReplayHit((int)n.HoldOffset, (int)n.HoldOffset - (int)currentOffset, n.Location - 1, Replay.HitType.MISS));
                                        if (Config.Spectated)
                                        {
                                            PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.HoldOffset, (int)(n.HoldOffset - currentOffset), n.Location - 1, (int)Replay.HitType.MISS);
                                        }
                                        if (!added)
                                        {
                                            added = true;
                                            miss();
                                        }
                                        n.Enabled = false;
                                        n.Texture.fade(0.0f, 0.1);
                                        if (n.Hold)
                                        {
                                            n.Holdbar.fade(0.0f, 0.1);
                                            n.HoldEnd.fade(0.0f, 0.1);
                                            n.HoldStart.fade(0.0f, 0.1);
                                        }
                                        comboText.Location = new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0])) / 2, frame.Location.Y + 200);
                                    }
                                }
                            }
                            else if ((n.Offset - currentOffset) < -hitWindow)
                            {
                                n.Enabled = false;
                                n.Texture.fade(0.0f, 0.1);
                                if (n.Hold)
                                {
                                    n.Holdbar.fade(0.0f, 0.1);
                                    n.HoldEnd.fade(0.0f, 0.1);
                                    n.HoldStart.fade(0.0f, 0.1);
                                    adjustHp(-24);
                                    currentReplay.HitTimings.Add(new ReplayHit((int)n.Offset, (int)n.Offset - (int)currentOffset, n.Location - 1, Replay.HitType.HOLDMISS));
                                    if (Config.Spectated)
                                    {
                                        PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.Offset, (int)(n.Offset - currentOffset), n.Location - 1, (int)Replay.HitType.HOLDMISS);
                                    }
                                }
                                else
                                {
                                    adjustHp(-12);
                                    currentReplay.HitTimings.Add(new ReplayHit((int)n.Offset, (int)n.Offset - (int)currentOffset, n.Location - 1, Replay.HitType.MISS));
                                    if (Config.Spectated)
                                    {
                                        PacketWriter.sendSpectateHit(Game.conn.Bw, (int)n.Offset, (int)(n.Offset - currentOffset), n.Location - 1, (int)Replay.HitType.MISS);
                                    }
                                }
                                burstAcc(0);
                                if (!added)
                                {
                                    added = true;
                                    miss();
                                }
                                comboText.Location = new Point(frame.Location.X + (int)frame.Width / 2 - (int)(GraphicalText.measureString(comboText.Text, frame.LaneWidth[0])) / 2, frame.Location.Y + 200);
                            }
                        }
                    }
                }
                else
                {

                }
                #endregion
            }
            for (int x = 0; x < keys.Length; x++)
            {
                prevKey[x] = keys[x];
            }
            if (paused)
            {
                if (!buffering && !specFailed)
                {
                    pauseScreen.OnUpdateFrame(e);
                }
            }
            if (scoreLabel.Text != score.TotalScore.ToString("D9"))
            {
                scoreLabel.Text = score.TotalScore.ToString("D9");
            }
            if (comboText.Text != score.Combo + "x")
            {
                comboText.Text = score.Combo + "x";
            }
            if (score.Accuracy > 0)
            {
                if (accuracyLabel.Text != "" + String.Format("{0:0.00}%", score.Accuracy))
                {
                    accuracyLabel.Text = "" + String.Format("{0:0.00}%", score.Accuracy);
                }
            }
            else
            {
                accuracyLabel.Text = "";
            }
            for (int z = accRem.Count - 1; z > -1; z--)
            {
                bursts.Remove(accRem[z]);
                accRem.Remove(accRem[z]);
            }
            for (int z = holdRem.Count - 1; z > -1; z--)
            {
                holdBursts.Remove(holdRem[z]);
                holdRem.Remove(holdRem[z]);
            }
            for (int z = hitTimingsToAdd.Count - 1; z > -1; z--)
            {
                currentReplay.HitTimings.Add(hitTimingsToAdd[z]);
                hitTimingsToAdd.RemoveAt(z);
            }
            for (int z = pressTimingsToAdd.Count - 1; z > -1; z--)
            {
                currentReplay.PressTimings.Add(pressTimingsToAdd[z]);
                pressTimingsToAdd.RemoveAt(z);
            }
            for (int z = releaseTimingsToAdd.Count - 1; z > -1; z--)
            {
                currentReplay.ReleaseTimings.Add(releaseTimingsToAdd[z]);
                releaseTimingsToAdd.RemoveAt(z);
            }
        }

        void uc_uploadDone(string arg1, int arg2)
        {
            try
            {
                File.Delete("tmp.xml");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (arg1 == "success")
                PacketWriter.sendUserUpdateSelf(Game.conn.Bw);
        }
        public override void OnRenderFrame(FrameEventArgs e)
        {
            int count = 0;
            bg.OnRenderFrame(e);
            if (!scores)
            {
                frame.draw(e);
                if (pulse)
                {
                    /*if (!glow.Scaling && up)
                    {
                        glow.scale(new Size((int)frame.Width - 10, 40), ((sectionSnap / 1000) / 10.0) * 9.0);
                        glow.move(new Point(glow.Bounds.X, (int)frame.HitHeight - 43), ((sectionSnap / 1000) / 10.0) * 9.0);
                        up = false;
                    }
                    else if (!glow.Scaling && !up)
                    {
                        glow.scale(new Size((int)frame.Width - 10, 80), (sectionSnap / 1000) / 10.0);
                        glow.move(new Point(glow.Bounds.X, (int)frame.HitHeight - 83), (sectionSnap / 1000) / 10.0);
                        up = true;
                    }*/
                    float temp = currentSong.getPulsePercent((int)music.Position);
                    glow.Bounds = new Rectangle((int)frame.Location.X, (int)((frame.HitHeight - 83) + (4 * temp)), (int)frame.Width, (int)(83 - (4 * temp)));

                }
                tlContainer.OnRenderFrame(e);
                glow.OnRenderFrame(e);
                hpBar.OnRenderFrame(e);
                foreach (Note n in chart.Notes)
                {
                    if (n.Enabled && (n.Offset - currentOffset < n.MoveTime || holding[n.Location - 1]) && n.Visible)
                    {
                        n.draw(e);
                    }
                }
                for (int x = 0; x < keys.Length; x++)
                {
                    if (lights[x].Alpha > 0)
                    {
                        lights[x].OnRenderFrame(e);
                    }
                    if (glows[x].Alpha > 0)
                    {
                        glows[x].OnRenderFrame(e);
                    }
                    if (presses[x].Alpha > 0)
                    {
                        presses[x].OnRenderFrame(e);
                    }
                }
                foreach (Animation t in bursts)
                {
                    if (t.Active)
                    {
                        t.draw(e);
                    }
                    else
                    {
                        accRem.Add(t);
                    }
                }
                foreach (Animation t in holdBursts)
                {
                    if (t.Active)
                    {
                        t.draw(e);
                    }
                    else
                    {
                        holdRem.Add(t);
                    }
                }
                if (accBurst != null && accBurst.Lifespan > 0)
                {
                    count++;
                    accBurst.OnRenderFrame(e);
                    accBurst.Lifespan -= e.Time;
                    if (accBurst.Lifespan <= 0)
                    {
                        accBurst.fade(0.0f, 0.2);
                    }
                }
                else if (accBurst != null && accBurst.Color.A > 0)
                {
                    accBurst.OnRenderFrame(e);
                }
            }
            if (comboText.Visible)
            {
                comboText.draw(e);
            }
            base.OnRenderFrame(e);
            scoreLabel.draw(e);
            accuracyLabel.draw(e);
            if (!userList.Line.Equals(""))
            {
                userList.OnRenderFrame(e);
            }
            if (scores)
            {
                overlay.OnRenderFrame(e);
                if (overlay.Color.A > 0.85f)
                {
                    scoreScreen.OnRenderFrame(e);
                }
            }
            if (paused)
            {
                if (!buffering && !specFailed)
                {
                    pauseScreen.OnRenderFrame(e);
                }
                else if (!specFailed)
                {
                    if (bufferOverlay != null)
                    {
                        bufferOverlay.OnRenderFrame(e);
                        bufferingLabel.OnRenderFrame(e);
                    }
                }
                else
                {
                    if (bufferOverlay != null)
                    {
                        bufferOverlay.OnRenderFrame(e);
                        specFailedLabel.OnRenderFrame(e);
                    }
                }
            }
        }
        Label specFailedLabel;
        double bufferOffset = 0;
        bool buffering = false;
    }
}
