using System;
using System.Collections.Generic;
using System.Linq;
using CasualConsole;

namespace PicrossSolver
{
    public static class Generic
    {
        public static void TestMatchingByDivided()
        {
            TestCase[] tests = new TestCase[] {
                new TestCase { CellsString  = "■■.  ", Values = new int[]{ 2, 1 }, CorrectAssignment = new int[][] { new[]{ 2 }, new[] { 1 } }  },

                new TestCase { CellsString  = "■■  .■.  ", Values = new int[]{ 2, 1, 2 }, CorrectAssignment = new int[][] { new[]{ 2 }, new[] { 1 }, new[] { 2 } }  },

                new TestCase { CellsString  = "   ■■.  ", Values = new int[]{ 2, 1 }, CorrectAssignment = new int[][] { new[]{ 2 }, new[] { 1 } }  },

                new TestCase { CellsString  = "   ■■.   ■     ", Values = new int[]{ 2, 1, 3, 2 }, CorrectAssignment = new int[][] { new[]{ 2 }, new[] { 1, 3, 2 } }  },

                new TestCase { CellsString = "     ■   .■■   ",  Values = new int[]{ 2, 3, 1, 2 }, CorrectAssignment = new int[][] { new[]{ 2, 3, 1 }, new[] { 2 } } },

                new TestCase { CellsString = " .■.      ", Values = new int[]{ 1,3,1 }, CorrectAssignment = new int[][] { new int [] { }, new[]{ 1 }, new[] { 3,1 } } },

                new TestCase { CellsString = "    ■  ■.■     ", Values = new int[]{ 1,1,2,1,2 }, CorrectAssignment = new int[][] { new[]{ 1,1,2,1 }, new[] { 2 } }  },

                new TestCase { CellsString = "     ■.■  ■    ", Values = new int[]{ 2,1,2,1,1 }, CorrectAssignment = new int[][] { new[]{ 2,1 }, new[] { 2,1,1 } } },

                new TestCase { CellsString = "    ■■■ ■.  ■   ■.  ", Values = new int[]{ 1,5,2,2,1 }, CorrectAssignment = new int[][] { new[]{ 1,5 }, new[] { 2,2 }, new[] { 1 } } },

                new TestCase { CellsString = "  .■   ■  .■ ■■■    ", Values = new int[]{ 1,2,2,5,1 }, CorrectAssignment = new int[][] { new[]{ 1 }, new[] { 2,2 }, new[] { 5,1 } } },

                new TestCase { CellsString = " .■  ■ ", Values = new int[]{ 1,3 }, CorrectAssignment = new int[][] { new int[]{  }, new[] { 1,3 } } },

                new TestCase { CellsString = "    .■  ■ ", Values = new int[] { 1,1 }, CorrectAssignment = new int[][] { new int[]{ }, new[] { 1,1 } } },

                new TestCase { CellsString = "    .■  ■ ", Values = new int[] { 2,2 }, CorrectAssignment = new int[][] { new int[]{ }, new[] { 2,2 } } },
            };

            Func<string, byte[]> strToBytes = s =>
            {
                byte[] bytes = new byte[s.Length];

                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    bytes[i] = (byte)(Form1.chars.IndexOf(c));
                }

                return bytes;
            };

            int testNumber = 1;
            foreach (var test in tests)
            {
                Form1.CellSeries cells = new Form1.CellSeries(strToBytes(test.CellsString), test.Values);

                List<Range> areaList = FindDividedAreas(cells);

                var forwardMatching = AssignMatchingForward(cells, areaList);

                if (forwardMatching.Length != test.CorrectAssignment.Length)
                    throw new Exception("Numbers don't match");

                for (int i = 0; i < forwardMatching.Length; i++)
                {
                    List<ColumnValue> assigned = forwardMatching[i];

                    int[] assignedValues = assigned.Select(x => x.Value).ToArray();
                    int[] correct = test.CorrectAssignment[i];

                    if (!Enumerable.SequenceEqual(assignedValues, correct))
                        throw new Exception("Assignment is wrong");
                }

                testNumber++;
            }

            Console.WriteLine("All test are OK!!!");
            Console.WriteLine();

            {
                string asd = "       ■    ■  ";
                int[] vals = { 2, 1, 1, 2, 3 };

                var cellsExternal = new Form1.CellSeries(strToBytes(asd), vals);

                List<Range> filledRanges = FindFilledGroups(cellsExternal, 0, cellsExternal.Length - 1);

                var res = GetFilledMatchingCandidates(cellsExternal, vals, filledRanges);
            }

            {
                string asd = "  ■    ■       ";
                int[] vals = { 3, 2, 1, 1, 2 };

                var cellsExternal = new Form1.CellSeries(strToBytes(asd), vals);

                List<Range> filledRanges = FindFilledGroups(cellsExternal, 0, cellsExternal.Length - 1);

                var res = GetFilledMatchingCandidates(cellsExternal, vals, filledRanges);
            }

        }

        public static void ProcessAllAlgorithms(Form1.CellSeries cells)
        {
            if (cells.cellColumnValues.asIterable.Any())
            {
                InitialProcessing(cells);
                ProcessSingles(cells);

                {
                    ProcessStart(cells);
                    ProcessStartingUnknowns(cells);
                }

                ProcessSetEmptiesByMax(cells);
                ProcessFillBetweenEmpties(cells);
                ProcessByMaxValues(cells);
                ProcessByDividedAreas(cells);
                TryMatchingFirstValue(cells);
                ProcessMatching(cells);
                ProcessInitialByMatchingFilled(cells);
                ProcessSpecialCases(cells);

                ProcessByFilledRanges(cells);
            }
            else
            {
                EmptyAll(cells);
            }
        }

        /// <summary>
        /// Fills all the cells with EMPTY
        /// </summary>
        public static void EmptyAll(Form1.CellSeries cells)
        {
            for (int i = 0; i < cells.Length; i++)
                cells[i] = Form1.EMPTY;
        }

        /// <summary>
        /// Does initial processing on the cells, fills the parts that are certain to be FILLED, should NOT set anything as EMPTY
        /// </summary>
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

        /// <summary>
        /// Does processing if there is only one value for the series. This includes:
        /// <para />
        /// Filling in between the first and the last filled bytes with FILLED, should NOT do filling other than this part
        /// <para />
        /// Filling unreachable bytes with EMPTY
        /// </summary>
        public static void ProcessSingles(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;
            int count = values.Length;

            if (count == 1)
            {
                int firstFilled = -1;
                int lastFilled = -1;

                int i;

                // Find first filled
                for (i = 0; i < cells.Length; i++)
                {
                    byte cell = cells[i];

                    if (cell == Form1.FILLED)
                    {
                        firstFilled = i;
                        break;
                    }
                }

                // Find last filled
                for (int j = cells.Length - 1; j >= i; j--)
                {
                    byte cell = cells[j];

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

                    for (int k = 0; k < firstFilled - reach; k++)
                        cells[k] = Form1.EMPTY;

                    for (int k = lastFilled + reach + 1; k < (cells.Length - 1); k++)
                        cells[k] = Form1.EMPTY;
                }
            }
        }

