using System;
using System.Collections.Generic;
using System.Linq;
using CasualConsole;

namespace PicrossSolver
{
    public static class Generic
    {
        public static void ProcessAllAlgorithms(Form1.CellSeries cells)
        {
            if (cells.cellColumnValues.asIterable.Any())
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
                FillBetweenFilled(cells);
            }
            else
            {
                EmptyAll(cells);
            }
        }

        public static void EmptyAll(Form1.CellSeries cells)
        {
            for (int i = 0; i < cells.Length; i++)
                cells[i] = Form1.EMPTY;
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
                    int val = values[valueIndex];

                    valueIndex++;

                    startIndex += val;
                }
                else
                    break;
            } while (true);

            int unknownCount = 0;

            int i;
            for (i = startIndex; i < cells.Length; i++)
            {
                int cell = cells[i];

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

            if (unknownCount > 0 && valueIndex < values.Length && unknownCount < values[valueIndex])
            {
                for (int j = 0; j < unknownCount; j++)
                    cells[j + startIndex] = Form1.EMPTY;
            }

            // Above is first check
            // Below is second check
            if (unknownCount > 0)
            {
                int secondUnknownCount = 0;

                while (i < cells.Length && cells[i] == Form1.EMPTY)
                    i++;

                startIndex = i;
                for (; i < cells.Length; i++)
                {
                    int cell = cells[i];

                    if (cell == Form1.UNKNOWN)
                        secondUnknownCount++;
                    else if (cell == Form1.FILLED)
                    {
                        secondUnknownCount = 0;
                        break;
                    }
                    else if (cell == Form1.EMPTY)
                        break;
                }

                if (secondUnknownCount > 0 &&
                    valueIndex + 1 < values.Length &&
                    secondUnknownCount < values[valueIndex] &&
                    secondUnknownCount < values[valueIndex + 1] &&
                    unknownCount < values[valueIndex] + values[valueIndex + 1] + 1)
                {
                    for (int j = 0; j < secondUnknownCount; j++)
                        cells[j + startIndex] = Form1.EMPTY;
                }
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

                    ProcessAllAlgorithms(slice);
                    ProcessAllAlgorithms(Form1.CellSeries.Reverse(slice));
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
                    int val = values[valueIndex];

                    valueIndex++;

                    startIndex += val;
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
                int maxValue = values.asIterable.Max();
                int secondMaxValue = values.asIterable.Where(x => x != maxValue).DefaultIfEmpty(0).Max();

                int maxCount = values.asIterable.Count(x => x == maxValue);

                int filledIndex = -1;

                List<Range> greaterThanSecondMaxList = new List<Range>();

                for (int i = 0; i < cells.Length; i++)
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

                            greaterThanSecondMaxList.Add(new Range(filledIndex, filledEndIndex, true));

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
                            else if (maxCount == 1) // TODO it's possible to make this a loop
                            {
                                var leftValues = values.asIterable.TakeWhile(x => x != maxValue).ToList();
                                var rightValues = values.asIterable.SkipWhile(x => x < maxValue).Skip(1).ToList();

                                int reachRange = maxValue - filledSize;

                                if (!leftValues.Any())
                                {
                                    for (int k = 0; k < filledIndex - reachRange; k++)
                                        cells[k] = Form1.EMPTY;
                                }
                                else if (leftValues.Count == 1)
                                {
                                    List<Range> leftFilledList = FindFilledGroups(cells, 0, leftOfFilled - 1);

                                    if (leftFilledList.Count == 1)
                                    {
                                        Range leftFilled = leftFilledList[0];
                                        int leftVal = leftValues[0];

                                        if (leftFilled.start < filledIndex - reachRange)
                                        {
                                            int leftReach = leftVal - leftFilled.size;

                                            // setting the empties on the left of the filled one on the left
                                            for (int k = 0; k < leftFilled.start - leftReach; k++)
                                                cells[k] = Form1.EMPTY;

                                            // setting the empties in between
                                            for (int k = leftFilled.end + leftReach + 1; k < filledIndex - reachRange; k++)
                                                cells[k] = Form1.EMPTY;
                                        }
                                    }
                                }

                                if (!rightValues.Any())
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

                        filledIndex = -1;
                    }
                }

                if (greaterThanSecondMaxList.Count > 1 && maxCount == 1)
                {
                    for (int i = 0; i < greaterThanSecondMaxList.Count - 1; i++)
                    {
                        Range thisRange = greaterThanSecondMaxList[i];
                        Range nextRange = greaterThanSecondMaxList[i + 1];

                        for (int k = thisRange.end + 1; k < nextRange.start; k++)
                            cells[k] = Form1.FILLED;
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
            bool containsFilled = false;
            for (int i = 0; i < cells.Length; i++)
            {
                int cell = cells[i];

                if (cell == Form1.FILLED)
                    containsFilled = true;

                if (cell != Form1.EMPTY && nonEmpty < 0)
                {
                    nonEmpty = i;
                }
                else if (cell == Form1.EMPTY && i - 1 >= 0 && cells[i - 1] != Form1.EMPTY && nonEmpty >= 0)
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

            // Above is preparing ranges
            // Below is range-values matching

            if (areaList.Count > 0)
            {
                ColumnValue[][] forwardMatching = new ColumnValue[areaList.Count][];
                ColumnValue[][] backwardMatching = new ColumnValue[areaList.Count][];

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

                        forwardMatching[area] = MyRange(loopValueIndex, valueIndex)
                            .Select(i => new ColumnValue { Value = values[i], Index = i })
                            .ToArray();
                    }
                    else
                    {
                        forwardMatching[area] = new ColumnValue[] { };
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

                        backwardMatching[area] = MyRangeDesc(loopValueIndex, valueIndex)
                            .Select(i => new ColumnValue { Value = values[i], Index = i })
                            .Reverse()
                            .ToArray();
                    }
                    else
                    {
                        backwardMatching[area] = new ColumnValue[] { };
                    }

                    loopValueIndex = valueIndex;
                }

                // Below is comparing the two range matchings
                for (int area = 0; area < areaList.Count; area++)
                {
                    Range range = areaList[area];
                    var forwardValues = forwardMatching[area];
                    var backwardValues = backwardMatching[area];

                    if (range.containsFilled && forwardValues.Any() && Enumerable.SequenceEqual(forwardValues, backwardValues))
                    {
                        Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, forwardValues.Select(x => x.Value).ToArray());

                        ProcessAllAlgorithms(slice);
                        ProcessAllAlgorithms(Form1.CellSeries.Reverse(slice));
                    }
                    else
                    {
                        // Below is for minimum matching
                        int[] newValues = ColumnValue.GetCommon(forwardValues, backwardValues);

                        if (newValues.Any())
                        {
                            Form1.CellSeries slice = Form1.CellSeries.Slice(cells, range.start, range.end, newValues);

                            InitialProcessing(slice);
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

        public static void FillBetweenFilled(Form1.CellSeries cells)
        {
            var values = cells.cellColumnValues;

            List<Range> filledRanges = FindFilledGroups(cells, 0, cells.Length - 1);

            FillBetweenFilled(cells, values.asIterable.ToArray(), filledRanges);
        }

        private static void FillBetweenFilled(Form1.CellSeries cells, int[] values, List<Range> filledRanges)
        {
            if (filledRanges.Count == values.Length + 1)
            {
                List<int> candidates = new List<int>();

                for (int rangeIndex = 0; rangeIndex < filledRanges.Count - 1; rangeIndex++)
                {
                    Range thisRange = filledRanges[rangeIndex];
                    Range nextRange = filledRanges[rangeIndex + 1];

                    int val = values[rangeIndex];

                    int start = thisRange.start;
                    int end = nextRange.end;

                    int rangeSize = end - start + 1;

                    if (rangeSize <= val)
                        candidates.Add(rangeIndex);
                }

                if (candidates.Count == 0)
                    throw new Exception("You messed up the candidates");
                else if (candidates.Count == 1)
                {
                    int rangeIndex = candidates[0];

                    Range thisRange = filledRanges[rangeIndex];
                    Range nextRange = filledRanges[rangeIndex + 1];

                    for (int i = thisRange.end + 1; i < nextRange.start; i++)
                        cells[i] = Form1.FILLED;
                }
                else
                {
                    // TODO process sometime
                }
            }
            else if (filledRanges.Count == values.Length)
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
            }
            else if (filledRanges.Count < values.Length && filledRanges.Count > 0)
            {
                if (Form1.iteration == 1 && cells.rowOrCol == 6 && cells.direction == Form1.Direction.Horizontal)
                    debug();

                // Forward candidate matching
                List<int>[] forwardFilledCandidates = Enumerable.Range(0, filledRanges.Count).Select(x => new List<int>()).ToArray();
                {
                    int filledRangeIndex = 0;

                    int valueIndex = 0;

                    int i = -1;

                    while (filledRangeIndex < filledRanges.Count && valueIndex < values.Length)
                    {
                        Range filledRange = filledRanges[filledRangeIndex];

                        int val = values[valueIndex];
                        i += val + 1;

                        if (i >= filledRange.start)
                        {
                            forwardFilledCandidates[filledRangeIndex].Add(valueIndex);

                            filledRangeIndex++;
                            i = filledRange.end + 1;
                        }

                        valueIndex++;
                    }
                }

                forwardFilledCandidates.Each((e, i) =>
                {
                    if (e.Count == 0)
                    {
                        int indexDiff = forwardFilledCandidates.Length - 1 - i;

                        e.Add(values.Length - 1 - indexDiff);
                    }
                });

                // Backward candidate matching
                List<int>[] backwardFilledCandidates = Enumerable.Range(0, filledRanges.Count).Select(x => new List<int>()).ToArray();
                {
                    int filledRangeIndex = filledRanges.Count - 1;

                    int valueIndex = values.Length - 1;

                    int i = cells.Length + 1;

                    while (filledRangeIndex >= 0 && valueIndex >= 0)
                    {
                        Range filledRange = filledRanges[filledRangeIndex];
                        int val = values[valueIndex];
                        i -= val + 1;

                        if (i <= filledRange.end + 1)
                        {
                            backwardFilledCandidates[filledRangeIndex].Add(valueIndex);

                            filledRangeIndex--;

                            i = filledRange.start;
                        }

                        valueIndex--;
                    }
                }

                backwardFilledCandidates.Each((e, i) =>
                {
                    if (e.Count == 0)
                    {
                        int indexDiff = i;

                        e.Add(indexDiff);
                    }
                });

                for (int i = 0; i < filledRanges.Count; i++)
                {
                    Range filledRange = filledRanges[i];
                    List<int> forwardCandidates = forwardFilledCandidates[i];
                    List<int> backwardCandidates = backwardFilledCandidates[i];

                    if (forwardCandidates.Count > 0 && backwardCandidates.Count > 0)
                    {
                        int forwardMin = forwardCandidates.Min();
                        int backwardMin = backwardCandidates.Min();

                        int forwardMax = forwardCandidates.Max();
                        int backwardMax = backwardCandidates.Max();

                        int minValueIndex = forwardMin < backwardMin ? forwardMin : backwardMin;
                        int maxValueIndex = forwardMax >= backwardMax ? forwardMax : backwardMax;

                        var newValues = MyRange(minValueIndex, maxValueIndex + 1).Select(x => values[x]).ToList();

                        // Buradan daha fazla devam etmek gerek

                        if (newValues.All(x => x == filledRange.size))
                        {
                            if (filledRange.start - 1 >= 0)
                                cells[filledRange.start - 1] = Form1.EMPTY;

                            if (filledRange.end + 1 < cells.Length)
                                cells[filledRange.end + 1] = Form1.EMPTY;
                        }
                    }
                }
            }
        }

        #region Private Methods
        private static List<Range> FindFilledGroups(Form1.CellSeries cells, int start, int end)
        {
            List<Range> filledRanges = new List<Range>();

            int filledStartIndex = -1;
            for (int i = start; i <= end; i++)
            {
                int cell = cells[i];

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

        private static void debug() { }
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

    public class ColumnValue : IEquatable<ColumnValue>
    {
        public int Index { get; set; }
        public int Value { get; set; }

        public static int[] GetCommon(ColumnValue[] forward, ColumnValue[] backward)
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

        public bool Equals(ColumnValue ex)
        {
            return this.Index == ex.Index
                && this.Value == ex.Value;
        }
    }
}
