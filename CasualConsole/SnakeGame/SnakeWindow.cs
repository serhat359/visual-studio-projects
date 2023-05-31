using Timer = System.Windows.Forms.Timer;

namespace SnakeGame;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

public record struct Point(int row, int col);

public partial class SnakeWindow : Form
{
    const int rowCount = 10;
    const int colCount = 20;
    const int sqaureCount = rowCount * colCount;
    const int squareWidth = 40;
    const byte EMPTY = 0;
    const byte SNAKEPART = 1;
    const byte FOOD = 2;
    private readonly byte[,] grid;
    private readonly Brush blackBrush = Brushes.Black;
    private readonly Brush greenBrush = Brushes.Green;
    private readonly Brush redBrush = Brushes.Red;
    private readonly Random random = new Random();
    private readonly Snake snake;
    private readonly Point[] initialSnakePoints = new Point[] { new(5, 12), new(5, 11), new(5, 10) };
    private readonly Graphics gr;

    public SnakeWindow()
    {
        InitializeComponent();
        //this.Paint += new PaintEventHandler(this.GameFrame_Paint);
        this.ClientSize = new Size(width: colCount * squareWidth, height: rowCount * squareWidth);
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        grid = new byte[rowCount, colCount];

        gr = this.CreateGraphics();
        this.snake = new Snake(initialSnakePoints, Direction.Right);

        var oneTimer = new Timer();
        oneTimer.Interval = 1;
        oneTimer.Tick += OneTimer_Tick;
        oneTimer.Start();

        var timer = new Timer();
        timer.Interval = 20000;
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    public void OneTimer_Tick(object? sender, EventArgs e)
    {
        InitialPaint(gr);
        var timer = (Timer)sender;
        timer.Stop();
        using (timer) { }
    }

    public void Timer_Tick(object? sender, EventArgs e)
    {
        GoDirection(Direction.Right);
    }

    private void GoDirection(Direction direction)
    {
        var (addedRow, addedCol, removedRow, removedCol) = snake.Go(direction);
        grid[addedRow, addedCol] = SNAKEPART;
        FillArea(this.gr, greenBrush, addedRow, addedCol);
        grid[removedRow, removedCol] = EMPTY;
        FillArea(this.gr, blackBrush, removedRow, removedCol);
    }

    private void FillArea(Graphics gr, Brush brush, int row, int col)
    {
        gr.FillRectangle(brush, col * squareWidth, row * squareWidth, squareWidth-1, squareWidth-1);
    }

    private void InitialPaint(Graphics gr)
    {
        // Initial painting
        gr.FillRectangle(blackBrush, 0, 0, colCount * squareWidth, rowCount * squareWidth);

        // Add and draw the snake
        foreach (var (row, col) in initialSnakePoints)
        {
            grid[row, col] = SNAKEPART;
            FillArea(gr, greenBrush, row, col);
        }

        // Add random food
        AddRandomFood(gr);
    }

    private void AddRandomFood(Graphics gr)
    {
        while (true)
        {
            int number = random.Next(0, sqaureCount);
            int randomCol = number % colCount;
            int randomRow = number / colCount;
            if (grid[randomRow, randomCol] == EMPTY)
            {
                grid[randomRow, randomCol] = FOOD;
                FillArea(gr, redBrush, randomRow, randomCol);
                break;
            }
        }
    }
}

public class Snake
{
    Direction facingDirection;
    Node head;
    Node tail;

    public Snake(Point[] points, Direction facingDirection)
    {
        this.facingDirection = facingDirection;

        Node first = new Node();
        first.row = points[0].row;
        first.col = points[0].col;
        head = first;
        tail = first;

        for (int i = 1; i < points.Length; i++)
        {
            AddNode(points[i].row, points[i].col);
        }
    }

    public (int addedRow, int addedCol, int removedRow, int removedCol) Go(Direction direction)
    {
        int removedRow = tail.row;
        int removedCol = tail.col;
        var newNode = tail;
        tail = tail.next;
        newNode.next = null;

        switch (direction)
        {
            case Direction.Up:
                newNode.row = head.row - 1;
                newNode.col = head.col;
                break;
            case Direction.Down:
                newNode.row = head.row + 1;
                newNode.col = head.col;
                break;
            case Direction.Left:
                newNode.row = head.row;
                newNode.col = head.col - 1;
                break;
            case Direction.Right:
                newNode.row = head.row;
                newNode.col = head.col + 1;
                break;
            default:
                throw new Exception();
        }
        head.next = newNode;
        head = newNode;

        return (newNode.row, newNode.col, removedRow, removedCol);
    }

    private void AddNode(int row, int col)
    {
        var node = new Node();
        node.row = row;
        node.col = col;
        node.next = tail;
        tail = node;
    }

    class Node
    {
        public int row;
        public int col;
        public Node? next = null;
    }
}