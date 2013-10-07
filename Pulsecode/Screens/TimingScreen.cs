using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Pulse.Mechanics;
using Pulse.UI;

namespace Pulse.Screens
{
    class TimingScreen : Screen
    {
        Song song;
        Button timeButton;
        Label bpmLabel;
        Label offsetLabel;
        double bpm;
        int offset;
        Button reset;
        Button back;
        Button save;
        List<int> times = new List<int>();
        List<double> bpms = new List<double>();
        int initial = -1;
        public TimingScreen(Game game, string name)
            : base(game, name)
        {
            reset = new Button(game, new Rectangle(600, 50, 200, 50), "Reset", delegate(int data)
            {
                song.Timings.Remove(offset);
                times.Clear();
                bpms.Clear();
                bpmLabel.Text = "BPM: ---";
                offsetLabel.Text = "Offset: ---";
                Music.play(true);
                
            });
            back = new Button(game, new Rectangle(100, 600, 200, 50), "Back", delegate(int data)
            {
                Music.stop();
                Game.setScreen(game.screens["editScreen"]);
            });
            UIComponents.Add(back);
            offsetLabel = new Label(game, new Point(200, 180), "Offset: ---");
            UIComponents.Add(offsetLabel);
            timeButton = new Button(game, new Rectangle(100, 50, 400, 50), "Press in time to the beat!", delegate(int data)
            {
                if (times.Count < 1)
                {
                    int off = Environment.TickCount - initial;
                    times.Add(off);
                    offset = (int)music.Position;
                    offsetLabel.Text = "Offset: " + music.Position;
                }
                else
                {
                    int diff = Environment.TickCount - initial;
                    double bpmtoadd = ((60d * 1000d) / (diff - times[times.Count - 1]));
                    if (!double.IsInfinity(bpmtoadd))
                    {
                        bpms.Add(bpmtoadd);
                        //Console.WriteLine(bpms.Average());
                        if (bpms.Count < 20)
                        {
                            bpm = Math.Round(bpms.Average(), 0);
                        }
                        else if (bpms.Count < 40)
                        {
                            bpm = Math.Round(bpms.Average(), 1);
                        }
                        else
                        {
                            bpm = Math.Round(bpms.Average(), 2);
                        }
                        bpmLabel.Text = "BPM: " + bpm;
                        //   if(bpms.Average() >= double.MaxValue) {
                        //    bpms.Clear();
                        //  }
                    }
                    times.Add(diff);
                }
            });
            save = new Button(game, new Rectangle(600, 125, 200, 50), "Save", delegate(int data)
            {
                try
                {
                    song.Timings[offset].Snap = 60 / bpm;
                }
                catch
                {
                    song.Timings.Add(offset, new TimingSection(offset, bpm, true));  
                }
                Game.addToast("Timing section saved");
            });
            UIComponents.Add(save);
            bpmLabel = new Label(game, new Point(200, 150), "BPM: ---");
            UIComponents.Add(bpmLabel);
            UIComponents.Add(reset);
            UIComponents.Add(timeButton);
        }
        public void loadSong(Audio.AudioFX fx, Song song)
        {
            times.Clear();
            bpms.Clear();
            initial = Environment.TickCount;
            fx.play(true);
            Music = fx;
            this.song = song;
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        public override void onSwitched()
        {
            base.onSwitched();
            bpmLabel.Text = "BPM: ---";
            offsetLabel.Text = "Offset: ---";

        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
        }
    }
}
