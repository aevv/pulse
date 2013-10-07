using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Pulse.UI
{
    public abstract class InterfaceComponent
    {
        protected bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        protected bool enabled = true;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        abstract public void OnRenderFrame(FrameEventArgs e);
        abstract public void OnUpdateFrame(FrameEventArgs e);
        abstract public void OnLoad(EventArgs e);
    }
}