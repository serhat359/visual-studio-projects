using System.Drawing;

namespace GameProject.Objects
{
    class Character : Drawable
    {
        private Rectangle rect;

        public Character(Point location) {
            this.rect = new Rectangle(location, new Size(100, 100));
        }

        public void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color.Black), rect);
        }
    }
}
