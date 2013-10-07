using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.UI;

namespace Pulse.Mechanics {
    class SongPair {
        public SelectComponent select {
            get;
            set;
            }
        public SongInfo Info;
        //This class was to prevent a bug with songs that occured when adding a new song for the first time - SongLibrary.Songs has new songs appended to the end of the list; however, since songInfos is a sorted dictionary it is sorted based on key, and the new song is not added to the end of the list. Song labels are added based on songInfos, while selection is based on the Song, causing a conflict
        public SongPair(SelectComponent t, SongInfo s) {
            select = t;
            Info = s;
            }
        //Convenience method
        public void draw(OpenTK.FrameEventArgs e) {
            select.OnRenderFrame(e);
            }
        }
    }
