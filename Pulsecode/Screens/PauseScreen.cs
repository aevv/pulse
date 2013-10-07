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

namespace Pulse.Screens
{
    class PauseScreen : Screen
    {
        int index = 0;
        bool failed;

        public bool Failed
        {
            get { return failed; }
            set { failed = value; buttons[0].Enabled = false; buttons[0].Visible = false; index = 1; buttons[index].Color = Color.Yellow; }
        }
        Button[] buttons = new Button[3];
        Rect overlay;
        IngameScreen screen;
        
        public PauseScreen(Game game, string text, IngameScreen screen) : base(game, text)
        {
            this.screen = screen;
            buttons[0] = new Button(game, new Rectangle(Config.ClientWidth / 2 - 100, 100, 200, 100), "Continue", delegate(int data)
            {
                screen.unpause();
            });
            buttons[1] = new Button(game, new Rectangle(Config.ClientWidth / 2 - 100, 300, 200, 100), "Retry", delegate(int data)
            {
                screen.loadSong(screen.CurrentSong, screen.Difficulty, screen.Mods, screen.CurrentReplay, screen.PlayType1);
                screen.onSwitched();
            });
            if (screen.PlayType1 == IngameScreen.PlayType.REPLAY || screen.PlayType1 == IngameScreen.PlayType.TEST)
            {
                buttons[1].Text = "Restart";
            }
            buttons[2] = new Button(game, new Rectangle(Config.ClientWidth / 2 - 100, 500, 200, 100), "Exit", delegate(int data)
            {
                Game.setScreen(game.screens["selectScreen"]);
                game.Title = "Pulse";
                if (Config.Spectating)
                {
                    try
                    {
                        Client.PacketWriter.sendSpectateCancel(Game.conn.Bw, Config.SpectatedUser);
                    }
                    catch
                    {
                    }
                    Config.Spectating = false;
                    Config.SpectatedUser = "";
                    Config.Specs = "";
                }
            });
            for (int x = 0; x < buttons.Length; x++)
            {
                UIComponents.Add(buttons[x]);
            }
            overlay = new Rect(new Rectangle(0, 0, Config.ResWidth, 768));
            overlay.Color = new Color4(0.0f, 0.0f, 0.0f, 0.75f);
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            index = 0;
            buttons[index].manualColour = true;
            buttons[index].Color = Color.FromArgb(255, 255, 100, 0);
        }
        public override void onSwitched()
        {
            base.onSwitched();
            index = 0;
            buttons[index].manualColour = true;
            buttons[index].Color = Color.FromArgb(255, 255, 100, 0);
        }
        public override void OnRenderFrame(FrameEventArgs e)
        {
            overlay.OnRenderFrame(e);
            base.OnRenderFrame(e);
        }
        int indexMin = 0;
        public override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!Game.pbox.expanded)
            {
                if (keyPress(Key.Up))
                {
                    if (failed)
                    {
                        indexMin = 1;
                    }
                    else
                        indexMin = 0;
                    if (index > indexMin)
                    {
                        buttons[index].Color = Color.White;
                        buttons[index].manualColour = false;
                        index--;
                        buttons[index].manualColour = true;
                        buttons[index].Color = Color.FromArgb(255, 255, 100, 0);
                    }
                }
                if (keyPress(Key.Down))
                {
                    if (index < 2)
                    {
                        buttons[index].Color = Color.White;
                        buttons[index].manualColour = false;
                        index++;
                        buttons[index].manualColour = true;
                        buttons[index].Color = Color.FromArgb(255, 255, 100, 0);
                    }
                }
                if (keyPress(Config.RestartKey))
                {
                    screen.loadSong(screen.CurrentSong, screen.Difficulty, screen.Mods, screen.CurrentReplay, screen.PlayType1);
                    screen.onSwitched();
                }
                if (keyPress(Key.Enter))
                {
                    switch (index)
                    {
                        case 0:
                            screen.unpause();
                            break;
                        case 1:
                            screen.loadSong(screen.CurrentSong, screen.Difficulty, screen.Mods, screen.CurrentReplay, screen.PlayType1);
                            screen.onSwitched();
                            break;
                        case 2:
                            Game.setScreen(game.screens["selectScreen"]);
                            game.Title = "Pulse";

                            if (Config.Spectating)
                            {
                                try
                                {
                                    Client.PacketWriter.sendSpectateCancel(Game.conn.Bw, Config.SpectatedUser);
                                }
                                catch
                                {
                                }
                                Config.Spectating = false;
                                Config.SpectatedUser = "";
                                Config.Specs = "";
                            }
                            break;
                    }
                }
            }
        }
    }
}
