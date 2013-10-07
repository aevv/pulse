using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTK.Input;

using Pulse.UI;
using System.Drawing;
using Pulse.Audio;
using System.IO;
using System.Drawing.Imaging;

namespace Pulse.Screens
{
    public class Screen : DrawableComponent
    {
#warning don't use music :D fix this
        /// <summary>
        /// deprecated
        /// </summary>
        
        protected AudioFX music;


        /// <summary>
        /// deprecated, use mediaplayer instead for greater interop between screens
        /// 
        /// </summary>
        
        public AudioFX Music
        {
            get
            {
                return music;
            }
            set
            {
                if (music != null)
                {
                    //music.stop();
                }
                music = value;
            }
        }

        protected List<Key> oldKeyState = new List<Key>(), newKeyState = new List<Key>();

        protected MouseInfo oldMouse, newMouse;

        protected Game game;

        protected string name;

        public bool loaded = false;
        public bool switched = false;

        protected Dictionary<string, Rect> textures = new Dictionary<string, Rect>();

        protected List<InterfaceComponent> UIComponents = new List<InterfaceComponent>();

        public Screen(Game game, string name)
        {
            this.game = game;
            this.name = name;
            enabled = false;
            visible = false;
        }
        public override void OnLoad(EventArgs e)
        {
            loaded = true;
        }
        public override void OnUpdateFrame(FrameEventArgs e)
        {
            oldMouse = newMouse;
            oldKeyState = newKeyState;
            newKeyState = Game.KeyboardState;

            newMouse = Game.MouseState;

            if (keyPress(Key.F9))
            {
                Bitmap b = Utils.GrabScreenshot(game);
                if (!Directory.Exists("screenshots"))
                {
                    Directory.CreateDirectory("screenshots");
                }
                String name = getName(1);
                b.Save("screenshots\\" + name);
                b.Dispose();
                Game.addToast("Saved screenshot to " + name);
            }
            foreach (InterfaceComponent i in UIComponents)
            {
                if (i.Enabled)
                {
                    i.OnUpdateFrame(e);
                }
            }
        }
        protected void updateKeys()
        {

        }
        private String getName(int toCheck)
        {
            if (File.Exists("screenshots\\screenshot" + toCheck + ".jpg"))
            {
                return getName(++toCheck);
            }
            else
            {
                return "screenshot" + toCheck + ".jpg";
            }
        }
        protected bool keyPress(Key k)
        {
            if (newKeyState.Contains(k) && !oldKeyState.Contains(k))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool keyHold(Key k)
        {
            if (newKeyState.Contains(k) && oldKeyState.Contains(k))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool leftClick()
        {
            if (newMouse.LeftButton && !oldMouse.LeftButton && game.Focused)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void OnRenderFrame(FrameEventArgs e)
        {
            foreach (InterfaceComponent i in UIComponents)
            {
                if (i.Visible)
                {
                    i.OnRenderFrame(e);
                }
            }
        }
        public void Show()
        {
            this.enabled = true;
            this.visible = true;
        }
        public void Hide()
        {
            this.enabled = false;
            this.visible = false;
        }
        public virtual void onSwitched()
        {
            switched = true;
        }
    }
}
