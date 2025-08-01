namespace TetSystem;

public class TetSystemWindow : Form
{
    private const int width = 1000;
    private const int height = 500;
    private const int margin = 100;
    Pen pen = new Pen(Color.DarkGray, 2);
    Pen redpen = new Pen(Color.Red, 2);
    Pen bluepen = new Pen(Color.Blue, 2);
    Font font = new Font("Consolas", 10, FontStyle.Bold);
    Brush blackbrush = new SolidBrush(Color.Black);
    Brush redbrush = new SolidBrush(Color.Red);
    Brush bluebrush = new SolidBrush(Color.Blue);

    int currentTet = 1;

    public TetSystemWindow()
    {
        this.Size = new Size(width: width, height: height);
        this.Paint += new PaintEventHandler(this.GameFrame_Paint);
        this.KeyDown += XWindow_KeyPress;
    }

    private void XWindow_KeyPress(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Right)
        {
            currentTet++;
            this.Refresh();
        }
        else if (e.KeyCode == Keys.Left)
        {
            if (currentTet <= 1)
                return;

            currentTet--;
            this.Refresh();
        }
    }

    private void GameFrame_Paint(object? sender, PaintEventArgs e)
    {
        int leftWithMargin = margin;
        int rightWithMargin = width - margin;
        int bottomWithMargin = height - margin;
        int lineWidth = rightWithMargin - leftWithMargin;

        Graphics g2d = e.Graphics;

        // Draw bottom line
        g2d.DrawLine(pen, margin, bottomWithMargin, rightWithMargin, bottomWithMargin);

        // Draw the best frequencies
        float[] ratios = { 1.5f, 1.33f, 1.25f, 1.2f, 1.66f };
        foreach (var ratio in ratios)
        {
            float left = leftWithMargin + ((ratio - 1) * lineWidth);
            g2d.DrawLine(redpen, (int)left, margin, (int)left, bottomWithMargin);
            g2d.DrawString(ratio.ToString("N2"), font, redbrush, new Rectangle((int)left - 20, bottomWithMargin + 20, 100, 100));
        }

        // Draw based on the tets
        g2d.DrawLine(bluepen, margin, margin, margin, bottomWithMargin);
        g2d.DrawString(1.ToString("N2"), font, bluebrush, new Rectangle(margin - 20, bottomWithMargin + 20, 100, 100));
        g2d.DrawLine(bluepen, rightWithMargin, margin, rightWithMargin, bottomWithMargin);
        g2d.DrawString(2.ToString("N2"), font, bluebrush, new Rectangle(rightWithMargin - 20, bottomWithMargin + 20, 100, 100));
        var baseInterval = Math.Pow(2, 1.0 / currentTet);
        for (int i = 1; i < currentTet; i++)
        {
            var interval = Math.Pow(baseInterval, i);
            double left = leftWithMargin + ((interval - 1) * lineWidth);
            g2d.DrawLine(bluepen, (int)left, margin, (int)left, bottomWithMargin);
            //g2d.DrawString(interval.ToString("N2"), font, bluebrush, new Rectangle((int)left - 20, bottomWithMargin + 20, 100, 100));
        }

        // Write the bottom text
        g2d.DrawString("Current tets: " + currentTet, font, blackbrush, new Rectangle(0, 0, 0, 0));
    }
}