using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using System.Xml;
using Pulse.UI;
using LuaInterface;
using Lua511;
namespace Pulse
{
    public class Skin
    {
        //hardcoded strings meh

        public static Dictionary<string, string> skindict
        {
            get;
            set;
        }
        private static int holdFrameCount = 3;

        public static int HoldFrameCount
        {
            get { return Skin.holdFrameCount; }
            set { Skin.holdFrameCount = value; }
        }
        private static int noteFrameRate = 10;

        public static int NoteFrameRate
        {
            get { return noteFrameRate; }
            set { noteFrameRate = value; }
        }
        private static int noteFrameCount = 1;

        public static int NoteFrameCount
        {
            get { return noteFrameCount; }
            set { noteFrameCount = value; }
        }
        private static int burstFrameCount = 10;

        public static int BurstFrameCount
        {
            get { return burstFrameCount; }
            set { burstFrameCount = value; }
        }
        private static int burstFrameRate = 30;
        private static Size accSize = new Size(30, 30);

        public static Size AccSize
        {
            get { return Skin.accSize; }
            set { Skin.accSize = value; }
        }
        private static Size scoreSize = new Size(30, 30);

        public static Size ScoreSize
        {
            get { return Skin.scoreSize; }
            set { Skin.scoreSize = value; }
        }
        public static int BurstFrameRate
        {
            get { return burstFrameRate; }
            set { burstFrameRate = value; }
        }
        private static int holdFrameRate = 30;

        public static int HoldFrameRate
        {
            get { return Skin.holdFrameRate; }
            set { Skin.holdFrameRate = value; }
        }
        private static int noteStyle = 0;
        private static bool maskBursts = false;

        public static bool MaskBursts
        {
            get { return Skin.maskBursts; }
            set { Skin.maskBursts = value; }
        }
        private static bool maskHolds = true;
        public static bool MaskHolds
        {
            get { return Skin.maskHolds; }
            set { Skin.MaskHolds = value; }
        }

        private static Color4 divColor;

        public static Color4 DivColor
        {
            get { return Skin.divColor; }
            set { Skin.divColor = value; }
        }
        public static int NoteStyle
        {
            get { return Skin.noteStyle; }
            set { Skin.noteStyle = value; }
        }
        private static bool colorHolds = true;

        public static bool ColorHolds
        {
            get { return Skin.colorHolds; }
            set { Skin.colorHolds = value; }
        }
        private static Color4[] endColors;

        public static Color4[] EndColors
        {
            get { return Skin.endColors; }
            set { Skin.endColors = value; }
        }private static Color4[] startColors;

        public static Color4[] StartColors
        {
            get { return Skin.startColors; }
            set { Skin.startColors = value; }
        }
        private static int[] laneLoc;

        public static int[] LaneLoc
        {
            get { return Skin.laneLoc; }
            set { Skin.laneLoc = value; }
        }
        private static int[] laneWidth;

        public static int[] LaneWidth
        {
            get { return Skin.laneWidth; }
            set { Skin.laneWidth = value; }
        }
        private static bool clearFrame = true;

        public static bool ClearFrame
        {
            get { return Skin.clearFrame; }
            set { Skin.clearFrame = value; }
        }
        private static string root; //never access this :/
        public static String Root
        {
            get
            {
                return "skin\\" + root + "\\";
            }
            set
            {
                root = value;
            }
        }
        private static Color4[] accColors;

        public static Color4[] AccColours
        {
            get { return Skin.accColors; }
            set { Skin.accColors = value; }
        }
        private static int textOverlap = 0;

        public static int TextOverlap
        {
            get { return Skin.textOverlap; }
            set { Skin.textOverlap = value; }
        }
        private static Color4[] barColors;

        public static Color4[] BarColours
        {
            get { return Skin.barColors; }
            set { Skin.barColors = value; }
        }
        private static bool bottomSpace = true;

        public static bool BottomSpace
        {
            get { return Skin.bottomSpace; }
            set { Skin.bottomSpace = value; }
        }
        private static Color4[] keyColors;

        public static Color4[] KeyColours
        {
            get { return Skin.keyColors; }
            set { Skin.keyColors = value; }
        }
        private static Color4[] lightColors;

        public static Color4[] LightColours
        {
            get { return Skin.lightColors; }
            set { Skin.lightColors = value; }
        }
        private static Color4[] burstColours;

        public static Color4[] BurstColours
        {
            get { return Skin.burstColours; }
            set { Skin.burstColours = value; }
        }
        private static Color4 pulseColor;

        public static Color4 PulseColour
        {
            get { return Skin.pulseColor; }
            set { Skin.pulseColor = value; }
        }
        private static Point scoreLocation = new Point(10, 703);

