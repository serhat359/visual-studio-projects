using System;
using System.Collections.Generic;
using System.Linq;

namespace PicrossSolver
{
    public static class Generic
    {
        public static void ProcessAllAlgorithms(Form1.CellSeries cells)
        {
            InitialProcessing(cells);
            ProcessSingles(cells);

            {
                ProcessStartsAndEnds(cells);
                ProcessStartingAndEndingUnknowns(cells);
            }

            ProcessSetEmptiesByMax(cells);
            RemoveSmallEmpties(cells);
            ProcessFillBetweenEmpties(cells);
            ProcessMaxValues(cells);
            ProcessDividedParts(cells);
            ProcessTryFindingMatchStartingAndEnding(cells);
            ProcessMatching(cells);
        }

        public static void InitialProcessing(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            int sum = values.Length - 1;

            sum += values.Sum();

            int rem = cells.Length - sum;

            int formerSize = 0;
            for (int i = 0; i < values.Length; i++)
            {
                int elem = values[i];

                int diff = elem - rem;

                if (diff > 0)
                {
                    for (int j = 0; j < diff; j++)
                        cells[j + rem + formerSize] = Form1.FILLED;
                }
                formerSize += 1 + elem;
            }
        }

        public static void ProcessSingles(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;
            int count = values.Length;

            if (count == 1)
            {
                int firstFilled = -1;
                int lastFilled = -1;

                int i;

                for (i = 0; i < cells.Length; i++)
                {
                    int cell = cells[i];

                    if (cell == Form1.FILLED)
                    {
                        firstFilled = i;
                        break;
                    }
                }

                for (int j = cells.Length - 1; j >= i; j--)
                {
                    int cell = cells[j];

                    if (cell == Form1.FILLED)
                    {
                        lastFilled = j;
                        break;
                    }
                }

                for (int k = firstFilled + 1; k < lastFilled; k++)
                    cells[k] = Form1.FILLED;

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
                            cells[firstFilled + 1 + k] = Form1.FILLED;
                        }
                    }
                    else
                        marginStart = 0;

                    int marginEnd = lastFilled + reach - (cells.Length - 1);
                    if (marginEnd > 0)
                    {
                        for (int k = 0; k < marginEnd; k++)
                        {
                            cells[lastFilled - 1 - k] = Form1.FILLED;
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
                        cells[k] = Form1.EMPTY;

                    for (int k = lastFilled + reach + 1; k < (cells.Length - 1); k++)
                        cells[k] = Form1.EMPTY;
                }
            }
        }

