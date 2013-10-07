using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Pulse.Client.irc;
using System.Windows.Forms;
using Pulse;
using System.Text.RegularExpressions;
using Pulse.Scripting;
using OpenTK.Input;
using System.Diagnostics;
using System.IO;
namespace Pulse.UI
{
    /// <summary>
    /// TODO : implement tabs sooner or later, or perhaps pWnd -- done!
    /// KNOWN ISSUE : measurestringsize is NOT accurate (perhaps due to resolution size) and therefore the carat is always off -- fixed!
    /// nearly 1000 lines now :\
    /// Note: Rect.Alpha doesn't update when fading, is there a reason?
    /// Hotkeys - F2 to toggle chatbox, alt+1-9 for quick tab switch, ctrl copy/paste/a (select all), home/end/delete/backspace/enter, up and down to access scrollback history, l/r cursor, shift+l/r to select
    ///                 //in the future, perhaps consider instead just storing list of strings instead (would also have to store attribute info like position and color) and in update create the list of textures (that are within range) so it would be a smaller list ~10 textures, instead of current making a text for every string and keeping in memory which eats a significant amount of RAM
    ///to keep efficiency, instead of clearing list everytime should just keep the same ones in there, add new ones, and delete unused
    /// </summary>

    public class PTextBox : Control
    {
        public bool expanded = true; //turn false for default closed
        public Rect bg; //box rect
        List<string> scrollBack = new List<string>(); //list for chatline scrollback
        int scrollbackpos = -1;
        const int spacing = 25; //pixel spacing between lines
        const int maxSize = 1500; //max # of lines PER TAB
        Text chatLine; //the actual text displayed when typing
        string backingText = ""; //backing text of the chat line
        Rect caret; //caret texture
        int startSelect; //start position for selection
        int endSelect; //end position for selection
        Rect selectBox; //select box texture
        int origpos; //only used for expanding/closing
        IrcClient ic;
        double caratcounter; //blinking counter for the text carat
        int position; //position of carat
        bool canPress = true; //bool used for ensuring click events triggered only once until mouse up
        private Tab ActiveTab;
        public Tab activeTab
        {
            get { return ActiveTab; }
            set
            {
                if (ActiveTab != null) { ActiveTab.IsActive = false; }
                // value.tabRect.Colour = Color.Orange;
                ActiveTab = value;
                value.IsActive = true;
            }
        }
        public Dictionary<string, Tab> tabs = new Dictionary<string, Tab>();
        List<Tab> toRemove = new List<Tab>(); //for closed tabs, a queue for them to be on until they finish transitioning
        List<Tab> removeFromRemove = new List<Tab>(); //since cannot modify collection while foreaching, need this; if clear after each loop was used something transitioning on the remove queued may be removed and lead to bugs
        public const string baseChannel = "PulseIRC"; //base channel that always stays open
        bool ctrldown;
        bool shiftdown;
        bool altdown;
        private bool Selecting;
        bool selecting
        {
            get
            {
                return Selecting;
            }
            set
            {
                Selecting = value;
                if (!Selecting)
                {
                    selectcounter = 0;
                }
            }
        } //bool whether selecting or not, when false selectbox does not render
        LuaScript theScript;
        int selectcounter; //used for keeping track of select pos when changing directions        
        public bool ircverbose = true;
        public PTextBox(Game game, Rectangle bounds, string text, IrcClient client)
            : base(game, bounds, text)
        {
            OnLoad(null);
            bg = new Rect(new Rectangle(bounds.Location, new Size(bounds.Size.Width, bounds.Size.Height + 10)));
            bg.Color = Color.FromArgb(200, 20, 20, 20);
            bg.Layer = 9;
            origpos = bg.Bounds.Y;
            Tab pulse = new Tab(0, new List<Text>(), baseChannel, this);
            pulse.Layer = 9.1;
            pulse.Scrollpos = -bounds.Height + spacing;
            activeTab = pulse;
            addTab(pulse);
            ic = client;
            chatLine = new Text(game.ClientSize, new Size(game.Bounds.Width, 50), new Point(bounds.X + 5, this.bounds.Y + this.bounds.Height - spacing));
            chatLine.textFont = boxFont;
            chatLine.Shadow = false;
            caret = new Rect(new Rectangle(0, 0, 2, 15));
            caret.Color = Color.White;
            selectBox = new Rect(new Rectangle(bounds.X + 5, chatLine.Location.Y + 3, 20, 19));
            selectBox.Color = Color.FromArgb(100, Color.LightGray);
            //activeTab.Scrollpos = 
            addLine("Welcome to pulse chat! Press F2 to maximize/minimize chat.", Color.SteelBlue);
            Game.lua.RegisterFunction("show", this, this.GetType().GetMethod("show", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
            //   Game.lua["pbox"] = this;
            updateCaret();
            //try /lua pbox.bg.Bounds = test(0,300,1024,500) 
            //Game.lua["cm"] = System.Drawing.Rectangle
            //  game.Keyboard.KeyRepeat = false;
            /*    foreach (string s in Game.lua.Globals)
                {
                    Console.WriteLine(s);
                }*/
        }
        //Required, because somehow I can't load .net classes in lua  -- nvm can luanet.load_assembly, luanet.import_type
        public Rectangle test(int x, int y, int h, int w)
        {
            return new Rectangle(x, y, h, w);
        }
        public void setIrc(IrcClient cl)
        {
            ic = cl;
        }

        //para lua
        public void show(string s)
        {
            addLine(s);
        }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            game.Mouse.WheelChanged += new EventHandler<OpenTK.Input.MouseWheelEventArgs>(Mouse_WheelChanged);
            game.KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(game_KeyPress);
            game.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            game.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
        }
        public void addTab(Tab t)
        {
            if (!tabs.ContainsKey(t.Title))
            {
                t.tabRect.Alpha = 0;
                t.tabText.Alpha = 0;
                if (expanded)
                {
                    t.tabRect.fade(1, .25);
                    t.tabText.fade(1, .25);
                }
                tabs.Add(t.Title, t);
                addLine("Type /close to close this tab.", Color.SteelBlue, t.Title);
            }
            else
            {
                addLine("Tab already open", Color.Red);
            }
        }
        public void addTab(string name)
        {

            Tab t = new Tab(-bounds.Height + spacing, new List<Text>(), name, this);
            addTab(t);
            /*  tabs.Add(name, t);
              t.tabRect.Alpha = 0;
              t.tabRect.fade(1, .25);
              t.tabText.Alpha = 0;
              t.tabText.fade(1, .25);
            
              addLine("Type /close to close this tab.", Color.SteelBlue, name); */
        }
        /*void updateCaret()
        {
            updateCaret(false, ' ');
        }*/

        /// <summary>
        /// restricting doesn't work very well, osu supports it only in chatbox but support is shit, cursor position is all fucked up and pressing home doesn't put cursor to true beginning and typing chars then appear off screen so worthless. besides, multi room title box just stretches which shouldn't be too hard (have method that splits text into several line array based on width) and the search function for players has the box go red at a certain point, so screw it
        /// updatecaret(bool,char)
        /// so main problem is cursor position basically and moving around text pos accurately
        /// </summary>
        /// <param name="deleted"></param>
        /// <param name="del"></param>
        void updateCaret()
        {
            Point p = new Point(((int)(Pulse.Text.getStringSizePBox(backingText.Substring(0, position), chatLine.TextFont).Width))/*(int)Math.Floor(chatLine.getStringSize().Width)*/ + bounds.X + (position == 0 ? 5 : 0), chatLine.Location.Y + 5);
            /*  if (p.X > chatLine.TextBitmap.Width + bounds.X && position > 1)
              {
                  p.X = chatLine.TextBitmap.Width + bounds.X + 7;
                  PointF p2 = chatLine.Position;
                  //somehow, getting the length of 1 char doesn't work but getting length of both strings and getting the difference works like a charm go figure
                  float subamt = 0;
                  if (deleted)
                  {
                      string text = (backingText + del).Substring(0, position + 1);
                      string text2 = backingText.Substring(0, position);
                      subamt = Pulse.Text.getStringSizePBox(text, chatLine.TextFont).Width - Pulse.Text.getStringSizePBox(text2, chatLine.TextFont).Width;
                      subamt = -subamt;
                  }
                  else
                  {
                      string text = (backingText + del).Substring(0, position);
                      string text2 = backingText.Substring(0, position-1);
                      subamt = Pulse.Text.getStringSizePBox(text, chatLine.TextFont).Width - Pulse.Text.getStringSizePBox(text2, chatLine.TextFont).Width;
                  }
                
                  p2.X = p2.X - subamt;
                  chatLine.Position = p2;
              }*/
            caret.Bounds = new Rectangle(p, caret.Bounds.Size);
        }
        void updateSelect()
        {
            int init = (int)Pulse.Text.getStringSizePBox(backingText.Substring(0, startSelect), chatLine.TextFont).Width;
            int w = (int)Pulse.Text.getStringSizePBox(backingText.Substring(startSelect, endSelect - startSelect), chatLine.TextFont).Width;
            selectBox.Bounds = new Rectangle(bounds.X + init + 3 + (startSelect == 0 ? 5 : 0), selectBox.Bounds.Y, w - 5 - (startSelect == 0 ? 3 : 0), selectBox.Bounds.Height);
            // Console.WriteLine(startSelect + " " + endSelect);
        }

        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.ControlLeft || e.Key == OpenTK.Input.Key.ControlRight)
            {
                ctrldown = false;
            }
            else if (e.Key == OpenTK.Input.Key.ShiftLeft || e.Key == OpenTK.Input.Key.ShiftRight)
            {
                shiftdown = false;
            }
            else if (e.Key == Key.AltLeft || e.Key == Key.AltRight)
            {
                altdown = false;
            }
        }
        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {

            if (expanded)
            {

                if (e.Key == OpenTK.Input.Key.ControlLeft || e.Key == OpenTK.Input.Key.ControlRight)
                {
                    ctrldown = true;
                }
                else if (e.Key == Key.ShiftLeft || e.Key == OpenTK.Input.Key.ShiftRight)
                {
                    shiftdown = true;
                }
                else if (e.Key == Key.AltLeft || e.Key == Key.AltRight)
                {
                    altdown = true;
                }
                if (e.Key == OpenTK.Input.Key.V && ctrldown)
                {
                    if (Clipboard.ContainsText())
                    {
                        string toadd = Clipboard.GetText().Replace('\n', ' ');
                        backingText += toadd;
                        position += toadd.Length;
                        chatLine.Update(backingText);
                        updateCaret();
                    }

                }
                if (e.Key == OpenTK.Input.Key.A && ctrldown)
                {
                    if (backingText.Length > 0)
                    {
                        selecting = true;
                        startSelect = 0;
                        endSelect = backingText.Length;
                        selectcounter += backingText.Length;
                        position = backingText.Length;
                        updateSelect();
                        updateCaret();
                    }
                }
                if (e.Key == OpenTK.Input.Key.C && ctrldown)
                {
                    if (selecting && startSelect != endSelect)
                    {
                        string sub = backingText.Substring(startSelect, endSelect - startSelect);
                        Clipboard.SetText(sub);
                    }
                }
                if (e.Key == Key.X && ctrldown)
                {
                    if (selecting && startSelect != endSelect)
                    {
                        string sub = backingText.Substring(startSelect, endSelect - startSelect);
                        Clipboard.SetText(sub);
                        position = startSelect;
                        backingText = backingText.Remove(startSelect, endSelect - startSelect);
                        selecting = false;
                        updateCaret();

                        chatLine.Update(backingText);
                    }
                }
                if (e.Key == OpenTK.Input.Key.Delete)
                {
                    if (!selecting)
                    {
                        if (backingText.Length > 0 && position < backingText.Length)
                        {
                            backingText = backingText.Remove(position, 1);
                            chatLine.Update(backingText);
                        }
                    }
                    else
                    {
                        this.game_KeyPress(null, new OpenTK.KeyPressEventArgs((char)8)); //"simulate" backspace
                    }
                }
                if (e.Key == OpenTK.Input.Key.Home)
                {
                    position = 0;
                    updateCaret();
                }
                if (e.Key == OpenTK.Input.Key.End)
                {
                    position = backingText.Length;
                    updateCaret();
                }
                if (e.Key == OpenTK.Input.Key.Left)
                {
                    if (position > 0)
                    {
                        if (shiftdown)
                        {

                            if (!selecting)
                            {
                                endSelect = position;
                                startSelect = position - 1;
                                selecting = true;
                                selectcounter--;
                            }
                            else
                            {
                                if (selectcounter > 0)
                                {
                                    endSelect--;
                                }
                                else
                                {
                                    startSelect--;
                                }
                                selectcounter--;
                            }
                            updateSelect();
                        }
                        else
                        {
                            selecting = false;
                            selectcounter = 0;
                        }
                        position--;
                        updateCaret();
                    }
                }
                else if (e.Key == OpenTK.Input.Key.Right)
                {
                    if (position < backingText.Length)
                    {
                        if (shiftdown)
                        {
                            //startSelect = 1;
                            //endSelect++;
                            if (!selecting)
                            {
                                startSelect = position;
                                endSelect = position + 1;
                                selecting = true;
                                selectcounter++;
                            }
                            else
                            {
                                if (selectcounter < 0)
                                {
                                    startSelect++;
                                }
                                else
                                {
                                    endSelect++;
                                }
                                selectcounter++;
                            }
                            updateSelect();
                        }
                        else
                        {
                            selecting = false;
                            selectcounter = 0;
                        }
                        position++;
                        updateCaret();
                    }
                }
                else if (e.Key == OpenTK.Input.Key.Up)
                {
                    if (scrollBack.Count > 0)
                    {
                        if (scrollbackpos == -1)
                        {
                            scrollbackpos = scrollBack.Count - 1;
                            backingText = scrollBack[scrollbackpos];
                            chatLine.Update(backingText);
                            position = backingText.Length;
                            updateCaret();
                        }
                        else if (scrollbackpos > 0)
                        {
                            scrollbackpos--;
                            backingText = scrollBack[scrollbackpos];
                            chatLine.Update(backingText);
                            position = backingText.Length;
                            updateCaret();
                        }
                    }
                }
                else if (e.Key == OpenTK.Input.Key.Down)
                {
                    if (scrollBack.Count > 0)
                    {
                        if (scrollbackpos != -1)
                        {
                            if (scrollbackpos < scrollBack.Count - 1)
                            {
                                scrollbackpos++;
                                backingText = scrollBack[scrollbackpos];
                                chatLine.Update(backingText);
                                position = backingText.Length;
                                updateCaret();
                            }
                        }
                    }
                }
                //pageup
                else if (e.Key == OpenTK.Input.Key.PageDown)
                {
                    if (!(activeTab.Scrollpos + bounds.Height - spacing - 20 < spacing)) // + bounds.Height - spacing is to orientate it so the scrollpos is based around the bottom of chat rather than top
                    {
                        activeTab.Scrollpos += 100;
                    }
                }
                //pagedown
                else if (e.Key == OpenTK.Input.Key.PageUp)
                {
                    if (!(activeTab.Scrollpos + bounds.Height - spacing - 20 < spacing)) // + bounds.Height - spacing is to orientate it so the scrollpos is based around the bottom of chat rather than top
                    {
                        activeTab.Scrollpos += -100;
                    }
                }
                else if (altdown)
                {
                    int c = (int)e.Key;
                    if (c >= 109 && c <= 118)
                    {
                        int index = c - 110;
                        if (index <= -1)
                        {
                            index = 9; //looparound for 0
                        }
                        if (tabs.Count > index)
                        {
                            activeTab = tabs.ElementAt(index).Value;
                            var i = from element in tabs orderby element descending where element.Value.IsActive select element;
                        }
                    }
                }
            }
            if (e.Key == OpenTK.Input.Key.F2)
            {
                //                Clipboard.GetText(TextDataFormat.)
                if (expanded)
                {
                    minimize(.5);
                }
                else
                {
                    maximize(.5);
                }
                //chatLine.Alpha = .4f;
                //chatLine.fade(.4f, 2);
            }
        }
        void game_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            if (expanded)
            {
                char c = e.KeyChar;
                bool del = false;
                char delchar = ' ';
                int code = (int)c;
                //   Console.WriteLine((int)c);
                /*     if (altdown && c >= 49 && c <= 57)
                     {
                         int index = c - 49;
                         if (tabs.Count > index)
                         {
                             activeTab = tabs.ElementAt(index).Value;
                         }
                     }*/
                if ((int)c == 8) //backspace
                {
                    try
                    {
                        if (!selecting)
                        {
                            delchar = backingText[position - 1];
                            backingText = backingText.Remove(position - 1, 1);
                            del = true;

                            if (position > 0)
                            {
                                position--;
                            }
                        }
                        else if (startSelect != endSelect)
                        {
                            backingText = backingText.Remove(startSelect, endSelect - startSelect);
                            position = startSelect;
                            selecting = false;
                            selectcounter = 0;
                        }
                    }
                    catch { }
                }
                else if ((int)c == 13) //enter 
                {

                    if (backingText != "")
                    {
                        backingText = backingText.Replace('\r', ' ');
                        backingText = backingText.Replace('\n', ' ');
                        if (selecting)
                        {
                            selecting = false;
                            selectcounter = 0;
                            //     updateSelect();
                        }
                        if (scrollBack.Count > 20)
                        {
                            scrollBack.RemoveAt(0);
                        }
                        scrollBack.Add(backingText);
                        scrollbackpos = -1;
                        Color addColor = Color.White;
                        bool toAdd = true;
                        bool timeStamp = false;
                        /* foreach (object o in Game.lua.DoString(backingText))
                         {
                             addLine(o.ToString());
                         }*/

                        if (backingText.StartsWith("/lua"))
                        {
                            if (backingText.StartsWith("/luasr"))
                            {
                                string dir = backingText.Substring(7);
                                if (System.IO.File.Exists(dir))
                                {
                                    theScript = new LuaScript(dir);
                                    theScript.execute();
                                }
                                else
                                {
                                    addLine("Could not find script " + dir, Color.Red);
                                }
                            }
                            else
                            {
                                try
                                {
                                    Game.lua.DoString(backingText.Substring(4));
                                    addColor = Color.Snow;
                                }
                                catch (Exception f)
                                {
                                    Console.WriteLine(f);
                                    addLine("Error in syntax.", Color.Red);
                                }
                            }
                        }
                        else
                        {
                            string[] refactorme = backingText.Split(' ');
                            if (refactorme[0].Equals("/help")) //looks weird with line being displayed then user input, consider refactoring to flip order? not a priority anyway :\ (or just completely hide user echo)
                            {
                                if (refactorme.Length == 1)
                                {
                                    addLine(Utils.chatCommands["help"], Color.SteelBlue);

                                }
                                else
                                {
                                    if (refactorme.Length > 1)
                                    {
                                        if (Utils.chatCommands.ContainsKey(refactorme[1].ToLower(Config.cultureEnglish)))
                                        {
                                            addLine(Utils.chatCommands[refactorme[1].ToLower(Config.cultureEnglish)], Color.SteelBlue);
                                        }
                                    }
                                }
                            }
                            else if (refactorme[0].Equals("/notice"))
                            {
                                Game.nm.addNotice(refactorme[1]);
                            }
                            else if (refactorme[0].Equals("/call"))
                            {
                                if (theScript != null && refactorme.Length > 1)
                                {

                                    theScript.CallFunction(refactorme[1]);
                                }
                            }
                            else if (ic != null)
                            {

                                if (!backingText.StartsWith("/"))
                                {
                                    ic.sendPm(activeTab.Title, backingText); //convenient :s
                                    backingText = "<" + ic.realNick + "> " + backingText;
                                    timeStamp = true;
                                }
                                else
                                {

                                    string[] splitted = backingText.Split(' '); //it's a command :D
                                    string com = splitted[0].ToLower(Config.cultureEnglish); //ignore case                                    
                                    if (com.Equals("/pm") || com.Equals("/msg"))
                                    {
                                        if (splitted.Length < 3)
                                        {
                                            addLine("Not enough parameters", Color.Red);
                                        }
                                        else
                                        {
                                            string user = splitted[1]; //to pm
                                            string message = backingText.Substring(backingText.IndexOf(splitted[2])); //the message
                                            ic.sendPm(user, message);
                                            //addLine(message, user);
                                            int index = backingText.IndexOf(com);
                                            //     Console.WriteLine(index);
                                            // Console.WriteLine(backingText.Substring(index));
                                            backingText = "<To " + user + "> " + backingText.Substring(backingText.IndexOf(splitted[2]));
                                            addColor = Color.Green;
                                            timeStamp = true;
                                        }
                                    }
                                    else if (com.Equals("/me"))
                                    {
                                        if (splitted.Length > 1)
                                        {
                                            string message = backingText.Substring(backingText.IndexOf(splitted[1]));
                                            backingText = ((char)1) + "ACTION " + message + ((char)1);
                                            ic.sendPm(activeTab.Title, backingText);
                                            backingText = "*" + ic.realNick + " " + message;
                                            timeStamp = true;
                                        }
                                    }
                                    else if (com.Equals("/np"))
                                    {
                                        string message = "is listening to " + (Game.M.CurrentSong.ID < 0 ? Game.M.CurrentSong.Artist + " - " + Game.M.CurrentSong.SongName : "http://p.ulse.net/song?id=" + Game.M.CurrentSong.ID);
                                        backingText = ((char)1) + "ACTION " + message + ((char)1);
                                        ic.sendPm(activeTab.Title, backingText);
                                        backingText = "*" + ic.realNick + " " + message;
                                        timeStamp = true;
                                    }
                                    else if (com.Equals("/spec"))
                                    {
                                        try
                                        {
                                            if (!Config.Spectating)
                                            {
                                                if (!backingText.Substring(backingText.IndexOf(splitted[1])).ToLower().Equals(Account.currentAccount.AccountName.ToLower()))
                                                {
                                                    Client.PacketWriter.sendSpectateHook(Game.conn.Bw, backingText.Substring(backingText.IndexOf(splitted[1])).ToLower());
                                                    addLine("Spectating " + backingText.Substring(backingText.IndexOf(splitted[1])).ToLower() + " (if they are online)");
                                                }
                                                else
                                                {
                                                    addLine("You cant spec yourself");
                                                }
                                            }
                                            else
                                            {
                                                if (!Config.SpectatedUser.Equals(""))
                                                {
                                                    addLine("Canceling spectate on " + Config.SpectatedUser);
                                                }
                                                Client.PacketWriter.sendSpectateCancel(Game.conn.Bw, Config.SpectatedUser);
                                                Config.SpectatedUser = "";
                                                Config.Spectating = false;
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    else if (com.Equals("/close"))
                                    {
                                        if (activeTab.Title != baseChannel)
                                        {
                                            if (activeTab.Title.StartsWith("#")) //we're in a channel
                                            {
                                                ic.sendPart(activeTab.Title, "Leaving");
                                            }
                                            activeTab.tabRect.fade(0, .25);
                                            activeTab.tabText.fade(0, .25);
                                            toRemove.Add(activeTab);
                                            foreach (var i in tabs)
                                            {
                                                activeTab = i.Value; //get the first one
                                                break;
                                            }
                                            toAdd = false;
                                        }
                                        else
                                        {
                                            addLine("You cannot leave " + baseChannel + ".", Color.Red);
                                        }
                                    }
                                    else if (com.Equals("/j") || com.Equals("/join"))
                                    {
                                        if (splitted.Length < 2)
                                        {
                                            addLine("Not enough parameters", Color.Red);
                                        }
                                        else
                                        {
                                            string channel = splitted[1];
                                            if (channel.StartsWith("#"))
                                            {
                                                if (!tabs.ContainsKey(channel))
                                                {
                                                    ic.joinChan(channel);
                                                    addTab(channel);
                                                    activeTab = tabs[channel];
                                                    addLine("Joining channel " + channel, Color.SteelBlue);
                                                }
                                                else
                                                {
                                                    addLine("Channel already open.", Color.Red);
                                                }
                                                toAdd = false;
                                            }
                                            else
                                            {
                                                addLine("Channels must start with #", Color.Red);
                                            }
                                        }
                                    }
                                    else if (com.Equals("/r")) //consider taking out, could be dangerous for non-admins, e.g could part
                                    {
                                        if (splitted.Length < 2)
                                        {
                                            addLine("Not enough parameters", Color.Red);
                                        }
                                        else
                                        {
                                            string rest = backingText.Substring(backingText.IndexOf(splitted[1]));
                                            ic.sendRaw(rest);
                                            backingText = "RAW: " + rest;
                                            addColor = Color.Red;
                                        }
                                    }
                                    else if (com.Equals("/savelog"))
                                    {
                                        string date = activeTab.Title + ";" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + ";" + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + "." + DateTime.Now.Millisecond + ".txt";
                                        if (!Directory.Exists("chatlogs"))
                                        {
                                            Directory.CreateDirectory("chatlogs");
                                        }
                                        File.WriteAllLines("chatlogs\\" + date, activeTab.Lines.ConvertAll<string>(new Converter<Text, string>(delegate(Pulse.Text t)
                                        {
                                            return t.Line;
                                        })));
                                        addLine("Saved to chatlogs\\" + date, Color.SteelBlue);
                                        toAdd = false;
                                    }
                                    else if (com.Equals("/set"))
                                    {
                                        if (splitted.Length > 1)
                                        {
                                            if (splitted[1].Equals("verbosity"))
                                            {
                                                ircverbose = !ircverbose;
                                                addLine("Irc verbosity set to " + ircverbose, Color.Red);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        addLine("Command not recognized", Color.Red);
                                    }
                                }

                            }
                            else
                            {
                                addLine("Not connected to irc!", Color.Red);
                            }
                        }
                        if (toAdd)
                        {

                            addLine((timeStamp ? DateTime.Now.Hour.ToString("D2") + ":" + DateTime.Now.Minute.ToString("D2") + " " : "") + backingText, addColor);
                        }

                        backingText = "";
                        position = 0;

                    }

                }
                else if (!Char.IsControl(c))
                {
                    //backingText += c;
                    if (selecting && startSelect != endSelect)
                    {
                        backingText = backingText.Remove(startSelect, endSelect - startSelect);
                        selecting = false;
                        selectcounter = 0;
                        //updateSelect();
                        position = startSelect;
                    }
                    //   chatLine.Position = chatLine.Position + new SizeF(-5, 0);
                    backingText = backingText.Insert(position, c.ToString());
                    position++;
                }

                chatLine.Update(backingText);
                //Console.WriteLine(Pulse.Text.getStringSize(backingText, chatLine.textFont).Width);
                updateCaret();
                //updateCaret(del, delchar);
            }
        }
        void Mouse_WheelChanged(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            // Console.WriteLine(lines.Count * spacing + " " + (scrollpos + bounds.Height - spacing) + " " + scrollpos); //clamp between that and 0 + spacing
            if (bg.ModifiedBounds.Contains(new Point(game.Mouse.X, game.Mouse.Y)))
            {
                //  if(scrollpos > (-bounds.Height + spacing) - (lines.Count * spacing))
                if (activeTab.Scrollpos + bounds.Height - spacing - 20 < spacing && e.DeltaPrecise > 0) // + bounds.Height - spacing is to orientate it so the scrollpos is based around the bottom of chat rather than top
                {
                    //scrollpos = spacing;
                    //  Console.WriteLine("hit limit upper!");
                    return; //return if user wants to scroll up but no more, same with next if
                }
                else if (activeTab.Scrollpos + bounds.Height - spacing + 20 > activeTab.Lines.Count * spacing && e.DeltaPrecise < 0) //lines.Count * spacing represents length of all texts combined, don't want bottom of text to go over that
                {
                    //scrollpos = scrollpos + bounds.Height - spacing;
                    //   Console.WriteLine("hit limit lower!");
                    return;
                }
                activeTab.Scrollpos += (e.DeltaPrecise > 0 ? -20 : 20);
                //   Console.WriteLine(e.DeltaPrecise + " " + (e.DeltaPrecise > 0 ? 5 : -5));
            }
        }
        #region animation

        public void minimize(double time)
        {
            bg.move(new Point(bg.Bounds.X, bg.Bounds.Y + bg.Bounds.Height), time);
            bg.fade(0, time);
            foreach (LinkText t in activeTab.Lines)
            {
                t.fade(0, time / 3);
                t.fadeLinks(0, time / 3);
            }
            foreach (var i in tabs)
            {
                i.Value.tabRect.fade(0, time / 3);
                i.Value.tabText.fade(0, time / 3);
            }
            caret.fade(0, time / 3);
            chatLine.fade(0, time / 3);
            expanded = false;
            //   game.Keyboard.KeyRepeat = false;
        }
        public void maximize(double time)
        {
            //   Point p = new Point(bg.Bounds.X, bg.Bounds.Y - bg.Bounds.Height);
            bg.move(new Point(bg.Bounds.X, origpos), time / 2);
            bg.fade((200 / 255f), time);
            foreach (LinkText t in activeTab.Lines)
            {
                t.fade(1, time / 2);
                t.fadeLinks((100f / 255), time / 2);
            }

            foreach (var i in tabs)
            {
                i.Value.tabRect.fade(1, time / 2);
                i.Value.tabText.fade(1, time / 2);
            }
            caret.fade(1, time / 2);
            chatLine.fade(1, time / 2);
            expanded = true;
            //   game.Keyboard.KeyRepeat = true;
            //   bg.draw(new OpenTK.FrameEventArgs(time));
        }
        #endregion
        public void closeTab(string name)
        {
            if (name != baseChannel)
            {
                if (name == activeTab.Title)
                {
                    activeTab.tabRect.fade(0, .25);
                    activeTab.tabText.fade(0, .25);
                    toRemove.Add(activeTab);
                    foreach (var i in tabs)
                    {
                        activeTab = i.Value; //get the first one
                        if (!expanded)
                        {
                            foreach (Text t in activeTab.Lines)
                            {
                                t.Alpha = 0;
                            }
                        }
                        break;
                    }
                }
                else
                {
                    if (tabs.ContainsKey(name))
                    {
                        tabs[name].tabRect.fade(0, .25);
                        tabs[name].tabText.fade(0, .25);
                        toRemove.Add(tabs[name]);
                    }
                }
            }
        }

        /// <summary>
        /// Deprecated. obsolete for any purpose.
        /// </summary>
        /// <param name="lines"></param>
        void calcPos(List<Text> lines)
        {
            Point bp = bounds.Location;
            int counter = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Location = new Point(bp.X + 5, bp.Y + counter);
                counter += spacing;
            }
        }
        public void addLine(string s)
        {
            addLine(s, Color.White, activeTab.Title);
        }
        public void addLine(string s, Color col)
        {
            addLine(s, col, activeTab.Title);
        }
        public void addLine(string s, string target)
        {
            addLine(s, Color.White, target);
        }
        public void addLine(string s, Color c, string target)
        {
            Dictionary<Pair<int, int>, Color> colors = new Dictionary<Pair<int, int>, Color>();
            colors.Add(new Pair<int, int>(0, s.Length), c);
            addLine(s, colors, target);
            /*  LinkText t = new LinkText(new Size(bounds.Width, spacing), new Point(bounds.Location.X + 5, bounds.Location.Y + (tabs.ContainsKey(target) ? tabs[target] : activeTab).lineCounter * spacing));
              t.Update(s);

              t.Colour = c;
           
              /*if(s.Length > 20) {
                   t.customcol = true;
                  t.colors.Add(new Pair<int,int>(0,5), Color.Green);
                  t.colors.Add(new Pair<int,int>(5, 10), Color.SteelBlue);
                  t.colors.Add(new Pair<int, int>(10, s.Length), Color.Red);
              }
           
             // t.colors.Add()
              if (!expanded && target == activeTab.Title)
              {
                  t.Alpha = 0;
              }
            
              t.TextFont = new Font("Myriad Pro", 15);
              t.Shadow = false;
              t.parse(); //parse after font set
              if (tabs.ContainsKey(target))
              {
                  tabs[target].lineCounter++;
                  tabs[target].Lines.Add(t);
                  tabs[target].Scrollpos += spacing;
                  if (tabs[target].Lines.Count > maxSize)
                  {
                      tabs[target].Lines.RemoveAt(0);
                      //  tabs[target].Scrollpos -= spacing;
                      //t.Location = new Point(bounds.Location.X + 5, bounds.Location.Y + (tabs[target].Lines.Count + 1) * spacing); //adding tab may not have succeeded
                      //  calcPos(tabs[target].Lines);
                  }

              }
              else if (target == "")
              {
                  activeTab.lineCounter++;
                  activeTab.Lines.Add(t);
                  activeTab.Scrollpos += spacing;
                  if (activeTab.Lines.Count > maxSize)
                  {
                      activeTab.Lines.RemoveAt(0);
                  }
              }
              else
              {
                  //not worried the tab will be full ^^
                  Tab toAdd = new Tab(-bounds.Height + spacing, new List<Text>(), target, this);
                  addTab(toAdd);
                  t.Location = new Point(bounds.Location.X + 5, bounds.Location.Y + (tabs.ContainsKey(target) ? tabs[target] : activeTab).lineCounter * spacing); //adding tab may not have succeeded

                  toAdd.Lines.Add(t);
                  toAdd.Scrollpos += spacing;
                  toAdd.lineCounter++;
                  /* if (!expanded)
                   {
                       toAdd.tabRect.Alpha = 0; //.Colour = new OpenTK.Graphics.Color4(toAdd.tabRect.Colour.R, toAdd.tabRect.Colour.G, toAdd.tabRect.Colour.B, 0);
                       toAdd.tabText.Alpha = 0;
                   }*/
            /*foreach (Text tb in toAdd.Lines)
            {
                Console.WriteLine(tb.Line);
            }
        }*/
        }
        public static readonly Font boxFont = new Font("Myriad Pro", 15);
        public void addLine(string s, Dictionary<Pair<int, int>, Color> ccc, string target)
        {
            bool multiline = false;
            Dictionary<Pair<int, int>, Color> newcolors = null;
            string rest = "";
            if (Pulse.Text.getStringSizePBox(s, boxFont).Width > Bounds.Width)
            {
                int cutpos = 0;
                string temp = s;
                int lastcutpos = 0;
                int lowerbound = 0;
                int upperbound = s.Length;
                while (true)
                {
                    lastcutpos = cutpos;
                    if (cutpos == 0)
                    {
                        cutpos = temp.Length / 2;
                    }
                    else
                    {
                        //    cutpos = temp.Substring(0, cutpos).Length / 2;
                    }
                    if (Pulse.Text.getStringSizePBox(temp.Substring(0, cutpos), boxFont).Width + 10 > Bounds.Width)
                    {
                        upperbound = cutpos;
                        cutpos = (lowerbound + upperbound) / 2;
                    }
                    else
                    {
                        lowerbound = cutpos;
                        cutpos = (lowerbound + upperbound) / 2;
                    }
                    // Console.WriteLine("cutpos " + cutpos + " " + s);
                    if (lastcutpos == cutpos)
                    {
                        Console.WriteLine("cutpos " + cutpos + " " + s);
                        break;
                    }
                }
                multiline = true;
                newcolors = new Dictionary<Pair<int, int>, Color>();
                Dictionary<Pair<int, int>, Color> toremove = new Dictionary<Pair<int, int>, Color>();
                Dictionary<Pair<int, int>, Color> toadd = new Dictionary<Pair<int, int>, Color>();
                foreach (var i in ccc)
                {
                    if (i.Key.key >= cutpos) //if it's greater or equal then we it goes on next string no questions asked
                    {
                        toremove.Add(i.Key, i.Value);
                        newcolors.Add(new Pair<int, int>(i.Key.key - cutpos, i.Key.value - cutpos), i.Value);
                    }
                    else if (i.Key.value > cutpos && i.Key.key < cutpos) //split the coloring into two
                    {
                        toremove.Add(i.Key, i.Value);
                        toadd.Add(new Pair<int, int>(i.Key.key, cutpos), i.Value);
                        newcolors.Add(new Pair<int, int>(0, i.Key.value - cutpos), i.Value);
                    }

                }
                foreach (var i in toremove)
                {
                    ccc.Remove(i.Key);
                }
                foreach (var i in toadd)
                {
                    ccc.Add(i.Key, i.Value);
                }
                rest = s.Substring(cutpos, s.Length - cutpos);
                s = s.Substring(0, cutpos);
                // var hack = ccc.ElementAt(ccc.Count - 1);
                //ccc.Add(new Pair<int, int>(hack.Key.key, s.Length), hack.Value);
                // ccc.Remove(hack.Key);
            }
            LinkText t = new LinkText(new Size(bounds.Width, spacing), new Point(bounds.Location.X + 5, bounds.Location.Y + (tabs.ContainsKey(target) ? tabs[target] : activeTab).lineCounter * spacing));
            t.Layer = 9.5;
            t.Update(s);
            t.Colour = Color.White;
            t.customcol = true;
            t.colors = ccc;
            // t.colors.Add()
            if (!expanded && target == activeTab.Title)
            {
                t.Alpha = 0;
            }
            t.TextFont = boxFont;
            t.Shadow = false;
            t.parse(); //parse after font set

            if (tabs.ContainsKey(target))
            {
                tabs[target].lineCounter++;
                tabs[target].Lines.Add(t);
                tabs[target].Scrollpos += spacing;
                if (tabs[target].Lines.Count > maxSize)
                {
                    tabs[target].Lines.RemoveAt(0);
                    //  tabs[target].Scrollpos -= spacing;
                    //t.Location = new Point(bounds.Location.X + 5, bounds.Location.Y + (tabs[target].Lines.Count + 1) * spacing); //adding tab may not have succeeded
                    //  calcPos(tabs[target].Lines);
                }
            }
            else if (target == "")
            {
                activeTab.lineCounter++;
                activeTab.Lines.Add(t);
                activeTab.Scrollpos += spacing;
                if (activeTab.Lines.Count > maxSize)
                {
                    activeTab.Lines.RemoveAt(0);
                }
            }
            else
            {
                //not worried the tab will be full ^^
                Tab toAdd = new Tab(-bounds.Height + spacing, new List<Text>(), target, this);
                addTab(toAdd);
                t.Location = new Point(bounds.Location.X + 5, bounds.Location.Y + (tabs.ContainsKey(target) ? tabs[target] : activeTab).lineCounter * spacing); //adding tab may not have succeeded

                toAdd.Lines.Add(t);
                toAdd.Scrollpos += spacing;
                toAdd.lineCounter++;
                /* if (!expanded)
                 {
                     toAdd.tabRect.Alpha = 0; //.Colour = new OpenTK.Graphics.Color4(toAdd.tabRect.Colour.R, toAdd.tabRect.Colour.G, toAdd.tabRect.Colour.B, 0);
                     toAdd.tabText.Alpha = 0;
                 }*/
                /*foreach (Text tb in toAdd.Lines)
                {
                    Console.WriteLine(tb.Line);
                }*/
            }
            if (multiline)
            {
                addLine(rest, newcolors, target);
            }
        }
        //    Stopwatch sw = new Stopwatch();
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            if (bg.Color.A > 0f)
            {
                bg.Layer = 9;
                bg.OnRenderFrame(e);
                var elements = from t in activeTab.Lines where (t.Location.Y - activeTab.Scrollpos) > bounds.Y && (t.Location.Y - activeTab.Scrollpos) < (bounds.Y + bounds.Height - spacing) select t;
                //    sw.Reset();
                //     sw.Start();
                //it appears linq is slightly faster but the difference is negligable, it has its spikes as well
                //consider doing updating in update

                foreach (LinkText t in elements)
                {
                    t.Layer = 9.5;
                    //     if ((t.Location.Y - activeTab.Scrollpos) > bounds.Y && (t.Location.Y - activeTab.Scrollpos) < (bounds.Y + bounds.Height - spacing))
                    t.Location = new Point(t.Location.X, t.Location.Y - activeTab.Scrollpos);
                    t.updateLinks();
                    t.OnRenderFrame(e);
                    t.Location = new Point(t.Location.X, t.Location.Y + activeTab.Scrollpos); //return text to original position, as changes in scrollpos will be reflected next subtract. if this line is removed, scrollpos will be subtracted everytime it comes in range :\ (so will jump out of range)

                }
                /*       if (sw.ElapsedMilliseconds != 0)
                       {
                           Console.WriteLine(sw.ElapsedMilliseconds);
                       }
                       sw.Stop();*/

                if (selecting && startSelect != endSelect)
                {
                    selectBox.Layer = 9.2;
                    selectBox.OnRenderFrame(e);
                }
                chatLine.Layer = 9.1;
                chatLine.OnRenderFrame(e);
                if (caratcounter < 700)
                {
                    caret.OnRenderFrame(e);
                }
                int counter = bounds.X;
                int ycounter = bounds.Y - 25;
                foreach (var t in tabs)
                {
                    Rect rD = t.Value.tabRect;//.draw(e);
                    rD.Layer = 9.1;
                    Text tD = t.Value.tabText;//.draw(e);   
                    tD.Layer = 9.2;
                    rD.OnRenderFrame(e);
                    tD.OnRenderFrame(e);
                    counter += rD.Bounds.Width + 10; //10 pixel padding
                }

                if (!Game.MouseState.LeftButton)
                {
                    canPress = true;
                }
            }
            else
            {
                bg.OnRenderFrame(e); //required so if it starts transitioning again, will update (really should be moved to an update() method tbh)
            }
        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {

            base.OnUpdateFrame(e);
            caratcounter += (e.Time * 1000);
            if (caratcounter > 1000)
            {
                caratcounter = 0;
            }

            foreach (Tab t in toRemove)
            {
                // Console.WriteLine(t.tabRect.Alpha);
                if (t.tabRect.Color.A <= 0 || !t.tabRect.Fading) //t.tabRect.Alpha doesn't update when fading, need to 
                {
                    tabs.Remove(t.Title);
                    removeFromRemove.Add(t);
                }
            }
            foreach (Tab t in removeFromRemove)
            {
                toRemove.Remove(t);
            }
            if (expanded)
            {
                int counter = bounds.X;
                int ycounter = bounds.Y - 25;
                foreach (var t in tabs)
                {
                    Rect rD = t.Value.tabRect;//.draw(e);

                    Text tD = t.Value.tabText;//.draw(e);
                    if (counter > bounds.Width - rD.Bounds.Width - 10)
                    {
                        counter = bounds.X;
                        ycounter -= rD.Bounds.Height + 5; //5 px padding
                    }
                    rD.Bounds = new Rectangle(new Point(counter, ycounter), rD.Bounds.Size);
                    tD.Location = new Point(counter + 5, ycounter);
                    if (rD.ModifiedBounds.Contains(new Point(game.Mouse.X, game.Mouse.Y)) && Game.MouseState.LeftButton && canPress)
                    {
                        activeTab = t.Value;
                        canPress = false;
                        Game.lClickFrame = true;
                        Console.WriteLine("click");
                    }
                    rD.OnRenderFrame(e);
                    tD.OnRenderFrame(e);
                    counter += rD.Bounds.Width + 10; //10 pixel padding
                }

                if (!Game.MouseState.LeftButton)
                {
                    canPress = true;
                }
            }
        }
    }

    public class Tab
    {
        private double layer;

        public double Layer
        {
            get { return layer; }
            set { layer = value; if (tabRect != null) tabRect.Layer = value; if (tabText != null) tabText.Layer = value + 0.1; }
        }
        private string title;
        public Rect tabRect;
        public Text tabText;
        private bool alert;
        private bool isActive;
        public int lineCounter;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                if (isActive)
                {
                    tabRect.Color = Color.Orange;
                    if (!pb.expanded)
                    {
                        tabRect.Alpha = 0;
                    }
                    if (Alert)
                    {
                        Alert = false;
                    }
                }
                else
                {
                    tabRect.Color = Color.SteelBlue;
                    if (!pb.expanded)
                    {
                        tabRect.Alpha = 0;
                    }
                }
            }
        }

        public bool Alert
        {
            get { return alert; }
            set
            {
                alert = value;
                if (alert && !this.IsActive) //make sure not the active tab
                {
                    tabRect.Color = Color.Blue;
                    if (!pb.expanded)
                    {
                        tabRect.Alpha = 0;
                    }
                }
                else //the only reason it will go to not-alert is if the tab becomes active
                {
                    if (!this.IsActive)
                    {
                        tabRect.Color = Color.SteelBlue;
                        if (!pb.expanded)
                        {
                            tabRect.Alpha = 0;
                        }
                    }
                }
            }
        }
        PTextBox pb;
        public Tab(int scrollpos, List<Text> lines, string title, PTextBox box)
        {
            this.scrollpos = scrollpos;
            this.lines = lines;
            foreach (Text t in lines)
            {
                t.Layer = 9.5;
            }
            Title = title;
            tabRect = new Rect(new Rectangle(0, 0, 150, 25), Skin.skindict["tabBG"]);
            tabRect.Color = Color.SteelBlue;
            tabText = new Text(Config.ClientSize, tabRect.Bounds.Size, Point.Empty);
            tabText.textFont = PTextBox.boxFont;
            tabText.Shadow = false;
            tabText.Update(title);
            pb = box;
        }
        public String Title //should be equal to the key in the dictionary, and not modified
        {
            get { return title; }
            private set { title = value; }
        }
        List<Text> lines;

        public List<Text> Lines
        {
            get { return lines; }
            set { lines = value; foreach (Text t in lines) t.Layer = 9.5; }
        }
        int scrollpos;

        public int Scrollpos
        {
            get { return scrollpos; }
            set { scrollpos = value; }
        }

        /*void draw()
        {

        }*/
    }
    public class LinkText : Pulse.Text //Class for texts with hyperlinks
    {