        public static Point ScoreLocation
        {
            get { return Skin.scoreLocation; }
            set { Skin.scoreLocation = value; }
        }
        private static Point accLocation = new Point(295, 703);

        public static Point AccLocation
        {
            get { return Skin.accLocation; }
            set { Skin.accLocation = value; }
        }
        private static Point frameLoc = new Point(0, 0);

        public static Point FrameLoc
        {
            get { return Skin.frameLoc; }
            set { Skin.frameLoc = value; }
        }
        private static void defaultskin(string rootr)
        {
            Root = rootr;
            skindict = new Dictionary<string, string>();
            keyColors = new Color4[8];
            burstColours = new Color4[8];
            lightColors = new Color4[8];
            barColors = new Color4[8];
            accColors = new Color4[5];
            EndColors = new Color4[8];
            startColors = new Color4[8];
            LaneWidth = new int[8];
            laneLoc = new int[8];
            for (int x = 0; x < 3; x++)
            {
                skindict.Add("holdBurst" + x, Root + "holdBurst" + x + ".png");
            }
            for (int x = 0; x < 8; x++)
            {
                skindict.Add("press" + (x + 1), Root + "press" + (x + 1) + ".png");
                skindict.Add("key" + (x + 1), Root + "key" + (x + 1) + ".png");
                keyColors[x] = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                lightColors[x] = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
                startColors[x] = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                endColors[x] = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                burstColours[x] = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
                laneWidth[x] = 40;
                if (x % 2 == 0)
                {
                    barColors[x] = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
                }
                else
                {
                    barColors[x] = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
                }
            }
            for (int x = 0; x < 5; x++)
            {
                accColors[x] = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            }
            DivColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            PulseColour = new Color4(0.0f, 0.0f, 1.0f, 0.5f);
            skindict.Add("mediaFB", Root + "mediaFB.png");
            skindict.Add("mediaFF", Root + "mediaFF.png");
            skindict.Add("mediaPY", Root + "mediaPY.png");
            skindict.Add("mediaPS", Root + "mediaPS.png");
            skindict.Add("mediaSP", Root + "mediaSP.png");
            skindict.Add("websiteText", Root + "linkToPulse.png");
            skindict.Add("frameLeft", Root + "frameLeft.png");
            skindict.Add("good", Root + "good.png");
            skindict.Add("great", Root + "great.png");
            skindict.Add("hitBar", Root + "hitBar.png");
            skindict.Add("holdBar", Root + "holdBar.png");
            skindict.Add("keyLight", Root + "keyLight.png");
            skindict.Add("menuButton", Root + "menuButton.png");
            skindict.Add("buttonMid", Root + "buttonMid.png");
            skindict.Add("buttonLeft", Root + "buttonLeft.png");
            skindict.Add("buttonRight", Root + "buttonRight.png");
            skindict.Add("miss", Root + "miss.png");
            skindict.Add("normal-hitclap", Root + "normal-hitclap.wav");
            skindict.Add("normal-hitfinish", Root + "normal-hitfinish.wav");
            skindict.Add("normal-hitnormal", Root + "normal-hitnormal.wav");
            skindict.Add("normal-hitwhistle", Root + "normal-hitwhistle.wav");
            skindict.Add("ok", Root + "ok.png");
            skindict.Add("perfect", Root + "perfect.png");
            skindict.Add("scorebg", Root + "scorebg.png");
            skindict.Add("toast", Root + "toast.png");
            skindict.Add("scroll", Root + "scroll.png");
            skindict.Add("bar", Root + "bar.png");
            skindict.Add("combobreak", Root + "combobreak.wav");
            skindict.Add("scoreback", Root + "scorel.png");
            skindict.Add("scoreentry", Root + "scoreentry.png");
            skindict.Add("volumecontrol", Root + "volumecontrol.png");
            skindict.Add("burst", Root + "burst.png");
            skindict.Add("hbar", Root + "hbar.png");
            skindict.Add("selecttexture", Root + "songselect.png");
            skindict.Add("frameSide", Root + "frameSide.png");
            skindict.Add("frameBack", Root + "frameBack.png");
            skindict.Add("frameDivide", Root + "frameDivide.png");
            skindict.Add("holdEnd", Root + "holdEnd.png");
            skindict.Add("scoreFrame", Root + "scoreFrame.png");
            skindict.Add("scoreBottom", Root + "scoreBottom.png");
            skindict.Add("tlContainer", Root + "tlContainer.png");
            skindict.Add("note", Root + "note0.png");
            skindict.Add("note0", Root + "note0.png");
            skindict.Add("note1", Root + "note1.png");
            skindict.Add("note2", Root + "note2.png");
            skindict.Add("holdStart", Root + "holdStart.png");
            skindict.Add("defaultbg", "skin\\defaultbg.png");
            skindict.Add("%", Root + "percent.png");
            skindict.Add(".", Root + "dot.png");
            skindict.Add(",", Root + "dot.png");
            skindict.Add("songdisplay", Root + "SongDisplay.png");
            for (int x = 0; x < 10; x++)
            {
                skindict.Add("" + x, Root + "" + x + ".png");
            }
            for (int x = 0; x < 11; x++)
            {
                skindict.Add("burst" + x, Root + "burst" + x + ".png");
            }
            for (int x = 0; x < 3; x++)
            {
                skindict.Add("notea" + x, Root + "notea" + x + ".png");
                skindict.Add("noteb" + x, Root + "noteb" + x + ".png");
                skindict.Add("notec" + x, Root + "notec" + x + ".png");
            } 
            skindict.Add("x", Root + "x.png");
            skindict.Add("tabBG", Root + "tabBG.png");
        }
        private static bool luaInitd = false;
        public static void luaInit()
        {
            if (Game.lua != null)
            {
                if (!luaInitd)
                {
                    luaInitd = true;
                    add("location");
                    add("color4");
                    add("color3");
                    add("color3b");
                    add("color4b");
                    add("size");
                    add("setFrameLocation");
                    add("getFrameLocation");
                    add("setAccuracyLocation");
                    add("getAccuracyLocation");
                    add("setScoreLocation");
                    add("getScoreLocation");
                    add("setPulseColor");
                    add("getPulseColor");
                    add("setBurstColor");
                    add("getBurstColor");
                    add("setLightColor");
                    add("getLightColor");
                    add("setKeyColor");
                    add("getKeyColor");
                    add("setBottomSpace");
                    add("getBottomSpace");
                    add("getBarColor");
                    add("setBarColor");
                    add("setTextOverlap");
                    add("getTextOverlap");
                    add("getAccuracyColor");
                    add("setAccuracyColor");
                    add("setClearFrame");
                    add("getClearFrame");
                    add("setLaneWidth");
                    add("getLaneWidth");
                    add("setEndColor");
                    add("getEndColor");
                    add("setStartColor");
                    add("getStartColor");
                    add("setColorHolds");
                    add("getColorHolds");
                    add("setDivColor");
                    add("getDivColor");
                    add("setMaskHolds");
                    add("getMaskHolds");
                    add("setMaskBursts");
                    add("getMaskBursts");
                    add("getHoldFramerate");
                    add("setHoldFramerate");
                    add("setBurstFramerate");
                    add("getBurstFramerate");
                    add("getHoldFramecount");
                    add("getNoteFramecount");
                    add("getNoteFramerate");
                    add("setNoteFramerate");
                    add("setNoteFramecount");
                    add("setBurstFramecount");
                    add("getBurstFramecount");
                    add("setHoldFramecount");
                    add("setScoreSize");
                    add("getScoreSize");
                    add("setAccuracySize");
                    add("getAccuracySize");
                }
                defaultskin(Config.SkinFolder);
                if (File.Exists(Root + "config.lua"))
                {
                    Game.lua.DoFile(Root + "config.lua");
                }
                noteStyle = 1;
                int total = 0;
                for (int x = 0; x < 8; x++)
                {
                    laneLoc[x] = total;
                    total += laneWidth[x];
                }
                playframe = new Frame(Skin.FrameLoc, false, false);
                editframe = new Frame(frameLoc, Config.EditMiddle, true);
                chatSound = Audio.AudioManager.loadFromFile(Root + "chatsound.mp3", true);
            }
        }
        private static Audio.AudioFX chatSound;

