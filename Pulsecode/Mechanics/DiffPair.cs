using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Mechanics {
    class DiffPair {
        public Text textData {
            get;
            set;
            }
        public KeyValuePair<int, Chart> chartData {
            get;
            set;
            }
        //Another data-binding implementation (display member, value member type thing) allowing arbitrary sorting without affecting the the selected chart
        public DiffPair(Text text, KeyValuePair<int,Chart> c) {
        textData = text;
        chartData = c;
            }
        public void draw(OpenTK.FrameEventArgs e) {
            textData.OnRenderFrame(e);
            }
        }
    }
