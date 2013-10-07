using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Input;
using Pulse.Mechanics;

namespace Pulse.UI
{
    class DropDownBox : Control
    {
        List<String> renderText;
        Boolean expanded = false;
        int index;
        Rect baseTexture;
        Rect expandTexture;
        Text baseText;
        static int linemodx;
        static int linemody = 1;
        public Text BaseText
        {
            get { return baseText; }
            set { baseText = value; }
        }
        //List<Text> expandedTexts;
       // List<Rectangle> expandedTextures;
        List<Pair<Text, Rect>> pairs = new List<Pair<Text, Rect>>();
        List<string> texts = new List<String>();

        public List<string> Texts
        {
            get { return texts; }
            set { texts = value; }
        }
        Pair<Text, Rect> top;
        Pair<Text, Rect> currentlySelected;
        List<Line> lines = new List<Line>();
        public event Action<int> selected;
        public DropDownBox(Game game, List<String> text, Rectangle bounds) : base(game,bounds)
        {
            renderText = text;
            texts = text;
            baseTexture = new Rect(bounds);
            baseTexture.Color = OpenTK.Graphics.Color4.Orange;
            baseText = new Text(game.Size, bounds.Size, bounds.Location);
            baseText.Line = text[0];
            index = 0;
            expandTexture = new Rect(new Rectangle(bounds.Location, new Size(bounds.Width, (int) (bounds.Height * text.Count))));
          //  expandTexture.Colour = OpenTK.Graphics.Color4.SteelBlue;
     //       expandedTexts = new List<Text>();
            int i = 0;
            foreach (string s in text)
            {
                Text tt = new Text(game.Size, bounds.Size, new Point(bounds.X, bounds.Y + (int)(i * bounds.Height) - 3));
                tt.Line = s;
               //expandedTexts.Add(tt);
              Rect rr = new Rect(new Rectangle(new Point(bounds.X,bounds.Y + (int)(i * bounds.Height)), bounds.Size));
              rr.Color = OpenTK.Graphics.Color4.Orange;
              pairs.Add(new Pair<Pulse.Text, Rect>(tt, rr));
           //   if (i != 0) //exclude the first line
            //  {
              Line test = new Line(new Point(bounds.X+linemodx, bounds.Y + (int)(i * bounds.Height)), new Point(bounds.X + bounds.Width + linemody, bounds.Y + (int)(i * bounds.Height)));
               //   Line l = new Line(new Point((bounds.X) + 4, bounds.Y + (int)(i * bounds.Height) - 1), new Point(bounds.X + bounds.Width - 1, bounds.Y + (int)(i * bounds.Height) - 1));
                  test.thickness = 2;
                  lines.Add(test);
             // }
                i++;
            }
            Line ending = new Line(new Point(bounds.X+linemodx, bounds.Y + (int)(i * bounds.Height) + 1), new Point(bounds.X + bounds.Width + linemody, bounds.Y + (int)(i * bounds.Height) + 1));
            ending.thickness = 2;
            lines.Add(ending);
            expanded = true;
        }
        bool transOut = false;
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            if (!expanded)
            {
                baseTexture.OnRenderFrame(e);
                baseText.OnRenderFrame(e);
                if (baseTexture.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)))
                {
                    baseTexture.Color = Color.SteelBlue;
                }
                else
                {
                    baseTexture.Color = Color.Orange;
                }
                if (baseTexture.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)) && Game.MouseState.LeftButton && canpress && game.Focused)
                {
                    expanded = true;
                    canpress = false;
                    int i = 0;
                    foreach (var s in pairs)
                    {
                        s.key.Location = new Point(bounds.X, bounds.Y );
                        s.value.Bounds = new Rectangle(new Point(bounds.X, bounds.Y), bounds.Size);
                      
                        s.key.move(new Point(bounds.X, bounds.Y + (int)(i * bounds.Height)), 0.1f);
                        s.value.move(new Point(bounds.X, bounds.Y + (int)(i * bounds.Height)),0.1f);
                        s.key.fade(1, .2);
                        s.value.fade(1, .2);
                        i++;
                    }
                    for (int j = 0; j < lines.Count; j++)
                    {
                        Line line = lines[j];
                     //   line.V1 = new Point(bounds.X - 1, bounds.Y);
                      //  line.V2 = new Point(bounds.X + bounds.Width - 1, bounds.Y),;
                  
                        line.move(new Point(bounds.X + linemodx , bounds.Y + (int)(j * bounds.Height) + 1), new Point(bounds.X + bounds.Width+ linemody, bounds.Y + (int)(j * bounds.Height) + 1), 0.1f);
                  
                    }
                    
                }
            }
            else
            {
                //expandTexture.draw(e);
               
                for(int i = 0; i < pairs.Count; i++) {
                    Pair<Text, Rect> pp = pairs[i];
             //       f.draw(e);
                    pp.value.OnRenderFrame(e);
                    pp.key.OnRenderFrame(e);
                    if (pp.value.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)) && Game.MouseState.LeftButton && canpress && game.Focused)
                    {
                        if (currentlySelected != null)
                        {
                            currentlySelected.value.Color = Color.Orange;
                        }
                        baseText.Line = pp.key.Line;
                        top = pp;
                        currentlySelected = pp;
                        pp.value.Color = OpenTK.Graphics.Color4.SteelBlue;
                        if (selected != null)
                        {
                            //important - set to invoke as 0 now instead of index because nothing uses it, perhaps change in future
                            selected.Invoke(pairs.IndexOf(pp)); //maybe can use i? lol
                        }
                    }
                    else
                    {
                        if (pp.value.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)))
                        {
                            pp.value.Color = Color.Purple;
                        }
                        else if (currentlySelected == pp)
                        {
                            pp.value.Color = Color.SteelBlue;
                        }
                        else
                        {
                            pp.value.Color = Color.Orange;
                        }
                    }
                }
                if (top != null)
                {
                    pairs.Remove(top);
                    pairs.Insert(0, top);
                    renderText.Remove(top.key.Line);
                    renderText.Insert(0, top.key.Line);
                    int i = 0;
                  //  pairs.Clear();
                    foreach (var s in pairs)
                    {
                       // string s = thing.key.Line;
                      //  Text tt = new Text(game.Size, bounds.Size, new Point(bounds.X, bounds.Y + (int)(i * bounds.Height)));
                      //  tt.Line = s;
                        s.key.Location = new Point(bounds.X, bounds.Y + (int)(i * bounds.Height) -3);
                        s.value.Bounds = new Rectangle(new Point(bounds.X, bounds.Y + (int)(i * bounds.Height)), bounds.Size);
                        //expandedTexts.Add(tt);
                     //   Rect rr = new Rect(new Rectangle(new Point(bounds.X, bounds.Y + (int)(i * bounds.Height)), bounds.Size));
                    //    rr.Colour = OpenTK.Graphics.Color4.Orange;
                   //     pairs.Add(new Pair<Pulse.Text, Rect>(tt, rr));
                        i++;
                    }

                    top = null;
                }
                foreach (Line l in lines)
                {
                    l.draw(e);
                }
                if (expandTexture.ModifiedBounds.Contains(new Point(Game.MouseState.X, Game.MouseState.Y)) && Game.MouseState.LeftButton && canpress && game.Focused)
                {
                  //  expanded = false;
                    int i = 0;
                    foreach (var s in pairs)
                    {
                        s.key.move(new Point(bounds.X, bounds.Y), 0.1f);
                        s.value.move(new Point(bounds.X, bounds.Y), 0.1f);
                        s.key.fade(0, .05);
                        s.value.fade(0, .05);
                        i++;
                    }
                    for (int j = 0; j < lines.Count; j++)
                    {
                        Line line = lines[j];
                        line.move(new Point(bounds.X + linemodx, bounds.Y), new Point(bounds.X + bounds.Width +linemody, bounds.Y), 0.1f);
                  
                    }
                    transOut = true;
                    canpress = false;
                }
                if (transOut && !pairs[0].value.Moving)
                {
                    expanded = false;
                    transOut = false;
                }
            }
            if (!Game.MouseState.LeftButton)
            {
                canpress = true;
            }
        }
        bool canpress;
    }
}
