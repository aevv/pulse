using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Mechanics
{
    public class Replay
    {
        private Mods mods = new Mods();

        internal Mods Mods
        {
            get { return mods; }
            set { mods = value; }
        }
        private bool dt = false;

        public bool Dt
        {
            get { return dt; }
            set { dt = value; }
        }
        private bool ht = false;

        public bool Ht
        {
            get { return ht; }
            set { ht = value; }
        }
        private bool nf = false;

        public bool Nf
        {
            get { return nf; }
            set { nf = value; }
        }
        private bool auto = false;

        public bool Auto
        {
            get { return auto; }
            set { auto = value; }
        }
        private bool hd = false;

        public bool Hd
        {
            get { return hd; }
            set { hd = value; }
        }
        private bool mr = false;

        public bool Mr
        {
            get { return mr; }
            set { mr = value; }
        }
        public enum HitType
        {
            HOLDMISS = 0, MISS = 1, OK = 2, GOOD = 3, GREAT = 4, PERFECT = 5
        }
        List<ReplayHit> hitTimings = new List<ReplayHit>();

        public List<ReplayHit> HitTimings
        {
            get { return hitTimings; }
            set { hitTimings = value; }
        }

        List<Pair<int, Pair<int, bool>>> pressTimings = new List<Pair<int, Pair<int, bool>>>();

        public List<Pair<int, Pair<int, bool>>> PressTimings
        {
            get { return pressTimings; }
            set { pressTimings = value; }
        }
        List<Pair<int, Pair<int, bool>>> releaseTimings = new List<Pair<int, Pair<int, bool>>>();

        public List<Pair<int, Pair<int, bool>>> ReleaseTimings
        {
            get { return releaseTimings; }
            set { releaseTimings = value; }
        }
        public Replay()
        {

        }
    }
}