        /// <summary>
        /// Does processing beginning on the start of the series. This includes:
        /// <para />
        /// Skipping starting empties
        /// <para />
        /// Filling the rest of the first occurence of FILLED and marking the end with EMPTY
        /// <para />
        /// Creating a slice for the rest if necessary
        /// </summary>
        public static void ProcessStart(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;
            int valuesIndex = 0;

            int i;
            for (i = 0; i < cells.Length; i++)
            {
                byte cell = cells[i];

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

                    cells.SafeSet(i, Form1.EMPTY);
                }
                else if (cell == Form1.EMPTY)
                {

                }
            }

            // i is pointing to an UNKNOWN at this point
            // If i is greater than 0, then we can slice
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

        /// <summary>
        /// Checks the number of starting UNKNOWNs and if less than the first value, fills them with EMPTY
        /// </summary>
        public static void ProcessStartingUnknowns(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            if (values.Length == 0)
                return;

            int unknownCount = 0;

            for (int i = 0; i < cells.Length; i++)
            {
                byte cell = cells[i];

                if (cell == Form1.UNKNOWN)
                    unknownCount++;
                else if (cell == Form1.FILLED)
                {
                    unknownCount = 0;
                    break;
                }
                else if (cell == Form1.EMPTY)
                    break;
            }

            if (unknownCount > 0 && unknownCount < values[0])
            {
                for (int j = 0; j < unknownCount; j++)
                    cells[j] = Form1.EMPTY;
            }
        }

        /// <summary>
        /// Marks the start and end of the greatest values, also creates slices based on the max values
        /// </summary>
        public static void ProcessSetEmptiesByMax(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            // TODO generate table for this
            SearchResult maxValueResult = GetMaxValue(values);

            int first = -1;

            List<FilledInfo> maxValueLocations = new List<FilledInfo>();

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

                        cells.SafeSet(leftEmptyCol, Form1.EMPTY);
                        cells.SafeSet(rightEmptyCol, Form1.EMPTY);

                        maxValueLocations.Add(new FilledInfo { CellIndexStart = first, Size = size });
                    }

