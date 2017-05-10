using System.Windows.Forms;
using System.Collections.Generic;
using GameProject.Objects;
using System.Drawing;

namespace GameProject
{
    public partial class GameFrame : Form
    {
        List<Drawable> objects = new List<Drawable>();

        public GameFrame()
        {
            InitializeComponent();

            // Write Code Here

            objects.Add(new Character(new Point(0, 0)));
            objects.Add(new Character(new Point(150, 150)));
        }

        private void GameFrame_Paint(object sender, PaintEventArgs e)
        {
            foreach (Drawable obj in objects)
            {
                obj.Draw(e.Graphics);
            }
        }
    }
}
