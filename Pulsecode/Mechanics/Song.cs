using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
namespace Pulse.Mechanics
{
    public class Song
    {
        private int id = -1;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        private int fileVersion = 0;

        public int FileVersion
        {
            get { return fileVersion; }
            set { fileVersion = value; }
        }
        private Dictionary<int, Chart> charts = new Dictionary<int, Chart>();
        private string creator = "N/A";

        public string Creator
        {
            get { return creator; }
            set { creator = value; }
        }

        public Dictionary<int, Chart> Charts
        {
            get { return charts; }
            set { charts = value; }
        }
        private List<TimingSection> timingsList = new List<TimingSection>();

        public List<TimingSection> TimingsList
        {
            get { return timingsList; }
            set { timingsList = value; }
        }

        private SortedDictionary<int, TimingSection> timings = new SortedDictionary<int, TimingSection>();

        public SortedDictionary<int, TimingSection> Timings
        {
            get { return timings; }
            set { timings = value; }
        }
        private string fileName, artist, songName;
        /// <summary>
        /// song mp3 name
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private string dir;

        public string Dir
        {
            get { return dir; }
            set { dir = value; }
        }
        private string pncName;

        public string PncName
        {
            get { return pncName; }
            set { pncName = value; }
        }
        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        public string SongName
        {
            get { return songName; }
            set { songName = value; }
        }
        private List<double> bookmarks = new List<double>();

        public List<double> Bookmarks
        {
            get { return bookmarks; }
            set { bookmarks = value; }
        }
        private int preview;

        public int Preview
        {
            get { return preview; }
            set { preview = value; }
        }
        public String PncNameNoExt
        {
            get;
            set;
        }
        private string bgName;

        public string BgName
        {
            get { return bgName; }
            set { bgName = value; }
        }
        public static int noteTexture = 0;
        public static int holdTexture = 0;
        public Song()
        {
        }
        public Song(string path)
        {
            addChart(path);
        }