        public LinkText(Size area, Point location)
            : base(Config.ClientSize, area, location)
        {

        }
        public override Point Location
        {
            get
            {
                return base.Location;
            }
            set
            {
                base.Location = value;
            }
        }
        public void updateLinks()
        {
            foreach (var i in links) //only y changes (HOPEFULLY)
            {
                i.Key.Bounds = new Rectangle(i.Key.Bounds.X, Location.Y + 3, i.Key.Bounds.Width, i.Key.Bounds.Height);
            }
        }
        public override void fade(float alpha, double timeSpan)
        {
            base.fade(alpha, timeSpan);
        }
        public void fadeLinks(float alpha, double timeSpan)
        {
            foreach (var i in links)
            {
                i.Key.fade(alpha, timeSpan);
            }
        }
        //Point lastLocation;
        public override void Update(string s)
        {
            base.Update(s);
            //parse(s);
        }
        //List<string> links = new List<string>();
        //todo
        Dictionary<Rect, string> links = new Dictionary<Rect, string>();

        void launchSite(string s)
        {
            System.Diagnostics.Process.Start(s);
        }
        bool canpress;
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            foreach (var i in links)
            {
                i.Key.Layer = 9.5;
                if (Game.pbox.expanded)
                {
                    if (i.Key.ModifiedBounds.Contains(new Point(Game.game.Mouse.X, Game.game.Mouse.Y)))
                    {
                        if (Game.MouseState.LeftButton && canpress)
                        {
                            launchSite(i.Value);
                            canpress = false;
                        }
                        if (!i.Key.Fading)
                        {
                            i.Key.Color = Color.FromArgb(100, Color.Orange);
                        }
                    }
                    else
                    {
                        if (!i.Key.Fading)
                        {
                            i.Key.Color = Color.FromArgb(100, Color.SteelBlue);
                        }
                    }
                }
                i.Key.OnRenderFrame(e);
                //    Console.WriteLine(i.Key.Fading + " " + i.Key.Colour.A);
            }
            base.OnRenderFrame(e);
            if (!Game.MouseState.LeftButton)
            {
                canpress = true;
            }
        }

        public void parse()
        {
            try
            {
                links.Clear();
                Regex regexObj = new Regex(@"\b(?:(?:https?|ftp|file)://|www\.|ftp\.)[-A-Z0-9+&@#/%=~_|$?!:,.]*[A-Z0-9+&@#/%=~_|$]", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Match matchResults = regexObj.Match(Line);
                while (matchResults.Success)
                {
                    // matched text: matchResults.Value
                    // match start: matchResults.Index
                    // match length: matchResults.Length
                    int w = (int)Text.getStringSizePBox(Line.Substring(matchResults.Index, matchResults.Length), textFont).Width;
                    Rect r = new Rect(new Rectangle(Location.X + ((int)Text.getStringSizePBox(this.Line.Substring(0, matchResults.Index), this.textFont).Width) + (matchResults.Index == 0 ? 0 : -4), this.Location.Y + 3, w, 19));
                    r.Color = Color.FromArgb(100, Color.SteelBlue);
                    links.Add(r, matchResults.Value);
                    matchResults = matchResults.NextMatch();
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}

