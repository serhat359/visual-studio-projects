using System.Collections.Generic;
using System.Drawing;

namespace GameProject.Objects
{
    class Character : Drawable
    {
        static List<Bitmap> sprites = new List<Bitmap>();
        static long animateDelayMs;
        static Character()
        {
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\stand1.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\stand2.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\stand3.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\stand4.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\stand5.png")));
            sprites.Add(new Bitmap(Extensions.GetPath(@"Sprites\stand6.png")));

            animateDelayMs = 1000 / sprites.Count;
        }

        private Point location;
        private long creationTime;

        public Character(Point location)
        {
            this.location = location;
            this.creationTime = Extensions.GetMicroSeconds();
        }

        public override void Draw(Graphics g, long microseconds)
        {
            long timediff = microseconds - creationTime;
            long timediffms = timediff / 1000;
            long index = (timediffms / animateDelayMs) % sprites.Count;
            g.DrawImageUnscaled(sprites[(int)index], location);
        }
    }
}