        public static void ProcessStartsAndEnds(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;
            int valuesIndex = 0;

            string oldCells = cells.asString;

            int i;
            for (i = 0; i < cells.Length; i++)
            {
                int cell = cells[i];

                if (cell == Form1.UNKNOWN)
                {
                    break;
                }
                else if (cell == Form1.FILLED)
                {
                    int val = values[valuesIndex++];
                    int max = i + val;

                    for (; i < max; i++)
                    {
                        cells[i] = Form1.FILLED;
                    }

                    if (i < cells.Length)
                        cells[i] = Form1.EMPTY;
                }
                else if (cell == Form1.EMPTY)
                {

                }
            }

            // Below is for checking remaining unknown cells
            bool isAllUnknown = true;
            for (int j = i; j < cells.Length; j++)
                if (cells[j] != Form1.UNKNOWN)
                {
                    isAllUnknown = false;
                    break;
                }

            // isAllUnknown below
            if (isAllUnknown)
            {
                int unknownSize = cells.Length - i;
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
                            cells[colIndex - 1] = Form1.EMPTY;

                        for (int k = 0; k < val; k++)
                            cells[colIndex++] = Form1.FILLED;
                    }
                }
                else if (unknownSize > 0 && valuesNewIndex == -1)
                {
                    for (int k = 0; k <= i; k++)
                        cells[k] = Form1.EMPTY;
                }
            }

            if (i > 0)
            {
                int startIndex = i;
                int endIndex = cells.Length - 1;

                int[] newValues = MyRange(valuesIndex, values.Length).Select(x => values[x]).ToArray();

                var slice = Form1.CellSeries.Slice(cells, startIndex, endIndex, newValues);

                if (slice.Length == cells.Length)
                    throw new Exception("You've done it again");

                ProcessAllAlgorithms(slice);
                ProcessAllAlgorithms(Form1.CellSeries.Reverse(slice));
            }
        }

        public static void ProcessStartingAndEndingUnknowns(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            int unknownCount = 0;

            int startIndex = 0;
            int valueIndex = 0;

            do
            {
                if (startIndex >= cells.Length)
                    break;

                int cell = cells[startIndex];

                if (cell == Form1.EMPTY)
                {
                    do
                    {
                        startIndex++;
                    } while (startIndex < cells.Length && cells[startIndex] == Form1.EMPTY);
                }
                else if (cell == Form1.FILLED)
                {
                    valueIndex++;

                    do
                    {
                        startIndex++;
                    } while (startIndex < cells.Length && cells[startIndex] == Form1.FILLED);
                }
                else
                    break;
            } while (true);

            for (int i = startIndex; i < cells.Length; i++)
            {
                int cell = cells[i];

                if (cell == Form1.UNKNOWN)
                    unknownCount++;
                else if (cell == Form1.FILLED)
                {
                    unknownCount = 0;
                    break;
                }
                else
                    break;
            }

            if (unknownCount > 0 && valueIndex < values.Length && unknownCount < values[valueIndex])
            {
                for (int i = 0; i < unknownCount; i++)
                    cells[i + startIndex] = Form1.EMPTY;
            }
        }

        public static void ProcessSetEmptiesByMax(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            // TODO generate table for this
            var maxValueResult = Form1.getMaxValue(values);

            int first = -1;

            List<FilledInfo> filledInfos = new List<FilledInfo>();

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != Form1.FILLED)
                    continue;
                else
                {
                    first = i;

                    int last = i;

                    for (i++; i < cells.Length; i++)
                    {
                        if (cells[i] == Form1.FILLED)
                            last = i;
                        else
                            break;
                    }

                    int size = last - first + 1;
                    if (size == maxValueResult.Value)
                    {
                        int leftEmptyCol = first - 1;
                        int rightEmptyCol = last + 1;

                        if (leftEmptyCol >= 0)
                            cells[leftEmptyCol] = Form1.EMPTY;

                        if (rightEmptyCol < cells.Length)
                            cells[rightEmptyCol] = Form1.EMPTY;

                        filledInfos.Add(new FilledInfo { CellIndexStart = first, Size = size });
                    }

                    i--;
                }
            }

            if (maxValueResult.Count > 0 && filledInfos.Count == maxValueResult.Count)
            {
                // TODO process

                for (int area = 0; area <= maxValueResult.Count; area++)
                {
                    var foundIndex = area;

                    int cellStart = area == 0 ? 0 : filledInfos[area - 1].CellIndexEnd + 2;
                    int cellEnd = area == maxValueResult.Count ? cells.Length - 1 : filledInfos[area].CellIndexStart - 1;

                    IEnumerable<int> selectedValuesIndices;

                    if (area == 0)
                        selectedValuesIndices = MyRange(0, maxValueResult.LocationIndices[area]);
                    else if (area == maxValueResult.Count)
                        selectedValuesIndices = MyRange(maxValueResult.LocationIndices[area - 1] + 1, values.Length);
                    else
                        selectedValuesIndices = MyRange(maxValueResult.LocationIndices[area - 1] + 1, maxValueResult.LocationIndices[area]);

                    int[] newValues = selectedValuesIndices.Select(x => values[x]).ToArray();

                    // below is ProcessInitial(cellStart, cellEnd, newValues);

                    Form1.CellSeries slice = Form1.CellSeries.Slice(cells, cellStart, cellEnd, newValues);

                    if (slice.cellColumnValues.Length > 0)
                        ProcessAllAlgorithms(slice);
                }
            }
        }

        public static void RemoveSmallEmpties(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            int unknownCount = 0;

            int startIndex = 0;
            int valueIndex = 0;

            do
            {
                if (startIndex >= cells.Length)
                    break;

                int cell = cells[startIndex];

                if (cell == Form1.EMPTY)
                {
                    do
                    {
                        startIndex++;
                    } while (startIndex < cells.Length && cells[startIndex] == Form1.EMPTY);
                }
                else if (cell == Form1.FILLED)
                {
                    valueIndex++;

                    do
                    {
                        startIndex++;
                    } while (startIndex < cells.Length && cells[startIndex] == Form1.FILLED);
                }
                else
                    break;
            } while (true);

            for (int i = startIndex; i < cells.Length; i++)
            {
                int cell = cells[i];

                if (cell == Form1.UNKNOWN)
                    unknownCount++;
                else if (cell == Form1.FILLED)
                {
                    unknownCount = 0;
                    break;
                }
                else
                    break;
            }

            if (unknownCount > 0 && valueIndex < values.Length && unknownCount < values[valueIndex])
            {
                for (int i = 0; i < unknownCount; i++)
                    cells[i + startIndex] = Form1.EMPTY;
            }
        }

        public static void ProcessFillBetweenEmpties(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            // TODO generate table for this
            int minValue = getMinValue(values);

            int lastEmptyIndex = -1;

            for (int i = 0; i < cells.Length; i++)
            {
                int cell = cells[i];

                if (cell == Form1.FILLED)
                    lastEmptyIndex = -1;
                if (cell == Form1.EMPTY)
                {
                    if (lastEmptyIndex >= 0 && i - lastEmptyIndex > 1 && i - lastEmptyIndex - 1 < minValue)
                    {
                        for (int k = lastEmptyIndex + 1; k < i; k++)
                            cells[k] = Form1.EMPTY;
                    }

                    lastEmptyIndex = i;
                }
            }
        }

        public static void ProcessMaxValues(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

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

                for (i = 0; i < cells.Length; i++)
                {
                    int cell = cells[i];

                    if (cell == Form1.FILLED && filledIndex < 0)
                    {
                        filledIndex = i;
                    }
                    else if (cell != Form1.FILLED && filledIndex >= 0)
                    {
                        int filledSize = i - filledIndex;
                        if (filledSize > secondMaxValue)
                        {
                            int filledEndIndex = i - 1;

                            int leftOfFilled = filledIndex - 1;
                            int rightOfFilled = filledEndIndex + 1;
                            if (leftOfFilled < 0 || cells[leftOfFilled] == Form1.EMPTY)
                            {
                                int k;
                                for (k = rightOfFilled; k < filledIndex + maxValue; k++)
                                {
                                    cells[k] = Form1.FILLED;
                                }
                                if (k < cells.Length)
                                    cells[k] = Form1.EMPTY;

                                filledIndex = -1;
                            }
                            else if (rightOfFilled > cells.Length - 1 || cells[rightOfFilled] == Form1.EMPTY)
                            {
                                int k;
                                for (k = leftOfFilled; k > filledEndIndex - maxValue; k--)
                                {
                                    cells[k] = Form1.FILLED;
                                }
                                if (k > 0)
                                    cells[k] = Form1.EMPTY;

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

        public static void ProcessTryFindingMatchStartingAndEnding(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            if (values.Length > 0)
            {
                int firstValue = values[0];

                bool foundFilled = false;
                bool foundEmpty = false;

                int i;
                for (i = 0; i < cells.Length; i++)
                {
                    int cell = cells[i];

                    if (cell == Form1.FILLED)
                        foundFilled = true;
                    else if (cell == Form1.EMPTY)
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
                            cells[k] = Form1.FILLED;
                    }
                }
            }
        }

        public static void ProcessMatching(Form1.CellSeries cells)
        {
            bool doContinue = false;
            int valueIndex = 0;
            int startIndex = 0;

            if (cells.cellColumnValues.Length > 0)
            {
                do
                {
                    doContinue = false;

                    int val = cells.cellColumnValues[valueIndex];

                    for (int i = 0; i + startIndex < cells.Length;)
                    {
                        if (cells[i + startIndex] != Form1.EMPTY)
                        {
                            break;
                        }

                        startIndex++;
                    }

                    int filledFoundIndex = -1;

                    for (int i = 0; i <= val; i++)
                    {
                        if (i + startIndex < cells.Length && cells[i + startIndex] == Form1.FILLED)
                        {
                            filledFoundIndex = i;
                            break;
                        }
                    }

                    if (filledFoundIndex >= 0)
                    {
                        // Dolunun arkasını doluyla doldurma
                        for (int i = filledFoundIndex + 1; i < val; i++)
                            cells[i + startIndex] = Form1.FILLED;

                        int filledLastIndex = val - 1;

                        for (int i = filledLastIndex + 1; ; i++)
                        {
                            if (i + startIndex < cells.Length && cells[i + startIndex] == Form1.FILLED)
                                filledLastIndex = i;
                            else
                                break;
                        }

                        int filledSize = filledLastIndex - filledFoundIndex + 1;
                        int reachRange = val - filledSize;

                        // Dolunun önününde ulaşılamayacak yerleri boş ile doldurma
                        for (int i = 0; i < filledSize + filledFoundIndex - val; i++)
                            cells[i + startIndex] = Form1.EMPTY;

                        if (cells[startIndex] == Form1.FILLED) // İlk karakterin dolu olma ihtimali
                        {
                            if (val + startIndex < cells.Length)
                                cells[val + startIndex] = Form1.EMPTY;

                            doContinue = true;
                            valueIndex++;
                            startIndex += val + 1;
                        }
                        else if (cells[val + startIndex] == Form1.EMPTY) // Doluların arkasının boş olma ihtimali
                        {
                            for (int i = 0; i < filledFoundIndex; i++)
                            {
                                cells[i + startIndex] = Form1.FILLED;
                            }

                            doContinue = true;
                            valueIndex++;
                            startIndex += val + 1;
                        }
                        else if (filledSize == val) // Doluların value ile aynı olma ihtimali
                        {
                            int emptyIndex1 = filledFoundIndex - 1;
                            int emptyIndex2 = filledLastIndex + 1;

                            if (emptyIndex1 + startIndex < cells.Length)
                                cells[emptyIndex1 + startIndex] = Form1.EMPTY;

                            if (emptyIndex2 + startIndex < cells.Length)
                                cells[emptyIndex2 + startIndex] = Form1.EMPTY;

                            bool isAllEmpty = true;
                            for (int i = 0; i < emptyIndex1; i++)
                            {
                                if (cells[i + startIndex] != Form1.EMPTY)
                                {
                                    isAllEmpty = false;
                                    break;
                                }
                            }

                            if (isAllEmpty)
                            {
                                doContinue = true;
                                valueIndex++;
                                startIndex += emptyIndex2 + 1;
                            }
                        }
                        else if (reachRange > 0 &&
                            filledLastIndex + reachRange + 1 + startIndex < cells.Length &&
                            cells[filledLastIndex + reachRange + 1 + startIndex] == Form1.FILLED) // Ulaşılamayacak yerin dolu olma ihtimali
                        {
                            bool containsEmpty = false;
                            int unknownCount = 0;
                            int unknownIndex = -1;

                            int checkStartIndex = filledLastIndex + 1;
                            for (int i = checkStartIndex; i < checkStartIndex + reachRange; i++)
                            {
                                int cell = cells[i + startIndex];

                                if (cell == Form1.EMPTY)
                                {
                                    containsEmpty = true;
                                    break;
                                }
                                else if (cell == Form1.UNKNOWN)
                                {
                                    unknownCount++;
                                    unknownIndex = i;
                                }
                            }

                            if (!containsEmpty && unknownCount == 1)
                            {
                                cells[unknownIndex + startIndex] = Form1.EMPTY;
                            }
                        }
                    }
                } while (doContinue && valueIndex < cells.cellColumnValues.Length);
            }
        }

        public static void ProcessDividedParts(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            List<Range> areaList = new List<Range>();

            int nonEmpty = -1;
            for (int i = 0; i < cells.Length; i++)
            {
                int cell = cells[i];

                if (cell != Form1.EMPTY && nonEmpty < 0)
                {
                    nonEmpty = i;
                }
                else if (cell == Form1.EMPTY && i - 1 >= 0 && cells[i - 1] != Form1.EMPTY && nonEmpty >= 0)
                {
                    areaList.Add(new Range(nonEmpty, i - 1));
                    nonEmpty = -1;
                }
            }

            if (nonEmpty > 0)
            {
                areaList.Add(new Range(nonEmpty, cells.Length - 1));
                nonEmpty = -1;
            }

            // Above is preparing ranges
            // Below is range-values matching

            if (areaList.Count > 0)
            {
                int[][] forwardMatching = new int[areaList.Count][];
                int[][] backwardMatching = new int[areaList.Count][];

                int loopValueIndex = 0;
                for (int area = 0; area < forwardMatching.Length; area++)
                {
                    Range range = areaList[area];

                    int valueIndex = loopValueIndex;

                    if (valueIndex < values.Length)
                    {
                        int currentSize = values[valueIndex];

                        while (currentSize <= range.size)
                        {
                            if (++valueIndex < values.Length)
                                currentSize += values[valueIndex] + 1;
                            else
                                break;
                        }

                        forwardMatching[area] = MyRange(loopValueIndex, valueIndex).Select(x => values[x]).ToArray();
                    }
                    else
                    {
                        forwardMatching[area] = new int[] { };
                    }

                    loopValueIndex = valueIndex;
                }

                // Above is regular iteration
                // Below is reverse iteration

                loopValueIndex = values.Length - 1;

                for (int area = forwardMatching.Length - 1; area >= 0; area--)
                {
                    Range range = areaList[area];

                    int valueIndex = loopValueIndex;

                    if (valueIndex >= 0)
                    {
                        int currentSize = values[valueIndex];

                        while (currentSize <= range.size)
                        {
                            if (--valueIndex >= 0)
                                currentSize += values[valueIndex] + 1;
                            else
                                break;
                        }

                        backwardMatching[area] = MyRangeDesc(loopValueIndex, valueIndex).Select(x => values[x]).ToArray();
                    }
                    else
                    {
                        backwardMatching[area] = new int[] { };
                    }

                    loopValueIndex = valueIndex;
                }

                // Below is comparing the two range matchings

                bool checkResult = Enumerable.Range(0, forwardMatching.Length).Select(x =>
                {
                    int[] forwardMatch = forwardMatching[x];
                    int[] backwardMatch = backwardMatching[x];

                    return Enumerable.SequenceEqual(forwardMatch, backwardMatch.Reverse());
                }).All(x => x == true);

                if (checkResult)
                {
                    for (int area = 0; area < areaList.Count; area++)
                    {
                        Range range = areaList[area];
                        int[] newValues = forwardMatching[area];

                        Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, newValues);

                        if (slice.cellColumnValues.Length > 0)
                            ProcessAllAlgorithms(slice);
                    }
                }
            }
        }

        #region Private Methods
        private static int getMinValue(Form1.CellColumnValues values)
        {
            if (values.Length > 0)
                return values.asIterable.Min();
            else
                return 0;
        }

        private static IEnumerable<int> MyRange(int from, int to)
        {
            for (int i = from; i < to; i++)
                yield return i;
        }

        private static IEnumerable<int> MyRangeDesc(int from, int to)
        {
            for (int i = from; i > to; i--)
                yield return i;
        }
        #endregion

        //private static void debug() { }
    }

    public class Range
    {
        public int start;
        public int end;
        public int size { get { return end - start + 1; } }

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

    public class FilledInfo
    {
        public int CellIndexStart { get; set; }
        public int CellIndexEnd { get { return CellIndexStart + Size - 1; } }
        public int Size { get; set; }
    }
}
