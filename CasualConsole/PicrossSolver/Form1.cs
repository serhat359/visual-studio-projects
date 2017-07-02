using System;
using System.Drawing;
using System.Windows.Forms;

namespace PicrossSolver
{
    public class Form1
    {
        static int[,] correct = {
            {2,2,2,1,1,1,1,2,2,2,1,1,2,1,1},
            {2,2,2,1,1,1,1,1,2,1,1,1,1,1,2},
            {2,2,2,1,1,1,2,2,1,1,2,1,1,2,2},
            {2,2,2,1,1,2,2,1,1,2,2,1,2,2,2},
            {2,2,2,1,1,1,1,1,1,1,1,1,2,2,2},
            {2,2,1,2,1,1,1,1,1,1,1,1,2,2,1},
            {1,1,1,2,2,1,1,1,2,2,1,1,2,1,1},
            {2,2,1,1,2,1,1,2,2,1,1,1,2,2,1},
            {2,2,2,1,1,1,1,1,1,1,2,1,2,1,1},
            {2,1,1,1,1,1,1,1,2,1,2,2,1,2,2},
            {1,2,1,2,1,1,1,2,2,2,1,1,2,2,1},
            {2,2,1,2,1,2,2,1,1,1,2,2,2,2,1},
            {2,2,1,2,1,1,1,2,2,2,2,2,2,1,1},
            {2,1,1,2,2,2,2,2,2,2,2,2,1,1,1},
            {1,1,1,1,2,2,2,2,2,2,1,1,2,1,1}
        };

        public Form1()
        {
            MyWindow w = new MyWindow(correct, "test");
            w.Show();
            w.Invalidate();

            Application.Run(w);
        }
    }

    public class MyWindow : Form
    {
        const int UNKNOWN = 0;
        const int FILLED = 1;
        const int EMPTY = 2;

        const int rowCount = 15;
        const int colCount = 15;
        const int displaySize = 20;
        const int lastRow = rowCount - 1;
        const int lastCol = colCount - 1;

        int[,] picture;

        public MyWindow(int[,] picture, string title)
        {
            this.picture = picture;
            this.Name = title;
            this.Text = title;

            this.Size = new Size(20 + displaySize * colCount, 40 + displaySize * rowCount);
            this.SetDesktopLocation(500, 300);

            this.Paint += new PaintEventHandler(this.GameFrame_Paint);
        }

        private void GameFrame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g2d = e.Graphics;

            int quarter = displaySize / 4;
            int threeQuarter = quarter + 2 * quarter;

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    int value = picture[row, col];

                    if (value == UNKNOWN)
                    {
                        g2d.FillRectangle(Brushes.White, col * displaySize, row * displaySize, displaySize, displaySize);
                    }
                    else if (value == FILLED)
                    {
                        g2d.FillRectangle(Brushes.Black, col * displaySize, row * displaySize, displaySize, displaySize);
                    }
                    else if (value == EMPTY)
                    {
                        Pen pen = new Pen(Color.DarkGray);

                        int xOffset = col * displaySize;
                        int yOffset = row * displaySize;
                        g2d.DrawLine(pen, xOffset + quarter, yOffset + quarter, xOffset + threeQuarter,
                                yOffset + threeQuarter);
                        g2d.DrawLine(pen, xOffset + quarter, yOffset + threeQuarter, xOffset + threeQuarter,
                                yOffset + quarter);
                    }
                    else
                        throw new Exception("Bakana!");
                }
            }

        }
    }

}
