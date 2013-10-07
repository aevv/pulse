using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pulse.Mechanics
{
    public class Chart
    {
        private int judgement = 5;

        public int Judgement
        {
            get { return judgement; }
            set { judgement = value; }
        }
        private List<String> tags = new List<String>();

        public List<String> Tags
        {
            get { return tags; }
            set { tags = value; }
        }
        private int keys = -1;

        public int Keys
        {
            get { return keys; }
            set { keys = value; }
        }
        private double leadInTime = 2;

        public double LeadInTime
        {
            get { return leadInTime; }
            set { leadInTime = value; }
        }
        private string bgName;

        public string BgName
        {
            get { return bgName; }
            set { bgName = value; }
        }
        private int difficulty;

        public int Difficulty
        {
            get { return difficulty; }
            set { difficulty = value; }
        }
        private List<Note> notes = new List<Note>();

        public List<Note> Notes
        {
            get { return notes; }
            set { notes = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private List<TimingSection> sections = new List<TimingSection>();

        public List<TimingSection> Sections
        {
            get { return sections; }
            set { sections = value; }
        }
        string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        public Chart(int diff, string name)
        {
            difficulty = diff;
            this.name = name;
        }
    }
}
