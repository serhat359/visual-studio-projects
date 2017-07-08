using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PicrossSolver
{
    public class Form1
    {
        public delegate void Algorithm(CellSeries s);

        public static char[] chars = { ' ', '■', '.' };
        public const int UNKNOWN = 0;
        public const int FILLED = 1;
        public const int EMPTY = 2;

        public const int rowCount = 25;
        public const int colCount = 25;
        public const int displaySize = 20;
        public const int lastRow = rowCount - 1;
        public const int lastCol = colCount - 1;

        public static int iteration = 0;
        static int[,] pictureRef = null;

        public static bool[] isRowCompleted;
        public static bool[] isColCompleted;

        static int[,] correct = {
            {2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,1,1,1,2,2,2,2,1,1,1},
{2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,1,1,2,2,2,2,2,1,1,1},
{1,2,2,2,2,1,1,1,1,1,2,2,2,2,1,1,1,2,2,2,2,2,1,1,1},
{1,2,2,2,2,1,1,1,1,1,1,2,1,2,1,1,1,1,1,2,2,1,1,1,1},
{2,2,2,2,2,1,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1},
{2,2,2,2,2,2,1,1,1,2,2,2,1,2,2,2,1,1,1,1,1,1,1,1,1},
{2,2,2,2,2,1,1,2,2,2,2,2,2,1,2,2,2,2,2,2,1,2,2,2,2},
{1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,1,1,2,2,2,2,2,2,2,2},
{1,1,2,1,1,1,2,2,2,2,2,2,2,1,1,1,2,2,2,2,2,2,2,2,2},
{1,1,1,1,1,1,2,2,2,2,2,2,2,1,1,1,2,2,2,2,2,2,1,2,2},
{2,1,1,1,1,2,2,2,2,1,2,2,2,1,1,1,2,2,2,2,1,1,1,2,2},
{2,1,1,1,1,2,2,1,1,1,2,2,2,1,1,1,1,2,1,2,1,1,1,1,2},
{1,2,2,2,1,1,2,1,1,1,2,2,1,1,1,1,1,2,2,2,1,1,1,1,1},
{1,2,2,2,2,1,1,1,1,1,2,2,1,1,1,1,1,2,2,2,1,1,1,1,1},
{1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,1,1,2,2,2},
{2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,2,2,2},
{2,2,1,1,1,1,1,2,1,1,1,1,1,1,1,1,2,1,2,1,1,1,2,2,2},
{2,2,2,2,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,1,2},
{2,2,2,2,2,2,1,2,2,1,1,1,1,1,2,2,2,1,2,1,1,1,1,1,1},
{2,2,2,2,1,2,1,2,2,2,2,1,1,1,1,1,2,2,2,2,2,1,1,1,1},
{2,2,2,2,2,2,1,2,2,2,2,1,1,1,1,1,1,1,2,2,2,2,2,2,1},
{1,1,1,1,1,1,1,1,1,2,2,1,1,1,1,1,1,1,2,2,2,2,2,2,2},
{1,1,1,2,1,1,1,1,1,1,1,1,1,1,2,1,1,1,2,2,2,2,2,2,2},
{1,1,1,1,2,1,1,1,1,1,1,2,1,2,2,2,2,2,2,1,2,2,2,2,2},
{2,2,2,2,2,1,1,1,1,1,1,2,2,2,2,2,2,2,2,1,2,2,2,2,2},
        };

        static bool checkCorrect = correct.Length > 0;

        public Form1()
        {
            int[][] upColumn = new int[colCount][];
            upColumn[0] = (arr(2, 3, 3, 3));
            upColumn[1] = (arr(5, 3));
            upColumn[2] = (arr(1, 3, 2, 3));
            upColumn[3] = (arr(5, 2, 1, 1));
            upColumn[4] = (arr(6, 4, 1, 2));
            upColumn[5] = (arr(3, 4, 6, 4));
            upColumn[6] = (arr(6, 12));
            upColumn[7] = (arr(4, 1, 5, 1, 4));
            upColumn[8] = (arr(4, 7, 4));
            upColumn[9] = (arr(4, 9, 3));
            upColumn[10] = (arr(1, 5, 3));
            upColumn[11] = (arr(9));
            upColumn[12] = (arr(3, 12));
            upColumn[13] = (arr(1, 1, 15));
            upColumn[14] = (arr(3, 9, 3));
            upColumn[15] = (arr(5, 10, 4));
            upColumn[16] = (arr(6, 1, 3, 3));
            upColumn[17] = (arr(1, 3, 3, 3));
            upColumn[18] = (arr(3, 1, 1));
            upColumn[19] = (arr(2, 4, 2));
            upColumn[20] = (arr(3, 9));
            upColumn[21] = (arr(3, 10));
            upColumn[22] = (arr(6, 5, 3));
            upColumn[23] = (arr(6, 3, 3));
            upColumn[24] = (arr(6, 2, 3));

            int[][] leftColumn = new int[rowCount][];
            leftColumn[0] = (arr(1, 3, 3));
            leftColumn[1] = (arr(1, 2, 3));
            leftColumn[2] = (arr(1, 5, 3, 3));
            leftColumn[3] = (arr(1, 6, 1, 5, 4));
            leftColumn[4] = (arr(4, 13));
            leftColumn[5] = (arr(3, 1, 9));
            leftColumn[6] = (arr(2, 1, 1));
            leftColumn[7] = (arr(8, 2));
            leftColumn[8] = (arr(2, 3, 3));
            leftColumn[9] = (arr(6, 3, 1));
            leftColumn[10] = (arr(4, 1, 3, 3));
            leftColumn[11] = (arr(4, 3, 4, 1, 4));
            leftColumn[12] = (arr(1, 2, 3, 5, 5));
            leftColumn[13] = (arr(1, 5, 5, 5));
            leftColumn[14] = (arr(1, 12, 2));
            leftColumn[15] = (arr(14, 3));
            leftColumn[16] = (arr(5, 8, 1, 3));
            leftColumn[17] = (arr(10, 7));
            leftColumn[18] = (arr(1, 5, 1, 6));
            leftColumn[19] = (arr(1, 1, 5, 4));
            leftColumn[20] = (arr(1, 7, 1));
            leftColumn[21] = (arr(9, 7));
            leftColumn[22] = (arr(3, 10, 3));
            leftColumn[23] = (arr(4, 6, 1, 1));
            leftColumn[24] = (arr(6, 1));

            isRowCompleted = new bool[rowCount];
            isColCompleted = new bool[colCount];

            int leftSum = leftColumn.Sum(x => x.Sum());
            int upSum = upColumn.Sum(x => x.Sum());

            if (leftSum != upSum)
                throw new Exception("Numbers are entered wrong!");

            solveAndDisplay(upColumn, leftColumn);

            if (checkCorrect)
                display(correct, "This is how it should be", true);
        }

        private static void solveAndDisplay(int[][] upColumn, int[][] leftColumn)
        {
            int[,] picture = new int[rowCount, colCount];

            solve(picture, upColumn, leftColumn);

            string joined = string.Join(",\n", array2dAsEnumerable(picture).Select(x => string.Join(",", x)));

            display(picture, "This is the solved one", true);
        }

        private static IEnumerable<IEnumerable<int>> array2dAsEnumerable(int[,] picture)
        {
            for (int row = 0; row < rowCount; row++)
            {
                IEnumerable<int> rowList = Enumerable.Range(0, colCount).Select(col => picture[row, col]);

                yield return rowList;
            }
        }

        private static void solve(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            dumpPicture(picture);

            ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.InitialProcessing);

            for (iteration = 0; ; iteration++)
            {
                Console.WriteLine("Running iteration: " + iteration);

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

                // serilerdeki dolu grup sayısı değer sayısını geçtiğinde bakıyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.FillBetweenFilled);
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
            ApplyAlgorithm(picture, upColumn, leftColumn, processing, true);
        }

        public static void ApplyAlgorithmOneWay(int[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing)
        {
            ApplyAlgorithm(picture, upColumn, leftColumn, processing, false);
        }

        private static void ApplyAlgorithm(int[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing, bool isTwoWay)
        {
            for (int row = 0; row < rowCount; row++)
            {
                if (!isRowCompleted[row])
                {
                    processing(new CellSeries(row, picture, Direction.Horizontal, leftColumn[row]));

                    if (isTwoWay)
                    {
                        processing(new CellSeries(row, picture, Direction.HorizontalReverse, leftColumn[row]));
                    }
                }
            }

            for (int col = 0; col < colCount; col++)
            {
                if (!isColCompleted[col])
                {
                    processing(new CellSeries(col, picture, Direction.Vertical, upColumn[col]));

                    if (isTwoWay)
                    {
                        processing(new CellSeries(col, picture, Direction.VerticalReverse, upColumn[col]));
                    }
                }
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

            if (checkCorrect)
                for (int i = 0; i < rowCount; i++)
                    for (int j = 0; j < colCount; j++)
                        if (picture[i, j] != UNKNOWN && picture[i, j] != correct[i, j])
                        {
                            int asIs = picture[i, j];
                            int correctOne = correct[i, j];
                            Console.WriteLine("Hata tespit edildi, iteration: " + iteration);
                            display(pictureRef, "Hatasız olan");
                            display(picture, "Hatalı olan");
                            display(correct, "Olması gereken", true);
                            throw new Exception("Önceki metot yanlış, iteration: " + iteration + ", row: " + i + ", col: " + j);
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
            if (values.Length > 0)
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
            else
            {
                return new SearchResult
                {
                    LocationIndices = new int[] { },
                    Value = 0
                };
            }
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

        private static string pictureToString(int[,] picture)
        {
            StringBuilder ss = new StringBuilder();

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    ss.Append(chars[picture[row, col]]);
                }
                ss.Append("\n");
            }

            return ss.ToString();
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

        //private static void debug() { }

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
            VerticalReverse,
            NotSet
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
            private Func<int, int> valueGetter;

            public CellColumnValues(int[] values, Direction direction)
            {
                this._length = values.Length;

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
                return iterable().Sum();
            }
        }

        public class CellSeries
        {
            public CellColumnValues cellColumnValues { get { return _cellColumnValues; } }
            public int Length { get { return _length; } }
            public int this[int i]
            {
                get { return valueGetter(i); }
                set
                {
                    var oldCell = valueGetter(i);
                    if ((oldCell == EMPTY && value == FILLED) || (oldCell == FILLED && value == EMPTY))
                        throw new Exception("Error Detected");

                    if (_correctValueGetter != null && checkCorrect)
                    {
                        var correctCell = _correctValueGetter(i);
                        if (correctCell != value)
                            throw new Exception("Error Detected");
                    }

                    valueSetter(i, value);
                }
            }
            public IEnumerable<int> asIterable { get { return iterable(); } }
            public Direction direction = Direction.NotSet;
            public int? rowOrCol = null;
            public string asString { get { return new string(asIterable.Select(x => Form1.chars[x]).ToArray()); } }
            public string firstTimeString { get; set; }

            private CellColumnValues _cellColumnValues;
            private int _length;
            private Func<int, int> valueGetter;
            private Func<int, int> _correctValueGetter;
            private Action<int, int> valueSetter;

            public CellSeries(int rowOrCol, int[,] picture, Direction direction, int[] columnValues)
            {
                _cellColumnValues = new CellColumnValues(columnValues, direction);
                this.direction = direction;
                this.rowOrCol = rowOrCol;

                switch (direction)
                {
                    case Direction.Horizontal:
                        {
                            int row = rowOrCol;
                            valueGetter = col => picture[row, col];
                            _correctValueGetter = col => correct[row, col];
                            valueSetter = (col, cell) => picture[row, col] = cell;
                            _length = colCount;
                        }
                        break;
                    case Direction.Vertical:
                        {
                            int col = rowOrCol;
                            valueGetter = row => picture[row, col];
                            _correctValueGetter = row => correct[row, col];
                            valueSetter = (row, cell) => picture[row, col] = cell;
                            _length = rowCount;
                        }
                        break;
                    case Direction.HorizontalReverse:
                        {
                            int row = rowOrCol;
                            valueGetter = col => picture[row, lastCol - col];
                            _correctValueGetter = col => correct[row, lastCol - col];
                            valueSetter = (col, cell) => picture[row, lastCol - col] = cell;
                            _length = colCount;
                        }
                        break;
                    case Direction.VerticalReverse:
                        {
                            int col = rowOrCol;
                            valueGetter = row => picture[lastRow - row, col];
                            _correctValueGetter = row => correct[lastRow - row, col];
                            valueSetter = (row, cell) => picture[lastRow - row, col] = cell;
                            _length = rowCount;
                        }
                        break;
                    default:
                        break;
                }

                this.firstTimeString = this.asString;
            }

            private CellSeries(CellColumnValues _cellColumnValues, int _length, Func<int, int> valueGetter, Action<int, int> valueSetter)
            {
                this._cellColumnValues = _cellColumnValues;
                this._length = _length;
                this.valueGetter = valueGetter;
                this.valueSetter = valueSetter;

                this.firstTimeString = this.asString;
            }

            public static CellSeries Slice(CellSeries cellSeries, int startIndex, int endIndex, int[] newValues)
            {
                int size = endIndex - startIndex + 1;
                Func<int, int> getter = x => cellSeries[startIndex + x];
                Action<int, int> setter = (x, cell) => cellSeries[startIndex + x] = cell;
                CellColumnValues cellColumnValues = new CellColumnValues(newValues, Direction.Horizontal);

                if (size < newValues.Sum() + newValues.Length - 1)
                    throw new Exception("Bre insafsız!");

                if (size < 0)
                    size = 0;

                return new CellSeries(cellColumnValues, size, getter, setter);
            }

            public IEnumerable<int> iterable()
            {
                for (int i = 0; i < _length; i++)
                    yield return valueGetter(i);
            }

            public static CellSeries Reverse(CellSeries old)
            {
                int[] newValues = old.cellColumnValues.asIterable.Reverse().ToArray();

                int cellLength = old.Length;
                int lastCellIndex = old.Length - 1;
                Func<int, int> getter = x => old[lastCellIndex - x];
                Action<int, int> setter = (x, cell) => old[lastCellIndex - x] = cell;
                CellColumnValues cellColumnValues = new CellColumnValues(newValues, Direction.Horizontal);

                CellSeries cells = new CellSeries(cellColumnValues, cellLength, getter, setter);

                var oldvals = old.cellColumnValues.asIterable;
                var newvals = cells.cellColumnValues.asIterable;

                if (!Enumerable.SequenceEqual(oldvals, newvals.Reverse()))
                    throw new Exception("Error at reversing");

                return cells;
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
