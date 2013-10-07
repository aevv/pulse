using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Graphics;
using System.Net;

using Pulse.UI;
using Pulse.Mechanics;
using System.Threading;

namespace Pulse.Screens
{
    class ScoreSelectScreen : Screen
    {
        private Score score;
        Button exitButton, replayButton;
        Label scoreLabel, accuracyLabel, maxComboLabel, songInfoLabel, statsLabel, statsNoLabel, dateLabel;
        Rect scoreBg;

        public Score Score
        {
            get { return score; }
            set
            {
                score = value;
            }
        }
        /*public ScoreSelectScreen(Game game, string name, Replay replay, Song song, int diff)
            : this(game, name)
        {
            foreach (var pair in replay.PressTimings)
            {
                pair.value.value = false;
            }
            foreach (var pair in replay.ReleaseTimings)
            {
                pair.value.value = false;
            }
            foreach (ReplayHit h in replay.HitTimings)
            {
                h.Used = false;
            }
            this.replay = replay;
            replayButton = new Button(game, new Rectangle(Config.ResWidth - (int)(Config.ResWidth * 0.3), 330, 100, 50), "Replay", delegate()
            {
                IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                try
                {
                    temp.loadSong(song, diff, new Mods(), replay, IngameScreen.PlayType.REPLAY);
                    temp.Music.stop();
                    Game.setScreen(game.screens["replayScreen"]);
                    //temp.onSwitched();
                    game.Title = "Pulse | Watch replay | " + song.Artist + " - " + song.SongName + " [" + song.Charts[diff].Name + "]";
                }
                catch { }
            });
            UIComponents.Add(replayButton);
            exitButton.del = delegate()
            {
                game.screens["selectScreen"].Music.stop();
                Game.setScreen(game.screens["selectScreen"]);
                game.Title = "Pulse";
            };
        }*/
        public ScoreSelectScreen(Game game, string name)
            : base(game, name)
        {
            songInfoLabel = new Label(game, new Point(180, 120), "0");
            songInfoLabel.TextTexture.TextFont = new Font("Myriad Pro", 35);
            dateLabel = new Label(game, new Point(Config.ResWidth / 2 - 200, 85), "0");
            UIComponents.Add(songInfoLabel);
            scoreLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 200), "0");
            accuracyLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 250), "0");
            maxComboLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 300), "0");
            statsLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 350), "0");
            statsNoLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.7), 350), "0");
            UIComponents.Add(dateLabel);
            UIComponents.Add(statsNoLabel);
            UIComponents.Add(statsLabel);
            UIComponents.Add(scoreLabel);
            UIComponents.Add(accuracyLabel);
            UIComponents.Add(maxComboLabel);
            scoreBg = new Rect(new Rectangle(new Point(0, 0), new Size(Config.ResWidth, 768)), Skin.skindict["scorebg"]);
            scoreBg.Color = Color4.SlateGray;
            exitButton = new Button(game, new Rectangle(Config.ResWidth - (int)(Config.ResWidth * 0.3), 430, 100, 50), "Exit", delegate(int data)
            {
                game.screens["selectScreen"].Music.stop();
                Game.setScreen(game.screens["selectScreen"]);
                game.Title = "Pulse";
            });
            UIComponents.Add(exitButton);
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (score != null)
            {/*
                scoreLabel.Text = score.TotalScore.ToString("D8");
                accuracyLabel.Text = String.Format("{0:0.00}%", score.Accuracy);
                maxComboLabel.Text = "" + score.MaxCombo + "x";
            */
            }
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            scoreBg.OnRenderFrame(e);
            base.OnRenderFrame(e);

        }
        public void setScore(Score score, Song song, int diff, int rank)
        {
            this.score = score;
            scoreLabel.Text = score.TotalScore.ToString("D8");
            accuracyLabel.Text = String.Format("{0:0.00}%", score.Accuracy);
            maxComboLabel.Text = "" + score.MaxCombo + "x";
            dateLabel.Text = score.dateString;
            statsLabel.Text = "Perfect: " + "\nGood: " + "\nOK: " + "\nMiss: ";
            statsNoLabel.Text = "" + score.Perfects + "\n" + score.Goods + "\n" + score.Oks + "\n" + score.Misses;
            string songinf = score.ArtistTitle;
            if (songinf.Length < 26)
            {
                songInfoLabel.Text = songinf.Substring(0, songinf.Length) + " [" + score.chartName + "]";
            }
            else
            {
                songInfoLabel.Text = songinf.Substring(0, 25) + "... [" + score.chartName + "]";
            }
            if (score.ReplayName != "")
            {
                replayButton = new Button(game, new Rectangle(Config.ResWidth - (int)(Config.ResWidth * 0.3), 330, 100, 50), "Replay", delegate(int data)
                {
                    IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                    if (Config.LocalScores)
                    {
                        Replay replay = ScoreLibrary.reconReplay("replay\\r\\" + score.ReplayName + ".pcr");
                        try
                        {
                            temp.loadSong(song, diff, replay.Mods, replay, IngameScreen.PlayType.REPLAY);
                            temp.Music.stop();
                            Game.setScreen(game.screens["ingameScreen"]);
                            game.Title = "Pulse | Watch replay | " + song.Artist + " - " + song.SongName + " [" + song.Charts[diff].Name + "]";
                        }
                        catch { }
                    }
                    else
                    {
                        string hash = Utils.calcHash(song.Charts[diff].Path);
                        string dl = "http://p.ulse.net/getreplay?r=" + rank + "&c=" + hash;
                        Console.WriteLine("{0} rank {1} hash", rank, hash);
                        downloadReplay dr = new downloadReplay();
                        dr.downloadFinish += new Action<Song, int>(dr_downloadFinish);
                        dr.DownloadReplay(dl, "replay\\" + hash + ".pcr", song, diff);
                       // wc.DownloadFileAsync(new Uri(dl), "replay\\temp.pcr");
                        //wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                        replayButton.Enabled = false;
                        replayButton.Visible = false;
                        exitButton.Enabled = false;
                        exitButton.Visible = false;
                        Game.addToast("Downloading replay...");
                    }
                });
                if (!UIComponents.Contains(replayButton))
                {
                    UIComponents.Add(replayButton);
                }
            }
        }

        void dr_downloadFinish(Song song, int diff)
        {
            Replay replay = null;
            if (System.IO.File.Exists("replay\\" + Utils.calcHash(song.Charts[diff].Path) +".pcr"))
            {
                replay = ScoreLibrary.reconReplay("replay\\" + Utils.calcHash(song.Charts[diff].Path) + ".pcr");
                System.IO.File.Delete("replay\\" + Utils.calcHash(song.Charts[diff].Path) + ".pcr");
            }
            IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
            if (replay != null)
            {
                temp.loadSong(song, diff, replay.Mods, replay, IngameScreen.PlayType.REPLAY);
                temp.Music.stop();
                Game.setScreen(game.screens["ingameScreen"]);
                game.Title = "Pulse | Watch replay | " + song.Artist + " - " + song.SongName + " [" + song.Charts[diff].Name + "]";
                exitButton.Visible = true;
                exitButton.Enabled = true;
            }
            else
            {
                Console.WriteLine("no replay");
                Game.addToast("No replay available for this score");
                exitButton.Visible = true;
                exitButton.Enabled = true;
            }
        }
    }
    class downloadReplay
    {
        public event Action<Song, int> downloadFinish;
        Song song;
        int diff;
        Thread t;
        public void abort()
        {
            if(t!=null)
            t.Abort();
        }
            public void DownloadReplay(string uri, string local, Song songg, int diffint)
        {
            song = songg;
            diff = diffint;
            t = new Thread(new ParameterizedThreadStart(download));
            t.IsBackground = true;
            List<object> o = new List<object>();
            o.Add(uri);
            o.Add(local);
            o.Add(songg);
            o.Add(diffint);
            t.Start(o);
        }
        void download(object o)
        {
            WebClient wc = null;
            try
            {
                List<object> obj = (List<object>)o;
                string uri = (string)obj[0];
                string local = (string)obj[1];
                Song song = (Song)obj[2];
                int diff = (int)obj[3];
                wc = new WebClient();
                wc.Proxy = null;
                wc.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
                wc.DownloadFile(uri, local);
                if (downloadFinish != null)
                {
                    downloadFinish.Invoke(song, diff);
                }
                wc.Dispose();
            }
            catch (ThreadAbortException)
            {
                if (wc != null)
                {
                    wc.Dispose();
                }
            }
            catch (Exception)
            {
                if (wc != null)
                {
                    wc.Dispose();
                }
            }
        }
    }
}
