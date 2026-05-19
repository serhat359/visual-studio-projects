namespace CratePuzzle;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        this.ClientSize = new Size(500, 500);

        const int EMPTY = 0;
        const int IMMOVABLE = 1;
        const int BOX = 2;
        var state = new[]
        {
            new int[]{1,0,0,0,1},
            new int[]{0,2,0,2,0},
            new int[]{0,0,1,0,0},
            new int[]{0,2,0,2,0},
            new int[]{1,0,0,0,0},
        };
        Point mainPos = new Point(2, 0);

        var bgBrush = new SolidBrush(Color.FromArgb(156, 143, 107));
        var immovableBrush = new SolidBrush(Color.FromArgb(91, 78, 73));
        var crateBrush = new SolidBrush(Color.FromArgb(71, 56, 38));
        var mainBrush = new SolidBrush(Color.FromArgb(255, 0, 0));

        this.Paint += (object? sender, PaintEventArgs e) =>
        {
            Graphics g2d = e.Graphics;

            g2d.FillRectangle(bgBrush, 0, 0, 500, 500);
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var v = state[row][col];
                    if (v == 1)
                    {
                        g2d.FillRectangle(immovableBrush, col * 100, row * 100, 100, 100);
                    }
                    else if (v == 2)
                    {
                        g2d.FillRectangle(crateBrush, col * 100, row * 100, 100, 100);
                    }
                }
            }
            g2d.FillRectangle(mainBrush, mainPos.col * 100, mainPos.row * 100, 100, 100);
        };

        this.KeyUp += (sender, e) =>
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        var p = mainPos.Right();
                        if (state[p.row][p.col] == IMMOVABLE)
                            return;
                        if (state[p.row][p.col] == BOX)
                        {
                            var p2 = p.Right();
                            state[p2.row][p2.col] = BOX;
                            state[p.row][p.col] = EMPTY;
                        }
                        mainPos = p;
                    }
                    break;
                case Keys.Left:
                    {
                        var p = mainPos.Left();
                        if (state[p.row][p.col] == IMMOVABLE)
                            return;
                        if (state[p.row][p.col] == BOX)
                        {
                            var p2 = p.Left();
                            state[p2.row][p2.col] = BOX;
                            state[p.row][p.col] = EMPTY;
                        }
                        mainPos = p;
                    }
                    break;
                case Keys.Up:
                    {
                        var p = mainPos.Up();
                        if (state[p.row][p.col] == IMMOVABLE)
                            return;
                        if (state[p.row][p.col] == BOX)
                        {
                            var p2 = p.Up();
                            state[p2.row][p2.col] = BOX;
                            state[p.row][p.col] = EMPTY;
                        }
                        mainPos = p;
                    }
                    break;
                case Keys.Down:
                    {
                        var p = mainPos.Down();
                        if (state[p.row][p.col] == IMMOVABLE)
                            return;
                        if (state[p.row][p.col] == BOX)
                        {
                            var p2 = p.Down();
                            state[p2.row][p2.col] = BOX;
                            state[p.row][p.col] = EMPTY;
                        }
                        mainPos = p;
                    }
                    break;
                default:
                    return;
            }
            this.Invalidate();
        };
    }
}

public readonly record struct Point(int row, int col)
{
    public Point Right()
    {
        return new Point(row, col + 1);
    }
    public Point Left()
    {
        return new Point(row, col - 1);
    }
    public Point Down()
    {
        return new Point(row + 1, col);
    }
    public Point Up()
    {
        return new Point(row - 1, col);
    }
}