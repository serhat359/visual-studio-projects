using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PicrossSolver
{
    public class Form1
    {
        public delegate void Algorithm(CellSeries s);

        public const string chars = " ■.";
        public const byte UNKNOWN = 0;
        public const byte FILLED = 1;
        public const byte EMPTY = 2;

        public const int displaySize = 20;

        public static int iteration = 0;
        static byte[,] pictureRef = null;

        public static bool[] isRowCompleted;
        public static bool[] isColCompleted;

        static byte[,] correct = null;

        static bool checkCorrect;

        public Form1()
        {
            string puzzlesHavingSolution = @"C:\Users\Xhertas\Documents\Visual Studio 2015\Projects\CasualConsole\PicrossSolver\Puzzles\has_solution\";

            SolveHavingSolution(puzzlesHavingSolution);

            string puzzlesNotHavingSolution = @"C:\Users\Xhertas\Documents\Visual Studio 2015\Projects\CasualConsole\PicrossSolver\Puzzles\has_no_solution\";

            SolveHavingSolution(puzzlesNotHavingSolution);
        }

        private static void SolveHavingSolution(string puzzleLocation)
        {
            string[] allpuzzles = Directory.GetFiles(puzzleLocation);

            for (int i = 0; i < allpuzzles.Length; i++)
            {
                string puzzleName = allpuzzles[i];
                string fileName = new FileInfo(puzzleName).Name;
                PuzzleJson puzzle = JsonConvert.DeserializeObject<PuzzleJson>(File.ReadAllText(puzzleName));
                Console.WriteLine("solving: " + fileName);

                var leftColumn = puzzle.LeftColumn;
                var upColumn = puzzle.UpColumn;
                correct = puzzle.Correct;
                int rowCount = leftColumn.Length;
                int colCount = upColumn.Length;
                checkCorrect = correct.Length > 0;

                isRowCompleted = new bool[rowCount];
                isColCompleted = new bool[colCount];

                int leftSum = leftColumn.Sum(x => x.Sum());
                int upSum = upColumn.Sum(x => x.Sum());

                if (leftSum != upSum)
                    throw new Exception("Numbers are entered wrong!");

                var solvedPicture = solveAndDisplay(upColumn, leftColumn);

                if (checkCorrect)
                    display(correct, "This is how it should be", true);
                else
                {
                    string solvedCase = JsonConvert.SerializeObject(solvedPicture);
                }
            }
        }

        private static byte[,] solveAndDisplay(int[][] upColumn, int[][] leftColumn)
        {
            byte[,] picture = new byte[leftColumn.Length, upColumn.Length];

            solve(picture, upColumn, leftColumn);

            string joined = string.Join(",\n", array2dAsEnumerable(picture).Select(x => string.Join(",", x)));

            display(picture, "This is the solved one", true);

            return picture;
        }

        private static IEnumerable<IEnumerable<byte>> array2dAsEnumerable(byte[,] picture)
        {
            for (int row = 0; row < picture.rowCount(); row++)
            {
                IEnumerable<byte> rowList = Enumerable.Range(0, picture.colCount()).Select(col => picture[row, col]);

                yield return rowList;
            }
        }

        private static void solve(byte[,] picture, int[][] upColumn, int[][] leftColumn)
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

                // serilerdeki çarpı arası boşluklara çarpı atıyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessFillBetweenEmpties);
                isChangeDetected |= testPicture(picture);

                // serideki outlier olan değere karşılık gelen dolmuşları işliyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessMaxValues);
                isChangeDetected |= testPicture(picture);

                // serideki çarpılarla ayrılmış kısımları bulup işliyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessDividedParts);
                isChangeDetected |= testPicture(picture);

                // seri başlarında ve sonlarında kendini bulmaya çalışıyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessTryFindingMatchStartingAndEnding);
                isChangeDetected |= testPicture(picture);

                // serileri genel olarak analiz ediyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessMatching);
                isChangeDetected |= testPicture(picture);

                // serideki dolulara bakarak eşleştirip initial processing yaptırıyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessInitialByMatching);
                isChangeDetected |= testPicture(picture);

                // seriyi önü arkası kapalı dolu gruplara göre ayırıp işliyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.DivideByEnclosed);
                isChangeDetected |= testPicture(picture);

                // Özel ve çok nadir durumları işliyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessSpecialCases);
                isChangeDetected |= testPicture(picture);

                if (!isChangeDetected)
                {
                    // serilerdeki dolu grup sayısı değer sayısını geçtiğinde bakıyor
                    ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.FillBetweenFilled);
                    isChangeDetected |= testPicture(picture);

                    if (!isChangeDetected)
                    {
                        break;
                    }
                }

                // serideki dolu ve boş sayılarını kontrol ediyor
                processCheckAllCounts(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);
            }

            Console.WriteLine("There was no change in the iteration: " + iteration);
        }

        public static void ApplyAlgorithmBackAndForth(byte[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing)
        {
            ApplyAlgorithm(picture, upColumn, leftColumn, processing, true);
        }

        public static void ApplyAlgorithmOneWay(byte[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing)
        {
            ApplyAlgorithm(picture, upColumn, leftColumn, processing, false);
        }

        private static void ApplyAlgorithm(byte[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing, bool isTwoWay)
        {
            for (int row = 0; row < picture.rowCount(); row++)
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

            for (int col = 0; col < picture.colCount(); col++)
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

        private static void processCheckAllCounts(byte[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            for (int col = 0; col < picture.colCount(); col++)
            {
                int[] values = upColumn[col];

                // TODO bunun için tablo oluştur
                int supposedFilledCount = getSum(values);
                int supposedEmptyCount = picture.rowCount() - supposedFilledCount;

                int actualFilledCount = 0;
                int actualEmptyCount = 0;

                for (int i = 0; i < picture.rowCount(); i++)
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
                    for (int i = 0; i < picture.rowCount(); i++)
                        if (picture[i, col] == UNKNOWN)
                            picture[i, col] = EMPTY;

                    isColCompleted[col] = true;
                }
                else if (supposedEmptyCount == actualEmptyCount)
                {
                    for (int i = 0; i < picture.rowCount(); i++)
                        if (picture[i, col] == UNKNOWN)
                            picture[i, col] = FILLED;

                    isColCompleted[col] = true;
                }
            }

            for (int row = 0; row < picture.rowCount(); row++)
            {
                int[] values = leftColumn[row];

                // TODO bunun için tablo oluştur
                int supposedFilledCount = getSum(values);
                int supposedEmptyCount = picture.colCount() - supposedFilledCount;

                int actualFilledCount = 0;
                int actualEmptyCount = 0;

                for (int i = 0; i < picture.colCount(); i++)
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
                    for (int i = 0; i < picture.colCount(); i++)
                        if (picture[row, i] == UNKNOWN)
                            picture[row, i] = EMPTY;

                    isRowCompleted[row] = true;
                }
                else if (supposedEmptyCount == actualEmptyCount)
                {
                    for (int i = 0; i < picture.colCount(); i++)
                        if (picture[row, i] == UNKNOWN)
                            picture[row, i] = FILLED;

                    isRowCompleted[row] = true;
                }
            }
        }

        private static bool testPicture(byte[,] picture)
        {
            bool isChangeDetected = false;

            for (int i = 0; i < picture.rowCount(); i++)
            {
                for (int j = 0; j < picture.colCount(); j++)
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
                for (int i = 0; i < picture.rowCount(); i++)
                    for (int j = 0; j < picture.colCount(); j++)
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

        private static byte[,] dumpPicture(byte[,] picture)
        {
            pictureRef = new byte[picture.rowCount(), picture.colCount()];

            for (int i = 0; i < picture.rowCount(); i++)
                for (int j = 0; j < picture.colCount(); j++)
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

        private static void display(byte[,] picture)
        {
            display(picture, "Latest");
        }

        private static string pictureToString(byte[,] picture)
        {
            StringBuilder ss = new StringBuilder();

            for (int row = 0; row < picture.rowCount(); row++)
            {
                for (int col = 0; col < picture.colCount(); col++)
                {
                    ss.Append(chars[picture[row, col]]);
                }
                ss.Append("\n");
            }

            return ss.ToString();
        }

        private static void display(byte[,] picture, string title, bool isApplication = false)
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
            public byte this[int i]
            {
                get { return valueGetter(i); }
                set
                {
                    byte oldCell = valueGetter(i);
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
            private Func<int, byte> valueGetter;
            private Func<int, byte> _correctValueGetter;
            private Action<int, byte> valueSetter;

            public CellSeries(int rowOrCol, byte[,] picture, Direction direction, int[] columnValues)
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
                            _length = picture.colCount();
                        }
                        break;
                    case Direction.Vertical:
                        {
                            int col = rowOrCol;
                            valueGetter = row => picture[row, col];
                            _correctValueGetter = row => correct[row, col];
                            valueSetter = (row, cell) => picture[row, col] = cell;
                            _length = picture.rowCount();
                        }
                        break;
                    case Direction.HorizontalReverse:
                        {
                            int row = rowOrCol;
                            int lastCol = picture.colCount() - 1;
                            valueGetter = col => picture[row, lastCol - col];
                            _correctValueGetter = col => correct[row, lastCol - col];
                            valueSetter = (col, cell) => picture[row, lastCol - col] = cell;
                            _length = picture.colCount();
                        }
                        break;
                    case Direction.VerticalReverse:
                        {
                            int col = rowOrCol;
                            int lastRow = picture.rowCount() - 1;
                            valueGetter = row => picture[lastRow - row, col];
                            _correctValueGetter = row => correct[lastRow - row, col];
                            valueSetter = (row, cell) => picture[lastRow - row, col] = cell;
                            _length = picture.rowCount();
                        }
                        break;
                    default:
                        break;
                }

                this.firstTimeString = this.asString;
            }

            private CellSeries(CellColumnValues _cellColumnValues, int _length, Func<int, byte> valueGetter, Action<int, byte> valueSetter)
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
                Func<int, byte> getter = x => cellSeries[startIndex + x];
                Action<int, byte> setter = (x, cell) => cellSeries[startIndex + x] = cell;
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
                Func<int, byte> getter = x => old[lastCellIndex - x];
                Action<int, byte> setter = (x, cell) => old[lastCellIndex - x] = cell;
                CellColumnValues cellColumnValues = new CellColumnValues(newValues, Direction.Horizontal);

                CellSeries cells = new CellSeries(cellColumnValues, cellLength, getter, setter);

                var oldvals = old.cellColumnValues.asIterable;
                var newvals = cells.cellColumnValues.asIterable;

                if (!Enumerable.SequenceEqual(oldvals, newvals.Reverse()))
                    throw new Exception("Error at reversing");

                return cells;
            }

            public bool SafeCheck(int index, Func<byte, bool> checker)
            {
                return index >= 0 && index < this.Length && checker(this[index]);
            }

            public void SafeSet(int index, byte value)
            {
                if (index >= 0 && index < this.Length)
                    this[index] = value;
            }
        }

        public class PuzzleJson
        {
            public byte[,] Correct { get; set; }
            public int[][] LeftColumn { get; set; }
            public int[][] UpColumn { get; set; }
        }
    }

    public class MyWindow : Form
    {
        byte[,] picture;

        public MyWindow(byte[,] picture, string title)
        {
            this.picture = picture;
            this.Name = title;
            this.Text = title;

            this.Size = new Size(16 + Form1.displaySize * picture.colCount(), 38 + Form1.displaySize * picture.rowCount());
            this.SetDesktopLocation(500, 300);

            this.Paint += new PaintEventHandler(this.GameFrame_Paint);
        }

        private void GameFrame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g2d = e.Graphics;

            int quarter = Form1.displaySize / 4;
            int threeQuarter = quarter + 2 * quarter;

            int rowCount = picture.rowCount();
            int colCount = picture.colCount();

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
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

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    g2d.DrawLine(new Pen(Brushes.Orange), (col + 1) * Form1.displaySize, 0, (col + 1) * Form1.displaySize, rowCount * Form1.displaySize);
                }

                g2d.DrawLine(new Pen(Brushes.Orange), 0, (row + 1) * Form1.displaySize, colCount * Form1.displaySize, (row + 1) * Form1.displaySize);
            }
        }
    }
}
