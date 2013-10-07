using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
namespace Pulse.UI
{
    public class NoticeManager
    {
        List<Notice> notices = new List<Notice>();
        List<Notice> toremove = new List<Notice>();
        public void update(OpenTK.FrameEventArgs e)
        {
            foreach (Notice n in notices)
            {
                if (n.texture.Colour.A == 0)
                {
                    toremove.Add(n);
                }
                else
                {
                    n.OnUpdateFrame(e);
                }
            }
            foreach (Notice n in toremove)
            {
                notices.Remove(n);
            }
            toremove.Clear();
        }
        public void addNotice(string text, int duration)
        {
            Notice n = new Notice(new Point(Config.ResWidth - 270, 768), duration, text);
            int temp = 768 - n.getHeight();
            //n.move(new Point(0, 768), 0);
            //temp += 5;

            foreach (Notice i in notices)
            {
                i.move(new Point(Config.ResWidth - 270, i.pos.Y - n.getHeight()), .5);
            }
            notices.Add(n);
            n.move(new Point(Config.ResWidth - 270, temp), .5);
        }
        //overload yeee
        public void addNotice(string text)
        {
            addNotice(text, 5000);
        }
        public void render(OpenTK.FrameEventArgs e)
        {
            foreach (Notice n in notices)
            {
                n.OnRenderFrame(e);
            }
        }
    }
    public class Notice : Control
    {
        Point Position;
       public Point pos
        {
            get { return Position; }
            set { Position = value; }
        }
        int life;
        String text;
        public Text texture;
        Rect bg;
        int numberOfLines = 1;
        public Notice(Point p, int duration, string txt)
            : base(Game.game, txt)
        {
            pos = p;
            life = duration;
            text = txt;
            float splits = txt.Length / 20f;
            if (splits > 1)
            {
                int upper = (int)Math.Ceiling(splits - 1); //if the number is an integer, take away one, otherwise if there's a decimal portion have it remain
                for (int i = 1; i <= upper; i++)
                {
                    int position = i * 20;
                    text = text.Insert(position, "\n");
                    numberOfLines++;
                }
            }
            texture = new Text(new Size(1024, 768), p);
            texture.textFont = new Font("Myriad Pro", 14); 
            texture.Line = text;
            texture.Shadow = false;
            bg = new Rect(new Rectangle(pos, new Size(270 , numberOfLines * 22)));
            bg.Color = Color.Gray;
            bg.Alpha = .9f;
            
        }
        public int getHeight()
        {
            return bg.Bounds.Height;
        }
        bool trigger;
        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            bg.OnRenderFrame(e);
            texture.OnRenderFrame(e);
        }
        public void move(Point p, double time)
        {
            bg.move(p, time);
            texture.move(p, time);
            pos = p;
        }
        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            life -= (int) (e.Time * 1000);
            if (life <= 0 && !trigger)
            {
                this.bg.fade(0, 1);
                this.texture.fade(0, 1);
                trigger = true;
            }
            base.OnUpdateFrame(e);
        }

    }
}
