using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OpenTK.Input;

using Pulse.Screens;

namespace Pulse.UI
{
    public partial class Options : Form
    {
        PropertyTable pt;
        public Options()
        {
            InitializeComponent();
            pt = new PropertyTable();
            pt.Properties.Add(new PropertySpec("Width", typeof(int), "Resolution/Display", "Set the screen width"));
            pt["Width"] = Config.ClientWidth;
            pt.Properties.Add(new PropertySpec("Height", typeof(int), "Resolution/Display", "Set the screen height"));
            pt["Height"] = Config.ClientHeight;
            pt.Properties.Add(new PropertySpec("VSync", typeof(bool), "Resolution/Display", "Enables or disables vsync"));
            pt["VSync"] = Config.Vsync;
            pt.Properties.Add(new PropertySpec("Display FPS", typeof(bool), "Resolution/Display", "Toggle for displaying fps"));
            pt["Display FPS"] = Config.DisplayFps;
            pt.Properties.Add(new PropertySpec("FPS Limit", typeof(int), "Resolution/Display", "FPS limit, will be used when program is restarted"));
            pt["FPS Limit"] = Config.Fps;
            pt.Properties.Add(new PropertySpec("Volume", typeof(int), "Audio", "Sets the volume"));
            pt["Volume"] = Config.Volume;
            pt.Properties.Add(new PropertySpec("Hitsound Volume", typeof(int), "Audio", "Sets the hitsound volume"));
            pt["Hitsound Volume"] = Config.HitVolume;
            pt.Properties.Add(new PropertySpec("Skin Folder", typeof(String), "General Settings", "Skin folder name, applies on restart"));
            pt["Skin Folder"] = Config.SkinFolder;
            pt.Properties.Add(new PropertySpec("Fullscreen", typeof(bool), "Resolution/Display", "Toggle for fullscreen"));
            pt["Fullscreen"] = Config.Fullscreen;
            pt.Properties.Add(new PropertySpec("Offset", typeof(int), "General Settings", "Adjustment of the timing window and graphical display"));
            pt["Offset"] = Config.Offset;
            for (int x = 0; x < 5; x++)
            {
                pt.Properties.Add(new PropertySpec("5K Key " + (x + 1), typeof(OpenTK.Input.Key), "Input 5K", "Key " + (x + 1) + " keybind"));
                pt["5K Key " + (x + 1)] = Config.keys[0][x];
            } 
            for (int x = 0; x < 6; x++)
            {
                pt.Properties.Add(new PropertySpec("6K Key " + (x + 1), typeof(OpenTK.Input.Key), "Input 6K", "Key " + (x + 1) + " keybind"));
                pt["6K Key " + (x + 1)] = Config.keys[1][x];
            }
            for (int x = 0; x < 7; x++)
            {
                pt.Properties.Add(new PropertySpec("7K Key " + (x + 1), typeof(OpenTK.Input.Key), "Input 7K", "Key " + (x + 1) + " keybind"));
                pt["7K Key " + (x + 1)] = Config.keys[2][x];
            }
            for (int x = 0; x < 8; x++)
            {
                pt.Properties.Add(new PropertySpec("8K Key " + (x + 1), typeof(OpenTK.Input.Key), "Input 8K", "Key " + (x + 1) + " keybind"));
                pt["8K Key " + (x + 1)] = Config.keys[3][x];
            }
            pt.Properties.Add(new PropertySpec("Disable mousewheel", typeof(bool), "Input", "Toggle for the enabling of mouse wheel to adjust volume"));
            pt["Disable mousewheel"] = Config.DisableMousewheel;
            pt.Properties.Add(new PropertySpec("Hold hitsounds", typeof(bool), "Audio", "Toggle for hitsounds to be played on the release of a hold note"));
            pt["Hold hitsounds"] = Config.HoldHitsounds;
            pt.Properties.Add(new PropertySpec("Edit middle", typeof(bool), "Editor", "Toggles the hit bar and timing occuring at the center of the frame, versus the bottom of the frame as ingame"));
            pt["Edit middle"] = Config.EditMiddle;
            pt.Properties.Add(new PropertySpec("Waveform", typeof(bool), "Resolution/Display", "Toggle menu waveform"));
            pt["Waveform"] = Config.Waveform;
            pt.Properties.Add(new PropertySpec("Widescreen", typeof(bool), "Resolution/Display", "Toggle wide rendering"));
            pt["Widescreen"] = Config.WideScreen;
            pt.Properties.Add(new PropertySpec("Confirm Close", typeof(bool), "General Settings", "Toggle to enable confirmation upon closing pulse"));
            pt["Confirm Close"] = Config.ConfirmClose;
            pt.Properties.Add(new PropertySpec("Chatsounds", typeof(bool), "General Settings", "Toggle to enable ping messages when receiving a private message or highlight"));
            pt["Chatsounds"] = Config.ChatSounds;
            pt.Properties.Add(new PropertySpec("Restart key", typeof(Key), "Input", "Key used to restart song from pause or fail screens"));
            pt["Restart key"] = Config.RestartKey;
            pt.Properties.Add(new PropertySpec("Skip key", typeof(Key), "Input", "Key used to skip the intro of a song"));
            pt["Skip key"] = Config.SkipKey;
            pt.SetValue += new PropertySpecEventHandler(pt_SetValue);
            this.propertyGrid1.SelectedObject = pt;
        }
        protected override void OnClosing(CancelEventArgs e)
        {

            Config.saveConfig();
            MenuScreen.isOptions = false;

#if debug
            Game.addToast("Saved options"); //was testing stuff in the queue :p
            Game.addToast("Saved options again");
            Game.addToast("Saved options again again");
#endif
            base.OnClosing(e);
            Game.addToast("Updated options");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        void pt_SetValue(object sender, PropertySpecEventArgs e)
        {
            Console.WriteLine(e.Property.Name);
            Console.WriteLine(e.Value);
            try
            {
                switch (e.Property.Name)
                {
                    case "Waveform":
                        Config.Waveform = (bool)e.Value;
                        break;
                    case "Edit middle":
                        Config.EditMiddle = (bool)e.Value;
                        break;
                    case "Width":
                        Config.ClientWidth = (int)e.Value;
                        //   if (Game.game != null) { Game.game.Bounds = new Rectangle(Game.game.Bounds.X, Game.game.Bounds.Y, Config.ClientWidth, Config.ClientHeight); }
                        break;
                    case "Height":
                        Config.ClientHeight = (int)e.Value;
                        //    if (Game.game != null) { Game.game.Bounds = new Rectangle(Game.game.Bounds.X, Game.game.Bounds.Y, Config.ClientWidth, Config.ClientHeight); }
                        break;
                    case "VSync":
                        Config.Vsync = (bool)e.Value;
                        break;
                    case "Fullscreen":
                        Config.Fullscreen = (bool)e.Value;
                        break;
                    case "Offset":
                        Config.Offset = (int)e.Value;
                        break;
                    case "Display FPS":
                        Config.DisplayFps = (bool)e.Value;
                        break;
                    case "FPS Limit":
                        Config.Fps = (int)e.Value;
                        break;
                    case "Volume":
                        Config.Volume = (int)e.Value;
                        break;
                    case "Skin Folder":
                        Config.SkinFolder = (String)e.Value;
                        break;
                    case "Hitsound Volume":
                        Config.HitVolume = (int)e.Value;
                        break;
                    case "Hold hitsounds":
                        Config.HoldHitsounds = (bool)e.Value;
                        break;
                    case "Disable mousewheel":
                        Config.DisableMousewheel = (bool)e.Value;
                        break;
                    case "Widescreen":
                        Config.WideScreen = (bool)e.Value;
                        break;
                    case "Chatsounds":
                        Config.ChatSounds = (bool)e.Value;
                        break;
                    case "Restart key":
                        Config.RestartKey = (Key)e.Value;
                        break;
                    case "Skip key":
                        Config.SkipKey = (Key)e.Value;
                        break;
                }
                for (int x = 0; x < 8; x++)
                {
                    if (e.Property.Name.Equals("5K Key " + (x + 1)))
                    {
                        Config.keys[0][x] = (OpenTK.Input.Key)e.Value;
                    } 
                    if (e.Property.Name.Equals("6K Key " + (x + 1)))
                    {
                        Config.keys[1][x] = (OpenTK.Input.Key)e.Value;
                    }
                    if (e.Property.Name.Equals("7K Key " + (x + 1)))
                    {
                        Config.keys[2][x] = (OpenTK.Input.Key)e.Value;
                    }
                    if (e.Property.Name.Equals("8K Key " + (x + 1)))
                    {
                        Config.keys[3][x] = (OpenTK.Input.Key)e.Value;
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("pls enter valid values");
            }
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void Options_Load(object sender, EventArgs e)
        {

        }
    }
}