                    i--;
                }
            }

            if (maxValueResult.Count > 0 && maxValueLocations.Count == maxValueResult.Count)
            {
                int sliceCount = maxValueResult.Count + 1;
                for (int area = 0; area < sliceCount; area++)
                {
                    int cellStart = area == 0 ? 0 : maxValueLocations[area - 1].CellIndexEnd + 2;
                    int cellEnd = area == maxValueResult.Count ? cells.Length - 1 : maxValueLocations[area].CellIndexStart - 1;

                    IEnumerable<int> selectedValuesIndices;

                    if (area == 0)
                        selectedValuesIndices = MyRange(0, maxValueResult.LocationIndices[area]);
                    else if (area == maxValueResult.Count)
                        selectedValuesIndices = MyRange(maxValueResult.LocationIndices[area - 1] + 1, values.Length);
                    else
                        selectedValuesIndices = MyRange(maxValueResult.LocationIndices[area - 1] + 1, maxValueResult.LocationIndices[area]);

                    int[] newValues = selectedValuesIndices.Select(x => values[x]).ToArray();

                    Form1.CellSeries slice = Form1.CellSeries.Slice(cells, cellStart, cellEnd, newValues);

                    ProcessAllAlgorithms(slice);
                    ProcessAllAlgorithms(Form1.CellSeries.Reverse(slice));
                }
            }
        }

        /// <summary>
        /// Finds small UNKNOWNs and if they are smaller then the smallest value, sets them EMPTY
        /// </summary>
        public static void ProcessFillBetweenEmpties(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            // TODO generate table for this
            int minValue = GetMinValue(values);

            int lastEmptyIndex = -1;

            for (int i = 0; i < cells.Length; i++)
            {
                byte cell = cells[i];

                if (cell == Form1.FILLED)
                    lastEmptyIndex = -1;
                else if (cell == Form1.EMPTY)
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

        /// <summary>
        /// Does process based on the greatest value. This includes:
        /// <para />
        /// Checking either side of the max value and if it is EMPTY and if so, filling the other way with FILLED and marking it
        /// <para />
        /// Checking for values on the left and right side of the maximum and if there is none, setting unreachable bytes EMPTY
        /// <para />
        /// Matching non-max filled parts with non-max values and setting unreachable bytes EMPTY accordingly
        /// <para />
        /// Merging the parts that are too big to belong to other parts
        /// </summary>
        public static void ProcessByMaxValues(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            if (values.Length >= 2)
            {
                SearchResult maxValueResult = GetMaxValue(values);

                int maxValue = maxValueResult.Value;
                int secondMaxValue = values.asIterable.Where(x => x != maxValue).DefaultIfEmpty(0).Max();

                int maxValueCountInValues = maxValueResult.Count;

                int filledStartIndex = -1;

                List<Range> greaterThanSecondMaxAreaList = new List<Range>();

                for (int i = 0; i < cells.Length; i++)
                {
                    byte cell = cells[i];

                    if (cell == Form1.FILLED && filledStartIndex < 0)
                    {
                        filledStartIndex = i;
                    }
                    else if (cell != Form1.FILLED && filledStartIndex >= 0)
                    {
                        int filledSize = i - filledStartIndex;
                        if (filledSize > secondMaxValue)
                        {
                            int filledEndIndex = i - 1;

                            greaterThanSecondMaxAreaList.Add(new Range(filledStartIndex, filledEndIndex, true));

                            int leftOfFilled = filledStartIndex - 1;
                            int rightOfFilled = filledEndIndex + 1;

                            if (leftOfFilled < 0 || cells[leftOfFilled] == Form1.EMPTY)
                            {
                                int k;
                                for (k = rightOfFilled; k < filledStartIndex + maxValue; k++)
                                {
                                    cells[k] = Form1.FILLED;
                                }
                                if (k < cells.Length)
                                    cells[k] = Form1.EMPTY;
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
                            }
                            else if (maxValueCountInValues == 1) // TODO it's possible to make this a loop
                            {
                                var leftValues = values.asIterable.TakeWhile(x => x != maxValue).ToList();
                                var rightValues = values.asIterable.SkipWhile(x => x < maxValue).Skip(1).ToList();

                                int reachRange = maxValue - filledSize;

                                if (leftValues.Count == 0)
                                {
                                    for (int k = 0; k < filledStartIndex - reachRange; k++)
                                        cells[k] = Form1.EMPTY;
                                }
                                else if (leftValues.Count == 1)
                                {
                                    List<Range> leftFilledList = FindFilledGroups(cells, 0, leftOfFilled - 1);

                                    if (leftFilledList.Count == 1)
                                    {
                                        Range leftFilled = leftFilledList[0];
                                        int leftVal = leftValues[0];

                                        // Checking for the possibility of merging
                                        if (leftFilled.start < filledStartIndex - reachRange)
                                        {
                                            int leftReach = leftVal - leftFilled.size;

                                            // Setting the empties on the left of the filled one on the left
                                            for (int k = 0; k < leftFilled.start - leftReach; k++)
                                                cells[k] = Form1.EMPTY;

                                            // Setting the empties in between
                                            for (int k = leftFilled.end + leftReach + 1; k < filledStartIndex - reachRange; k++)
                                                cells[k] = Form1.EMPTY;
                                        }
                                    }
                                }

                                if (rightValues.Count == 0)
                                {
                                    for (int k = rightOfFilled + reachRange; k < cells.Length; k++)
                                        cells[k] = Form1.EMPTY;
                                }
                                else if (rightValues.Count == 1)
                                {
                                    List<Range> rightFilledList = FindFilledGroups(cells, rightOfFilled + 1, cells.Length - 1);

                                    if (rightFilledList.Count == 1)
                                    {
                                        Range rightFilled = rightFilledList[0];
                                        int rightVal = rightValues[0];

                                        // Checking for the possibility of merging
                                        if (rightFilled.end > filledEndIndex + reachRange)
                                        {
                                            int rightReach = rightVal - rightFilled.size;

                                            // setting the empties on the right of the filled one on the right
                                            for (int k = rightFilled.end + rightReach + 1; k < cells.Length; k++)
                                                cells[k] = Form1.EMPTY;

                                            // setting the empties in between
                                            for (int k = filledEndIndex + reachRange + 1; k < rightFilled.start - rightReach; k++)
                                                cells[k] = Form1.EMPTY;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // TODO other process
                            }
                        }

                        filledStartIndex = -1;
                    }
                }

                // Below is merging the found filled parts
                if (greaterThanSecondMaxAreaList.Count > 1 && maxValueCountInValues == 1)
                {
                    for (int i = 0; i < greaterThanSecondMaxAreaList.Count - 1; i++)
                    {
                        Range thisRange = greaterThanSecondMaxAreaList[i];
                        Range nextRange = greaterThanSecondMaxAreaList[i + 1];

                        for (int k = thisRange.end + 1; k < nextRange.start; k++)
                            cells[k] = Form1.FILLED;
                    }
                }
            }
        }

        /// <summary>
        /// Checkes the first value range and if FILLED found fills the range by FILLED
        /// </summary>
        public static void TryMatchingFirstValue(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            if (values.Length > 0)
            {
                int firstValue = values[0];

                for (int k = 0; k < firstValue; k++)
                {
                    if (cells[k] == Form1.FILLED)
                    {
                        for (k++; k < firstValue; k++)
                        {
                            cells[k] = Form1.FILLED;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Tries to find FILLED in the range of the first value and if found, does processing accordingly
        /// </summary>
        public static void ProcessMatching(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            bool doContinue = false;
            int valueIndex = 0;
            int startIndex = 0;

            if (values.Length > 0)
            {
                do
                {
                    doContinue = false;

                    int val = values[valueIndex];

                    while (startIndex < cells.Length && cells[startIndex] == Form1.EMPTY)
                    {
                        startIndex++;
                    }

                    int filledFoundIndex = -1;

                    for (int i = 0; i <= val; i++)
                    {
                        if (cells.SafeCheck(i + startIndex, x => x == Form1.FILLED))
                        {
                            filledFoundIndex = i;
                            break;
                        }
                    }

                    if (filledFoundIndex >= 0)
                    {
                        // Dolunun arkasını doluyla doldurma (reach kadar)
                        for (int i = filledFoundIndex + 1; i < val; i++)
                            cells[i + startIndex] = Form1.FILLED;

                        int filledLastIndex = val - 1;

                        for (int i = filledLastIndex + 1; ; i++)
                        {
                            if (cells.SafeCheck(i + startIndex, x => x == Form1.FILLED))
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
                            cells.SafeSet(val + startIndex, Form1.EMPTY);

                            doContinue = true;
                            valueIndex++;
                            startIndex += val + 1;
                        }
                        else if (cells.SafeCheck(val + startIndex, x => x == Form1.EMPTY))  /// cells[val + startIndex] == Form1.EMPTY) // Doluların arkasının boş olma ihtimali
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

                            cells.SafeSet(emptyIndex1 + startIndex, Form1.EMPTY);
                            cells.SafeSet(emptyIndex2 + startIndex, Form1.EMPTY);

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
                            cells.SafeCheck(filledLastIndex + reachRange + 1 + startIndex, x => x == Form1.FILLED)) // Ulaşılamayacak yerin dolu olma ihtimali
                        {
                            bool containsEmpty = false;
                            int unknownCount = 0;
                            int unknownIndex = -1;

                            int checkStartIndex = filledLastIndex + 1;
                            for (int i = checkStartIndex; i < checkStartIndex + reachRange; i++)
                            {
                                byte cell = cells[i + startIndex];

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

                            if (reachRange > 0)
                            {
                                if (startIndex + filledFoundIndex - 1 >= 0)
                                    cells[startIndex + filledFoundIndex - 1] = Form1.FILLED;
                            }
                        }
                        else if (valueIndex + 1 < values.Length) // Sıradaki value da bulunursa
                        {
                            int nextVal = values[valueIndex + 1];

                            int lastCheckIndex = filledLastIndex + 1 + nextVal;
                            for (int i = filledLastIndex + reachRange + 1; i <= lastCheckIndex; i++)
                            {
                                byte cell = cells[i + startIndex];

                                if (cell == Form1.FILLED)
                                {
                                    for (int k = i + 1; k <= lastCheckIndex; k++)
                                        cells[k + startIndex] = Form1.FILLED;
                                }
                            }
                        }
                    }
                } while (doContinue && valueIndex < values.Length);
            }
        }

        /// <summary>
        /// Does processing by EMPTY seperated areas, which includes matching values with divided areas and slicing them
        /// </summary>
        public static void ProcessByDividedAreas(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            List<Range> areaList = FindDividedAreas(cells);

            if (areaList.Count >= 2)
            {
                int cellsLastIndex = cells.Length - 1;

                List<ColumnValue>[] forwardMatching = AssignMatchingForward(cells, areaList);
                List<ColumnValue>[] backwardMatching = AssignMatchingForward(Form1.CellSeries.Reverse(cells),
                    areaList
                    .Select(x => new Range(cellsLastIndex - x.end, cellsLastIndex - x.start, x.containsFilled))
                    .Reverse()
                    .ToList()
                    );

                int lastValueIndex = values.Length - 1;
                backwardMatching = backwardMatching
                    .Select(columnValueList =>
                    {
                        foreach (var item in columnValueList)
                        {
                            item.Index = lastValueIndex - item.Index;
                        }
                        columnValueList.Reverse();
                        return columnValueList;
                    })
                    .Reverse()
                    .ToArray();

                // Debug Example
                if (cells.cellColumnValues.asIterable.SequenceEqual(new int[] { 2, 1, 1, 1 }) && cells.asIterable.Where(x => x == Form1.FILLED).Count() == 6)
                    debug();

                bool doMatchPerfectly = true;

                // Check whether assignments match perfectly, if so we can process empty ones too
                for (int area = 0; area < areaList.Count; area++)
                {
                    var forwardValues = forwardMatching[area];
                    var backwardValues = backwardMatching[area];

                    if (!Enumerable.SequenceEqual(forwardValues, backwardValues))
                    {
                        doMatchPerfectly = false;
                        break;
                    }
                }

                // Below is comparing the two range matchings
                for (int area = 0; area < areaList.Count; area++)
                {
                    Range range = areaList[area];
                    var forwardValues = forwardMatching[area];
                    var backwardValues = backwardMatching[area];

                    bool forwardBackwardMatchCase = range.containsFilled && forwardValues.Any() && Enumerable.SequenceEqual(forwardValues, backwardValues);

                    bool indexOffByOneCase = range.containsFilled && forwardValues.Count == 1 && backwardValues.Count == 1 && forwardValues[0].Index - backwardValues[0].Index == 1;

                    if (forwardValues.Count == 0 && backwardValues.Count == 0 && (area == 0 || area == areaList.Count - 1))
                    {
                        Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, new int[] { });

                        ProcessAllAlgorithms(slice);
                    }

                    if (doMatchPerfectly || forwardBackwardMatchCase)
                    {
                        Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, forwardValues.Select(x => x.Value).ToArray());

                        ProcessAllAlgorithms(slice);
                        ProcessAllAlgorithms(Form1.CellSeries.Reverse(slice));
                    }

                    if (indexOffByOneCase)
                    {
                        int minValue = Min(forwardValues[0].Value, backwardValues[0].Value);
                        int maxValue = Max(forwardValues[0].Value, backwardValues[0].Value);

                        Form1.CellSeries initialProcessingSlice = Form1.CellSeries.Slice(cells, range.start, range.end, new int[] { minValue });
                        InitialProcessing(initialProcessingSlice);

                        if (range.size == maxValue + 1)
                        {
                            Form1.CellSeries singleProcessingSlice = Form1.CellSeries.Slice(cells, range.start, range.end, new int[] { maxValue });
                            ProcessSingles(singleProcessingSlice);
                        }
                    }
                    
                    int forwardMinRange = forwardValues.Sum(x => x.Value) + forwardValues.Count - 1;
                    int backwardMinRange = backwardValues.Sum(x => x.Value) + backwardValues.Count - 1;
                    if ((range.size == forwardMinRange || range.size == backwardMinRange) && range.containsFilled && backwardValues.Count - forwardValues.Count == 1 && forwardValues.Max(x => x.Index) - backwardValues.Min(x => x.Index) <= 1)
                    {
                        byte[] cellsCopy1 = Form1.CellSeries.Slice(cells, range.start, range.end, new int[] { }).asIterable.ToArray();
                        byte[] cellsCopy2 = cellsCopy1.ToArray();

                        var forwardCells = new Form1.CellSeries(cellsCopy1, forwardValues.Select(x => x.Value).ToArray());
                        var backwardCells = new Form1.CellSeries(cellsCopy2, backwardValues.Select(x => x.Value).ToArray());

                        ProcessAllAlgorithms(forwardCells);
                        ProcessAllAlgorithms(backwardCells);

                        for (int i = 0; i < cellsCopy1.Length; i++)
                        {
                            if (cellsCopy1[i] == cellsCopy2[i] && cellsCopy1[i] != Form1.UNKNOWN)
                                cells[range.start + i] = cellsCopy1[i];
                        }
                    }

                    {
                        // Below is for minimum matching
                        int[] newValues = GetCommonValues(forwardValues, backwardValues);

                        if (newValues.Any())
                        {
                            Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, newValues);

                            InitialProcessing(slice);
                        }

                        if ((cells[range.start] == Form1.FILLED || cells[range.end] == Form1.FILLED))
                        {
                            var uniqueValues = Enumerable.Concat(forwardValues.Select(x => x.Value), backwardValues.Select(x => x.Value)).Distinct().ToList();

                            int minOfAll = uniqueValues.Min();

                            if (uniqueValues.Count > 1 && minOfAll > 1)
                            {
                                Form1.CellSeries singleProcessingSlice = Form1.CellSeries.Slice(cells, range.start, range.end, new int[] { minOfAll });
                                ProcessStartSetFilledOnly(singleProcessingSlice);
                                ProcessStartSetFilledOnly(Form1.CellSeries.Reverse(singleProcessingSlice));
                            }

                            if (minOfAll == 1 && uniqueValues.Count == 1)
                            {
                                int maxIndex = forwardValues.Max(x => x.Index);
                                int minIndex = backwardValues.Min(x => x.Index);

                                if (maxIndex - minIndex <= 1)
                                {
                                    if (cells[range.start] == Form1.FILLED)
                                        cells.SafeSet(range.start + 1, Form1.EMPTY);
                                    else if (cells[range.end] == Form1.FILLED)
                                        cells.SafeSet(range.end - 1, Form1.EMPTY);
                                }
                            }

                            if (cells[range.start] == Form1.FILLED)
                            {
                                ColumnValue forwardFirst = forwardValues[0];
                                ColumnValue backwardFirst = backwardValues[0];

                                if (forwardFirst == backwardFirst && forwardFirst.Index - backwardFirst.Index <= 1)
                                {
                                    Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, new int[] { forwardFirst.Value });
                                    ProcessStartSetFilledOnly(slice);
                                }
                            }

                            if (cells[range.end] == Form1.FILLED)
                            {
                                ColumnValue forwardLast = forwardValues[forwardValues.Count - 1];
                                ColumnValue backwardLast = backwardValues[backwardValues.Count - 1];

                                if (forwardLast.Value == backwardLast.Value && forwardLast.Index - backwardLast.Index <= 1)
                                {
                                    Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, new int[] { forwardLast.Value });
                                    ProcessStartSetFilledOnly(Form1.CellSeries.Reverse(slice));
                                }
                            }
                        }

                        // Below is setting common values on the forward side
                        {
                            List<int> backwardCommonValues = new List<int>();
                            for (int i = 0; i < Min(forwardValues.Count, backwardValues.Count); i++)
                            {
                                if (forwardValues[forwardValues.Count - 1 - i].Index == backwardValues[backwardValues.Count - 1 - i].Index)
                                    backwardCommonValues.Insert(0, forwardValues[forwardValues.Count - 1 - i].Value);
                                else
                                    break;
                            }
                            if (backwardCommonValues.Count >= 2)
                            {
                                Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, backwardCommonValues.ToArray());

                                slice = Form1.CellSeries.Reverse(slice);

                                ProcessSpecialCases(slice);
                            }
                        }

                        // Below is setting common values on the backward side
                        {
                            List<int> forwardCommonValues = new List<int>();
                            for (int i = 0; i < Min(forwardValues.Count, backwardValues.Count); i++)
                            {
                                if (forwardValues[i].Index == backwardValues[i].Index)
                                    forwardCommonValues.Add(forwardValues[i].Value);
                                else
                                    break;
                            }
                            if (forwardCommonValues.Count >= 2)
                            {
                                Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, forwardCommonValues.ToArray());
                                ProcessSpecialCases(slice);
                            }
                        }
                    }
                }

                // Below is matching filled areas with values
                List<Range> filledContainingAreas = areaList.Where(x => x.containsFilled).ToList();

                if (filledContainingAreas.Count == values.Length)
                {
                    for (int area = 0; area < filledContainingAreas.Count; area++)
                    {
                        Range range = filledContainingAreas[area];

                        int[] newValues = new int[] { values[area] };

                        Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, newValues);

                        ProcessAllAlgorithms(slice);
                        ProcessAllAlgorithms(Form1.CellSeries.Reverse(slice));
                    }
                }
            }
        }

        private static List<ColumnValue>[] AssignMatchingForward(Form1.CellSeries cells, List<Range> areaList)
        {
            var values = cells.cellColumnValues;

            List<ColumnValue>[] forwardMatching = Enumerable.Repeat(new List<ColumnValue>(), areaList.Count).ToArray();

            int loopValueIndex = 0;
            for (int area = 0; area < forwardMatching.Length; area++)
            {
                bool willCheckThis = false;

                Range range = areaList[area];

                int valueIndex = loopValueIndex;

                if (valueIndex < values.Length)
                {
                    int currentSize = values[valueIndex];

                    while (cells.SafeCheck(range.start + currentSize, x => x == Form1.FILLED))
                        currentSize++;

                    bool added = false;
                    if (currentSize <= range.size)
                    {
                        valueIndex++; // Increasing this means adding the value to assignment
                        added = true;
                    }

                    if (added && cells.SafeCheck(range.start + currentSize - values[valueIndex - 1] - 1, x => x == Form1.FILLED))
                    {
                        willCheckThis = true;
                    }

                    while (valueIndex < values.Length)
                    {
                        int value = values[valueIndex];
                        currentSize += value + 1;

                        while (cells.SafeCheck(range.start + currentSize, x => x == Form1.FILLED))
                            currentSize++;

                        if (currentSize > range.size)
                            break;

                        int filledCount = MyRange(currentSize - value, currentSize).Select(x => cells.SafeCheck(range.start + x, a => a == Form1.FILLED)).Count(x => x == true);

                        if (filledCount == value)
                        {
                            int i = currentSize - value - 1;

                            while (cells.SafeCheck(range.start + i, x => x == Form1.FILLED))
                            {
                                i--;
                                filledCount++;
                            }
                        }

                        if (filledCount > value)
                        {
                            //break;
                            //valueIndex--;
                            //shouldAdd = false;
                            // DO NOT ADD IN THIS CASE
                            continue;
                        }
                        else if (currentSize <= range.size)
                        {
                            valueIndex++; // Increasing this means adding the value to assignment
                        }
                    }

                    int assignedValueCount = valueIndex - loopValueIndex;

                    // Below is for error checking
                    if (valueIndex == values.Length && assignedValueCount == 1)
                    {
                        for (int i = range.start + currentSize; i < range.end; i++)
                        {
                            if (cells[i] == Form1.FILLED)
                            {
                                int firstFilledIndex = -1;
                                int lastFilledIndex = -1;

                                for (i = range.start; i <= range.end; i++)
                                {
                                    if (cells[i] == Form1.FILLED)
                                    {
                                        firstFilledIndex = i;
                                        break;
                                    }
                                }

                                for (i = range.end; i >= range.start; i--)
                                {
                                    if (cells[i] == Form1.FILLED)
                                    {
                                        lastFilledIndex = i;
                                        break;
                                    }
                                }

                                int filledrange = lastFilledIndex - firstFilledIndex + 1;

                                int assigedValue = values[loopValueIndex];

                                if (filledrange > assigedValue)
                                {
                                    willCheckThis = true;
                                    break;
                                }
                            }
                        }
                    }

                    List<ColumnValue> assignedValues = MyRange(loopValueIndex, valueIndex)
                        .Select(i => new ColumnValue { Value = values[i], Index = i })
                        .ToList();

                    forwardMatching[area] = assignedValues;
                }
                else
                {
                    forwardMatching[area] = new List<ColumnValue>();
                }

                //-------------------------------

                List<ColumnValue> valuesChecked = forwardMatching[area];
                Range rangeChecked = range;
                int areaChecked = area;

                while (true)
                {
                    bool checkNext;

                    List<ColumnValue> formerAssignment = null;

                    int areaForLoop = -1;

                    if (willCheckThis || valuesChecked.Sum(x => x.Value) < GetFilledCount(cells, rangeChecked))
                    {
                        areaForLoop = areaChecked;

                        // Find the assignment with values
                        do
                        {
                            areaForLoop--;
                            formerAssignment = forwardMatching[areaForLoop];
                        } while (formerAssignment.Count <= 0);

                        valueIndex = formerAssignment[formerAssignment.Count - 1].Index;
                        formerAssignment.RemoveAt(formerAssignment.Count - 1);

                        int areaMoved = areaChecked - areaForLoop;
                        area -= areaMoved;

                        checkNext = true;

                        if (willCheckThis)
                            willCheckThis = false;
                    }
                    else
                        checkNext = false;

                    if (checkNext)
                    {
                        valuesChecked = formerAssignment;
                        areaChecked = areaForLoop;
                        rangeChecked = areaList[areaChecked];
                        continue;
                    }
                    else
                        break;
                }

                loopValueIndex = valueIndex;
            }

            return forwardMatching;
        }

        /// <summary>
        /// Finds filled ranges and does processing including:
        /// <para />
        /// If all the filled ranges are 1s and all the values except the first one are 1s, marks the 1s that are out of the range of the first value
        /// <para />
        /// If all the values are the same, marks the filled ranges that match the values
        /// <para />
        /// Tries merging and if cannot merge, sets definite EMPTY bytes
        /// </summary>
        public static void ProcessByFilledRanges(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            List<Range> filledRanges = FindFilledGroups(cells, 0, cells.Length - 1);

            if (filledRanges.Count >= 2
                && filledRanges.All(x => x.size == 1)
                && values.asIterable.Skip(1).All(x => x == 1)
                && filledRanges[1].end - filledRanges[0].start + 1 > values.asIterable.First()
                )
            {
                for (int i = 1; i < filledRanges.Count; i++)
                {
                    Range lastRange = filledRanges[i];

                    cells.SafeSet(lastRange.start - 1, Form1.EMPTY);
                    cells.SafeSet(lastRange.end + 1, Form1.EMPTY);
                }
            }

            FillBetweenFilled(cells, values.asIterable.ToArray(), filledRanges);

            TryMerging(cells, values.asIterable.ToArray(), filledRanges);

            MarkIfValuesAreAllTheSame(cells, values.asIterable, filledRanges);
        }

        /// <summary>
        /// Does initial processing by looking for extra FILLED values and finding the real range
        /// </summary>
        public static void ProcessInitialByMatchingFilled(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            // i : size covered
            int i = 0;

            for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                int val = values[valueIndex];

                // This whole loop skips empties and really small empties
                bool willContinue = true;
                while (willContinue)
                {
                    willContinue = false;

                    // Check for empties in this range
                    for (int k = val - 1; k >= 0; k--)
                    {
                        if (cells.SafeCheck(k + i, x => x == Form1.EMPTY))
                        {
                            i += k + 1;
                            willContinue = true;
                            break;
                        }
                    }
                }

                //if (!emptyFound && valueIndex != 0)
                //    i++;

                if (cells.SafeCheck(i + val, x => x == Form1.FILLED))
                {
                    int k = i + val + 1;

                    while (cells.SafeCheck(k, x => x == Form1.FILLED))
                    {
                        k++;
                    }

                    int lastFilled = k - 1;

                    var slice = Form1.CellSeries.Slice(cells, lastFilled + 2, cells.Length - 1, values.asIterable.Skip(valueIndex + 1).ToArray());

                    InitialProcessing(slice);
                }

                i += val + 1;
            }
        }

        /// <summary>
        /// Does processing for extremely rare cases, such as:
        /// <para />
        /// When the first value is a 1 and the third byte is FILLED, setting second byte as EMPTY
        /// </summary>
        public static void ProcessSpecialCases(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            if (values.Length > 0 && values[0] == 1 && cells.SafeCheck(2, x => x == Form1.FILLED))
            {
                cells[1] = Form1.EMPTY;
            }

            if (values.Length > 1 && values[0] == 1 && values[1] == 1 && cells.SafeCheck(4, x => x == Form1.FILLED))
            {
                cells[3] = Form1.EMPTY;
            }
        }

        private static void debug() { }

        #region Private Methods

        private static List<Range> FindFilledGroups(Form1.CellSeries cells, int start, int end)
        {
            List<Range> filledRanges = new List<Range>();

            int filledStartIndex = -1;
            for (int i = start; i <= end; i++)
            {
                byte cell = cells[i];

                if (cell == Form1.FILLED && filledStartIndex < 0)
                {
                    filledStartIndex = i;
                }
                else if (cell != Form1.FILLED && filledStartIndex >= 0)
                {
                    filledRanges.Add(new Range(filledStartIndex, i - 1, true));
                    filledStartIndex = -1;
                }
            }

            if (filledStartIndex > 0)
            {
                filledRanges.Add(new Range(filledStartIndex, end, true));
                filledStartIndex = -1;
            }

            return filledRanges;
        }

        private static List<Range> FindDividedAreas(Form1.CellSeries cells)
        {
            List<Range> areaList = new List<Range>();

            int nonEmpty = -1;
            bool containsFilled = false;
            for (int i = 0; i < cells.Length; i++)
            {
                byte cell = cells[i];

                if (cell == Form1.FILLED)
                    containsFilled = true;

                if (cell != Form1.EMPTY && nonEmpty < 0)
                {
                    nonEmpty = i;
                }
                else if (cell == Form1.EMPTY && cells.SafeCheck(i - 1, x => x != Form1.EMPTY) && nonEmpty >= 0)
                {
                    areaList.Add(new Range(nonEmpty, i - 1, containsFilled));
                    nonEmpty = -1;
                    containsFilled = false;
                }
            }

            if (nonEmpty > 0)
            {
                areaList.Add(new Range(nonEmpty, cells.Length - 1, containsFilled));
                nonEmpty = -1;
            }

            return areaList;
        }

        private static int GetMinValue(Form1.CellColumnValues values)
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

        private static int GetFilledCount(Form1.CellSeries cells, Range range)
        {
            return MyRange(range.start, range.end + 1).Select(x => cells[x]).Where(x => x == Form1.FILLED).Count();
        }

        private static int Max(int a, int b)
        {
            return a > b ? a : b;
        }

        private static int Min(int a, int b)
        {
            return a < b ? a : b;
        }

        /// <summary>
        /// Does processing based on conditions
        /// </summary>
        private static void FillBetweenFilled(Form1.CellSeries cells, int[] values, List<Range> filledRanges)
        {
            if (filledRanges.Count == values.Length + 1)
            {
                List<int> possibleMergedAreas = new List<int>();

                for (int rangeIndex = 0; rangeIndex < filledRanges.Count - 1; rangeIndex++)
                {
                    Range thisRange = filledRanges[rangeIndex];
                    Range nextRange = filledRanges[rangeIndex + 1];

                    int val = values[rangeIndex];

                    int start = thisRange.start;
                    int end = nextRange.end;

                    int mergedSize = end - start + 1;

                    if (mergedSize <= val)
                        possibleMergedAreas.Add(rangeIndex);
                }

                if (possibleMergedAreas.Count == 0)
                    throw new Exception("You messed up the candidates");
                else if (possibleMergedAreas.Count == 1)
                {
                    int rangeIndex = possibleMergedAreas[0];

                    Range thisRange = filledRanges[rangeIndex];
                    Range nextRange = filledRanges[rangeIndex + 1];

                    for (int i = thisRange.end + 1; i < nextRange.start; i++)
                        cells[i] = Form1.FILLED;
                }
                else
                {
                    // TODO write some more code if necessary
                }
            }

            if (filledRanges.Count == values.Length)
            {
                int violatingValueIndex = -1;

                for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
                {
                    if (filledRanges[valueIndex].size > values[valueIndex])
                    {
                        violatingValueIndex = valueIndex;
                        break;
                    }
                }

                if (violatingValueIndex >= 0)
                {
                    int[] newValues = values.Where((e, i) => i != violatingValueIndex).ToArray();

                    FillBetweenFilled(cells, newValues, filledRanges);
                }
                else
                {
                    bool canMerge = TryMerging(cells, values, filledRanges);

                    if (!canMerge)
                    {
                        for (int areaIndex = 0; areaIndex <= filledRanges.Count; areaIndex++)
                        {
                            Range leftRange = areaIndex == 0 ? null : filledRanges[areaIndex - 1];
                            Range rightRange = areaIndex == filledRanges.Count ? null : filledRanges[areaIndex];
                            int? leftVal = areaIndex == 0 ? (int?)null : values[areaIndex - 1];
                            int? rightVal = areaIndex == filledRanges.Count ? (int?)null : values[areaIndex];

                            int startIndex = leftRange == null ? 0 : leftRange.end + (leftVal.Value - leftRange.size) + 1;
                            int quitIndex = rightRange == null ? cells.Length : rightRange.start - (rightVal.Value - rightRange.size);

                            for (int i = startIndex; i < quitIndex; i++)
                                cells[i] = Form1.EMPTY;
                        }
                    }
                }
            }

            if (filledRanges.Count == values.Length - 1)
            {
                bool canMerge = TryMerging(cells, values, filledRanges);

                if (!canMerge)
                {
                    for (int i = 0; i < filledRanges.Count - 1; i++)
                    {
                        Range thisRange = filledRanges[i];
                        int thisVal = values[i];
                        int nextVal = values[i + 1];

                        if (thisVal == nextVal && nextVal == thisRange.size)
                        {
                            if (thisRange.start - 1 >= 0)
                                cells[thisRange.start - 1] = Form1.EMPTY;
                            if (thisRange.end + 1 < cells.Length)
                                cells[thisRange.end + 1] = Form1.EMPTY;
                        }
                    }
                }
            }

            if (filledRanges.Count < values.Length && filledRanges.Count > 0)
            {
                // Forward candidate matching
                List<ColumnValueExtended>[] forwardFilledCandidates = GetFilledMatchingCandidates(cells, values, filledRanges);

                int cellsLastIndex = cells.Length - 1;
                // New Backward candidate matching
                List<ColumnValueExtended>[] backwardFilledCandidates = GetFilledMatchingCandidates(
                    Form1.CellSeries.Reverse(cells), values.Reverse().ToArray(),
                    filledRanges.Select(x => new Range(cellsLastIndex - x.end, cellsLastIndex - x.start, x.containsFilled)).OrderBy(x => x.start).ToList()
                );

                backwardFilledCandidates = backwardFilledCandidates
                    .Select(candList => candList
                        .Select(x => new ColumnValueExtended { Index = values.Length - 1 - x.Index, Value = x.Value, CanMarkAfter = x.CanMarkAfter, CanMarkBefore = x.CanMarkBefore })
                        .ToList()
                    )
                    .Reverse()
                    .ToArray();

                for (int i = 0; i < filledRanges.Count; i++)
                {
                    Range filledRange = filledRanges[i];
                    List<ColumnValueExtended> forwardCandidates = forwardFilledCandidates[i];
                    List<ColumnValueExtended> backwardCandidates = backwardFilledCandidates[i];

                    if (forwardCandidates.Count > 0 && backwardCandidates.Count > 0)
                    {
                        int forwardMinIndex = forwardCandidates.Min(x => x.Index);
                        int backwardMinIndex = backwardCandidates.Min(x => x.Index);

                        int forwardMaxIndex = forwardCandidates.Max(x => x.Index);
                        int backwardMaxIndex = backwardCandidates.Max(x => x.Index);

                        int minValueIndex = forwardMinIndex < backwardMinIndex ? forwardMinIndex : backwardMinIndex;
                        int maxValueIndex = forwardMaxIndex >= backwardMaxIndex ? forwardMaxIndex : backwardMaxIndex;

                        var newValues = MyRange(minValueIndex, maxValueIndex + 1).Select(x => values[x]).ToList();

                        // TODO write some more code if necessary

                        if (newValues.All(x => x == filledRange.size))
                        {
                            cells.SafeSet(filledRange.start - 1, Form1.EMPTY);
                            cells.SafeSet(filledRange.end + 1, Form1.EMPTY);
                        }

                        if (i + 1 < filledRanges.Count)
                        {
                            List<ColumnValueExtended> nextForwardCandidates = forwardFilledCandidates[i + 1];
                            List<ColumnValueExtended> nextBackwardCandidates = backwardFilledCandidates[i + 1];

                            if (forwardCandidates.Count == 1 &&
                                backwardCandidates.Count == 1 &&
                                forwardCandidates[0] == backwardCandidates[0] &&
                                nextForwardCandidates.Count == 1 &&
                                nextBackwardCandidates.Count == 1 &&
                                nextForwardCandidates[0] == nextBackwardCandidates[0]
                            )
                            {
                                int thisIndex = forwardCandidates[0].Index;
                                int nextIndex = nextForwardCandidates[0].Index;

                                int thisValue = values[thisIndex];
                                int nextValue = values[nextIndex];

                                filledRange = filledRanges[i];
                                Range nextFilledRange = filledRanges[i + 1];

                                int cellSize = nextFilledRange.end - filledRange.start + 1;

                                if (cellSize > thisValue + nextValue && nextIndex - thisIndex == 1)
                                {
                                    Form1.CellSeries slice = Form1.CellSeries.Slice(cells, filledRange.start, nextFilledRange.end, new int[] { thisValue, nextValue });
                                    SetMiddleUnreachables(slice);
                                }
                            }
                        }
                    }

                    if (forwardCandidates.Count == 1 && backwardCandidates.Count == 1)
                    {
                        var forward = forwardCandidates[0];
                        var backward = backwardCandidates[0];

                        int indexOff = forward.Index - backward.Index;

                        if (indexOff == 1 || indexOff == 0)
                        {
                            if (forward.CanMarkBefore && backward.CanMarkAfter)
                            {
                                cells.SafeSet(filledRange.start - 1, Form1.EMPTY);
                            }

                            if (forward.CanMarkAfter && backward.CanMarkBefore)
                            {
                                cells.SafeSet(filledRange.end + 1, Form1.EMPTY);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does processing regargless of conditions
        /// </summary>
        private static bool TryMerging(Form1.CellSeries cells, int[] values, List<Range> filledRanges)
        {
            bool canMerge = false;

            for (int i = 1; i < filledRanges.Count; i++)
            {
                Range thisRange = filledRanges[i - 1];
                Range nextRange = filledRanges[i];

                int mergedSize = nextRange.end - thisRange.start + 1;

                bool isPossibleToMerge = values.Any(x => x >= mergedSize);

                // If the ranges are one apart and cannot merge, set the between to EMPTY
                if (nextRange.start - thisRange.end == 2)
                {
                    if (!isPossibleToMerge)
                        cells[nextRange.start - 1] = Form1.EMPTY;
                }

                // This part sets the further side of the 1 with EMPTY
                if (values.Length == 2 && values.Any(x => x == mergedSize) && values.Any(x => x == 1))
                {
                    if (values[0] == 1)
                    {
                        cells.SafeSet(thisRange.start - 1, Form1.EMPTY);
                    }
                    else if (values[1] == 1)
                    {
                        cells.SafeSet(nextRange.end + 1, Form1.EMPTY);
                    }
                }

                if (isPossibleToMerge)
                    canMerge = true;
            }

            return canMerge;
        }

        /// <summary>
        /// Does processing if all the values are the same
        /// </summary>
        private static void MarkIfValuesAreAllTheSame(Form1.CellSeries cells, IEnumerable<int> values, List<Range> filledRanges)
        {
            int[] distinctValues = values.Distinct().ToArray();

            if (distinctValues.Length == 1)
            {
                int val = distinctValues[0];

                foreach (var range in filledRanges)
                {
                    if (range.size == val)
                    {
                        cells.SafeSet(range.start - 1, Form1.EMPTY);
                        cells.SafeSet(range.end + 1, Form1.EMPTY);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the starting range and sets FILLED values if it can, this is a trimmed-down version of the function "ProcessStart"
        /// </summary>
        private static void ProcessStartSetFilledOnly(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;
            int valuesIndex = 0;

            int i;
            for (i = 0; i < cells.Length; i++)
            {
                byte cell = cells[i];

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

                    break;
                }
                else if (cell == Form1.EMPTY)
                {

                }
            }
        }

        private static void SetMiddleUnreachables(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            if (values.Length == 2 && cells[0] == Form1.FILLED && cells[cells.Length - 1] == Form1.FILLED)
            {
                int startValue = values[0];
                int endValue = values[1];

                for (int i = startValue; i < cells.Length - endValue; i++)
                {
                    cells[i] = Form1.EMPTY;
                }
            }
        }

        private static List<ColumnValueExtended>[] GetFilledMatchingCandidates(Form1.CellSeries cells, int[] values, List<Range> filledRanges)
        {
            List<ColumnValueExtended>[] forwardFilledCandidates = Enumerable.Range(0, filledRanges.Count).Select(x => new List<ColumnValueExtended>()).ToArray();

            int i = 0;

            int valueIndex = 0;
            int filledRangeIndex = 0;

            for (; valueIndex < values.Length && filledRangeIndex < filledRanges.Count; valueIndex++)
            {
                int val = values[valueIndex];
                Range range = filledRanges[filledRangeIndex];

                // Skip the all and possible empties
                bool doContinue = true;
                while (doContinue)
                {
                    doContinue = false;
                    for (int k = val - 1; k >= 0; k--)
                    {
                        if (cells[i + k] == Form1.EMPTY)
                        {
                            i += k + 1;
                            doContinue = true;
                            break;
                        }
                    }
                }

                // Check one over for filled
                while (cells.SafeCheck(i + val, x => x == Form1.FILLED))
                {
                    i++;
                }

                while (true)
                {
                    if (filledRangeIndex >= filledRanges.Count)
                        break;

                    range = filledRanges[filledRangeIndex];

                    if (range.start >= i && range.end < i + val)
                    {
                        if (filledRangeIndex == 0)
                        {
                            var formerValues = Enumerable.Range(0, valueIndex + 1).Select(x => values[x]);

                            if (formerValues.All(x => x == range.size))
                            {
                                cells.SafeSet(range.start - 1, Form1.EMPTY);
                                cells.SafeSet(range.end + 1, Form1.EMPTY);
                            }
                        }

                        forwardFilledCandidates[filledRangeIndex].Add(new ColumnValueExtended { Index = valueIndex, Value = val, CanMarkBefore = range.start == i, CanMarkAfter = val == 1 });
                        filledRangeIndex++;
                    }
                    else
                        break;
                }

                i += 1 + val;
            }

            return forwardFilledCandidates;
        }

        private static SearchResult GetMaxValue(Form1.CellColumnValues values)
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

        private static int[] GetCommonValues(List<ColumnValue> forward, List<ColumnValue> backward)
        {
            HashSet<int> commonIndices = new HashSet<int>();

            foreach (var item in forward)
            {
                if (backward.Any(x => x.Index == item.Index))
                    commonIndices.Add(item.Index);
            }

            foreach (var item in backward)
            {
                if (forward.Any(x => x.Index == item.Index))
                    commonIndices.Add(item.Index);
            }

            return commonIndices.Select(i => forward.First(e => e.Index == i).Value).ToArray();
        }

        #endregion

        #region Extensions
        public static int rowCount(this byte[,] picture)
        {
            return picture.GetLength(0);
        }

        public static int colCount(this byte[,] picture)
        {
            return picture.GetLength(1);
        }
        #endregion
    }

    public class Range
    {
        public int start;
        public int end;
        public int size { get { return end - start + 1; } }
        public bool containsFilled;

        public Range(int start, int end, bool containsFilled)
        {
            this.start = start;
            this.end = end;
            this.containsFilled = containsFilled;
        }

        public override string ToString()
        {
            return "{start: " + start + ", end: " + end + "}";
        }
    }

    public class SearchResult
    {
        public int Value { get; set; }
        public int Count { get { return LocationIndices.Length; } }

        public int[] LocationIndices { get; set; }
    }

    public class FilledInfo
    {
        public int CellIndexStart { get; set; }
        public int CellIndexEnd { get { return CellIndexStart + Size - 1; } }
        public int Size { get; set; }
    }

    public class ColumnValue : IEquatable<ColumnValue>
    {
        public int Index { get; set; }
        public int Value { get; set; }

        public bool Equals(ColumnValue ex)
        {
            return this.Index == ex.Index
                && this.Value == ex.Value;
        }

        public override string ToString()
        {
            return "{Index: " + Index + ", Value: " + Value + "}";
        }
    }

    public class ColumnValueExtended : ColumnValue
    {
        public bool CanMarkBefore { get; set; }
        public bool CanMarkAfter { get; set; }
    }

    public class TestCase
    {
        public string CellsString { get; set; }

        public int[] Values { get; set; }

        public int[][] CorrectAssignment { get; set; }
    }
}
