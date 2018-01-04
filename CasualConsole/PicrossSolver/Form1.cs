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

#if DEBUG
        public const bool isDebug = true; 
#endif

#if !DEBUG
        public const bool isDebug = false;
#endif

        public const int windowLeft = 16;
        public const int windowUp = 38;

        public const string chars = " ■x";
        public const byte UNKNOWN = 0;
        public const byte FILLED = 1;
        public const byte EMPTY = 2;

        public const int displaySize = 20;

        public static int iteration = 0;
        static byte[,] pictureRef = null;

        public static bool[] isRowCompleted;
        public static bool[] isColCompleted;

        static byte[,] correct = null;

        static bool correctExists;

        private static DateTime programStartTime;

        enum Mode
        {
            Development,
            GUI
        }

        Mode mode = Mode.Development;

        public Form1()
        {
            string puzzlesHavingSolution = @"C:\Users\Xhertas\Documents\Visual Studio 2015\Projects\CasualConsole\PicrossSolver\Puzzles\has_solution\";
            string puzzlesNotHavingSolution = @"C:\Users\Xhertas\Documents\Visual Studio 2015\Projects\CasualConsole\PicrossSolver\Puzzles\has_no_solution\";

            if (mode == Mode.Development)
            {
#if DEBUG
                Generic.TestMatchingByDivided();
                Generic.TestMatchingByFilled();

                Console.WriteLine("All test are OK!!!");
                Console.WriteLine();
#endif
                programStartTime = DateTime.Now;

                SolveHavingSolution(puzzlesHavingSolution);
                SolveHavingSolution(puzzlesNotHavingSolution);

                TimeSpan timeDiff = DateTime.Now - programStartTime;

                Console.WriteLine("Time it took: {0}", timeDiff);
                Console.ReadKey();
            }
            else if (mode == Mode.GUI)
            {
                string puzzleLocation = puzzlesNotHavingSolution;

                string[] allpuzzles = Directory.GetFiles(puzzleLocation);

                string puzzlePath = allpuzzles.First();
                string fileName = new FileInfo(puzzlePath).Name;

                PuzzleJson puzzle = JsonConvert.DeserializeObject<PuzzleJson>(File.ReadAllText(puzzlePath));

                var leftColumn = puzzle.LeftColumn;
                var upColumn = puzzle.UpColumn;

                byte[,] picture = new byte[leftColumn.Length, upColumn.Length];

                MyWindow solverWindow = new MyWindow(picture, fileName, leftColumn, upColumn);

                solverWindow.Show();
                solverWindow.Invalidate();

                Application.Run(solverWindow);
            }
        }

        private static void SolveHavingSolution(string puzzleLocation)
        {
            string[] allpuzzles = Directory.GetFiles(puzzleLocation);

            for (int i = 0; i < allpuzzles.Length; i++)
            {
                string puzzlePath = allpuzzles[i];
                string fileName = new FileInfo(puzzlePath).Name;
                PuzzleJson puzzle = JsonConvert.DeserializeObject<PuzzleJson>(File.ReadAllText(puzzlePath));
                Console.WriteLine("solving: " + fileName);

                //FixJsonFormat(puzzlePath, puzzle);

                var leftColumn = puzzle.LeftColumn;
                var upColumn = puzzle.UpColumn;
                correct = puzzle.Correct;
                int rowCount = leftColumn.Length;
                int colCount = upColumn.Length;
                correctExists = correct.Length > 0;

                isRowCompleted = new bool[rowCount];
                isColCompleted = new bool[colCount];

                int leftSum = leftColumn.Sum(x => x.Sum());
                int upSum = upColumn.Sum(x => x.Sum());

                if (leftSum != upSum)
                    throw new Exception("Numbers are entered wrong!");

                bool isSolved;
                var solvedPicture = solveAndDisplay(upColumn, leftColumn, out isSolved);

                if (correctExists)
                {
                    var allPuzzleBytes = array2dAsEnumerable(puzzle.Correct).SelectMany(x => x);

                    if (allPuzzleBytes.Count(x => x == Form1.FILLED) != leftSum)
                        throw new Exception("Solution are entered wrong!");
                }

                if (!isSolved && correctExists)
                {
                    display(correct, "This is how it should be", leftColumn, upColumn, true);
                }

                if (isSolved && !correctExists)
                {
                    display(solvedPicture, "I solved it!", leftColumn, upColumn, true);
                    string solvedCase = ToJson2D(solvedPicture);
                }
            }
        }

        private static void FixJsonFormat(string puzzlePath, PuzzleJson puzzle)
        {
            string correctToJson = ToJson2D(puzzle.Correct);
            string upToJson = ToJson2D(puzzle.UpColumn);
            string leftToJson = ToJson2D(puzzle.LeftColumn);

            string newLine = "\n";

            string wholeJson = "{" + newLine + "\"Correct\"" + ":" + correctToJson + "," + newLine + "\"LeftColumn\"" + ":" + leftToJson + "," + newLine + "\"UpColumn\"" + ":" + upToJson + newLine + "}";

            File.WriteAllText(puzzlePath, wholeJson);
        }

        private static string ToJson2D(byte[,] arr)
        {
            return ToJson2D(array2dAsEnumerable(arr));
        }

        private static string ToJson2D<T>(IEnumerable<IEnumerable<T>> arr) where T : struct
        {
            if (!arr.Any())
                return "[]";

            return "[\n" + string.Join(",\n", arr.Select(x => "[" + string.Join(",", x) + "]")) + "\n]";
        }

        private static byte[,] solveAndDisplay(int[][] upColumn, int[][] leftColumn, out bool isSolved)
        {
            byte[,] picture = new byte[leftColumn.Length, upColumn.Length];

            isSolved = solve(picture, upColumn, leftColumn);

            string joined = string.Join(",\n", array2dAsEnumerable(picture).Select(x => string.Join(",", x)));

            if (!isSolved)
            {
                TimeSpan timeDiff = DateTime.Now - programStartTime;

                Console.WriteLine("Time it took: {0}", timeDiff);

                display(picture, "I could only solve this much", leftColumn, upColumn, true);
            }

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

        private static bool solve(byte[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            dumpPicture(picture);

            ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.InitialProcessing);

            for (iteration = 1; ; iteration++)
            {
                Console.WriteLine("Running iteration: " + iteration);

                bool isChangeDetected = false;

                // tek sayı olanların ara boşluğunu doldurup, ulaşamayacağı yerlere çarpı atıyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessSingles);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                {
                    // seri başından ve sonundan itibaren bir tarafı kapalı sayıların kalanını ayarlayıp çarpı atıyor
                    ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessStart);
                    isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                    // seri başlarındaki ve sonlarındaki küçük boşluklara çarpı atıyor, BU METOD processStartsAndEnds METODUNDAN HEMEN SONRA ÇALIŞMALI!!!
                    ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessStartingUnknowns);
                    isChangeDetected |= testPicture(picture, leftColumn, upColumn);
                }

                // serilerdeki en büyük değerler dolduysa başına ve sonuna çarpı atıyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessSetEmptiesByMax);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // serilerdeki çarpı arası boşluklara çarpı atıyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessFillBetweenEmpties);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // serideki outlier olan değere karşılık gelen dolmuşları işliyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessByMaxValues);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // seri başlarında ve sonlarında kendini bulmaya çalışıyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.TryMatchingFirstValue);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // serileri genel olarak analiz ediyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessMatching);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // serideki dolulara bakarak eşleştirip initial processing yaptırıyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessInitialByMatchingFilled);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // Özel ve çok nadir durumları işliyor
                ApplyAlgorithmBackAndForth(picture, upColumn, leftColumn, Generic.ProcessSpecialCases);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // serideki çarpılarla ayrılmış kısımları bulup işliyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessByDividedAreas);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                // serilerdeki dolu grup sayısı değer sayısını geçtiğinde bakıyor
                ApplyAlgorithmOneWay(picture, upColumn, leftColumn, Generic.ProcessByFilledRanges);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                if (!isChangeDetected)
                {
                    if (!isChangeDetected)
                    {
                        break;
                    }
                }

                // serideki dolu ve boş sayılarını kontrol ediyor
                processCheckAllCounts(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture, leftColumn, upColumn);

                //display(picture, "test", true);
            }

            Console.WriteLine("There was no change in the iteration: " + iteration);

            bool isSolvedCompletely = isRowCompleted.All(x => x == true) || isColCompleted.All(x => x == true);

            return isSolvedCompletely;
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

        private static bool testPicture(byte[,] picture, int[][] leftColumn, int[][] upColumn)
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

            if (correctExists)
                for (int i = 0; i < picture.rowCount(); i++)
                    for (int j = 0; j < picture.colCount(); j++)
                        if (picture[i, j] != UNKNOWN && picture[i, j] != correct[i, j])
                        {
                            int asIs = picture[i, j];
                            int correctOne = correct[i, j];
                            Console.WriteLine("Hata tespit edildi, iteration: " + iteration);
                            display(pictureRef, "Hatasız olan", leftColumn, upColumn);
                            display(picture, "Hatalı olan", leftColumn, upColumn);
                            display(correct, "Olması gereken", leftColumn, upColumn, true);
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

        private static int getSum(int[] values)
        {
            int sum = 0;

            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum;
        }

        private static void display(byte[,] picture, int[][] leftColumn, int[][] upColumn)
        {
            display(picture, "Latest", leftColumn, upColumn);
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

        private static void display(byte[,] picture, string title, int[][] leftColumn, int[][] upColumn, bool isApplication = false)
        {
            dumpPicture(picture);

            var w = new MyWindow(picture, title, leftColumn, upColumn);
            w.Show();
            w.Invalidate();

            if (isApplication)
                Application.Run(w);

            programStartTime = DateTime.Now;
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
#if DEBUG
                    byte oldCell = valueGetter(i);
                    if ((oldCell == EMPTY && value == FILLED) || (oldCell == FILLED && value == EMPTY))
                        throw new Exception("Error Detected");

                    if (_correctValueGetter != null && correctExists)
                    {
                        var correctCell = _correctValueGetter(i);
                        if (correctCell != value)
                            throw new Exception("Error Detected");
                    }
#endif
                    valueSetter(i, value);
                }
            }
            public IEnumerable<byte> asIterable { get { return iterable(); } }
            public Direction direction = Direction.NotSet;
            public int? rowOrCol = null;
            public string asString { get { return new string(asIterable.Select(x => chars[x]).ToArray()); } }

#if DEBUG
            public string firstTimeString { get; set; }
#endif

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

#if DEBUG
                this.firstTimeString = this.asString;
#endif
            }

            private CellSeries(CellColumnValues _cellColumnValues, int _length, Func<int, byte> valueGetter, Action<int, byte> valueSetter)
            {
                this._cellColumnValues = _cellColumnValues;
                this._length = _length;
                this.valueGetter = valueGetter;
                this.valueSetter = valueSetter;

#if DEBUG
                this.firstTimeString = this.asString;
#endif
            }

            public static CellSeries Slice(CellSeries old, int startIndex, int endIndex, int[] newValues)
            {
                int size = endIndex - startIndex + 1;
                Func<int, byte> getter = x => old[startIndex + x];
                Action<int, byte> setter = (x, cell) => old[startIndex + x] = cell;
                CellColumnValues cellColumnValues = new CellColumnValues(newValues, Direction.Horizontal);

                if (size < 0)
                    size = 0;
#if DEBUG
                if (size < newValues.Sum() + newValues.Length - 1)
                    throw new Exception("Bre insafsız!");
#endif
                return new CellSeries(cellColumnValues, size, getter, setter);
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
#if DEBUG
                if (!Enumerable.SequenceEqual(oldvals, newvals.Reverse()))
                    throw new Exception("Error at reversing");
#endif
                return cells;
            }

            public CellSeries(byte[] bytes, int[] values)
            {
                _length = bytes.Length;
                valueGetter = x => bytes[x];
                valueSetter = (x, b) => bytes[x] = b;
                _cellColumnValues = new CellColumnValues(values, Direction.Horizontal);

#if DEBUG
                this.firstTimeString = this.asString;
#endif
            }

            public IEnumerable<byte> iterable()
            {
                for (int i = 0; i < _length; i++)
                    yield return valueGetter(i);
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
        int[][] leftColumn;
        int[][] upColumn;
        private int leftMarginCount;
        private int upMarginCount;
        private int leftMarginSize;
        private int upMarginSize;

        private bool isCheckEnabled = false;
        private bool isKeyDown = false;

        private Color[] leftColumnColors;
        private Color[] upColumnColors;
        private List<RadioButton> radioButtons = new List<RadioButton>();

        private const string FILLED = "FILLED";
        private const string EMPTY = "EMPTY";

        public MyWindow(byte[,] picture, string title, int[][] leftColumn, int[][] upColumn)
        {
            ConstructorCall(picture, title, leftColumn, upColumn);
        }

        private void ConstructorCall(byte[,] picture, string title, int[][] leftColumn, int[][] upColumn)
        {
            this.picture = picture;
            this.leftColumn = leftColumn;
            this.upColumn = upColumn;
            this.Name = title;
            this.Text = title;

            this.leftMarginCount = GetMarginCount(leftColumn);
            this.upMarginCount = GetMarginCount(upColumn);

            this.leftMarginSize = leftMarginCount * Form1.displaySize;
            this.upMarginSize = upMarginCount * Form1.displaySize;

            this.Size = new Size(Form1.windowLeft + Form1.displaySize * picture.colCount() + leftMarginSize, Form1.windowUp + Form1.displaySize * picture.rowCount() + upMarginSize);
            this.SetDesktopLocation(500, 300);

            this.Paint += new PaintEventHandler(this.GameFrame_Paint);
            this.KeyPreview = true;

            if (leftColumn != null || upColumn != null)
                isCheckEnabled = true;

            if (isCheckEnabled)
            {
                this.Click += new EventHandler(this.MyWindow_Click);
                this.KeyDown += new KeyEventHandler(this.MyWindow_KeyDown);
                this.KeyUp += new KeyEventHandler(this.MyWindow_KeyUp);

                this.Controls.Add(AddRadioButton(FILLED, 0));
                this.Controls.Add(AddRadioButton(EMPTY, 20));
            }

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            FixFlicker();

            if (isCheckEnabled)
            {
                leftColumnColors = GetBlackColors(leftColumn.Length);
                upColumnColors = GetBlackColors(upColumn.Length);
            }

            CheckAllRowsAndColumns();
        }

        private void FixFlicker()
        {
            this.SetStyle(ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        private void MyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !isKeyDown)
            {
                radioButtons.First(x => x.Name == EMPTY).Checked = true;

                isKeyDown = true;

                this.Refresh();
            }
        }

        private void MyWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                radioButtons.First(x => x.Name == FILLED).Checked = true;

                isKeyDown = false;

                this.Refresh();
            }
        }

        private void MyWindow_Click(object sender, EventArgs e)
        {
            MouseEventArgs args = (MouseEventArgs)e;

            int xPixel = args.X;
            int yPixel = args.Y;

            int col = xPixel / Form1.displaySize - leftMarginCount;
            int row = yPixel / Form1.displaySize - upMarginCount;

            if (col >= 0 && row >= 0)
            {
                string checkedButton = radioButtons.Where(x => x.Checked).First().Name;

                if (checkedButton == FILLED)
                {
                    if (picture[row, col] == Form1.FILLED)
                        picture[row, col] = Form1.UNKNOWN;
                    else
                        picture[row, col] = Form1.FILLED;
                }
                else if (checkedButton == EMPTY)
                {
                    if (picture[row, col] == Form1.EMPTY)
                        picture[row, col] = Form1.UNKNOWN;
                    else
                        picture[row, col] = Form1.EMPTY;
                }
                else
                    throw new Exception();

                CheckRow(row);
                CheckCol(col);

                // trigger re-paint
                this.Refresh();
            }
        }

        private Color[] GetBlackColors(int size)
        {
            Color[] colors = new Color[size];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.Black;
            }

            return colors;
        }

        private void CheckRow(int row)
        {
            if (isCheckEnabled)
            {
                byte[] originalValues = new Form1.CellSeries(row, picture, Form1.Direction.Horizontal, leftColumn[row]).asIterable.ToArray();

                byte[] copyValues = originalValues.ToArray();

                var cells = new Form1.CellSeries(copyValues, leftColumn[row]);

                bool hasError = false;

                try
                {
                    Generic.ProcessAllAlgorithms(cells);
                    Generic.ProcessAllAlgorithms(Form1.CellSeries.Reverse(cells));
                }
                catch (Exception)
                {
                    hasError = true;
                }

                if (hasError)
                {
                    leftColumnColors[row] = Color.Red;
                }
                else if (Enumerable.SequenceEqual(originalValues, copyValues))
                {
                    leftColumnColors[row] = Color.Black;
                }
                else
                {
                    leftColumnColors[row] = Color.Blue;
                }
            }
        }

        private void CheckCol(int col)
        {
            if (isCheckEnabled)
            {
                byte[] originalValues = new Form1.CellSeries(col, picture, Form1.Direction.Vertical, upColumn[col]).asIterable.ToArray();

                byte[] copyValues = originalValues.ToArray();

                var cells = new Form1.CellSeries(copyValues, upColumn[col]);

                bool hasError = false;

                try
                {
                    Generic.ProcessAllAlgorithms(cells);
                    Generic.ProcessAllAlgorithms(Form1.CellSeries.Reverse(cells));
                }
                catch (Exception)
                {
                    hasError = true;
                }

                if (hasError)
                {
                    upColumnColors[col] = Color.Red;
                }
                else if (Enumerable.SequenceEqual(originalValues, copyValues))
                {
                    upColumnColors[col] = Color.Black;
                }
                else
                {
                    upColumnColors[col] = Color.Blue;
                }
            }
        }

        private void CheckAllRowsAndColumns()
        {
            if (isCheckEnabled)
            {
                for (int i = 0; i < leftColumn.Length; i++)
                {
                    CheckRow(i);
                }

                for (int i = 0; i < upColumn.Length; i++)
                {
                    CheckCol(i);
                }
            }
        }

        private RadioButton AddRadioButton(string text, int y)
        {
            RadioButton radioButton1 = new RadioButton();
            radioButton1.Location = new Point(10, y + 5);
            radioButton1.Name = text;
            radioButton1.Size = new Size(85, 17);
            radioButton1.TabIndex = 0;
            radioButton1.TabStop = true;
            radioButton1.Text = text;
            radioButton1.UseVisualStyleBackColor = true;

            radioButtons.Add(radioButton1);

            return radioButton1;
        }

        private static int GetMarginCount(int[][] columns)
        {
            if (columns != null)
                return columns.Select(x => x.Length).Max();
            else
                return 0;
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
                        g2d.FillRectangle(Brushes.White, col * Form1.displaySize + leftMarginSize, row * Form1.displaySize + upMarginSize, Form1.displaySize, Form1.displaySize);
                    }
                    else if (value == Form1.FILLED)
                    {
                        g2d.FillRectangle(Brushes.Black, col * Form1.displaySize + leftMarginSize, row * Form1.displaySize + upMarginSize, Form1.displaySize, Form1.displaySize);
                    }
                    else if (value == Form1.EMPTY)
                    {
                        Pen pen = new Pen(Color.DarkGray);

                        int xOffset = col * Form1.displaySize + leftMarginSize;
                        int yOffset = row * Form1.displaySize + upMarginSize;
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
                    // Drawing vertical line
                    g2d.DrawLine(new Pen(Brushes.Orange), (col + 1) * Form1.displaySize + leftMarginSize, 0 + upMarginSize, (col + 1) * Form1.displaySize + leftMarginSize, rowCount * Form1.displaySize + upMarginSize);

                    if (col % 5 == 4)
                    {
                        g2d.DrawLine(new Pen(Brushes.Orange), (col + 1) * Form1.displaySize + leftMarginSize + 1, 0 + upMarginSize, (col + 1) * Form1.displaySize + leftMarginSize + 1, rowCount * Form1.displaySize + upMarginSize);
                    }
                }

                // Drawing horizontal line
                g2d.DrawLine(new Pen(Brushes.Orange), 0 + leftMarginSize, (row + 1) * Form1.displaySize + upMarginSize, colCount * Form1.displaySize + leftMarginSize, (row + 1) * Form1.displaySize + upMarginSize);

                if (row % 5 == 4)
                {
                    g2d.DrawLine(new Pen(Brushes.Orange), 0 + leftMarginSize, (row + 1) * Form1.displaySize + upMarginSize + 1, colCount * Form1.displaySize + leftMarginSize, (row + 1) * Form1.displaySize + upMarginSize + 1);
                }
            }

            if (isCheckEnabled)
            {
                // Drawing vertical line
                g2d.DrawLine(new Pen(Brushes.Orange), (0) * Form1.displaySize + leftMarginSize, 0 + upMarginSize, (0) * Form1.displaySize + leftMarginSize, rowCount * Form1.displaySize + upMarginSize);
                g2d.DrawLine(new Pen(Brushes.Orange), (0) * Form1.displaySize + leftMarginSize + 1, 0 + upMarginSize, (0) * Form1.displaySize + leftMarginSize + 1, rowCount * Form1.displaySize + upMarginSize);

                // Drawing horizontal line
                g2d.DrawLine(new Pen(Brushes.Orange), 0 + leftMarginSize, (0) * Form1.displaySize + upMarginSize, colCount * Form1.displaySize + leftMarginSize, (0) * Form1.displaySize + upMarginSize);
                g2d.DrawLine(new Pen(Brushes.Orange), 0 + leftMarginSize, (0) * Form1.displaySize + upMarginSize + 1, colCount * Form1.displaySize + leftMarginSize, (0) * Form1.displaySize + upMarginSize + 1);
            }

            if (leftColumn != null)
            {
                for (int i = 0; i < leftColumn.Length; i++)
                {
                    var leftColumnRow = leftColumn[i];

                    int marginCount = leftMarginCount - leftColumnRow.Length;

                    for (int y = 0; y < leftColumnRow.Length; y++)
                    {
                        Rectangle location = new Rectangle((y + marginCount) * Form1.displaySize, (upMarginCount + i) * Form1.displaySize, Form1.displaySize, Form1.displaySize);
                        int val = leftColumnRow[y];
                        DrawString(g2d, val.ToString(), location, leftColumnColors[i]);
                    }
                }
            }

            if (upColumn != null)
            {
                for (int i = 0; i < upColumn.Length; i++)
                {
                    var upColumnRow = upColumn[i];

                    int marginCount = upMarginCount - upColumnRow.Length;

                    for (int y = 0; y < upColumnRow.Length; y++)
                    {
                        Rectangle location = new Rectangle((leftMarginCount + i) * Form1.displaySize, (y + marginCount) * Form1.displaySize, Form1.displaySize, Form1.displaySize);
                        int val = upColumnRow[y];
                        DrawString(g2d, val.ToString(), location, upColumnColors[i]);
                    }
                }
            }
        }

        private static void DrawString(Graphics g2d, string text, Rectangle location, Color color)
        {
            Brush brush = new SolidBrush(color);
            Font font = new Font("Consolas", 10, FontStyle.Bold);
            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            g2d.DrawString(text, font, brush, location, sf);
        }
    }
}
