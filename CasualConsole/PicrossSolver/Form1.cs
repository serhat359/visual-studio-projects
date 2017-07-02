using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace PicrossSolver
{
    public class Form1
    {
        public delegate void Algorithm(CellSeries s);

        public const int UNKNOWN = 0;
        public const int FILLED = 1;
        public const int EMPTY = 2;

        public const int rowCount = 15;
        public const int colCount = 15;
        public const int displaySize = 20;
        public const int lastRow = rowCount - 1;
        public const int lastCol = colCount - 1;

        public static int iteration = 0;
        static int[,] pictureRef = null;

        public static bool[] isRowCompleted;
        public static bool[] isColCompleted;

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
            int[][] upColumn = new int[colCount][];
            upColumn[0] = (arr(1, 1, 1));
            upColumn[1] = (arr(1, 1, 2));
            upColumn[2] = (arr(3, 6));
            upColumn[3] = (arr(5, 3, 1));
            upColumn[4] = (arr(6, 5));
            upColumn[5] = (arr(3, 7, 1));
            upColumn[6] = (arr(2, 7, 1));
            upColumn[7] = (arr(1, 4, 2, 1));
            upColumn[8] = (arr(4, 1, 1));
            upColumn[9] = (arr(2, 2, 3, 1));
            upColumn[10] = (arr(2, 4, 1, 1));
            upColumn[11] = (arr(9, 1, 1));
            upColumn[12] = (arr(2, 1, 1));
            upColumn[13] = (arr(2, 1, 1, 3));
            upColumn[14] = (arr(1, 4, 5));

            int[][] leftColumn = new int[rowCount][];
            leftColumn[0] = (arr(4, 2, 2));
            leftColumn[1] = (arr(5, 5));
            leftColumn[2] = (arr(3, 2, 2));
            leftColumn[3] = (arr(2, 2, 1));
            leftColumn[4] = (arr(9));
            leftColumn[5] = (arr(1, 8, 1));
            leftColumn[6] = (arr(3, 3, 2, 2));
            leftColumn[7] = (arr(2, 2, 3, 1));
            leftColumn[8] = (arr(7, 1, 2));
            leftColumn[9] = (arr(7, 1, 1));
            leftColumn[10] = (arr(1, 1, 3, 2, 1));
            leftColumn[11] = (arr(1, 1, 3, 1));
            leftColumn[12] = (arr(1, 3, 2));
            leftColumn[13] = (arr(2, 3));
            leftColumn[14] = (arr(4, 2, 2));

            isRowCompleted = new bool[rowCount];
            isColCompleted = new bool[colCount];

            solveAndDisplay(upColumn, leftColumn);

            display(correct, "This is how it should be", true);
        }

        private static void solveAndDisplay(int[][] upColumn, int[][] leftColumn)
        {
            int[,] picture = new int[rowCount, colCount];

            solve(picture, upColumn, leftColumn);

            display(picture);
        }

        private static void solve(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            ApplyGivenAlgorithmForAll(picture, upColumn, leftColumn, Generic.InitialProcessing);

            dumpPicture(picture);

            testPicture(picture);

            for (iteration = 0; ; iteration++)
            {
                bool isChangeDetected = false;

                // tek sayı olanların ara boşluğunu doldurup, ulaşamayacağı yerlere çarpı atıyor
                processSingles(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                {
                    // seri başından ve sonundan itibaren bir tarafı kapalı sayıların kalanını ayarlayıp çarpı atıyor
                    processStartsAndEnds(picture, upColumn, leftColumn);
                    isChangeDetected |= testPicture(picture);

                    // seri başlarındaki ve sonlarındaki küçük boşluklara çarpı atıyor, BU METOD processStartsAndEnds METODUNDAN HEMEN SONRA ÇALIŞMALI!!!
                    Generic.processStartingAndEndingUnknowns(picture, upColumn, leftColumn);
                    isChangeDetected |= testPicture(picture);
                }

                // serilerdeki en büyük değerler dolduysa başına ve sonuna çarpı atıyor
                Generic.processSetEmptiesByMax(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                // serilerdeki çarpı arası boşlukları boşlukla dolduruyor
                processFillBetweenEmpties(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                // serideki outlier olan değere karşılık gelen dolmuşları işliyor
                processMaxValues(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                // serideki çarpılarla ayrılmış kısımları bulup işliyor, tasarım hatasından dolayı kaldırıldı
                // processDividedParts(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                // serideki dolu ve boş sayılarını kontrol ediyor
                processCheckAllCounts(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                // seri başlarında ve sonlarında kendini bulmaya çaşılışyor
                processTryFindingMatchStartingAndEnding(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                // serileri genel olarak analiz ediyor
                Generic.processMatching(picture, upColumn, leftColumn);
                isChangeDetected |= testPicture(picture);

                if (!isChangeDetected)
                {
                    break;
                }
            }

            Console.WriteLine("There was no change in the iteration: " + iteration);
        }

        public static void ApplyGivenAlgorithmForAll(int[,] picture, int[][] upColumn, int[][] leftColumn, Algorithm processing)
        {
            for (int row = 0; row < Form1.rowCount
                //&& !Form1.isRowCompleted[row]
                // TODO remove comment
                ; row++)
            {
                processing(new Form1.CellSeries(row, picture, Form1.Direction.Horizontal, leftColumn[row]));
                processing(new Form1.CellSeries(row, picture, Form1.Direction.HorizontalReverse, leftColumn[row]));
            }

            for (int col = 0; col < Form1.rowCount
                //&& !Form1.isColCompleted[col]
                // TODO remove comment
                ; col++)
            {
                processing(new Form1.CellSeries(col, picture, Form1.Direction.Vertical, upColumn[col]));
                processing(new Form1.CellSeries(col, picture, Form1.Direction.VerticalReverse, upColumn[col]));
            }
        }

        private static void processTryFindingMatchStartingAndEnding(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            for (int col = 0; col < colCount; col++)
            {
                int[] values = upColumn[col];

                int firstValue = values[0];

                bool foundFilled = false;
                bool foundEmpty = false;

                int i;
                for (i = 0; i < rowCount; i++)
                {
                    int cell = picture[i, col];

                    if (cell == FILLED)
                        foundFilled = true;
                    else if (cell == EMPTY)
                    {
                        foundEmpty = true;
                        break;
                    }
                }

                if (foundEmpty && foundFilled)
                {
                    // TODO this requires range processing
                    if (i == firstValue)
                    {
                        for (int k = 0; k < i; k++)
                            picture[k, col] = FILLED;
                    }
                }

                // Above is regular iteration
                // Below is reverse iteration

                int lastValue = values[values.Length - 1];

                foundFilled = false;
                foundEmpty = false;

                for (i = lastRow; i >= 0; i--)
                {
                    int cell = picture[i, col];

                    if (cell == FILLED)
                        foundFilled = true;
                    else if (cell == EMPTY)
                    {
                        foundEmpty = true;
                        break;
                    }
                }

                if (foundEmpty && foundFilled)
                {
                    // TODO this requires range processing
                    if (lastRow - i == lastValue)
                    {
                        for (int k = i + 1; k < rowCount; k++)
                            picture[k, col] = FILLED;
                    }
                }
            }

            for (int row = 0; row < rowCount; row++)
            {
                int[] values = leftColumn[row];

                int firstValue = values[0];

                bool foundFilled = false;
                bool foundEmpty = false;

                int i;
                for (i = 0; i < colCount; i++)
                {
                    int cell = picture[row, i];

                    if (cell == FILLED)
                        foundFilled = true;
                    else if (cell == EMPTY)
                    {
                        foundEmpty = true;
                        break;
                    }
                }

                if (foundEmpty && foundFilled)
                {
                    // TODO this requires range processing
                    if (i == firstValue)
                    {
                        for (int k = 0; k < i; k++)
                            picture[row, k] = FILLED;
                    }
                }

                // Above is regular iteration
                // Below is reverse iteration

                int lastValue = values[values.Length - 1];

                foundFilled = false;
                foundEmpty = false;

                for (i = lastCol; i >= 0; i--)
                {
                    int cell = picture[row, i];

                    if (cell == FILLED)
                        foundFilled = true;
                    else if (cell == EMPTY)
                    {
                        foundEmpty = true;
                        break;
                    }
                }

                if (foundEmpty && foundFilled)
                {
                    // TODO this requires range processing
                    if (lastCol - i == lastValue)
                    {
                        for (int k = i + 1; k < colCount; k++)
                            picture[row, k] = FILLED;
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

                if (supposedFilledCount == actualFilledCount && supposedEmptyCount == actualEmptyCount) { }
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

                if (supposedFilledCount == actualFilledCount && supposedEmptyCount == actualEmptyCount) { }
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

        [Obsolete("Dangerous Function")]
        private static void processDividedParts(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            for (int col = 0; col < colCount; col++)
            {

                int[] values = upColumn[col];
                // TODO Auto-generated method stub
            }

            for (int row = 0; row < rowCount; row++)
            {
                int[] values = leftColumn[row];

                List<Range> dividedParts = new List<Range>();

                int nonEmpty = -1;
                for (int i = 0; i < colCount; i++)
                {
                    int cell = picture[row, i];

                    if (cell != EMPTY && nonEmpty < 0)
                    {
                        nonEmpty = i;
                    }
                    else if (cell == EMPTY && i - 1 >= 0 && picture[row, i - 1] != EMPTY && nonEmpty >= 0)
                    {
                        dividedParts.Add(new Range(nonEmpty, i - 1));
                        nonEmpty = -1;
                    }
                }

                if (nonEmpty > 0)
                {
                    dividedParts.Add(new Range(nonEmpty, colCount - 1));
                    nonEmpty = -1;
                }

                if (dividedParts.Count == values.Length)
                {
                    int i = 0;
                    foreach (Range range in dividedParts)
                    {
                        int val = values[i];
                        int rangeVal = range.end - range.start + 1;
                        int reach = rangeVal - val;

                        for (int j = range.start + reach; j < range.end - reach + 1; j++)
                        {
                            if (j == 10)
                            {
                                display(picture);
                                debug();
                            }
                            picture[row, j] = FILLED;
                        }

                        i++;
                    }
                }
            }
        }

        private static void processMaxValues(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            for (int col = 0; col < colCount; col++)
            {
                int[] values = upColumn[col];

                if (values.Length > 1)
                {
                    int secondMaxValue = values[0];
                    int maxValue = values[0];

                    int i;
                    for (i = 1; i < values.Length; i++)
                    {
                        int val = values[i];

                        if (val > maxValue)
                        {
                            if (maxValue > secondMaxValue)
                                secondMaxValue = maxValue;

                            maxValue = val;
                        }
                        else if (val > secondMaxValue)
                            secondMaxValue = val;
                    }

                    int filledIndex = -1;
                    i = 0;

                    for (i = 0; i < rowCount; i++)
                    {
                        int cell = picture[i, col];

                        if (cell == FILLED && filledIndex < 0)
                        {
                            filledIndex = i;
                        }
                        else if (cell != FILLED && filledIndex >= 0)
                        {
                            int filledSize = i - filledIndex;
                            if (filledSize > secondMaxValue)
                            {
                                int filledEndIndex = i - 1;

                                int leftOfFilled = filledIndex - 1;
                                int rightOfFilled = filledEndIndex + 1;
                                if (leftOfFilled < 0 || picture[leftOfFilled, col] == EMPTY)
                                {
                                    int k;
                                    for (k = rightOfFilled; k < filledIndex + maxValue; k++)
                                    {
                                        picture[k, col] = FILLED;
                                    }
                                    if (k < rowCount)
                                        picture[k, col] = EMPTY;

                                    filledIndex = -1;
                                }
                                else if (rightOfFilled > lastRow || picture[rightOfFilled, col] == EMPTY)
                                {
                                    int k;
                                    for (k = leftOfFilled; k > filledEndIndex - maxValue; k--)
                                    {
                                        picture[k, col] = FILLED;
                                    }
                                    if (k > 0)
                                        picture[k, col] = EMPTY;

                                    filledIndex = -1;
                                }
                                else
                                {
                                    // TODO other process
                                }
                            }
                            else
                            {
                                filledIndex = -1;
                            }
                        }
                    }
                }
            }

            for (int row = 0; row < rowCount; row++)
            {
                int[] values = leftColumn[row];

                if (values.Length > 1)
                {
                    int secondMaxValue = values[0];
                    int maxValue = values[0];

                    int i;
                    for (i = 1; i < values.Length; i++)
                    {
                        int val = values[i];

                        if (val > maxValue)
                        {
                            if (maxValue > secondMaxValue)
                                secondMaxValue = maxValue;

                            maxValue = val;
                        }
                        else if (val > secondMaxValue)
                            secondMaxValue = val;
                    }

                    int filledIndex = -1;
                    i = 0;

                    for (i = 0; i < colCount; i++)
                    {
                        int cell = picture[row, i];

                        if (cell == FILLED && filledIndex < 0)
                        {
                            filledIndex = i;
                        }
                        else if (cell != FILLED && filledIndex >= 0)
                        {
                            int filledSize = i - filledIndex;
                            if (filledSize > secondMaxValue)
                            {
                                int filledEndIndex = i - 1;

                                int leftOfFilled = filledIndex - 1;
                                int rightOfFilled = filledEndIndex + 1;
                                if (leftOfFilled < 0 || picture[row, leftOfFilled] == EMPTY)
                                {
                                    int k;
                                    for (k = rightOfFilled; k < filledIndex + maxValue; k++)
                                    {
                                        picture[row, k] = FILLED;
                                    }
                                    if (k < colCount)
                                        picture[row, k] = EMPTY;

                                    filledIndex = -1;
                                }
                                else if (rightOfFilled > lastCol || picture[row, rightOfFilled] == EMPTY)
                                {
                                    int k;
                                    for (k = leftOfFilled; k > filledEndIndex - maxValue; k--)
                                    {
                                        picture[row, k] = FILLED;
                                    }
                                    if (k > 0)
                                        picture[row, k] = EMPTY;

                                    filledIndex = -1;
                                }
                                else
                                {
                                    // TODO other process
                                }
                            }
                            else
                            {
                                filledIndex = -1;
                            }
                        }
                    }
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

        private static void processFillBetweenEmpties(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            for (int col = 0; col < colCount; col++)
            {
                int[] values = upColumn[col];

                // TODO generate table for this
                int minValue = getMinValue(values);

                int lastEmptyIndex = -1;

                for (int i = 0; i < rowCount; i++)
                {
                    int cell = picture[i, col];

                    if (cell == FILLED)
                        lastEmptyIndex = -1;
                    if (cell == EMPTY)
                    {
                        if (lastEmptyIndex >= 0 && i - lastEmptyIndex > 1 && i - lastEmptyIndex - 1 < minValue)
                        {
                            for (int k = lastEmptyIndex + 1; k < i; k++)
                                picture[k, col] = EMPTY;
                        }

                        lastEmptyIndex = i;
                    }
                }
            }

            for (int row = 0; row < rowCount; row++)
            {
                int[] values = leftColumn[row];

                // TODO generate table for this
                int minValue = getMinValue(values);

                int lastEmptyIndex = -1;

                for (int i = 0; i < colCount; i++)
                {
                    int cell = picture[row, i];

                    if (cell == FILLED)
                        lastEmptyIndex = -1;
                    if (cell == EMPTY)
                    {
                        if (lastEmptyIndex >= 0 && i - lastEmptyIndex > 1 && i - lastEmptyIndex - 1 < minValue)
                        {
                            for (int k = lastEmptyIndex + 1; k < i; k++)
                                picture[row, k] = EMPTY;
                        }

                        lastEmptyIndex = i;
                    }
                }
            }
        }

        private static int getMinValue(int[] values)
        {
            int minIndex = 0;
            int minValue = values[minIndex];

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] < minValue)
                {
                    minIndex = i;
                    minValue = values[minIndex];
                }
            }

            return minValue;
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

        private static void processStartsAndEnds(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            for (int col = 0; col < colCount; col++)
            {
                int[] values = upColumn[col];
                int valuesIndex = 0;

                int i;
                for (i = 0; i < rowCount; i++)
                {
                    int cell = picture[i, col];

                    if (cell == UNKNOWN)
                    {
                        break;
                    }
                    else if (cell == FILLED)
                    {

                        int val = values[valuesIndex++];
                        int max = i + val;

                        for (; i < max; i++)
                        {
                            picture[i, col] = FILLED;
                        }

                        if (i < rowCount)
                            picture[i, col] = EMPTY;

                    }
                    else if (cell == EMPTY)
                    {

                    }
                }

                // Below is for checking remaining unknown cells
                bool isAllUnknown = true;
                for (int j = i; j < rowCount; j++)
                    if (picture[j, col] != UNKNOWN)
                    {
                        isAllUnknown = false;
                        break;
                    }

                // isAllUnknown below
                if (isAllUnknown)
                {
                    int unknownSize = rowCount - i;
                    int valuesNewIndex = valuesIndex;

                    int sum = values.Length - 1 - valuesIndex;
                    for (int j = valuesIndex; j < values.Length; j++)
                        sum += values[j];

                    // TODO this needs range process optimization
                    if (unknownSize > 0 && sum == unknownSize)
                    {
                        for (int rowIndex = i, j = valuesNewIndex; j < values.Length; j++, rowIndex++)
                        {
                            int val = values[j];

                            if (rowIndex - 1 >= 0)
                                picture[rowIndex - 1, col] = EMPTY;

                            for (int k = 0; k < val; k++)
                                picture[rowIndex++, col] = FILLED;
                        }
                    }
                    else if (unknownSize > 0 && valuesNewIndex == -1)
                    {
                        for (int k = 0; k <= i; k++)
                            picture[k, col] = EMPTY;
                    }
                }

                // Above is regular iteration
                // Below is for reverse iteration

                valuesIndex = values.Length - 1;

                for (i = rowCount - 1; i >= 0; i--)
                {
                    int cell = picture[i, col];

                    if (cell == UNKNOWN)
                    {
                        break;
                    }
                    else if (cell == FILLED)
                    {

                        int val = values[valuesIndex--];
                        int max = i - val;

                        for (; i > max; i--)
                        {
                            picture[i, col] = FILLED;
                        }

                        if (i >= 0)
                            picture[i, col] = EMPTY;

                    }
                    else if (cell == EMPTY)
                    {

                    }
                }

                // Below is for checking remaining unknown cells
                isAllUnknown = true;
                for (int j = 0; j <= i; j++)
                    if (picture[j, col] != UNKNOWN)
                    {
                        isAllUnknown = false;
                        break;
                    }

                // isAllUnknown below
                if (isAllUnknown)
                {
                    int unknownSize = i + 1;
                    int valuesNewIndex = valuesIndex;

                    int sum = valuesIndex;
                    for (int j = 0; j <= valuesNewIndex; j++)
                        sum += values[j];

                    // TODO this needs range process optimization
                    if (unknownSize > 0 && sum == unknownSize)
                    {
                        for (int rowIndex = 0, j = 0; j <= valuesNewIndex; j++, rowIndex++)
                        {
                            int val = values[j];

                            if (rowIndex - 1 >= 0)
                                picture[rowIndex - 1, col] = EMPTY;

                            for (int k = 0; k < val; k++)
                                picture[rowIndex++, col] = FILLED;
                        }
                    }
                    else if (unknownSize > 0 && valuesNewIndex == -1)
                    {
                        for (int k = 0; k <= i; k++)
                            picture[k, col] = EMPTY;
                    }
                }
            }

            for (int row = 0; row < rowCount; row++)
            {
                int[] values = leftColumn[row];
                int valuesIndex = 0;

                int i;
                for (i = 0; i < colCount; i++)
                {
                    int cell = picture[row, i];

                    if (cell == UNKNOWN)
                    {
                        break;
                    }
                    else if (cell == FILLED)
                    {

                        int val = values[valuesIndex++];
                        int max = i + val;

                        for (; i < max; i++)
                        {
                            picture[row, i] = FILLED;
                        }

                        if (i < rowCount)
                            picture[row, i] = EMPTY;

                    }
                    else if (cell == EMPTY)
                    {

                    }
                }

                // Below is for checking remaining unknown cells
                bool isAllUnknown = true;
                for (int j = i; j < colCount; j++)
                    if (picture[row, j] != UNKNOWN)
                    {
                        isAllUnknown = false;
                        break;
                    }

                // isAllUnknown below
                if (isAllUnknown)
                {
                    int unknownSize = colCount - i;
                    int valuesNewIndex = valuesIndex;

                    int sum = values.Length - 1 - valuesIndex; // CHANGE! sum = Length - 1
                    for (int j = valuesIndex; j < values.Length; j++)
                        sum += values[j];

                    // TODO this needs range process optimization
                    if (unknownSize > 0 && sum == unknownSize)
                    {
                        for (int colIndex = i, j = valuesNewIndex; j < values.Length; j++, colIndex++)
                        { // CHANGE colIndex++
                            int val = values[j];

                            if (colIndex - 1 >= 0)
                                picture[row, colIndex - 1] = EMPTY;

                            for (int k = 0; k < val; k++)
                                picture[row, colIndex++] = FILLED;
                        }
                    }
                    else if (unknownSize > 0 && valuesNewIndex == -1)
                    {
                        for (int k = 0; k <= i; k++)
                            picture[row, k] = EMPTY;
                    }
                }

                // Above is regular iteration
                // Below is for reverse iteration

                valuesIndex = values.Length - 1;

                for (i = colCount - 1; i >= 0; i--)
                {
                    int cell = picture[row, i];

                    if (cell == UNKNOWN)
                    {
                        break;
                    }
                    else if (cell == FILLED)
                    {

                        int val = values[valuesIndex--];
                        int max = i - val;

                        for (; i > max; i--)
                        {
                            picture[row, i] = FILLED;
                        }

                        if (i >= 0)
                            picture[row, i] = EMPTY;

                    }
                    else if (cell == EMPTY)
                    {

                    }
                }

                // Below is for checking remaining unknown cells
                isAllUnknown = true;
                for (int j = 0; j <= i; j++)
                    if (picture[row, j] != UNKNOWN)
                    {
                        isAllUnknown = false;
                        break;
                    }

                // isAllUnknown below
                if (isAllUnknown)
                {
                    int unknownSize = i + 1;
                    int valuesNewIndex = valuesIndex;

                    int sum = valuesIndex; // CHANGE! remove -1
                    for (int j = 0; j <= valuesNewIndex; j++) // CHANGE! put <=
                        sum += values[j];

                    // TODO this needs range process optimization
                    if (unknownSize > 0 && sum == unknownSize)
                    {
                        for (int colIndex = 0, j = 0; j <= valuesNewIndex; j++, colIndex++)
                        { // CHANGE colIndex++ Added
                            int val = values[j];

                            if (colIndex - 1 >= 0)
                                picture[row, colIndex - 1] = EMPTY;

                            for (int k = 0; k < val; k++)
                                picture[row, colIndex++] = FILLED;
                        }
                    }
                    else if (unknownSize > 0 && valuesNewIndex == -1)
                    {
                        for (int k = 0; k <= i; k++)
                            picture[row, k] = EMPTY;
                    }
                }
            }
        }

        private static void processSingles(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            // Fill in betweens for each column
            for (int col = 0; col < colCount; col++)
            {
                int[] values = upColumn[col];
                int count = values.Length;

                if (count == 1)
                {
                    int firstFilled = -1;
                    int lastFilled = -1;

                    int i;

                    for (i = 0; i < rowCount; i++)
                    {
                        int cell = picture[i, col];

                        if (cell == FILLED)
                        {
                            firstFilled = i;
                            break;
                        }
                    }

                    for (int j = lastRow; j >= i; j--)
                    {
                        int cell = picture[j, col];

                        if (cell == FILLED)
                        {
                            lastFilled = j;
                            break;
                        }
                    }

                    for (int k = firstFilled + 1; k < lastFilled; k++)
                        picture[k, col] = FILLED;

                    // Above is for filling in betweens
                    // Below is for finding reaching

                    if (firstFilled >= 0)
                    {
                        int filledSize = lastFilled - firstFilled + 1;
                        int reach = values[0] - filledSize;

                        int marginStart = reach - firstFilled;
                        if (marginStart > 0)
                        {
                            for (int k = 0; k < marginStart; k++)
                            {
                                picture[firstFilled + 1 + k, col] = FILLED;
                            }
                        }
                        else
                            marginStart = 0;

                        int marginEnd = lastFilled + reach - lastRow;
                        if (marginEnd > 0)
                        {
                            for (int k = 0; k < marginEnd; k++)
                            {
                                picture[lastFilled - 1 - k, col] = FILLED;
                            }
                        }
                        else
                            marginEnd = 0;

                        // Above is for filling reaching
                        // Below is for setting empties

                        // Update variables
                        firstFilled -= marginEnd;
                        lastFilled += marginStart;
                        filledSize += marginEnd + marginStart;
                        reach = values[0] - filledSize;

                        for (int k = 0; k < firstFilled - reach; k++)
                            picture[k, col] = EMPTY;

                        for (int k = lastFilled + reach + 1; k < rowCount; k++)
                            picture[k, col] = EMPTY;
                    }
                }
            }

            // Fill in betweens for each row
            for (int row = 0; row < rowCount; row++)
            {
                int[] values = leftColumn[row];
                int count = values.Length;

                if (count == 1)
                {
                    int firstFilled = -1;
                    int lastFilled = -1;

                    int i;

                    for (i = 0; i < colCount; i++)
                    {
                        int cell = picture[row, i];

                        if (cell == FILLED)
                        {
                            firstFilled = i;
                            break;
                        }
                    }

                    for (int j = lastCol; j >= i; j--)
                    {
                        int cell = picture[row, j];

                        if (cell == FILLED)
                        {
                            lastFilled = j;
                            break;
                        }
                    }

                    for (int k = firstFilled + 1; k < lastFilled; k++)
                        picture[row, k] = FILLED;

                    // Above is for filling in betweens
                    // Below is for finding reaching

                    if (firstFilled >= 0)
                    {
                        int filledSize = lastFilled - firstFilled + 1;
                        int reach = values[0] - filledSize;

                        int marginStart = reach - firstFilled;
                        if (marginStart > 0)
                        {
                            for (int k = 0; k < marginStart; k++)
                            {
                                picture[row, firstFilled + 1 + k] = FILLED;
                            }
                        }
                        else
                            marginStart = 0;

                        int marginEnd = lastFilled + reach - lastCol;
                        if (marginEnd > 0)
                        {
                            for (int k = 0; k < marginEnd; k++)
                            {
                                picture[row, lastFilled - 1 - k] = FILLED;
                            }
                        }
                        else
                            marginEnd = 0;

                        // Above is for filling reaching
                        // Below is for setting empties

                        // Update variables
                        firstFilled -= marginEnd;
                        lastFilled += marginStart;
                        filledSize += marginEnd + marginStart;
                        reach = values[0] - filledSize;

                        for (int k = 0; k < firstFilled - reach; k++)
                            picture[row, k] = EMPTY;

                        for (int k = lastFilled + reach + 1; k < colCount; k++)
                            picture[row, k] = EMPTY;
                    }
                }
            }
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

        // TODO remove this function when it's over
        private static void debug()
        {
            var x = 1;
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
