using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Graphics;

using Pulse.UI;
using Pulse.Mechanics;

namespace Pulse.Screens
{
    class ScoreScreen : Screen
    {
        private Score score;
        Button exitButton, retryButton, replayButton;
        Label scoreLabel, accuracyLabel, maxComboLabel, songInfoLabel, statsLabel, statsNoLabel,dateLabel;
        Rect scoreBg;
        Replay replay;

        public Score Score
        {
            get { return score; }
            set { score = value; scoreLabel.Text = score.TotalScore.ToString("D8"); 
                accuracyLabel.Text = String.Format("{0:0.00}%", score.Accuracy);
                dateLabel.Text = score.dateString;
                maxComboLabel.Text = "" + score.MaxCombo + "x";
                statsLabel.Text = "Perfect: " + "\nGreat: " + "\nOK: " + "\nMiss: ";
                statsNoLabel.Text = "" + score.Perfects + "\n" + score.Goods + "\n" + score.Oks + "\n" + score.Misses;
            }
        }
        public ScoreScreen(Game game, string name)
            : base(game, name)
        {

        }
        public ScoreScreen(Game game, string name, Replay replay, Song song, int diff, bool play)
            : base(game, name)
        {
            if (play)
            {
                retryButton = new Button(game, new Rectangle(Config.ResWidth - (int)(Config.ResWidth * 0.3), 230, 100, 50), "Retry", delegate(int data)
                {
                    game.screens["ingameScreen"].Music.stop();
                    IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                    temp.loadSong(temp.CurrentSong, temp.Difficulty, temp.Mods, null, IngameScreen.PlayType.PLAY);
                    Game.setScreen(game.screens["ingameScreen"]);
                });
                UIComponents.Add(retryButton);
            }
            if (replay != null)
            {
                
                this.replay = replay;
                replayButton = new Button(game, new Rectangle(Config.ResWidth - (int)(Config.ResWidth * 0.3), 330, 100, 50), "Replay", delegate(int data)
                {
                    IngameScreen temp = (IngameScreen)game.screens["ingameScreen"];
                    try
                    {                        
                        if (temp.Music != null)
                        {
                            temp.Music.stop();
                        }
                        temp.loadSong(song, diff, temp.Mods, this.replay, IngameScreen.PlayType.REPLAY);
                        Game.setScreen(temp);
                        game.Title = "Pulse | Watch replay | " + song.Artist + " - " + song.SongName + " [" + song.Charts[diff].Name + "]";
                    }
                    catch { }
                });
                UIComponents.Add(replayButton);
            }
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IngameScreen tempPlayScreen = (IngameScreen)game.screens["ingameScreen"];
            string songinf = tempPlayScreen.CurrentSong.Artist + " - " + tempPlayScreen.CurrentSong.SongName;
            
            songInfoLabel = new Label(game, new Point(180, 120), "0");
            dateLabel = new Label(game, new Point(200, 85), "0");
            UIComponents.Add(dateLabel);
            songInfoLabel.TextTexture.TextFont = new Font("Myriad Pro", 35);
            if (songinf.Length < 26)
            {
                songInfoLabel.Text = songinf.Substring(0, songinf.Length) + " [" + tempPlayScreen.Chart.Name + "]";
            }
            else
            {
                songInfoLabel.Text = songinf.Substring(0, 25) + "... [" + tempPlayScreen.Chart.Name + "]";
            }
            UIComponents.Add(songInfoLabel);
            scoreLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 200), "0");
            accuracyLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 250), "0");
            maxComboLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 300), "0");
            statsLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.8), 350), "0");
            statsNoLabel = new Label(game, new Point(Config.ResWidth - (int)(Config.ResWidth * 0.7), 350), "0");
            UIComponents.Add(statsNoLabel);
            UIComponents.Add(statsLabel);
            UIComponents.Add(scoreLabel);
            UIComponents.Add(accuracyLabel);
            UIComponents.Add(maxComboLabel);
            scoreBg = new Rect(new Rectangle(new Point(0, 0), new Size(Config.ResWidth, 768)), Skin.skindict["scorebg"]);
            scoreBg.Color = Color4.SlateGray;
            exitButton = new Button(game, new Rectangle(Config.ResWidth - (int)(Config.ResWidth * 0.3), 430, 100, 50), "Exit", delegate(int data)
            {
                game.screens["ingameScreen"].Music.stop();
                Game.setScreen(game.screens["selectScreen"]);
                game.Title = "Pulse";
            });
            UIComponents.Add(exitButton);
        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (score != null)
            {/*
                scoreLabel.Text = score.TotalScore.ToString("D8");
                accuracyLabel.Text = String.Format("{0:0.00}%", score.Accuracy);
                maxComboLabel.Text = "" + score.MaxCombo + "x";
            */}
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            scoreBg.OnRenderFrame(e); 
            base.OnRenderFrame(e);
        }

    }
}
