using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PicrossSolver
{
    public class Form1
    {
        public delegate void Algorithm(CellSeries s);

        public const int UNKNOWN = 0;
        public const int FILLED = 1;
        public const int EMPTY = 2;

        public const int rowCount = 10;
        public const int colCount = 10;
        public const int displaySize = 20;
        public const int lastRow = rowCount - 1;
        public const int lastCol = colCount - 1;

        public static int iteration = 0;
        static int[,] pictureRef = null;

        public static bool[] isRowCompleted;
        public static bool[] isColCompleted;

        public Form1()
        {
            int[][] upColumn = new int[colCount][];
            upColumn[0] = (arr(1, 2));
            upColumn[1] = (arr(5, 1));
            upColumn[2] = (arr(1, 2));
            upColumn[3] = (arr(2, 1));
            upColumn[4] = (arr(1, 2));
            upColumn[5] = (arr(1, 6));
            upColumn[6] = (arr(1, 2, 2, 1));
            upColumn[7] = (arr(2));
            upColumn[8] = (arr(2, 1, 2));
            upColumn[9] = (arr(2));

            int[][] leftColumn = new int[rowCount][];
            leftColumn[0] = (arr(1, 1, 2));
            leftColumn[1] = (arr(1));
            leftColumn[2] = (arr(1, 1, 2, 1));
            leftColumn[3] = (arr(1, 1, 2, 1));
            leftColumn[4] = (arr(1, 1));
            leftColumn[5] = (arr(2, 2, 1));
            leftColumn[6] = (arr(8));
            leftColumn[7] = (arr(1, 1));
            leftColumn[8] = (arr(1, 1, 1));
            leftColumn[9] = (arr(2, 1, 1, 1));

            isRowCompleted = new bool[rowCount];
            isColCompleted = new bool[colCount];

            solveAndDisplay(upColumn, leftColumn);

            //display(correct, "This is how it should be", true);

        }

        private static void solveAndDisplay(int[][] upColumn, int[][] leftColumn)
        {
            int[,] picture = new int[rowCount, colCount];

            solve(picture, upColumn, leftColumn);

            display(picture, "This is the solver one", true);
        }

        private static void solve(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.InitialProcessing);

            dumpPicture(picture);

            testPicture(picture);

            for (iteration = 0; ; iteration++)
            {
                bool isChangeDetected = false;

                // tek sayı olanların ara boşluğunu doldurup, ulaşamayacağı yerlere çarpı atıyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessSingles);
                isChangeDetected |= testPicture(picture);

                {
                    // seri başından ve sonundan itibaren bir tarafı kapalı sayıların kalanını ayarlayıp çarpı atıyor
                    ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessStartsAndEnds);
                    isChangeDetected |= testPicture(picture);

                    // seri başlarındaki ve sonlarındaki küçük boşluklara çarpı atıyor, BU METOD processStartsAndEnds METODUNDAN HEMEN SONRA ÇALIŞMALI!!!
                    ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessStartingAndEndingUnknowns);
                    isChangeDetected |= testPicture(picture);
                }

                // serilerdeki en büyük değerler dolduysa başına ve sonuna çarpı atıyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessSetEmptiesByMax);
                isChangeDetected |= testPicture(picture);

                // serilerdeki çarpı arası boşlukları boşlukla dolduruyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessFillBetweenEmpties);
                isChangeDetected |= testPicture(picture);

                // serideki outlier olan değere karşılık gelen dolmuşları işliyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessMaxValues);
                isChangeDetected |= testPicture(picture);

                // serideki çarpılarla ayrılmış kısımları bulup işliyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessDividedParts);
                isChangeDetected |= testPicture(picture);

                // serideki dolu ve boş sayılarını kontrol ediyor
                processCheckAllCounts(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                // seri başlarında ve sonlarında kendini bulmaya çalışıyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessTryFindingMatchStartingAndEnding);
                isChangeDetected |= testPicture(picture);

                // serileri genel olarak analiz ediyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessMatching);
                isChangeDetected |= testPicture(picture);

                if (!isChangeDetected)
                {
                    break;
                }
            }

            Console.WriteLine("There was no change in the iteration: " + iteration);
        }

        public static void ApplyAlgorithmBackAndForth(int[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing)
        {
            for (int row = 0; row < rowCount; row++)
            {
                if (!isRowCompleted[row])
                {
                    processing(new CellSeries(row, picture, Direction.Horizontal, leftColumn[row]));
                    processing(new CellSeries(row, picture, Direction.HorizontalReverse, leftColumn[row]));
                }
            }

            for (int col = 0; col < rowCount; col++)
            {
                if (!isColCompleted[col])
                {
                    processing(new CellSeries(col, picture, Direction.Vertical, upColumn[col]));
                    processing(new CellSeries(col, picture, Direction.VerticalReverse, upColumn[col]));
                }
            }
        }

        public static void ApplyAlgorithmOneWay(int[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing)
        {
            for (int row = 0; row < rowCount; row++)
            {
                if (!isRowCompleted[row])
                    processing(new CellSeries(row, picture, Direction.Horizontal, leftColumn[row]));
            }

            for (int col = 0; col < rowCount; col++)
            {
                if (!isColCompleted[col])
                    processing(new CellSeries(col, picture, Direction.Vertical, upColumn[col]));
            }
        }

        private static void processCheckAllCounts(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            for (int col = 0; col < colCount; col++)
            {
                int[] values = upColumn[col];

                // TODO bunun için tablo oluştur
                int supposedFilledCount = getSum(values);
                int supposedEmptyCount = rowCount - supposedFilledCount;

                int actualFilledCount = 0;
                int actualEmptyCount = 0;

                for (int i = 0; i < rowCount; i++)
                {
                    int cell = picture[i, col];

                    if (cell == FILLED)
                        actualFilledCount++;
                    else if (cell == EMPTY)
                        actualEmptyCount++;
                    else { }
                }

                if (supposedFilledCount == actualFilledCount && supposedEmptyCount == actualEmptyCount)
                {
                    isColCompleted[col] = true;
                }
                else if (supposedFilledCount == actualFilledCount)
                {
                    for (int i = 0; i < rowCount; i++)
                        if (picture[i, col] == UNKNOWN)
                            picture[i, col] = EMPTY;

                    isColCompleted[col] = true;
                }
                else if (supposedEmptyCount == actualEmptyCount)
                {
                    for (int i = 0; i < rowCount; i++)
                        if (picture[i, col] == UNKNOWN)
                            picture[i, col] = FILLED;

                    isColCompleted[col] = true;
                }
            }

            for (int row = 0; row < rowCount; row++)
            {
                int[] values = leftColumn[row];

                // TODO bunun için tablo oluştur
                int supposedFilledCount = getSum(values);
                int supposedEmptyCount = colCount - supposedFilledCount;

                int actualFilledCount = 0;
                int actualEmptyCount = 0;

                for (int i = 0; i < colCount; i++)
                {
                    int cell = picture[row, i];

                    if (cell == FILLED)
                        actualFilledCount++;
                    else if (cell == EMPTY)
                        actualEmptyCount++;
                    else { }
                }

                if (supposedFilledCount == actualFilledCount && supposedEmptyCount == actualEmptyCount)
                {
                    isRowCompleted[row] = true;
                }
                else if (supposedFilledCount == actualFilledCount)
                {
                    for (int i = 0; i < colCount; i++)
                        if (picture[row, i] == UNKNOWN)
                            picture[row, i] = EMPTY;

                    isRowCompleted[row] = true;
                }
                else if (supposedEmptyCount == actualEmptyCount)
                {
                    for (int i = 0; i < colCount; i++)
                        if (picture[row, i] == UNKNOWN)
                            picture[row, i] = FILLED;

                    isRowCompleted[row] = true;
                }
            }
        }

        private static bool testPicture(int[,] picture)
        {
            bool isChangeDetected = false;

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    if (pictureRef[i, j] != picture[i, j])
                    {
                        isChangeDetected = true;
                        break;
                    }
                }

                if (isChangeDetected)
                    break;
            }



            dumpPicture(picture);

            return isChangeDetected;
        }

        private static int[,] dumpPicture(int[,] picture)
        {
            pictureRef = new int[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < colCount; j++)
                    pictureRef[i, j] = picture[i, j];

            return pictureRef;
        }

        public static SearchResult getMaxValue(CellColumnValues values)
        {
            List<int> indices = new List<int>();
            indices.Add(0);
            int maxValue = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > maxValue)
                {
                    indices = new List<int>();
                    indices.Add(i);
                    maxValue = values[i];
                }
                else if (values[i] == maxValue)
                {
                    indices.Add(i);
                }
            }

            return new SearchResult
            {
                LocationIndices = indices.ToArray(),
                Value = maxValue
            };
        }

        private static int getSum(int[] values)
        {
            int sum = 0;

            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum;
        }

        private static void display(int[,] picture)
        {
            display(picture, "Latest");
        }

        private static void display(int[,] picture, string title, bool isApplication = false)
        {
            dumpPicture(picture);

            var w = new MyWindow(picture, title);
            w.Show();
            w.Invalidate();

            if (isApplication)
                Application.Run(w);
        }

        private static int[] arr(params int[] values)
        {
            return values;
        }

        public class SearchResult
        {
            public int Value { get; set; }
            public int Count { get { return LocationIndices.Length; } }

            public int[] LocationIndices { get; set; }
        }

        class Range
        {
            public int start;
            public int end;

            public Range(int start, int end)
            {
                this.start = start;
                this.end = end;
            }

            public string toString()
            {
                return "{start: " + start + ", end: " + end + "}";
            }
        }

        public enum Direction
        {
            Horizontal,
            Vertical,
            HorizontalReverse,
            VerticalReverse
        }

        public class CellColumnValues
        {
            public int Length { get { return _length; } }
            public int this[int i]
            {
                get { return valueGetter(i); }
            }
            public IEnumerable<int> asIterable { get { return iterable(); } }

            private int _length;
            private int[] _values;
            private Func<int, int> valueGetter;

            public CellColumnValues(int[] values, Direction direction)
            {
                this._length = values.Length;
                this._values = values;

                switch (direction)
                {
                    case Direction.Horizontal:
                    case Direction.Vertical:
                        {
                            valueGetter = i => values[i];
                        }
                        break;
                    case Direction.HorizontalReverse:
                    case Direction.VerticalReverse:
                        {
                            valueGetter = i => values[values.Length - 1 - i];
                        }
                        break;
                    default:
                        break;
                }
            }
            public IEnumerable<int> iterable()
            {
                for (int i = 0; i < _length; i++)
                    yield return valueGetter(i);
            }
            public int Sum()
            {
                return _values.Sum();
            }
        }

        public class CellSeries
        {
            public CellColumnValues cellColumnValues { get { return _cellColumnValues; } }
            public int Length { get { return _length; } }
            public int this[int i]
            {
                get { return valueGetter(i); }
                set { valueSetter(i, value); }
            }
            public IEnumerable<int> asIterable { get { return iterable(); } }
            public Direction direction;

            private CellColumnValues _cellColumnValues;
            private int _length;
            private Func<int, int> valueGetter;
            private Action<int, int> valueSetter;

            public CellSeries(int rowOrCol, int[,] picture, Direction direction, int[] columnValues)
            {
                _cellColumnValues = new CellColumnValues(columnValues, direction);
                this.direction = direction;

                switch (direction)
                {
                    case Direction.Horizontal:
                        {
                            int row = rowOrCol;
                            valueGetter = col => picture[row, col];
                            valueSetter = (col, cell) => picture[row, col] = cell;
                            _length = colCount;
                        }
                        break;
                    case Direction.Vertical:
                        {
                            int col = rowOrCol;
                            valueGetter = row => picture[row, col];
                            valueSetter = (row, cell) => picture[row, col] = cell;
                            _length = rowCount;
                        }
                        break;
                    case Direction.HorizontalReverse:
                        {
                            int row = rowOrCol;
                            valueGetter = col => picture[row, lastCol - col];
                            valueSetter = (col, cell) => picture[row, lastCol - col] = cell;
                            _length = colCount;
                        }
                        break;
                    case Direction.VerticalReverse:
                        {
                            int col = rowOrCol;
                            valueGetter = row => picture[lastRow - row, col];
                            valueSetter = (row, cell) => picture[lastRow - row, col] = cell;
                            _length = rowCount;
                        }
                        break;
                    default:
                        break;
                }
            }

            private CellSeries(CellColumnValues _cellColumnValues, int _length, Func<int, int> valueGetter, Action<int, int> valueSetter)
            {
                this._cellColumnValues = _cellColumnValues;
                this._length = _length;
                this.valueGetter = valueGetter;
                this.valueSetter = valueSetter;
            }

            public static CellSeries Slice(CellSeries cellSeries, int startIndex, int endIndex, int[] newValues)
            {
                int size = endIndex - startIndex + 1;
                Func<int, int> getter = x => cellSeries[startIndex + x];
                Action<int, int> setter = (x, cell) => cellSeries[startIndex + x] = cell;
                CellColumnValues cellColumnValues = new CellColumnValues(newValues, cellSeries.direction);

                return new CellSeries(cellColumnValues, size, getter, setter);
            }

            public IEnumerable<int> iterable()
            {
                for (int i = 0; i < _length; i++)
                    yield return valueGetter(i);
            }
        }
    }

    public class MyWindow : Form
    {
        int[,] picture;

        public MyWindow(int[,] picture, string title)
        {
            this.picture = picture;
            this.Name = title;
            this.Text = title;

            this.Size = new Size(20 + Form1.displaySize * Form1.colCount, 40 + Form1.displaySize * Form1.rowCount);
            this.SetDesktopLocation(500, 300);

            this.Paint += new PaintEventHandler(this.GameFrame_Paint);
        }

        private void GameFrame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g2d = e.Graphics;

            int quarter = Form1.displaySize / 4;
            int threeQuarter = quarter + 2 * quarter;

            for (int row = 0; row < Form1.rowCount; row++)
            {
                for (int col = 0; col < Form1.colCount; col++)
                {
                    int value = picture[row, col];

                    if (value == Form1.UNKNOWN)
                    {
                        g2d.FillRectangle(Brushes.White, col * Form1.displaySize, row * Form1.displaySize, Form1.displaySize, Form1.displaySize);
                    }
                    else if (value == Form1.FILLED)
                    {
                        g2d.FillRectangle(Brushes.Black, col * Form1.displaySize, row * Form1.displaySize, Form1.displaySize, Form1.displaySize);
                    }
                    else if (value == Form1.EMPTY)
                    {
                        Pen pen = new Pen(Color.DarkGray);

                        int xOffset = col * Form1.displaySize;
                        int yOffset = row * Form1.displaySize;
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