        public static Audio.AudioFX ChatSound
        {
            get { return Skin.chatSound; }
            set { Skin.chatSound = value; }
        }
        private static Frame playframe;
        public static Frame PlayFrame
        {
            set { playframe = value; }
            get { return Skin.playframe; }
        }
        private static Frame editframe;
        internal static Frame Editframe
        {
            get { return Skin.editframe; }
        }
        private static void add(string name)
        {
            Game.lua.RegisterFunction(name, typeof(Skin), typeof(Skin).GetMethod(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
        }
        public static Point location(int x, int y)
        {
            return new Point(x, y);
        }
        public static Color4 color4(float r, float g, float b, float a)
        {
            return new Color4(r, g, b, a);
        }
        public static Color4 color3(float r, float g, float b)
        {
            return new Color4(r, g, b, 1.0f);
        }
        public static Color4 color3b(byte r, byte g, byte b)
        {
            return new Color4(r, g, b, 255);
        }
        public static Color4 color4b(byte r, byte g, byte b, byte a)
        {
            return new Color4(r, g, b, a);
        }
        public static Size size(int w, int h)
        {
            return new Size(w, h);
        }
        public static void setScoreSize(Size s)
        {
            scoreSize = s;
        }
        public static Size getScoreSize()
        {
            return scoreSize;
        }
        public static void setAccuracySize(Size s)
        {
            accSize = s;
        }
        public static Size getAccuracySize()
        {
            return accSize;
        }
        public static int getHoldFramecount()
        {
            return holdFrameCount;
        }
        public static void setHoldFramecount(int count)
        {
            holdFrameCount = count;
        }
        public static int getBurstFramecount()
        {
            return burstFrameCount;
        }
        public static void setBurstFramecount(int count)
        {
            burstFrameCount = count;
        }
        public static int getNoteFramecount()
        {
            return noteFrameCount;
        }
        public static void setNoteFramecount(int count)
        {
            noteFrameCount = count;
        }
        public static int getNoteFramerate()
        {
            return noteFrameRate;
        }
        public static void setNoteFramerate(int rate)
        {
            noteFrameRate = rate;
        }
        public static int getBurstFramerate()
        {
            return burstFrameRate;
        }
        public static void setBurstFramerate(int rate)
        {
            burstFrameRate = rate;
        }
        public static int getHoldFramerate()
        {
            return holdFrameRate;
        }
        public static void setHoldFramerate(int rate)
        {
            holdFrameRate = rate;
        }
        public static bool getMaskHolds()
        {
            return maskHolds;
        }
        public static void setMaskHolds(bool b)
        {
            maskHolds = b;
        }
        public static bool getMaskBursts()
        {
            return maskBursts;
        }
        public static void setMaskBursts(bool b)
        {
            maskBursts = b;
        }
        public static void setDivColor(Color4 c)
        {
            divColor = c;
        }
        public static Color4 getDivColor()
        {
            return divColor;
        }
        public static void setColorHolds(bool hold)
        {
            colorHolds = hold;
        }
        public static bool getColorHolds()
        {
            return colorHolds;
        }
        public static int getLaneWidth(int index)
        {
            return laneWidth[index];
        }
        public static void setLaneWidth(int width, int index)
        {
            laneWidth[index] = width;
        }
        public static bool getClearFrame()
        {
            return clearFrame;
        }
        public static void setClearFrame(bool frame)
        {
            clearFrame = frame;
        }
        public static bool getBottomSpace()
        {
            return bottomSpace;
        }
        public static void setTextOverlap(int t)
        {
            textOverlap = t;
        }
        public static int getTextOverlap()
        {
            return textOverlap;
        }
        public static void setBottomSpace(bool space)
        {
            bottomSpace = space;
        }
        public static void setFrameLocation(Point loc)
        {
            frameLoc = loc;
        }
        public static Point getFrameLocation()
        {
            return frameLoc;
        }
        public static Point getAccuracyLocation()
        {
            return accLocation;
        }
        public static void setAccuracyLocation(Point loc)
        {
            accLocation = loc;
        }
        public static void setScoreLocation(Point loc)
        {
            scoreLocation = loc;
        }
        public static Point getScoreLocation()
        {
            return scoreLocation;
        }
        public static void setPulseColor(Color4 col)
        {
            pulseColor = col;
        }
        public static Color4 getPulseColor()
        {
            return pulseColor;
        }
        public static void setBurstColor(Color4 col, int index)
        {
            burstColours[index] = col;
        }
        public static Color4 getBurstColor(int index)
        {
            return burstColours[index];
        }
        public static void setLightColor(Color4 col, int index)
        {
            lightColors[index] = col;
        }
        public static Color4 getLightColor(int index)
        {
            return lightColors[index];
        }
        public static void setKeyColor(Color4 col, int index)
        {
            keyColors[index] = col;
        }
        public static Color4 getKeyColor(int index)
        {
            return keyColors[index];
        }
        public static void setBarColor(Color4 col, int index)
        {
            barColors[index] = col;
        }
        public static Color4 getBarColor(int index)
        {
            return barColors[index];
        }
        public static void setAccuracyColor(Color4 col, int index)
        {
            accColors[index] = col;
        }
        public static Color4 getAccuracyColor(int index)
        {
            return accColors[index];
        }
        public static void setStartColor(Color4 col, int index)
        {
            startColors[index] = col;
        }
        public static Color4 getStartColor(int index)
        {
            return startColors[index];
        }
        public static void setEndColor(Color4 col, int index)
        {
            endColors[index] = col;
        }
        public static Color4 getEndColor(int index)
        {
            return endColors[index];
        }
    }
}
