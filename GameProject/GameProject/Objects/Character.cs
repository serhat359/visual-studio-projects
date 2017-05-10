using System.Drawing;
using System.Collections.Generic;
using System;

namespace GameProject.Objects
{
    class Character : Drawable
    {
        static List<Bitmap> sprites = new List<Bitmap>();
        static long animateDelayMs;
        static Character()
        {
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\1.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\2.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\3.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\4.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\5.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\6.png")));

            animateDelayMs = 1000 / sprites.Count;
        }

        private Point location;
        private long creationTime;

        public Character(Point location)
        {
            this.location = location;
            this.creationTime = Extensions.GetMicroSeconds();
        }

        public void Draw(Graphics g, long microseconds)
        {
            long timediff = microseconds - creationTime;
            long timediffms = timediff / 1000;
            long index = (timediffms / animateDelayMs) % sprites.Count;
            g.DrawImageUnscaled(sprites[(int)index], location);
        }
    }
}
