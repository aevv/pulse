using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Pulse.Mechanics;

namespace Pulse
{
    public class SongLibrary
    {
        static public SortedDictionary<string, SongInfo> songInfos = new SortedDictionary<string, SongInfo>();

        // static public SortedDictionary<string, Song> songList = new SortedDictionary<string, Song>();

        static public List<SongInfo> Songs = new List<SongInfo>();
        /*    public static void cacheSongInfo1()
            {

                if (!Directory.Exists("songs"))
                {
                    Directory.CreateDirectory("songs");
                }
                DirectoryInfo directory = new DirectoryInfo("songs");
                DirectoryInfo[] subDirs = directory.GetDirectories();
                foreach (DirectoryInfo d in subDirs)
                {
                    FileInfo[] files = d.GetFiles();
                    if (songInfos.ContainsKey(d.Name))
                    {
                        foreach (FileInfo f in files)
                        {
                            if (f.Extension.Equals(".pnc"))
                            {
                                if(songInfos[d.Name].Diffs.ContainsKey(f.Name)) {
                                    bool different = !songInfos[d.Name].Diffs[f.Name].Equals(Utils.calcHash(f.FullName));
                                }
                            }                        
                        }
                    }                
                }
            }*/
        public static void cacheSongInfo()
        {
            if (!Directory.Exists("songs"))
            {
                Directory.CreateDirectory("songs");
            }
            DirectoryInfo directory = new DirectoryInfo("songs");
            DirectoryInfo[] subDirs = directory.GetDirectories();
            foreach (DirectoryInfo d in subDirs)
            {
                FileInfo[] files = d.GetFiles();

                foreach (FileInfo f in files)
                {
                    if (f.Extension.Equals(".pnc"))
                    {
                        string filename = f.Name.Substring(0, f.Name.Length - 4);
                        bool changed = false;
                        String hash = "";
                        if (songInfos.ContainsKey(d.Name))
                        { //note... probably should make check more dynamically? (when song is loaded) don't know how expensive calculating hash is
                            hash = Utils.calcHash(f.FullName);
                            // Console.WriteLine(f.FullName);
                            //changed = !songInfos[d.Name].Hash.Equals(hash);
                        }
                        if (!songInfos.ContainsKey(d.Name) || changed)
                        {
                            if (changed)
                            {
                                //SongLibrary.Songs.Remove(songInfos[d.Name]);
                                //songInfos.Remove(d.Name);
                            }
                            string artist = "";
                            string songName = "";
                            List<String> diffNames = new List<String>();
                            using (StreamReader sr = new StreamReader("songs\\" + d.Name + "\\" + f.Name))
                            {
                                string tempS;
                                while ((tempS = sr.ReadLine()) != null)
                                {
                                    if (tempS.Substring(0, 1).Equals("["))
                                    {
                                        if (!tempS.Substring(1, tempS.Length - 2).Equals("song") && !tempS.Substring(1, tempS.Length - 2).Equals("timing"))
                                        {
                                            diffNames.Add(tempS.Substring(1, tempS.Length - 2));
                                        }
                                    }
                                    else
                                    {
                                        string[] split = tempS.Split('=');
                                        switch (split[0].ToLower(Config.cultureEnglish))
                                        {
                                            case "artist":
                                                artist = split[1];
                                                break;
                                            case "songname":
                                                songName = split[1];
                                                break;
                                            case "difficulty":
                                                break;
                                        }
                                    }
                                }
                            }
                            SongInfo temp = new SongInfo(d.Name, filename, artist, songName, diffNames, string.IsNullOrWhiteSpace(hash) ? Utils.calcHash(f.FullName) : hash);
                            if (!temp.ChartMd5s.Contains(Utils.calcHash(f.FullName)))
                            {
                                temp.ChartMd5s.Add(Utils.calcHash(f.FullName));
                            }
                            songInfos.Add(d.Name, temp);
                            Songs.Add(temp);
                        }
                        else
                        {
                            if (changed)
                                Console.WriteLine("ignoring " + filename);
                            SongInfo temp = songInfos[d.Name];
                            if (!temp.ChartMd5s.Contains(Utils.calcHash(f.FullName)))
                            {
                                temp.ChartMd5s.Add(Utils.calcHash(f.FullName));
                            }

                        }
                    }
                }
            }
        }

        static public void loadSongInfo()
        {
            if (File.Exists("songs.bin"))
            {
                Stream stream = null;
                try
                {
                    stream = File.Open("songs.bin", FileMode.Open);
                    stream.Position = 0;
                    BinaryFormatter b = new BinaryFormatter();
                    //temp = (SongInfo)b.Deserialize(stream);
                    songInfos = (SortedDictionary<string, SongInfo>)b.Deserialize(stream);
                    stream.Close();
                    List<SongInfo> toRemove = new List<SongInfo>();
                    foreach (var pair in songInfos)
                    {
                        //Console.WriteLine(pair.Value.SongName + " " + pair.Value.ID);
                        if (File.Exists("songs\\" + pair.Value.Dir + "\\" + pair.Value.Name + ".pnc"))
                        {
                            Songs.Add(pair.Value);
                        }
                        else
                        {
                            toRemove.Add(pair.Value);
                            // Console.WriteLine("nop"); //wtf no exception thrown for modifying?
                        }
                        for (int i = toRemove.Count - 1; i > -1; i--)
                        {
                            Songs.Remove(toRemove[i]);
                        }
                        toRemove.Clear();
                    }
                    // songInfos.Clear();
                    cacheSongInfo();
                    saveSongInfo();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace + "\nLoading cache manually");
                    stream.Close();
                    cacheSongInfo();
                    saveSongInfo();
                }
            }
            else
            {
                cacheSongInfo();
                if (Songs.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Please put at least one notechart in your songs directory!");
                    Environment.Exit(0);
                }
            }
        }

        static public void saveSongInfo()
        {
            Stream stream = File.Open("songs.bin", FileMode.Create);
            stream.Position = 0;
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(stream, songInfos);
            stream.Close();
        }

        static public Song loadSong(SongInfo song)
        {
            if (song.FileVersion == 0)
            {
                if (File.Exists("songs\\" + song.Dir + "\\" + song.Name + ".pnc"))
                {
                    Song temp = new Song(song.Dir + "\\" + song.Name + ".pnc");
                    temp.ID = song.ID;
                    return temp;
                }
                else
                {
                    Game.addToast("press f1, failed to load song " + song.Name);
                    Songs.Remove(song);
                    songInfos.Remove(song.Name);
                    return new Song();
                }
            }
            else if (song.FileVersion == 1)
            {
                Song temp2 = new Song(song.Dir);
                temp2.ID = song.ID;
                return temp2;
            }
            return null;
        }
        static public SongInfo findByMD5(string md5, ref int diff)
        {
            foreach (SongInfo s in Songs)
            {
                for (int x = 0; x < s.ChartMd5s.Count; x++)
                {
                    if (md5.Equals(s.ChartMd5s[x]))
                    {
                        diff = x;
                        return s;
                    }
                }
            }
            return null;
        }
    }
}
