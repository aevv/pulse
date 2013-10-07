using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Reflection;
using OpenTK.Input;
using System.Globalization;

namespace Pulse
{
    public class Config
    {
        private static double aspectRatio = 4.0 / 3.0;

        public static double AspectRatio
        {
            get { return Config.aspectRatio; }
            set { Config.aspectRatio = value; }
        }
        private static Key skipKey = Key.Space;

        public static Key SkipKey
        {
            get { return Config.skipKey; }
            set { Config.skipKey = value; }
        }
        private static Key restartKey = Key.R;

        public static Key RestartKey
        {
            get { return Config.restartKey; }
            set { Config.restartKey = value; }
        }
        private static readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long UnixTime()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalSeconds;
        }
        private static string specsOnMe = "";

        public static string SpecsOnMe
        {
            get { return Config.specsOnMe; }
            set { Config.specsOnMe = value; }
        }
        private static string specs = "";

        public static string Specs
        {
            get { return Config.specs; }
            set { Config.specs = value; }
        }
        public static Size oldRes;
        public static CultureInfo cultureEnglish = new CultureInfo("en-US");
        public static void groupKey(int group, string key, string res)
        {
            switch (key)
            {
                case "key1":
                    setKey(group, 1, res);
                    break;
                case "key2":
                    setKey(group, 2, res);
                    break;
                case "key3":
                    setKey(group, 3, res);
                    break;
                case "key4":
                    setKey(group, 4, res);
                    break;
                case "key5":
                    setKey(group, 5, res);
                    break;
                case "key6":
                    setKey(group, 6, res);
                    break;
                case "key7":
                    setKey(group, 7, res);
                    break;
                case "key8":
                    setKey(group, 8, res);
                    break;
            }
        }
        public static void load()
        {
            oldRes = new Size();
            oldRes.Width = OpenTK.DisplayDevice.Default.Width;
            oldRes.Height = OpenTK.DisplayDevice.Default.Height;
            keys = new Key[4][];
            for (int x = 0; x < 4; x++)
            {
                keys[x] = new Key[8];
            }
            setKey(3, 1, "S");
            setKey(3, 2, "D");
            setKey(3, 3, "F");
            setKey(3, 4, "G");
            setKey(3, 5, "H");
            setKey(3, 6, "J");
            setKey(3, 7, "K");
            setKey(3, 8, "L");
            for (int x = 0; x < 5; x++)
            {
                keys[0][x] = keys[3][x];
            }
            for (int x = 0; x < 6; x++)
            {
                keys[1][x] = keys[3][x];
            }
            for (int x = 0; x < 7; x++)
            {
                keys[2][x] = keys[3][x];
            }
            try
            {
                using (StreamReader sr = new StreamReader("pulseconfig.ini"))
                {
                    string line;
                    bool gotFirstRes = false;
                    double firstRes = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] split = line.Split('=');
                        try
                        {
                            bool reflect = true;
                            object temp = null;
                            switch (split[0].ToLower(Config.cultureEnglish))
                            {
                                case "clientwidth":
                                    reflect = false;
                                    double width = Convert.ToDouble(split[1]);
                                    Config.clientWidth = (int)width;
                                    if (gotFirstRes)
                                    {
                                        setAspect(width, firstRes);
                                    }
                                    else
                                    {
                                        gotFirstRes = true;
                                        firstRes = width;
                                    }
                                    break;
                                case "clientheight":
                                    reflect = false;
                                    double height = Convert.ToDouble(split[1]);
                                    Config.clientHeight = (int)height;
                                    if (gotFirstRes)
                                    {
                                        setAspect(firstRes, height);
                                    }
                                    else
                                    {
                                        gotFirstRes = true;
                                        firstRes = height;
                                    }
                                    break;
                                case "volume":
                                case "hitvolume":
                                case "fps":
                                case "offset":
                                    temp = Convert.ToInt32(split[1]);
                                    break;
                                case "vsync":
                                case "fullscreen":
                                case "displayfps":
                                case "disablemousewheel":
                                case "holdhitsounds":
                                case "confirmclose":
                                case "editmiddle":
                                case "waveform":
                                case "chatsounds":
                                    temp = Convert.ToBoolean(split[1]);
                                    break;
                                case "restartkey":
                                case "skipkey":
                                    temp = Enum.Parse(typeof(Key), split[1]);
                                    break;
                                case "skinfolder":
                                    temp = split[1];
                                    break;
                                case "group5":
                                    groupKey(0, split[1], split[2]);
                                    reflect = false;
                                    break;
                                case "group6":
                                    groupKey(1, split[1], split[2]);
                                    reflect = false;
                                    break;
                                case "group7":
                                    groupKey(2, split[1], split[2]);
                                    reflect = false;
                                    break;
                                case "group8":
                                    groupKey(3, split[1], split[2]);
                                    reflect = false;
                                    break;
                                default:
                                    temp = split[1];
                                    break;
                            }
                            if (reflect)
                            {
                                Type conf = typeof(Config);
                                PropertyInfo field = conf.GetProperty(split[0]);
                                field.SetValue(null, temp, null);
                            }
                            //Console.WriteLine("Config: " + split[0] + "=" + split[1]);
                        }
                        catch { Console.WriteLine("failed:" + line); }
                    }
                }
            }
            catch
            {
                Console.WriteLine("No config file found, creating one");                
                saveConfig();
            }
            if (wideScreen)
            {
                resWidth = 1280;
            }
            if (fullscreen)
            {
                OpenTK.DisplayDevice.Default.ChangeResolution(clientWidth, clientHeight, OpenTK.DisplayDevice.Default.BitsPerPixel, OpenTK.DisplayDevice.Default.RefreshRate);
            }
        }
        private static void setAspect(double width, double height)
        {
            aspectRatio = width / height;
            float t = (float)(768f * Config.AspectRatio);
            resWidth = (int)t;
        }
        public static void saveConfig()
        {
            using (StreamWriter sr = new StreamWriter("pulseconfig.ini", false))
            {
                sr.WriteLine("ClientWidth=" + clientWidth);
                sr.WriteLine("ClientHeight=" + clientHeight);
                sr.WriteLine("WideScreen=" + wideScreen);
                sr.WriteLine("Vsync=" + vsync);
                sr.WriteLine("DisplayFps=" + displayFps);
                sr.WriteLine("Volume=" + volume);
                sr.WriteLine("HitVolume=" + hitvol);
                sr.WriteLine("SkinFolder=" + skinfolder);
                sr.WriteLine("Fullscreen=" + fullscreen);
                sr.WriteLine("Offset=" + offset);
                sr.WriteLine("Fps=" + fps);
                sr.WriteLine("HoldHitsounds=" + holdHitsounds);
                sr.WriteLine("DisableMousewheel=" + disableMousewheel);
                sr.WriteLine("EditMiddle=" + editMiddle);
                sr.WriteLine("Waveform=" + waveform);
                sr.WriteLine("ConfirmClose=" + confirmClose);
                sr.WriteLine("ChatSounds=" + chatSounds);
                sr.WriteLine("RestartKey=" + restartKey);
                sr.WriteLine("SkipKey=" + skipKey);
                for (int x = 0; x < 5; x++)
                {
                    sr.WriteLine("group5=key" + (x + 1) + "=" + keys[0][x]);
                }
                for (int x = 0; x < 6; x++)
                {
                    sr.WriteLine("group6=key" + (x + 1) + "=" + keys[1][x]);
                }
                for (int x = 0; x < 7; x++)
                {
                    sr.WriteLine("group7=key" + (x + 1) + "=" + keys[2][x]);
                }
                for (int x = 0; x < 8; x++)
                {
                    sr.WriteLine("group8=key" + (x + 1) + "=" + keys[3][x]);
                }
            }
        }
        static string spectatedUser = "";

        public static string SpectatedUser
        {
            get { return Config.spectatedUser; }
            set { Config.spectatedUser = value; }
        }
        static bool spectating = false;

        public static bool Spectating
        {
            get { return Config.spectating; }
            set { Config.spectating = value; }
        }
        static bool spectated = false;

        public static bool Spectated
        {
            get { return Config.spectated; }
            set { Config.spectated = value; }
        }
        static bool localScores = true;

        public static bool LocalScores
        {
            get { return Config.localScores; }
            set { Config.localScores = value; }
        }
        private static bool editMiddle = false;

        public static bool EditMiddle
        {
            get { return Config.editMiddle; }
            set { Config.editMiddle = value; }
        }
        private static bool mirror = false;
        private static bool confirmClose = false;

        public static bool ConfirmClose
        {
            get { return Config.confirmClose; }
            set { Config.confirmClose = value; }
        }

        public static bool Mirror
        {
            get { return Config.mirror; }
            set { Config.mirror = value; }
        }
        private static bool ht = false;

        public static bool Ht
        {
            get { return Config.ht; }
            set { Config.ht = value; }
        }
        private static bool dt = false;

        public static bool Dt
        {
            get { return Config.dt; }
            set { Config.dt = value; }
        }
        private static bool hidden = false;

        public static bool Hidden
        {
            get { return Config.hidden; }
            set { Config.hidden = value; }
        }
        //currently WIP
        private static bool autoPlay = false;

        public static bool AutoPlay
        {
            get { return Config.autoPlay; }
            set { Config.autoPlay = value; }
        }
        private static void setKey(int group, int no, string key)
        {
            keys[group][no - 1] = (Key)Enum.Parse(typeof(Key), key);
        }
        private static bool disableMousewheel = false;

        public static bool DisableMousewheel
        {
            get { return Config.disableMousewheel; }
            set { Config.disableMousewheel = value; }
        }
        private static bool holdHitsounds = false;

        public static bool HoldHitsounds
        {
            get { return Config.holdHitsounds; }
            set { Config.holdHitsounds = value; }
        }
        private static bool noFail = false;
        public static bool NoFail
        {
            get { return Config.noFail; }
            set { Config.noFail = value; }
        }
        private static int offset = 0;

        public static int Offset
        {
            get { return Config.offset; }
            set { Config.offset = value; }
        }

        private static Size clientSize = new Size();

        public static Size ClientSize
        {
            get { return Config.clientSize; }
            set { Config.clientSize = value; }
        }
        private static int resWidth = 1024;

        public static int ResWidth
        {
            get { return Config.resWidth; }
            set { Config.resWidth = value; }
        }
        private static int clientWidth = 1024;

        public static int ClientWidth
        {
            get { return Config.clientWidth; }
            set { Config.clientWidth = value; clientSize.Width = value; }
        }

        private static int clientHeight = 768;

        public static int ClientHeight
        {
            get { return Config.clientHeight; }
            set { Config.clientHeight = value; clientSize.Height = value; }
        }

        private static int volume = 50;

        public static int Volume
        {
            get { return Config.volume; }
            set { Config.volume = value; Game.setVolume(value); }
        }
        private static int hitvol = 0;
        public static int HitVolume
        {
            get
            {
                return hitvol;
            }
            set
            {
                Config.hitvol = value;
            }
        }
        private static bool waveform = false;

        public static bool Waveform
        {
            get { return Config.waveform; }
            set { Config.waveform = value; }
        }
        private static String skinfolder = "default";
        public static String SkinFolder
        {
            get
            {
                return skinfolder;
            }
            set
            {
                skinfolder = value;
                Skin.luaInit();
            }
        }
        private static bool fullscreen = false;

        public static bool Fullscreen
        {
            get { return Config.fullscreen; }
            set
            {
                Config.fullscreen = value;
                Game.setFullscreen(value);
            }
        }

        private static int fps = 120;

        public static int Fps
        {
            get { return Config.fps; }
            set { Config.fps = value; }
        }

        private static bool vsync = false;

        public static bool Vsync
        {
            get { return Config.vsync; }
            set { Config.vsync = value; Game.setVSync(value); }
        }

        private static bool displayFps = false;

        public static bool DisplayFps
        {
            get { return Config.displayFps; }
            set { Config.displayFps = value; }
        }

        private static int boundTexture = -1;

        public static int BoundTexture
        {
            get { return Config.boundTexture; }
            set { Config.boundTexture = value; }
        }

        public static Key[][] keys;

        private static bool editing = false;

        public static bool Editing
        {
            get { return Config.editing; }
            set { Config.editing = value; }
        }
        private static double version = 0.3;
        public static string SongLibraryHash = "Y=UE8tL=%etb84hCHDGAZ)4JzHxrXhcPy696TBhDE5H)C-dY4-K/ULAkCwZXEQ$7";
        public static double Version
        {
            get { return Config.version; }
            set { Config.version = value; }
        }
        private static bool wideScreen = false;

        public static bool WideScreen
        {
            get { return Config.wideScreen; }
            set
            {
                Config.wideScreen = value;
                if (wideScreen)
                {
                    resWidth = 1280;
                }
                else
                {
                    resWidth = 1024;
                }
            }
        }
        private static float playSpeed = 1.0f;

        public static float PlaySpeed
        {
            get { return Config.playSpeed; }
            set { Config.playSpeed = value; }
        }
        private static bool chatSounds = true;

        public static bool ChatSounds
        {
            get { return Config.chatSounds; }
            set { Config.chatSounds = value; }
        }
    }
}
