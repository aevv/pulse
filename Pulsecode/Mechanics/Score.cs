using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Pulse.Mechanics
{
    public class Score
    {
        int unixTime = 0;

        public int UnixTime
        {
            get { return unixTime; }
            set { unixTime = value; }
        }
        string player = "";

        public string Player
        {
            get { return player; }
            set { player = value; }
        }
        string replayName;

        public string ReplayName
        {
            get { return replayName; }
            set { replayName = value; }
        }
        private int flags = 0;

        public int Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        private int maxCombo = 0;

        public int MaxCombo
        {
            get
            {
                return maxCombo;
            }
            set
            {
                maxCombo = value;
            }
        }

        private int totalScore;

        public int TotalScore
        {
            get
            {
                return totalScore;
            }
            set
            {
                totalScore = value;
            }
        }
        private int combo;

        public int Combo
        {
            get
            {
                return combo;
            }
            set
            {
                combo = value;
                if (value > maxCombo)
                    maxCombo = value;
            }
        }
        private float accuracy = 0.00f;

        public float Accuracy
        {
            get
            {
                float temp = 0;
                foreach (float i in accuracyTotal)
                {
                    temp += i;
                }
                float total = accuracyTotal.Count * 3;
                if (total == 0)
                {
                    return 0;
                }
                else
                {
                    accuracy = (temp / total) * 100;
                    return accuracy;
                }
            }
            set
            {
                accuracy = value;
            }
        }
        private int perfects;
        public int Perfects
        {
            get
            {
                perfects = 0;
                foreach (float f in accuracyTotal)
                {
                    if (f == 3f)
                    {
                        perfects++;
                    }
                }
                return perfects;
            }
            set
            {
                perfects = value; for (int z = accuracyTotal.Count - 1; z > -1; z--)
                {
                    if (accuracyTotal[z] == 3f)
                    {
                        accuracyTotal.RemoveAt(z);
                    }
                }
                for (int x = 0; x < value; x++)
                {
                    accuracyTotal.Add(3f);
                }
            }
        }
        private int goods;
        public int Goods
        {
            get
            {
                goods = 0;
                foreach (float f in accuracyTotal)
                {
                    if (f == 2f)
                    {
                        goods++;
                    }
                }
                return goods;
            }
            set
            {
                for (int z = accuracyTotal.Count - 1; z > -1; z--)
                {
                    if (accuracyTotal[z] == 2f)
                    {
                        accuracyTotal.RemoveAt(z);
                    }
                }
                goods = value; for (int x = 0; x < value; x++)
                {
                    accuracyTotal.Add(2f);
                }
            }
        }
        private int oks;
        public int Oks
        {
            get
            {
                oks = 0;
                foreach (float f in accuracyTotal)
                {
                    if (f == 1f)
                    {
                        oks++;
                    }
                }
                return oks;
            }
            set
            {
                for (int z = accuracyTotal.Count - 1; z > -1; z--)
                {
                    if (accuracyTotal[z] == 1f)
                    {
                        accuracyTotal.RemoveAt(z);
                    }
                }
                oks = value; for (int x = 0; x < value; x++)
                {
                    accuracyTotal.Add(1f);
                }
            }
        }
        private int misses;
        public int Misses
        {
            get
            {
                misses = 0;
                foreach (float f in accuracyTotal)
                {
                    if (f == 0.0f)
                    {
                        misses++;
                    }
                }
                return misses;
            }
            set
            {
                misses = value;
                for (int z = accuracyTotal.Count - 1; z > -1; z--)
                {
                    if (accuracyTotal[z] == 0f)
                    {
                        accuracyTotal.RemoveAt(z);
                    }
                }
                for (int x = 0; x < value; x++)
                {
                    accuracyTotal.Add(0f);
                }
            }
        }
        private List<float> accuracyTotal = new List<float>();
        public String chartName
        {
            get;
            set;
        }
        public String ArtistTitle
        {
            get;
            set;
        }
        public String dateString
        {
            get;
            set;
        }
        public List<float> AccuracyTotal
        {
            get
            {
                return accuracyTotal;
            }
            set
            {
                accuracyTotal = value;
            }
        }
        public Score()
        {

        }
    }
}
