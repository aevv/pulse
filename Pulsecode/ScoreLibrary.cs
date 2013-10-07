using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Pulse.Mechanics;
namespace Pulse
{
    class ScoreLibrary
    {
        public static void serializeScores(List<Score> scores, string name)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, "yes");
            doc.AppendChild(dec);
            XmlElement root = doc.CreateElement("scoredb");
            root.SetAttribute("Version", "1");
            #region loop
            foreach (Score s in scores)
            {
                XmlElement baseE = doc.CreateElement("score");
                XmlElement oks = doc.CreateElement("oks");
                oks.InnerText = s.Oks.ToString();
                baseE.AppendChild(oks);
                XmlElement perfects = doc.CreateElement("perfects");
                perfects.InnerText = s.Perfects.ToString();
                baseE.AppendChild(perfects);
                XmlElement goods = doc.CreateElement("goods");
                goods.InnerText = s.Goods.ToString();
                baseE.AppendChild(goods);
                XmlElement misses = doc.CreateElement("misses");
                misses.InnerText = s.Misses.ToString();
                baseE.AppendChild(misses);
                XmlElement maxC = doc.CreateElement("maxc");
                maxC.InnerText = s.MaxCombo.ToString();
                baseE.AppendChild(maxC);
                XmlElement totalScore = doc.CreateElement("totalscore");
                totalScore.InnerText = s.TotalScore.ToString();
                baseE.AppendChild(totalScore);
                XmlElement acc = doc.CreateElement("acc");
                acc.InnerText = s.Accuracy.ToString();
                baseE.AppendChild(acc);
                XmlElement date = doc.CreateElement("date");
                date.InnerText = s.dateString;
                baseE.AppendChild(date);
                XmlElement at = doc.CreateElement("at");
                at.InnerText = s.ArtistTitle;
                baseE.AppendChild(at);
                XmlElement cname = doc.CreateElement("cname");
                cname.InnerText = s.chartName;
                baseE.AppendChild(cname);
                XmlElement flags = doc.CreateElement("flags");
                flags.InnerText = "" + s.Flags;
                baseE.AppendChild(flags);
                XmlElement replay = doc.CreateElement("replay");
                replay.InnerText = "" + s.ReplayName;
                baseE.AppendChild(replay);
                root.AppendChild(baseE);
            }
            #endregion
            doc.AppendChild(root);
            doc.Save(name);
        }
        public static Replay reconReplay(string name)
        {
            FileStream f = null;
            try
            {
                if (File.Exists(name))
                {
                    XmlDocument doc = new XmlDocument();
                    f = new FileStream(name, FileMode.Open);
                    doc.Load(f);
                    Replay r = new Replay();
                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case "m":
                                foreach (XmlNode n in node.ChildNodes)
                                {
                                    switch (n.Name)
                                    {
                                        case "c":
                                            r.Mods.Scroll = Convert.ToDouble(n.InnerText);
                                            break;
                                        case "s":
                                            r.Mods.Speed = Convert.ToDouble(n.InnerText);
                                            break;
                                        case "f":
                                            r.Mods.Flags = Convert.ToUInt32(n.InnerText);
                                            break;
                                    }
                                }
                                break;
                            case "h":
                                ReplayHit h = new ReplayHit(0, 0, 0, 0);
                                foreach (XmlNode n in node.ChildNodes)
                                {
                                    switch (n.Name)
                                    {
                                        case "t":
                                            h.Hit = (Replay.HitType)Convert.ToInt32(n.InnerText);
                                            break;
                                        case "l":
                                            h.Lane = Convert.ToInt32(n.InnerText);
                                            break;
                                        case "o":
                                            h.NoteOffset = Convert.ToInt32(n.InnerText);
                                            break;
                                        case "d":
                                            h.OffsetDifference = Convert.ToInt32(n.InnerText);
                                            break;
                                    }
                                }
                                r.HitTimings.Add(h);
                                break;
                            case "p":
                                Pair<int, Pair<int, bool>> p = new Pair<int, Pair<int, bool>>(0, new Pair<int, bool>(0, false));
                                foreach (XmlNode n in node.ChildNodes)
                                {
                                    switch (n.Name)
                                    {
                                        case "o":
                                            p.key = Convert.ToInt32(n.InnerText);
                                            break;
                                        case "l":
                                            p.value.key = Convert.ToInt32(n.InnerText);
                                            break;
                                    }
                                }
                                r.PressTimings.Add(p);
                                break;
                            case "r":
                                Pair<int, Pair<int, bool>> e = new Pair<int, Pair<int, bool>>(0, new Pair<int, bool>(0, false));
                                foreach (XmlNode n in node.ChildNodes)
                                {
                                    switch (n.Name)
                                    {
                                        case "o":
                                            e.key = Convert.ToInt32(n.InnerText);
                                            break;
                                        case "l":
                                            e.value.key = Convert.ToInt32(n.InnerText);
                                            break;
                                    }
                                }
                                r.ReleaseTimings.Add(e);
                                break;
                        }
                    }
                    f.Close();
                    return r;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                if (f != null)
                {
                    f.Close();
                }
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static List<Score> reconstruct(string name)
        {
            XmlDocument doc = new XmlDocument();
            using (FileStream fs = new FileStream(name, FileMode.Open))
            {
                doc.Load(fs);
            }
            List<Score> scores = new List<Score>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                Score s = new Score();
                #region nodeparse

                foreach (XmlNode node1 in node.ChildNodes)
                {
                    switch (node1.Name)
                    {
                        case "oks":
                            s.Oks = Convert.ToInt32(node1.InnerText);
                            break;
                        case "perfects":
                            s.Perfects = Convert.ToInt32(node1.InnerText);
                            break;
                        case "goods":
                            s.Goods = Convert.ToInt32(node1.InnerText);
                            break;
                        case "misses":
                            s.Misses = Convert.ToInt32(node1.InnerText);
                            break;
                        case "maxc":
                            s.MaxCombo = Convert.ToInt32(node1.InnerText);
                            break;
                        case "totalscore":
                            s.TotalScore = Convert.ToInt32(node1.InnerText);
                            break;
                        case "acc":
                            s.Accuracy = (float)Convert.ToDouble(node1.InnerText, Config.cultureEnglish);
                            break;
                        case "date":
                            s.dateString = node1.InnerText;
                            break;
                        case "at":
                            s.ArtistTitle = node1.InnerText;
                            break;
                        case "cname":
                            s.chartName = node1.InnerText;
                            break;
                        case "flags":
                            s.Flags = Convert.ToInt32(node1.InnerText);
                            break;
                        case "replay":
                            s.ReplayName = node1.InnerText;
                            break;
                    }
                }
                #endregion
                scores.Add(s);
            }
            return scores;
        }

        public static string getFileFromDiff(SongInfo si, string diff)
        {
            string fullDir = Path.GetFullPath("songs\\" + si.Dir);
            string tempFileName = "";
            foreach (string s in Directory.GetFiles(fullDir))
            {
                bool found = false;
                if (Path.GetExtension(s).Equals(".pnc"))
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        string thisLine = "";
                        while ((thisLine = sr.ReadLine()) != null)
                        {
                            if (thisLine.Contains("="))
                            {
                                string[] splitted = thisLine.Split('=');
                                if (splitted[0].Equals("difficulty") && splitted[1].Equals(diff))
                                {
                                    tempFileName = s;
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                }
            }
            return tempFileName;
        }
        public static int CompareScores(Score a, Score b)
        {
            return -(a.TotalScore).CompareTo(b.TotalScore);
        }
        public static List<Score> parseOnline(string s, ref Score myScore, Song song, int diff)
        {
            string[] youthem = s.Split('/');
            List<Score> scores = new List<Score>();
            if (youthem[0].Equals("no chart"))
            {
                return null;
            }
            if (!youthem[0].Equals(""))
            {
                Score temp = new Score();
                temp.ArtistTitle = song.Artist + " - " + song.SongName;
                temp.chartName = song.Charts[diff].Name;
                string[] i = youthem[0].Split('|');
                temp.Player = i[0];
                temp.TotalScore = Convert.ToInt32(i[1]);
                temp.Perfects = Convert.ToInt32(i[2]);
                temp.Goods = Convert.ToInt32(i[3]);
                temp.Oks = Convert.ToInt32(i[4]);
                temp.Misses = Convert.ToInt32(i[5]);
                temp.Accuracy = (float)Convert.ToDouble(i[6]);
                temp.MaxCombo = Convert.ToInt32(i[7]);
                temp.UnixTime = Convert.ToInt32(i[8]);
                myScore = temp;
            }
            try
            {
                string[] sc = youthem[1].Split('\\');
                for (int x = 0; x < sc.Length - 1; x++)
                {
                    Score temp = new Score();

                    temp.ArtistTitle = song.Artist + " - " + song.SongName;
                    temp.chartName = song.Charts[diff].Name;
                    string[] i = sc[x].Split('|');
                    temp.Player = i[0];
                    temp.TotalScore = Convert.ToInt32(i[1]);
                    temp.Perfects = Convert.ToInt32(i[2]);
                    temp.Goods = Convert.ToInt32(i[3]);
                    temp.Oks = Convert.ToInt32(i[4]);
                    temp.Misses = Convert.ToInt32(i[5]);
                    temp.Accuracy = (float)Convert.ToDouble(i[6]);
                    temp.MaxCombo = Convert.ToInt32(i[7]);
                    temp.UnixTime = Convert.ToInt32(i[8]);
                    scores.Add(temp);
                }
                Comparison<Score> scoreCompare = new Comparison<Score>(ScoreLibrary.CompareScores);
                scores.Sort(scoreCompare);
            }
            catch
            {
                return null;
            }
            return scores;
        }
    }
}
