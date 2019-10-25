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

        private static DateTime programStartTime;

        enum Mode
        {
            Development,
            GUI,
            BruteForce
        }

        Mode mode = Mode.GUI;

        public Form1()
        {
            string puzzlesHavingSolution = @"C:\Users\Xhertas\Documents\Visual Studio 2017\Projects\CasualConsole\PicrossSolver\Puzzles\has_solution\";
            string puzzlesNotHavingSolution = @"C:\Users\Xhertas\Documents\Visual Studio 2017\Projects\CasualConsole\PicrossSolver\Puzzles\has_no_solution\";

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
            else if (mode == Mode.BruteForce)
            {
                Console.WriteLine("No tests required");
                Console.WriteLine();

                programStartTime = DateTime.Now;

                SolveHavingSolutionBrute(puzzlesHavingSolution);
                SolveHavingSolutionBrute(puzzlesNotHavingSolution);

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

        private static void SolveHavingSolutionBrute(string puzzleLocation)
        {
            string[] allpuzzles = Directory.GetFiles(puzzleLocation);

            for (int i = 0; i < allpuzzles.Length; i++)
            {
                string puzzlePath = allpuzzles[i];
                string fileName = new FileInfo(puzzlePath).Name;
                string allText = File.ReadAllText(puzzlePath);

                PuzzleJson puzzleJson = ConvertToPuzzle(allText);
                Console.WriteLine("solving: " + fileName);

                //FixJsonFormat(puzzlePath, puzzle);

                var leftColumn = puzzleJson.LeftColumn;
                var upColumn = puzzleJson.UpColumn;
                var correct = puzzleJson.Correct;
                int rowCount = leftColumn.Length;
                int colCount = upColumn.Length;

                Puzzle puzzle = new Puzzle
                {
                    PuzzleJson = puzzleJson,
                    IsRowCompleted = new bool[rowCount],
                    IsColCompleted = new bool[colCount],
                    Correct = correct.Length > 0 ? correct : null,
                    CorrectExists = correct.Length > 0,
                };

                int leftSum = leftColumn.Sum(x => x.Sum());
                int upSum = upColumn.Sum(x => x.Sum());

                if (leftSum != upSum)
                    throw new Exception("Numbers are entered wrong!");

                var solvedPicture = SolveAndDisplayBrute(puzzle, out bool isSolved);

                bool correctExists = puzzle.CorrectExists;

                if (correctExists)
                {
                    var allPuzzleBytes = Array2dAsEnumerable(puzzleJson.Correct).SelectMany(x => x);

                    if (allPuzzleBytes.Count(x => x == Form1.FILLED) != leftSum)
                        throw new Exception("Solution are entered wrong!");
                }

                if (!isSolved && correctExists)
                {
                    Display(correct, "This is how it should be", puzzle, true);
                }

                if (isSolved && !correctExists)
                {
                    Display(solvedPicture, "I solved it!", puzzle, true);
                    string solvedCase = ToJson2D(solvedPicture);
                }
            }
        }

        private static void SolveHavingSolution(string puzzleLocation)
        {
            string[] allpuzzles = Directory.GetFiles(puzzleLocation);

            for (int i = 0; i < allpuzzles.Length; i++)
            {
                string puzzlePath = allpuzzles[i];
                string fileName = new FileInfo(puzzlePath).Name;
                string allText = File.ReadAllText(puzzlePath);

                PuzzleJson puzzleJson = ConvertToPuzzle(allText);
                Console.WriteLine("solving: " + fileName);

                //FixJsonFormat(puzzlePath, puzzle);

                var leftColumn = puzzleJson.LeftColumn;
                var upColumn = puzzleJson.UpColumn;
                var correct = puzzleJson.Correct;
                int rowCount = leftColumn.Length;
                int colCount = upColumn.Length;

                Puzzle puzzle = new Puzzle
                {
                    PuzzleJson = puzzleJson,
                    IsRowCompleted = new bool[rowCount],
                    IsColCompleted = new bool[colCount],
                    Correct = correct.Length > 0 ? correct : null,
                    CorrectExists = correct.Length > 0,
                };

                int leftSum = leftColumn.Sum(x => x.Sum());
                int upSum = upColumn.Sum(x => x.Sum());

                if (leftSum != upSum)
                    throw new Exception("Numbers are entered wrong!");

                var solvedPicture = SolveAndDisplay(puzzle, out bool isSolved);

                bool correctExists = puzzle.CorrectExists;

                if (correctExists)
                {
                    var allPuzzleBytes = Array2dAsEnumerable(puzzleJson.Correct).SelectMany(x => x);

                    if (allPuzzleBytes.Count(x => x == Form1.FILLED) != leftSum)
                        throw new Exception("Solution are entered wrong!");
                }

                if (!isSolved && correctExists)
                {
                    Display(correct, "This is how it should be", puzzle, true);
                }

                if (isSolved && !correctExists)
                {
                    Display(solvedPicture, "I solved it!", puzzle, true);
                    string solvedCase = ToJson2D(solvedPicture);
                }
            }
        }

        private static PuzzleJson ConvertToPuzzle(string allText)
        {
            //JsonConvert.DeserializeObject<PuzzleJson>(allText);
            //return JsonConvert.DeserializeObject<PuzzleJson>(allText);

            PuzzleJson puzzle = new PuzzleJson();

            var colon = allText.IndexOf(':');
            var quote = allText.IndexOf('"', colon);

            string correctPart = allText.Substring(colon + 4, quote - colon - 9);

            var subPuzzles = correctPart.Split(new string[] { "],\n[" }, StringSplitOptions.None);

            int rowLength = subPuzzles.Length;
            int colLength = subPuzzles[0].Split(',').Length;

            byte[,] correct = new byte[rowLength, colLength];

            for (int row = 0; row < rowLength; row++)
            {
                var wholeRow = subPuzzles[row].Split(',');

                for (int col = 0; col < colLength; col++)
                {
                    correct[row, col] = byte.Parse(wholeRow[col]);
                }
            }

            puzzle.Correct = correct;

            var leftColon = allText.IndexOf(':', quote);
            var leftQuote = allText.IndexOf('"', leftColon);
            var leftPart = allText.Substring(leftColon + 4, leftQuote - leftColon - 9);
            var leftSplit = leftPart.Split(new string[] { "],\n[" }, StringSplitOptions.None);
            puzzle.LeftColumn = leftSplit.Select(x => x.Split(',').Select(y => int.Parse(y)).ToArray()).ToArray();

            var rightColon = allText.IndexOf(':', leftQuote);
            var rightQuote = allText.IndexOf('}', rightColon);
            var rightPart = allText.Substring(rightColon + 4, rightQuote - rightColon - 8);
            var rightSplit = rightPart.Split(new string[] { "],\n[" }, StringSplitOptions.None);
            puzzle.UpColumn = rightSplit.Select(x => x.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(y => int.Parse(y)).ToArray()).ToArray();

            return puzzle;
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
            return ToJson2D(Array2dAsEnumerable(arr));
        }

        private static string ToJson2D<T>(IEnumerable<IEnumerable<T>> arr) where T : struct
        {
            if (!arr.Any())
                return "[]";

            return "[\n" + string.Join(",\n", arr.Select(x => "[" + string.Join(",", x) + "]")) + "\n]";
        }

        private static byte[,] SolveAndDisplayBrute(Puzzle puzzle, out bool isSolved)
        {
            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;

            byte[,] picture = new byte[leftColumn.Length, upColumn.Length];
            puzzle.PictureRef = new byte[leftColumn.Length, upColumn.Length];

            isSolved = SolveBrute(picture, puzzle);

            //string joined = string.Join(",\n", Array2dAsEnumerable(picture).Select(x => string.Join(",", x)));

            if (!isSolved)
            {
                TimeSpan timeDiff = DateTime.Now - programStartTime;

                Console.WriteLine("Time it took: {0}", timeDiff);

                Display(picture, "I could only solve this much", puzzle, true);
            }

            return picture;
        }

        private static byte[,] SolveAndDisplay(Puzzle puzzle, out bool isSolved)
        {
            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;

            byte[,] picture = new byte[leftColumn.Length, upColumn.Length];
            puzzle.PictureRef = new byte[leftColumn.Length, upColumn.Length];

            isSolved = Solve(picture, puzzle);

            //string joined = string.Join(",\n", Array2dAsEnumerable(picture).Select(x => string.Join(",", x)));

            if (!isSolved)
            {
                TimeSpan timeDiff = DateTime.Now - programStartTime;

                Console.WriteLine("Time it took: {0}", timeDiff);

                Display(picture, "I could only solve this much", puzzle, true);
            }

            return picture;
        }

        private static IEnumerable<IEnumerable<byte>> Array2dAsEnumerable(byte[,] picture)
        {
            for (int row = 0; row < picture.rowCount(); row++)
            {
                IEnumerable<byte> rowList = Enumerable.Range(0, picture.colCount()).Select(col => picture[row, col]);

                yield return rowList;
            }
        }

        private static bool SolveBrute(byte[,] picture, Puzzle puzzle)
        {
            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;
            var isColCompleted = puzzle.IsColCompleted;
            var isRowCompleted = puzzle.IsRowCompleted;

            int iteration = 0;
            bool isSolvedCompletely;
            while (true)
            {
                for (int row = 0; row < picture.rowCount(); row++)
                {
                    if (!isRowCompleted[row])
                    {
                        var cells = new CellSeries(row, picture, Direction.Horizontal, leftColumn[row], puzzle.Correct, 0, picture.colCount());
                        Generic.SetPossibles(cells);
                        if (cells.AsIterable.All(x => x != 0))
                            isRowCompleted[row] = true;
                    }
                }

                for (int col = 0; col < picture.colCount(); col++)
                {
                    if (!isColCompleted[col])
                    {
                        var cells = new CellSeries(col, picture, Direction.Vertical, upColumn[col], puzzle.Correct, 0, picture.rowCount());
                        Generic.SetPossibles(cells);
                        if (cells.AsIterable.All(x => x != 0))
                            isColCompleted[col] = true;
                    }
                }

                isSolvedCompletely = puzzle.IsRowCompleted.All(x => x == true) || puzzle.IsColCompleted.All(x => x == true);

                if (isSolvedCompletely)
                    break;
                else if (iteration == 500)
                    break;
                else
                {
                    ++iteration;
                    //Console.WriteLine("Trying iteration: " + (++iteration));
                    //Display(picture, "", puzzle, true);
                }
            }

            if (isSolvedCompletely)
                Console.WriteLine($"Took {iteration} iterations");
            return isSolvedCompletely;
        }

        private static bool Solve(byte[,] picture, Puzzle puzzle)
        {
            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;

            DumpPicture(picture, puzzle.PictureRef);

            ApplyAlgorithmOneWay(picture, puzzle, Generic.InitialProcessing);

            for (iteration = 1; ; iteration++)
            {
                //Console.WriteLine("Running iteration: " + iteration);

                bool isChangeDetected = false;

                // tek sayı olanların ara boşluğunu doldurup, ulaşamayacağı yerlere çarpı atıyor
                ApplyAlgorithmOneWay(picture, puzzle, Generic.ProcessSingles);
                isChangeDetected |= TestPicture(picture, puzzle);

                {
                    // seri başından ve sonundan itibaren bir tarafı kapalı sayıların kalanını ayarlayıp çarpı atıyor
                    ApplyAlgorithmBackAndForth(picture, puzzle, Generic.ProcessStart);
                    isChangeDetected |= TestPicture(picture, puzzle);

                    // seri başlarındaki ve sonlarındaki küçük boşluklara çarpı atıyor, BU METOD processStartsAndEnds METODUNDAN HEMEN SONRA ÇALIŞMALI!!!
                    ApplyAlgorithmBackAndForth(picture, puzzle, Generic.ProcessStartingUnknowns);
                    isChangeDetected |= TestPicture(picture, puzzle);
                }

                // serilerdeki en büyük değerler dolduysa başına ve sonuna çarpı atıyor
                ApplyAlgorithmOneWay(picture, puzzle, Generic.ProcessSetEmptiesByMax);
                isChangeDetected |= TestPicture(picture, puzzle);

                // serilerdeki çarpı arası boşluklara çarpı atıyor
                ApplyAlgorithmOneWay(picture, puzzle, Generic.ProcessFillBetweenEmpties);
                isChangeDetected |= TestPicture(picture, puzzle);

                // serideki outlier olan değere karşılık gelen dolmuşları işliyor
                ApplyAlgorithmOneWay(picture, puzzle, Generic.ProcessByMaxValues);
                isChangeDetected |= TestPicture(picture, puzzle);

                // seri başlarında ve sonlarında kendini bulmaya çalışıyor
                ApplyAlgorithmBackAndForth(picture, puzzle, Generic.TryMatchingFirstValue);
                isChangeDetected |= TestPicture(picture, puzzle);

                // serileri genel olarak analiz ediyor
                ApplyAlgorithmBackAndForth(picture, puzzle, Generic.ProcessMatching);
                isChangeDetected |= TestPicture(picture, puzzle);

                // serideki dolulara bakarak eşleştirip initial processing yaptırıyor
                ApplyAlgorithmBackAndForth(picture, puzzle, Generic.ProcessInitialByMatchingFilled);
                isChangeDetected |= TestPicture(picture, puzzle);

                // Özel ve çok nadir durumları işliyor
                ApplyAlgorithmBackAndForth(picture, puzzle, Generic.ProcessSpecialCases);
                isChangeDetected |= TestPicture(picture, puzzle);

                // serideki çarpılarla ayrılmış kısımları bulup işliyor
                ApplyAlgorithmOneWay(picture, puzzle, Generic.ProcessByDividedAreas);
                isChangeDetected |= TestPicture(picture, puzzle);

                // serilerdeki dolu grup sayısı değer sayısını geçtiğinde bakıyor
                ApplyAlgorithmOneWay(picture, puzzle, Generic.ProcessByFilledRanges);
                isChangeDetected |= TestPicture(picture, puzzle);

                if (!isChangeDetected)
                {
                    if (!isChangeDetected)
                    {
                        break;
                    }
                }

                // serideki dolu ve boş sayılarını kontrol ediyor
                ProcessCheckAllCounts(picture, puzzle);
                isChangeDetected |= TestPicture(picture, puzzle);

                //display(picture, "test", true);
            }

            //Console.WriteLine("There was no change in the iteration: " + iteration);

            bool isSolvedCompletely = puzzle.IsRowCompleted.All(x => x == true) || puzzle.IsColCompleted.All(x => x == true);

            return isSolvedCompletely;
        }

        public static void ApplyAlgorithmBackAndForth(byte[,] picture, Puzzle puzzle, Algorithm processing)
        {
            ApplyAlgorithm(picture, puzzle, processing, true);
        }

        public static void ApplyAlgorithmOneWay(byte[,] picture, Puzzle puzzle, Algorithm processing)
        {
            ApplyAlgorithm(picture, puzzle, processing, false);
        }

        private static void ApplyAlgorithm(byte[,] picture, Puzzle puzzle, Algorithm processing, bool isTwoWay)
        {
            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;
            var isColCompleted = puzzle.IsColCompleted;
            var isRowCompleted = puzzle.IsRowCompleted;

            if (!puzzle.isInitialized)
            {
                int rowCount = picture.rowCount();
                puzzle.horizontals = new CellSeries[rowCount];
                puzzle.horizontalReverses = new CellSeries[rowCount];
                for (int row = 0; row < rowCount; row++)
                {
                    puzzle.horizontals[row] = new CellSeries(row, picture, Direction.Horizontal, leftColumn[row], puzzle.Correct, 0, picture.colCount());
                    puzzle.horizontalReverses[row] = new CellSeries(row, picture, Direction.HorizontalReverse, leftColumn[row], puzzle.Correct, 0, picture.colCount());
                }

                int colCount = picture.colCount();
                puzzle.verticals = new CellSeries[colCount];
                puzzle.verticalReverses = new CellSeries[colCount];
                for (int col = 0; col < colCount; col++)
                {
                    puzzle.verticals[col] = new CellSeries(col, picture, Direction.Vertical, upColumn[col], puzzle.Correct, 0, picture.rowCount());
                    puzzle.verticalReverses[col] = new CellSeries(col, picture, Direction.VerticalReverse, upColumn[col], puzzle.Correct, 0, picture.rowCount());
                }

                puzzle.isInitialized = true;
            }

            for (int row = 0; row < picture.rowCount(); row++)
            {
                if (!isRowCompleted[row])
                {
                    processing(puzzle.horizontals[row]);

                    if (isTwoWay)
                    {
                        processing(puzzle.horizontalReverses[row]);
                    }
                }
            }

            for (int col = 0; col < picture.colCount(); col++)
            {
                if (!isColCompleted[col])
                {
                    processing(puzzle.verticals[col]);

                    if (isTwoWay)
                    {
                        processing(puzzle.verticalReverses[col]);
                    }
                }
            }
        }

        private static void ProcessCheckAllCounts(byte[,] picture, Puzzle puzzle)
        {
            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;
            var isColCompleted = puzzle.IsColCompleted;
            var isRowCompleted = puzzle.IsRowCompleted;

            for (int col = 0; col < picture.colCount(); col++)
            {
                int[] values = upColumn[col];

                // TODO bunun için tablo oluştur
                int supposedFilledCount = GetSum(values);
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
                int supposedFilledCount = GetSum(values);
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

        private static bool TestPicture(byte[,] picture, Puzzle puzzle)
        {
            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;
            var correct = puzzle.Correct;
            var pictureRef = puzzle.PictureRef;

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

            if (puzzle.CorrectExists)
                for (int i = 0; i < picture.rowCount(); i++)
                    for (int j = 0; j < picture.colCount(); j++)
                        if (picture[i, j] != UNKNOWN && picture[i, j] != correct[i, j])
                        {
                            int asIs = picture[i, j];
                            int correctOne = correct[i, j];
                            Console.WriteLine("Hata tespit edildi, iteration: " + iteration);
                            Display(pictureRef, "Hatasız olan", puzzle);
                            Display(picture, "Hatalı olan", puzzle);
                            Display(correct, "Olması gereken", puzzle, true);
                            throw new Exception("Önceki metot yanlış, iteration: " + iteration + ", row: " + i + ", col: " + j);
                        }

            DumpPicture(picture, puzzle.PictureRef);

            return isChangeDetected;
        }

        private static byte[,] DumpPicture(byte[,] picture, byte[,] pictureRef)
        {
#if DEBUG
            if (pictureRef == null)
                throw new Exception("this should not be null");
#endif

            for (int i = 0; i < picture.rowCount(); i++)
                for (int j = 0; j < picture.colCount(); j++)
                    pictureRef[i, j] = picture[i, j];

            return pictureRef;
        }

        private static int GetSum(int[] values)
        {
            int sum = 0;

            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum;
        }

        private static void Display(byte[,] picture, Puzzle puzzle)
        {
            Display(picture, "Latest", puzzle);
        }

        private static string PictureToString(byte[,] picture)
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

        private static void Display(byte[,] picture, string title, Puzzle puzzle, bool isApplication = false)
        {
            DumpPicture(picture, puzzle.PictureRef);

            var leftColumn = puzzle.PuzzleJson.LeftColumn;
            var upColumn = puzzle.PuzzleJson.UpColumn;

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
            public IEnumerable<int> AsIterable { get { return Iterable(); } }
            public string Debug { get { return string.Format("[{0}]", string.Join(",", AsIterable)); } }

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
            public IEnumerable<int> Iterable()
            {
                for (int i = 0; i < _length; i++)
                    yield return valueGetter(i);
            }
            public int Sum()
            {
                return Iterable().Sum();
            }
        }

        public class CellSeries
        {
            public CellColumnValues CellColumnValues { get { return this._cellColumnValues; } }
            public int Length { get { return this._length; } }
            public byte this[int i]
            {
                get { return this.valueGetter(i); }
                set
                {
#if DEBUG
                    byte oldCell = this.valueGetter(i);
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
            public IEnumerable<byte> AsIterable
            {
                get
                {
                    for (int i = 0; i < this._length; i++)
                        yield return this.valueGetter(i);
                }
            }
            public IEnumerable<byte> AsCorrectIterable
            {
                get
                {
                    for (int i = 0; i < this._length; i++)
                        yield return this._correctValueGetter(i);
                }
            }
            public Direction direction = Direction.NotSet;
            public int? rowOrCol = null;
            public string AsString { get { return new string(AsIterable.Select(x => chars[x]).ToArray()); } }
            public string AsCorrectString { get { return new string(AsCorrectIterable.Select(x => chars[x]).ToArray()); } }

#if DEBUG
            public string FirstTimeString { get; set; }
#endif

            private CellColumnValues _cellColumnValues;
            private int _length;
            private int start;
            private byte[,] picture;
            private byte[,] correct;
            private Func<int, byte> valueGetter;
            private Func<int, byte> _correctValueGetter;
            private Action<int, byte> valueSetter;
            private bool correctExists;

            public CellSeries(int rowOrCol, byte[,] picture, Direction direction, int[] columnValues, byte[,] correct, int start, int length)
            {
                this._cellColumnValues = new CellColumnValues(columnValues, direction);
                this.direction = direction;
                this.rowOrCol = rowOrCol;
                this.picture = picture;
                this.correct = correct;
                this.start = start;

                this.correctExists = correct != null;

                switch (direction)
                {
                    case Direction.Horizontal:
                        {
                            int row = rowOrCol;
                            this.valueGetter = col => picture[row, start + col];
                            this._correctValueGetter = col => correct[row, start + col];
                            this.valueSetter = (col, cell) => picture[row, start + col] = cell;
                            this._length = length;
                        }
                        break;
                    case Direction.Vertical:
                        {
                            int col = rowOrCol;
                            this.valueGetter = row => picture[start + row, col];
                            this._correctValueGetter = row => correct[start + row, col];
                            this.valueSetter = (row, cell) => picture[start + row, col] = cell;
                            this._length = length;
                        }
                        break;
                    case Direction.HorizontalReverse:
                        {
                            int row = rowOrCol;
                            int lastCol = picture.colCount() - 1;
                            this.valueGetter = col => picture[row, lastCol - col - start];
                            this._correctValueGetter = col => correct[row, lastCol - col - start];
                            this.valueSetter = (col, cell) => picture[row, lastCol - col - start] = cell;
                            this._length = length;
                        }
                        break;
                    case Direction.VerticalReverse:
                        {
                            int col = rowOrCol;
                            int lastRow = picture.rowCount() - 1;
                            this.valueGetter = row => picture[lastRow - row - start, col];
                            this._correctValueGetter = row => correct[lastRow - row - start, col];
                            this.valueSetter = (row, cell) => picture[lastRow - row - start, col] = cell;
                            this._length = length;
                        }
                        break;
                    default:
                        break;
                }

#if DEBUG
                this.FirstTimeString = this.AsString;
#endif
            }

            public static CellSeries Slice(CellSeries old, int startIndex, int endIndex, int[] newValues)
            {
                int newLength = endIndex - startIndex + 1;

                if (newLength < 0)
                    newLength = 0;
#if DEBUG
                if (newLength < newValues.Sum() + newValues.Length - 1)
                    throw new Exception("Bre insafsız!");
#endif
                if (old.direction == Direction.HorizontalReverse || old.direction == Direction.VerticalReverse)
                {
                    newValues = newValues.Reverse().ToArray();
                }
                return new CellSeries(old.rowOrCol.Value, old.picture, old.direction, newValues, old.correct, old.start + startIndex, newLength);
                //return new CellSeries(cellColumnValues, newLength, getter, setter, old._correctValueGetter);
            }

            public static CellSeries Reverse(CellSeries old)
            {
                int[] newValues;

                Direction newDirection;
                switch (old.direction)
                {
                    case Direction.Horizontal:
                        newDirection = Direction.HorizontalReverse;
                        newValues = old.CellColumnValues.AsIterable.ToArray();
                        break;
                    case Direction.Vertical:
                        newDirection = Direction.VerticalReverse;
                        newValues = old.CellColumnValues.AsIterable.ToArray();
                        break;
                    case Direction.HorizontalReverse:
                        newDirection = Direction.Horizontal;
                        newValues = old.CellColumnValues.AsIterable.Reverse().ToArray();
                        break;
                    case Direction.VerticalReverse:
                        newDirection = Direction.Vertical;
                        newValues = old.CellColumnValues.AsIterable.Reverse().ToArray();
                        break;
                    case Direction.NotSet:
                        newDirection = Direction.NotSet;
                        newValues = null;
                        break;
                    default:
                        throw new Exception();
                }

                var oldStart = old.start;
                var oldRangeLength = old.Length;
                var oldTotalLength = old.TotalLength();
                var newStart = oldTotalLength - oldStart - oldRangeLength;

                CellSeries cells = new CellSeries(old.rowOrCol.Value, old.picture, newDirection, newValues, old.correct, newStart, old.Length);
                //CellSeries cells = new CellSeries(cellColumnValues, cellLength, getter, setter, old._correctValueGetter);

#if DEBUG
                var oldvals = old.CellColumnValues.AsIterable;
                var newvals = cells.CellColumnValues.AsIterable;

                if (!Enumerable.SequenceEqual(oldvals, newvals.Reverse()))
                    throw new Exception("Error at reversing");

                if (!Enumerable.SequenceEqual(old.AsIterable, cells.AsIterable.Reverse()))
                    throw new Exception("Error at reversing");
#endif
                return cells;
            }

            public static CellSeries FromBytes(byte[] bytes, int[] values)
            {
                var newSource = new byte[1, bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    newSource[0, i] = bytes[i];
                }
                return new CellSeries(0, newSource, Direction.Horizontal, values, null, 0, bytes.Length);
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

            private int TotalLength()
            {
                switch (direction)
                {
                    case Direction.Horizontal:
                    case Direction.HorizontalReverse:
                        return picture.colCount();
                    case Direction.Vertical:
                    case Direction.VerticalReverse:
                        return picture.rowCount();
                    case Direction.NotSet:
                    default:
                        throw new Exception();
                }
            }
        }

        public class Puzzle
        {
            public PuzzleJson PuzzleJson { get; set; }
            public bool[] IsRowCompleted { get; set; }
            public bool[] IsColCompleted { get; set; }
            public byte[,] Correct { get; set; }
            public byte[,] PictureRef { get; set; }
            public bool CorrectExists { get; set; }

            public bool isInitialized;
            public CellSeries[] horizontals;
            public CellSeries[] horizontalReverses;
            public CellSeries[] verticals;
            public CellSeries[] verticalReverses;
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
                var originalCells = new Form1.CellSeries(row, picture, Form1.Direction.Horizontal, leftColumn[row], null, 0, picture.colCount());
                byte[] originalValues = originalCells.AsIterable.ToArray();

                byte[] copyValues = originalValues.ToArray();

                var copyCells = Form1.CellSeries.FromBytes(copyValues, leftColumn[row]);

                bool hasError = false;

                try
                {
                    Generic.ProcessAllAlgorithms(copyCells);
                    Generic.ProcessAllAlgorithms(Form1.CellSeries.Reverse(copyCells));
                }
                catch (Exception)
                {
                    hasError = true;
                }

                if (hasError)
                {
                    leftColumnColors[row] = Color.Red;
                }
                else if (Enumerable.SequenceEqual(originalCells.AsIterable, copyCells.AsIterable))
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
                var originalCells = new Form1.CellSeries(col, picture, Form1.Direction.Vertical, upColumn[col], null, 0, picture.rowCount());
                byte[] originalValues = originalCells.AsIterable.ToArray();

                byte[] copyValues = originalValues.ToArray();

                var copyCells = Form1.CellSeries.FromBytes(copyValues, upColumn[col]);

                bool hasError;

                try
                {
                    Generic.ProcessAllAlgorithms(copyCells);
                    Generic.ProcessAllAlgorithms(Form1.CellSeries.Reverse(copyCells));
                    hasError = false;
                }
                catch (Exception)
                {
                    hasError = true;
                }

                if (hasError)
                {
                    upColumnColors[col] = Color.Red;
                }
                else if (Enumerable.SequenceEqual(originalCells.AsIterable, copyCells.AsIterable))
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
