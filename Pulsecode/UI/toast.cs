using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.UI {
   public class toast {
        public int DisplayTime {
            get;
            set;
            }
        public int y {
            get;
            set;
            }
        public String text {
            get;
            set;
            }
        public toast(int Displaytime, String ttext) {
        text = ttext;
        DisplayTime = Displaytime;
        y = -50;
            }
        }
    }
