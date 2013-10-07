using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.UI;

namespace Pulse.Client
{
    public enum PlayMode : int
    {
        AFK = 1,
        IDLE = 2,
        INGAME = 3,
        SPEC = 4,
        REPLAY = 5,
        LOBBY = 6,
        MULTI = 7,
        EDITING = 8,
        TESTING = 9
    }
    public class User
    {
        bool updateGraphics = false;
        public bool UpdateGraphics
        {
            get { return updateGraphics; }
            set { updateGraphics = value; }
        }
        private UserDisplay disp;

        public UserDisplay Disp
        {
            get { return disp; }
            set { disp = value; }
        }
        double speed;
        public static Dictionary<string, User> users = new Dictionary<string, User>();
        public double Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        double scroll;
        public bool downloadAvatar = true;
        public Rect avatarRect;
        public double Scroll
        {
            get { return scroll; }
            set { scroll = value; }
        }
        int modFlags;

        public int ModFlags
        {
            get { return modFlags; }
            set { modFlags = value; }
        }
        string currentChart = "";

        public string CurrentChart
        {
            get { return currentChart; }
            set { currentChart = value; }
        }
        int level;

        public int Level
        {
            get { return level; }
            set { level = value; }
        }
        int accountType;
        float accuracy;

        public float Accuracy
        {
            get { return accuracy; }
            set { accuracy = value; }
        }
        public int AccountType
        {
            get { return accountType; }
            set { accountType = value; }
        }
        int playcount;

        public int Playcount
        {
            get { return playcount; }
            set { playcount = value; }
        }
        int totalScore;

        public int TotalScore
        {
            get { return totalScore; }
            set { totalScore = value; }
        }
        string name = "";

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        string avatar;
        string realName;

        public string RealName
        {
            get { return realName; }
            set { realName = value; }
        }
        public string Avatar
        {
            get { return avatar; }
            set { avatar = value; }
        }
        string currentSong = "";

        public string CurrentSong
        {
            get { return currentSong; }
            set { currentSong = value; }
        }
        PlayMode mode = PlayMode.IDLE;

        public PlayMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }
    }
}
