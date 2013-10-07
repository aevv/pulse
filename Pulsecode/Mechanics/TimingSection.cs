using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Mechanics
{
    public class TimingSection
    {
        double offset;

        public double Offset
        {
            get { return offset; }
            set { offset = value; }
        }
        double snap;

        public double Snap
        {
            get { return snap; }
            set { snap = value; }
        }
        int moveTime;

        public int MoveTime
        {
            get { return moveTime; }
            set { moveTime = value; }
        }
        private bool changeSnap;

        public bool ChangeSnap
        {
            get { return changeSnap; }
            set { changeSnap = value; }
        }
        public TimingSection(double offset, double snap, bool changeSnap, int movetime)
        {
            this.changeSnap = changeSnap;
            this.offset = offset;
            this.snap = snap;
            this.moveTime = movetime;
        }
        public TimingSection(double offset, double snap, bool changeSnap)
            : this(offset, snap, changeSnap, 1200)
        {
        }
    }
}
