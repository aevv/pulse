using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pulse.Mechanics
{
    [Serializable()]
    public class SongInfo : ISerializable
    {
        private List<string> chartMd5s = new List<string>();

        public List<string> ChartMd5s
        {
            get { return chartMd5s; }
            set { chartMd5s = value; }
        }
        private int id = -1;

        public int ID
        {
            get { return id; }
            set
            {
                id = value; if (value == -1)
                {
                }
            }
        }
        private int fileVersion;

        public int FileVersion
        {
            get { return fileVersion; }
            set { fileVersion = value; }
        }
        private string dir;

        public string Dir
        {
            get { return dir; }
            set { dir = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string artist;

        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }
        public string Hash {
            get;
            set;
            }
        private string songName;
        //overridden for the score dictionary, otherwise it wouldn't work
        //by default c# compares by reference, so it'll only be equal if theyre both the exact same instance of the object, which obviously isn't true after serialization/deserialization (it's only a copy of the data)
        //however, this assumes that the hashes are unique, so ...
        //maybe should do something to identify with IDs and the folder name since windows guarantees distinct folder names otherwise theyre merged
        //finally, songs with the exact same pnc file name will prob cause an argument exception :/
        public override bool Equals(object obj) {
            SongInfo rhs = obj as SongInfo;
          //  Console.WriteLine("work please :(");
            if(rhs == null)
                return false;

            return Hash == rhs.Hash;//base.Equals(this, rhs) && Equals(rhs, this);
            }

        public override int GetHashCode() {
            return this.Hash.GetHashCode(); 
          }
        public string SongName
        {
            get { return songName; }
            set { songName = value; }
        }

        private List<string> diffs;

        public List<string> Diffs
        {
            get { return diffs; }
            set { diffs = value; }
        }
        public SongInfo() {
            }
        public SongInfo(string dir, string name, string artist, string songName, List<string> diffs, String hash)
        {
            this.dir = dir;
            this.name = name;
            this.artist = artist;
            this.songName = songName;
            this.diffs = diffs;
            this.Hash = hash;
            fileVersion = 0;
        }
        public SongInfo(SerializationInfo info, StreamingContext ctxt)
        {
            dir = (string)info.GetValue("dir", typeof(string));
            name = (string)info.GetValue("name", typeof(string));
            artist = (string)info.GetValue("artist", typeof(string));
            songName = (string)info.GetValue("songName", typeof(string));
            diffs = (List<string>)info.GetValue("diffs", typeof(List<string>));
            Hash = (string)info.GetValue("Hash", typeof(string));
            fileVersion = (int)info.GetValue("fileVersion", typeof(int));
            id = (int)info.GetValue("id", typeof(int));
            chartMd5s = (List<string>)info.GetValue("md5s", typeof(List<string>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("dir", dir);
            info.AddValue("name", name);
            info.AddValue("artist", artist);
            info.AddValue("songName", songName);
            info.AddValue("diffs", diffs);
            info.AddValue("Hash", Hash);
            info.AddValue("fileVersion", fileVersion);
            info.AddValue("id", id);
            info.AddValue("md5s", chartMd5s);
        }
    }
}
