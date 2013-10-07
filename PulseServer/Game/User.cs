using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using PulseServer.Handlers;

namespace PulseServer.Game
{
    class User
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
        double speed;

        public double Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        double scroll;

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
        ClientHandler handler;

        internal ClientHandler Handler
        {
            get { return handler; }
            set { handler = value; }
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
        float accuracy;

        public float Accuracy
        {
            get { return accuracy; }
            set { accuracy = value; }
        }
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
        PlayMode mode;

        public PlayMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }
    }
}
