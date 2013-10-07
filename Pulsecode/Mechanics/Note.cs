using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Mechanics
{
    public class Note
    {
        private int moveTime = 1200;
        public int MoveTime
        {
            get { return moveTime; }
            set { moveTime = value; }
        }
        private bool holdHighlight = false;

        public bool HoldHighlight
        {
            get { return holdHighlight; }
            set { holdHighlight = value; }
        }
        private bool highlight = false;

        public bool Highlight
        {
            get { return highlight; }
            set { highlight = value; }
        }
        private int xOffset = 0, yOffset = 0;

        public int YOffset
        {
            get { return yOffset; }
            set { yOffset = value; }
        }

        public int XOffset
        {
            get { return xOffset; }
            set { xOffset = value; }
        }
        private double offset;

        public double Offset
        {
            get { return offset; }
            set { offset = value; }
        }
        private int location;

        public int Location
        {
            get { return location; }
            set { location = value; }
        }
        private bool hold = false;

        public bool Hold
        {
            get { return hold; }
            set
            {
                hold = value;
            }
        }
        private double holdOffset = 0;

        public double HoldOffset
        {
            get { return holdOffset; }
            set { holdOffset = value; }
        }
        private Rect holdbar;

        public Rect Holdbar
        {
            get { return holdbar; }
            set { holdbar = value; }
        }
        private Rect holdStart;

        public Rect HoldStart
        {
            get { return holdStart; }
            set { holdStart = value; }
        }
        private Rect holdHighlightTexture;
        private Rect highlightTexture;
        private Animation texture;
        private Rect holdEnd;

        public Rect HoldEnd
        {
            get { return holdEnd; }
            set { holdEnd = value; }
        }
        private bool active = true;

        public bool Enabled
        {
            get { return active; }
            set { active = value; }
        }
        private bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public Animation Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        private double holdVertical = 0;

        public double HoldVertical
        {
            get { return holdVertical; }
            set
            {
                holdVertical = value;
                int temp = 600;
                if (Skin.BottomSpace && !Config.Editing)
                {
                    temp = 570;
                }
                if (vertical < temp || Config.Editing)
                {
                    temp = (int)vertical;
                }
                holdbar.Bounds = new System.Drawing.Rectangle((Skin.LaneLoc[location-1]) + (1 * location) + xOffset + 4, (int)value + yOffset + 14, holdbar.Bounds.Width, temp - (int)value - 17);
                holdHighlightTexture.Bounds = new System.Drawing.Rectangle(((location - 1) + Skin.LaneLoc[location-1]) + (1 * location) + xOffset - 2, (int)value + yOffset - 4, holdbar.Bounds.Width + 8, (int)vertical - (int)value);
                holdEnd.Bounds = new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + xOffset + 4, (int)value + yOffset, holdbar.Bounds.Width, 14);
                if (temp - 20 + yOffset < holdEnd.Bounds.Y + holdEnd.Bounds.Height)
                {
                    holdStart.Alpha = 0.0f;
                }
                else
                {
                    holdStart.Alpha = holdbar.Alpha;
                }
                holdStart.Bounds = new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + xOffset + 4, temp - 14 + yOffset, holdbar.Bounds.Width, 14);
            }
        }

        private double vertical;
        public double Vertical
        {
            get { return vertical; }
            set
            {
                vertical = value; 
                texture.Bounds = new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + 4 + xOffset, (int)value - 7 + yOffset, texture.Bounds.Width, texture.Bounds.Height);
                highlightTexture.Bounds = new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + xOffset, (int)value - 7 + yOffset - 4, texture.Bounds.Width + 8, texture.Bounds.Height + 8);
            }
        }
        public Note(int offset, int location, string texturePath)
            : this(offset, location, texturePath, false, 0, Skin.skindict["holdBar"])
        {

        }
        public Note(int offset, int location, string texturePath, bool hold, int holdOffset, string holdPath)
        {
            this.offset = offset;
            this.location = location;
            this.hold = hold;
            this.holdOffset = holdOffset;
            texture = new Animation(new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + 2 + xOffset, -14 + yOffset, Skin.LaneWidth[location - 1], 14), Skin.NoteFrameRate, "note" + texturePath, Skin.NoteFrameCount, true, false);
            //texture = new Rect(new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + 2 + xOffset, -14 + yOffset, Skin.LaneWidth[location - 1], 14), texturePath);
            holdbar = new Rect(new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + 2 + xOffset, -14 + yOffset, Skin.LaneWidth[location - 1], 14), holdPath);
            holdHighlightTexture = new Rect(new System.Drawing.Rectangle((Skin.LaneLoc[location - 1] + (1 * location) + 2 + xOffset - 4), yOffset, Skin.LaneWidth[location - 1] + 8, 28), holdPath);
            holdHighlightTexture.Color = new OpenTK.Graphics.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            highlightTexture = new Rect(new System.Drawing.Rectangle((Skin.LaneLoc[location - 1] + (1 * location) + 2 + xOffset - 4), -14 + yOffset - 4, Skin.LaneWidth[location - 1] + 8, 28), texturePath);
            highlightTexture.Color = new OpenTK.Graphics.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            holdEnd = new Rect(new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + 2 + xOffset, -14 + yOffset, Skin.LaneWidth[location - 1], 14), Skin.skindict["holdEnd"]);
            holdStart = new Rect(new System.Drawing.Rectangle((Skin.LaneLoc[location - 1]) + (1 * location) + 2 + xOffset, -14 + yOffset, Skin.LaneWidth[location - 1], 14), Skin.skindict["holdStart"]);
            setColor();
        }
        public void draw(OpenTK.FrameEventArgs e)
        {
            if (hold)
            {
                if (HoldHighlight)
                {
                    holdHighlightTexture.OnRenderFrame(e);
                }
                holdbar.OnRenderFrame(e);
                holdEnd.OnRenderFrame(e);
                holdStart.OnRenderFrame(e);
            }
            if (highlight)
            {
                highlightTexture.OnRenderFrame(e);
            }
            texture.draw(e);
        }
        public void setAlpha(float a)
        {
            if (hold)
            {
                holdbar.Color = new OpenTK.Graphics.Color4(holdbar.Color.R, holdbar.Color.G, holdbar.Color.B, a);
                holdEnd.Color = new OpenTK.Graphics.Color4(holdEnd.Color.R, holdEnd.Color.G, holdEnd.Color.B, a);
                holdStart.Color = new OpenTK.Graphics.Color4(holdStart.Color.R, holdStart.Color.G, holdStart.Color.B, a);
            }
            texture.Colour = new OpenTK.Graphics.Color4(texture.Colour.R, texture.Colour.G, texture.Colour.B, a);
        }
        public float getAlpha()
        {
            return texture.Colour.A;
        }
        public void setColor()
        {
            texture.Colour = Skin.KeyColours[location - 1];
            if (hold && Skin.ColorHolds)
            {
                holdEnd.Color = Skin.EndColors[location - 1];
                holdEnd.Alpha = 1.0f;
                holdbar.Color = Skin.KeyColours[location - 1];
                holdbar.Alpha = 1.0f;
                holdStart.Color = Skin.StartColors[location - 1];
                holdStart.Alpha = 1.0f;
            }
        }
    }
}
