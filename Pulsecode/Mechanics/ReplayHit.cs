using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Mechanics
{
    public class ReplayHit
    {
        Replay.HitType hit;

        public Replay.HitType Hit
        {
            get { return hit; }
            set { hit = value; }
        }
        int noteOffset, offsetDifference, lane;

        public int Lane
        {
            get { return lane; }
            set { lane = value; }
        }

        public int OffsetDifference
        {
            get { return offsetDifference; }
            set { offsetDifference = value; }
        }

        public int NoteOffset
        {
            get { return noteOffset; }
            set { noteOffset = value; }
        }
        bool used;

        public bool Used
        {
            get { return used; }
            set { used = value; }
        }
        public ReplayHit(int n, int o, int l, Replay.HitType hit)
        {
            this.hit = hit;
            noteOffset = n;
            offsetDifference = o;
            lane = l;
            used = false;
        }
    }
}
