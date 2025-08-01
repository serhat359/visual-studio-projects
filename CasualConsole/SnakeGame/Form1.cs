using System.Runtime.CompilerServices;

namespace snake_cs;

public partial class Form1 : Form
{
    public const byte EMPTY = 0;
    public const byte SNAKE = 1;
    public const byte APPLE = 3;

    private const int rowCount = 20;
    private const int colCount = 30;
    private const int cellSizePixels = 25;
    public static Brush emptyBrush = new SolidBrush(Color.Black);
    public static Brush gridBrush = new SolidBrush(Color.FromArgb(20,20,20));
    public static Brush snakeBrush = new SolidBrush(Color.Green);
    public static Brush snakeHeadBrush = new SolidBrush(Color.LightGreen);
    public static Brush appleBrush = new SolidBrush(Color.Red);

    private byte[][] stage;
    private byte[][] stageBackup;
    private Snake snake;
    private Random random = Random.Shared;

    public Form1()
    {
        InitializeComponent();

        this.MinimizeBox = false;
        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Paint += GameOfLifeWindow_Paint;
        this.KeyDown += GameOfLifeWindow_KeyDown;

        this.ClientSize = new Size(width: colCount * cellSizePixels, height: rowCount * cellSizePixels);
        stage = Enumerable.Range(0, rowCount).Select(x => new byte[colCount]).ToArray();
        stageBackup = Enumerable.Range(0, rowCount).Select(x => new byte[colCount]).ToArray();

        snake = new Snake();
        snake.Mark(stage);
        Copy2D(stage, stageBackup);

        RandomizeApple();

        FixFlicker();
    }

    private void FixFlicker()
    {
        this.SetStyle(ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.SupportsTransparentBackColor,
            true);
    }

    private void RandomizeApple()
    {
        while (true)
        {
            int row = random.Next(0, rowCount);
            int col = random.Next(0, colCount);
            if (stage[row][col] == EMPTY)
            {
                stage[row][col] = APPLE;
                return;
            }
        }
    }