        public float getPulsePercent(int offset)
        {
            TimingSection section = null;
            if (timingsList.Count > 0)
            {
                section = timingsList[0];
                foreach (TimingSection s in timingsList)
                {
                    if (offset >= s.Offset && s.ChangeSnap)
                    {
                        section = s;
                    }
                }
                double snap = section.Snap;
                double perc;
                if (offset < section.Offset)
                {
                    perc = (offset + section.Offset) % snap;
                }
                else
                {
                    perc = (offset - section.Offset) % snap;
                }
                if (perc <= perc / 10)
                {
                    return (float)Math.Abs(((perc / (snap / 10)) * 100));
                }
                else
                {
                    return (float)Math.Abs((((perc / 10) / (snap)) * 100));
                }
            }
            else
            {
                return 0;
            }
        }
        public void addChart(string path)
        {
            //Console.WriteLine("Parsing " + path);
            string totalPath = "songs\\" + path;
            string[] temp = path.Split('\\');
            dir = temp[0];
            pncName = temp[0];
            PncNameNoExt = Path.GetFileNameWithoutExtension(path);
            if (path.Contains(".pnc"))
            {
                StreamReader tempr = new StreamReader("songs\\" + path);
                string templine = tempr.ReadLine();
                if (templine.Length > 3 && templine.Substring(0, 4).Equals("pncv"))
                {
                    fileVersion = Convert.ToInt32(templine.Substring(4));
                }
                tempr.Close();
            }
            else
            {
                fileVersion = 1;
            }
            if (fileVersion == 0)
            {
                using (StreamReader sr = new StreamReader("songs\\" + path))
                {
                    bool timing = false, difficulty = false, song = false, editor = false;
                    int diff = 0;
                    string line;
                    while ((line = sr.ReadLine()) != null && !line.Equals(""))
                    {
                        try
                        {
                            if (line.Substring(0, 1).Equals("["))
                            {
                                timing = false;
                                difficulty = false;
                                song = false;
                                if (line.Equals("[timing]"))
                                {
                                    timing = true;
                                }
                                else if (line.Equals("[song]"))
                                {
                                    song = true;
                                }
                                else if (line.Equals("[editor]"))
                                {
                                    editor = true;
                                }
                                else
                                {
                                    diff = charts.Count;
                                    charts.Add(diff, new Chart(diff, line.Substring(1, line.Length - 2)));
                                    difficulty = true;
                                }
                            }
                            else if (timing)
                            {
                                string[] split = line.Split(',');
                                if (split.Length == 4)
                                {
                                    try
                                    {
                                        if (Convert.ToDouble(split[1], Config.cultureEnglish) <= 0 && Convert.ToBoolean(split[2]))
                                        {
                                            split[2] = "False";
                                        }
                                        if (!timings.ContainsKey(Convert.ToInt32(split[0])))
                                        {
                                            timings.Add(Convert.ToInt32(split[0]), new TimingSection(Convert.ToDouble(split[0], Config.cultureEnglish), Convert.ToDouble(split[1], Config.cultureEnglish), Convert.ToBoolean(split[2]), Convert.ToInt32(split[3])));
                                        }
                                    }
                                    catch { }
                                }
                                else
                                {
                                    timings.Add(Convert.ToInt32(split[0]), new TimingSection(Convert.ToDouble(split[0], Config.cultureEnglish), Convert.ToDouble(split[1], Config.cultureEnglish), Convert.ToBoolean(split[2])));
                                }
                                bool found = false;
                                foreach (TimingSection s in timingsList)
                                {
                                    if (s.Offset == Convert.ToDouble(split[0], Config.cultureEnglish) && s.Snap == Convert.ToDouble(split[1], Config.cultureEnglish))
                                    {
                                        found = false;
                                    }
                                }
                                if (!found)
                                {
                                    timingsList.Add(new TimingSection(Convert.ToDouble(split[0], Config.cultureEnglish), Convert.ToDouble(split[1], Config.cultureEnglish), Convert.ToBoolean(split[2])));
                                }
                            }
                            else if (difficulty)
                            {
                                string[] split = line.Split(',');
                                try
                                {
                                    string temp2 = "a";
                                    int tempInt = 0;
                                    if (split[0].Equals("n"))
                                    {

                                        if (Skin.NoteStyle == 1)
                                        {
                                            if (Convert.ToInt32(split[2]) % 2 == 0)
                                            {
                                                tempInt++;
                                            }
                                        }
                                        else if (Skin.NoteStyle == 2)
                                        {
                                            if (Convert.ToInt32(split[2]) % 2 == 0)
                                            {
                                                tempInt++;
                                            }
                                            if (Convert.ToInt32(split[2]) == 4)
                                            {
                                                tempInt++;
                                            }
                                        }
                                        switch (tempInt)
                                        {
                                            case 0:
                                                temp2 = "a";
                                                break;
                                            case 1:
                                                temp2 = "b";
                                                break;
                                            case 2:
                                                temp2 = "c";
                                                break;
                                        }
                                        if (split.Length == 3)
                                        {
                                            charts[diff].Notes.Add(new Note(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), temp2));
                                        }
                                        else
                                        {
                                            charts[diff].Notes.Add(new Note(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), temp2, Convert.ToBoolean(split[3]), Convert.ToInt32(split[4]), Skin.skindict["holdBar"]));
                                        }
                                    }
                                    else if (split[0].Equals("t"))
                                    {
                                        charts[diff].Sections.Add(new TimingSection(Convert.ToDouble(split[1], Config.cultureEnglish), 0, false, Convert.ToInt32(split[2])));
                                    }
                                    else
                                    {
                                        if (split.Length == 2)
                                        {
                                            charts[diff].Notes.Add(new Note(Convert.ToInt32(split[0]), Convert.ToInt32(split[1]), temp2));
                                        }
                                        else
                                        {
                                            charts[diff].Notes.Add(new Note(Convert.ToInt32(split[0]), Convert.ToInt32(split[1]), temp2, Convert.ToBoolean(split[2]), Convert.ToInt32(split[3]), Skin.skindict["holdBar"]));
                                        }
                                    }
                                }
                                catch { }
                            }
                            else if (song)
                            {
                                string[] split = line.Split('=');
                                switch (split[0].ToLower(Config.cultureEnglish))
                                {
                                    case "filename":
                                        fileName = split[1];
                                        break;
                                    case "songname":
                                        songName = split[1];
                                        break;
                                    case "artist":
                                        artist = split[1];
                                        break;
                                    case "bg":
                                        bgName = split[1];
                                        break;
                                    case "preview":
                                        preview = Convert.ToInt32(split[1]);
                                        break;
                                    case "creator":
                                        creator = split[1];
                                        break;
                                    default:
                                        Console.WriteLine("invalid command in song parsing: " + split[0]);
                                        break;
                                }
                            }
                            else if (editor)
                            {
                                string[] split = line.Split(',');
                                if (split[0].ToLower(Config.cultureEnglish).Equals("bm"))
                                {
                                    bookmarks.Add(Convert.ToDouble(split[1], Config.cultureEnglish));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorLog.log(e);
                        }
                    }
                }
            }
            else if (fileVersion == 1)
            {
                path = path.Split('\\')[0];
                DirectoryInfo directory = new DirectoryInfo("songs\\" + path);
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo f in files)
                {
                    if (f.Extension.Equals(".pnc"))
                    {
                        using (StreamReader sr = new StreamReader("songs\\" + path + "\\" + f.Name))
                        {
                            bool timing = false, difficulty = false, song = false, editor = false;
                            int diff = 0;
                            string line;
                            diff = charts.Count;
                            charts.Add(diff, new Chart(diff, "tempname"));
                            charts[diff].Path = "songs\\" + path + "\\" + f.Name;
                            while ((line = sr.ReadLine()) != null && !line.Equals(""))
                            {
                                try
                                {
                                    if (line.Substring(0, 1).Equals("["))
                                    {
                                        timing = false;
                                        difficulty = false;
                                        song = false;
                                        if (line.Equals("[timing]"))
                                        {
                                            timing = true;
                                        }
                                        else if (line.Equals("[song]"))
                                        {
                                            song = true;
                                        }
                                        else if (line.Equals("[editor]"))
                                        {
                                            editor = true;
                                        }
                                        else if (line.Equals("[objects]"))
                                        {
                                            difficulty = true;
                                        }
                                    }
                                    else if (timing)
                                    {
                                        string[] split = line.Split(',');
                                        if (split.Length == 4)
                                        {
                                            try
                                            {
                                                if (Convert.ToDouble(split[1], Config.cultureEnglish) <= 0 && Convert.ToBoolean(split[2]))
                                                {
                                                    split[2] = "False";
                                                }
                                                if (!timings.ContainsKey(Convert.ToInt32(split[0])))
                                                {
                                                    timings.Add(Convert.ToInt32(split[0]), new TimingSection(Convert.ToDouble(split[0], Config.cultureEnglish), Convert.ToDouble(split[1], Config.cultureEnglish), Convert.ToBoolean(split[2]), Convert.ToInt32(split[3])));
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            timings.Add(Convert.ToInt32(split[0]), new TimingSection(Convert.ToDouble(split[0], Config.cultureEnglish), Convert.ToDouble(split[1], Config.cultureEnglish), Convert.ToBoolean(split[2])));
                                        } bool found = false;
                                        foreach (TimingSection s in timingsList)
                                        {
                                            if (s.Offset == Convert.ToDouble(split[0], Config.cultureEnglish) && s.Snap == Convert.ToDouble(split[1], Config.cultureEnglish))
                                            {
                                                found = true;
                                            }
                                        }
                                        if (!found)
                                        {
                                            timingsList.Add(new TimingSection(Convert.ToDouble(split[0], Config.cultureEnglish), Convert.ToDouble(split[1], Config.cultureEnglish), Convert.ToBoolean(split[2])));
                                        }
                                    }
                                    else if (difficulty)
                                    {
                                        string[] split = line.Split(',');
                                        try
                                        {
                                            string temp2 = "a";
                                            int tempInt = 0;
                                            if (Skin.NoteStyle == 1)
                                            {
                                                if (Convert.ToInt32(split[2]) % 2 == 0)
                                                {
                                                    tempInt++;
                                                }
                                            }
                                            else if (Skin.NoteStyle == 2)
                                            {
                                                if (Convert.ToInt32(split[2]) % 2 == 0)
                                                {
                                                    tempInt++;
                                                }
                                                if (Convert.ToInt32(split[2]) == 4)
                                                {
                                                    tempInt++;
                                                }
                                            }
                                            switch (tempInt)
                                            {
                                                case 0:
                                                    temp2 = "a";
                                                    break;
                                                case 1:
                                                    temp2 = "b";
                                                    break;
                                                case 2:
                                                    temp2 = "c";
                                                    break;
                                            }
                                            if (split[0].Equals("n"))
                                            {

                                                if (split.Length == 3)
                                                {
                                                    charts[diff].Notes.Add(new Note(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), "" + temp2));
                                                }
                                                else
                                                {
                                                    charts[diff].Notes.Add(new Note(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), "" + temp2, Convert.ToBoolean(split[3]), Convert.ToInt32(split[4]), Skin.skindict["holdBar"]));
                                                }
                                            }
                                            else if (split[0].Equals("t"))
                                            {
                                                charts[diff].Sections.Add(new TimingSection(Convert.ToDouble(split[1], Config.cultureEnglish), 0, false, Convert.ToInt32(split[2])));
                                            }
                                            else
                                            {
                                                if (split.Length == 2)
                                                {
                                                    charts[diff].Notes.Add(new Note(Convert.ToInt32(split[0]), Convert.ToInt32(split[1]), "" + temp2));
                                                }
                                                else
                                                {
                                                    charts[diff].Notes.Add(new Note(Convert.ToInt32(split[0]), Convert.ToInt32(split[1]), "" + temp2, Convert.ToBoolean(split[2]), Convert.ToInt32(split[3]), Skin.skindict["holdBar"]));
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    else if (song)
                                    {
                                        string[] split = line.Split('=');
                                        switch (split[0].ToLower(Config.cultureEnglish))
                                        {
                                            case "keys":
                                                charts[diff].Keys = Convert.ToInt32(split[1]);
                                                if (charts[diff].Keys > 8)
                                                {
                                                    charts[diff].Keys = 8;
                                                }
                                                else if (charts[diff].Keys < 5)
                                                {
                                                    charts[diff].Keys = 5;
                                                }
                                                break;
                                            case "judge":
                                                charts[diff].Judgement = Convert.ToInt32(split[1]);
                                                if (charts[diff].Judgement > 10)
                                                    charts[diff].Judgement = 10;
                                                else if (charts[diff].Judgement < 1)
                                                    charts[diff].Judgement = 1;
                                                break;
                                            case "tags":
                                                string[] sp = split[1].Split(',');
                                                for (int x = 0; x < sp.Length; x++)
                                                {
                                                    charts[diff].Tags.Add(sp[x]);
                                                }
                                                break;
                                            case "filename":
                                                fileName = split[1];
                                                break;
                                            case "difficulty":
                                                charts[diff].Name = split[1];
                                                break;
                                            case "songname":
                                                songName = split[1];
                                                break;
                                            case "artist":
                                                artist = split[1];
                                                break;
                                            case "bg":
                                                charts[diff].BgName = split[1];
                                                break;
                                            case "leadin":
                                                charts[diff].LeadInTime = Math.Floor(Convert.ToDouble(split[1], Config.cultureEnglish) / 1000);
                                                break;
                                            case "preview":
                                                preview = Convert.ToInt32(split[1]);
                                                break;
                                            case "creator":
                                                creator = split[1];
                                                break;
                                            default:
                                                Console.WriteLine("invalid command in song parsing: " + split[0]);
                                                break;
                                        }
                                    }
                                    else if (editor)
                                    {
                                        string[] split = line.Split(',');
                                        if (split[0].ToLower(Config.cultureEnglish).Equals("bm"))
                                        {
                                            bookmarks.Add(Convert.ToDouble(split[1], Config.cultureEnglish));
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    ErrorLog.log(e);
                                }
                            }
                            if (charts[diff].Keys == -1)
                            {
                                int keys = 5;
                                foreach (Note n in charts[diff].Notes)
                                {
                                    if (n.Location > keys)
                                    {
                                        keys = n.Location;
                                    }
                                }
                                charts[diff].Keys = keys;
                            }
                        }
                    }
                }
            }
        }
        public void addChart(int difficulty, string name)
        {
            charts.Add(difficulty, new Chart(difficulty, name));
        }
        public void reIDCharts()
        {
            Dictionary<int, Chart> temp = new Dictionary<int, Chart>();
            List<Chart> tempList = new List<Chart>();
            foreach (var pair in charts)
            {
                tempList.Add(pair.Value);
            }
            for (int x = 0; x < charts.Count; x++)
            {
                temp.Add(x, tempList[x]);
            }
            charts = temp;
        }
        public void remakeLists(Chart c)
        {
            timingsList.Clear();
            foreach (var pair in timings)
            {
                timingsList.Add(pair.Value);
            }
            foreach (TimingSection s in c.Sections)
            {
                timingsList.Add(s);
            }
            timingsList.Sort(delegate(TimingSection a, TimingSection b)
            {
                return a.Offset.CompareTo(b.Offset);
            });
        }
    }
}