    private void GameOfLifeWindow_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g2d = e.Graphics;
        g2d.FillRectangle(gridBrush, 0, 0, cellSizePixels * colCount, cellSizePixels * rowCount);

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                var cell = stage[row][col];
                g2d.FillRectangle(emptyBrush, x: col * cellSizePixels, y: row * cellSizePixels, width: cellSizePixels-1, height: cellSizePixels-1);
                if (cell == APPLE)
                    g2d.FillEllipse(appleBrush, x: col * cellSizePixels, y: row * cellSizePixels, width: cellSizePixels-1, height: cellSizePixels-1);
            }
        }

        snake.Paint(g2d);
    }

    private void GameOfLifeWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.Control && !e.Alt && !e.Shift) // No modifier
        {
            bool ateApple;
            switch (e.KeyValue)
            {
                case (int)Keys.Right:
                    if (snake.TryMove(Direction.Right, stage, stageBackup, out ateApple))
                    {
                        if (ateApple)
                            RandomizeApple();
                        this.Invalidate();
                    }
                    break;
                case (int)Keys.Left:
                    if (snake.TryMove(Direction.Left, stage, stageBackup, out ateApple))
                    {
                        if (ateApple)
                            RandomizeApple();
                        this.Invalidate();
                    }
                    break;
                case (int)Keys.Up:
                    if (snake.TryMove(Direction.Up, stage, stageBackup, out ateApple))
                    {
                        if (ateApple)
                            RandomizeApple();
                        this.Invalidate();
                    }
                    break;
                case (int)Keys.Down:
                    if (snake.TryMove(Direction.Down, stage, stageBackup, out ateApple))
                    {
                        if (ateApple)
                            RandomizeApple();
                        this.Invalidate();
                    }
                    break;
            }
        }

        if (e.Control && !e.Alt && !e.Shift) // Ctrl modifier
        {
            if (e.KeyValue == (int)Keys.Z)
            {
                snake.Undo(stage, stageBackup);
                this.Invalidate();
            }
        }
    }

    public static void Copy2D(byte[][] source, byte[][] destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            source[i].AsSpan().CopyTo(destination[i]);
        }
    }

    enum Direction
    {
        Up,
        Right,
        Down,
        Left,
    }

    readonly record struct Point(int row, int col);

    class Snake
    {
        Point[] pointsBackup;
        int headBackup;
        Point[] points; // head to tail in the array
        int head;

        public Snake()
        {
            points = new[]{
                new Point(1,3),
                new Point(1,2),
                new Point(1,1),
            };
            head = 0;
            pointsBackup = points.ToArray();
        }

        public void Mark(byte[][] stage)
        {
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                stage[p.row][p.col] = SNAKE;
            }
        }

        public void Paint(Graphics g2d)
        {
            var headP = points[head];
            g2d.FillRectangle(snakeHeadBrush, x: headP.col * cellSizePixels + 1, y: headP.row * cellSizePixels + 1, width: cellSizePixels - 2, height: cellSizePixels - 2);

            var prev = headP;

            for (int i = 1; i < points.Length; i++)
            {
                var p = points[(i + head) % points.Length];

                if (p.row == prev.row)
                {
                    if (p.col < prev.col)
                        g2d.FillRectangle(snakeBrush, x: p.col * cellSizePixels + 1, y: p.row * cellSizePixels + 1, width: cellSizePixels, height: cellSizePixels - 2);
                    else
                        g2d.FillRectangle(snakeBrush, x: p.col * cellSizePixels - 1, y: p.row * cellSizePixels + 1, width: cellSizePixels, height: cellSizePixels - 2);
                }
                else
                {
                    if (p.row < prev.row)
                        g2d.FillRectangle(snakeBrush, x: p.col * cellSizePixels + 1, y: p.row * cellSizePixels + 1, width: cellSizePixels - 2, height: cellSizePixels);
                    else
                        g2d.FillRectangle(snakeBrush, x: p.col * cellSizePixels + 1, y: p.row * cellSizePixels - 1, width: cellSizePixels - 2, height: cellSizePixels);
                }

                prev = p;
            }
        }

        public bool TryMove(Direction dir, byte[][] stage, byte[][] stageBackup, out bool ateApple)
        {
            ateApple = false;

            var headp = points[head];
            var p = GetNext(headp, dir);
            if (p.row < 0 || p.col < 0)
                return false;
            if (p.row >= rowCount || p.col >= colCount)
                return false;
            var cell = stage[p.row][p.col];
            var canMove = cell == EMPTY || cell == APPLE;
            if (!canMove)
                return false;

            if (cell != APPLE)
            {
                points.AsSpan().CopyTo(pointsBackup);
                Form1.Copy2D(stage, stageBackup);
                headBackup = head;

                head--;
                if (head < 0)
                    head += points.Length;

                var tailP = points[head];
                stage[tailP.row][tailP.col] = EMPTY;

                stage[p.row][p.col] = SNAKE;
                points[head] = p;
            }
            else
            {
                var newPoints = new Point[points.Length + 1];
                newPoints[0] = p;
                var headToEnd = points.AsSpan()[head..];
                headToEnd.CopyTo(newPoints.AsSpan()[1..]);
                var beginToHead = points.AsSpan()[..head];
                beginToHead.CopyTo(newPoints.AsSpan((1 + headToEnd.Length)..));
                head = 0;
                stage[p.row][p.col] = SNAKE;
                points = newPoints;
                ateApple = true;
                pointsBackup = points.ToArray();
            }

            return true;
        }

        public void Undo(byte[][] stage, byte[][] stageBackup)
        {
            Copy2D(stageBackup, stage);
            head = headBackup;
            pointsBackup.AsSpan().CopyTo(points);
        }

        private Point GetNext(Point p, Direction dir)
        {
            return dir switch
            {
                Direction.Up => new Point(p.row - 1, p.col),
                Direction.Down => new Point(p.row + 1, p.col),
                Direction.Left => new Point(p.row, p.col - 1),
                Direction.Right => new Point(p.row, p.col + 1),
                _ => throw new Exception(),
            };
        }
    }
}
