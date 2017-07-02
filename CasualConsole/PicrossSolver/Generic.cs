using System;
using System.Collections.Generic;
using System.Linq;

namespace PicrossSolver
{
    public static class Generic
    {
        public static void processMatching(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            Action<Form1.CellSeries> processing = cells =>
            {
                bool doContinue = false;
                int valueIndex = 0;
                int startIndex = 0;

                do
                {
                    doContinue = false;

                    int val = cells.cellColumnValues[valueIndex];

                    for (int i = 0; i + startIndex < cells.Length; startIndex++)
                    {
                        if (cells[i + startIndex] == Form1.EMPTY)
                        {

                        }
                        else
                        {
                            break;
                        }
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
                    }
                } while (doContinue && valueIndex < cells.cellColumnValues.Length);
            };

            ApplyDimensionsForwardAndBackward(processing, picture, upColumn, leftColumn);
        }

        public static void processStartingAndEndingUnknowns(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            Action<Form1.CellSeries> processing = cells =>
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
            };

            ApplyDimensionsForwardAndBackward(processing, picture, upColumn, leftColumn);
        }

        public static void processSetEmptiesByMax(int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            Action<Form1.CellSeries> processing = cells =>
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

                if (filledInfos.Count == maxValueResult.Count)
                {
                    // TODO process

                    for (int area = 0; area <= maxValueResult.Count; area++)
                    {
                        var foundIndex = area;

                        int cellStart = area == 0 ? 0 : filledInfos[area - 1].CellIndexEnd + 1;
                        int cellEnd = area == maxValueResult.Count ? cells.Length - 1 : filledInfos[area].CellIndexStart - 1;

                        IEnumerable<int> selectedValuesIndices;

                        if (area == 0)
                            selectedValuesIndices = MyRange(0, maxValueResult.LocationIndices[area]);
                        else if (area == maxValueResult.Count)
                            selectedValuesIndices = MyRange(maxValueResult.LocationIndices[area - 1] + 1, values.Length - 1);
                        else
                            selectedValuesIndices = MyRange(maxValueResult.LocationIndices[area - 1] + 1, maxValueResult.LocationIndices[area]);

                        int[] newValues = selectedValuesIndices.Select(x => values[x]).ToArray();

                        // below is ProcessInitial(cellStart, cellEnd, newValues);

                        Form1.CellSeries slice = Form1.CellSeries.Slice(cells, cellStart, cellEnd, newValues);

                        ProcessAllAlgorithms(slice);
                    }
                }
            };

            ApplyDimensionsForward(processing, picture, upColumn, leftColumn);
        }

        private static void ProcessAllAlgorithms(Form1.CellSeries cells)
        {
            InitialProcessing(cells);
            RemoveSmallEmpties(cells);
        }

        private static void RemoveSmallEmpties(Form1.CellSeries cells)
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

        private static void InitialProcessing(Form1.CellSeries cells)
        {
            int cellCount = cells.Length;
            var newValues = cells.cellColumnValues;

            int minCellOccupation = newValues.Sum() + newValues.Length - 1;
            int rem = cellCount - minCellOccupation;

            int innerRange = 0;
            for (int i = 0; i < newValues.Length; i++)
            {
                int val = newValues[i];

                int k;
                for (k = rem; k < val; k++)
                    cells[innerRange + k] = Form1.FILLED;

                if (rem == 0)
                    cells[innerRange + k] = Form1.EMPTY;

                innerRange += val + 1;
            }
        }

        private static IEnumerable<int> MyRange(int from, int to)
        {
            for (int i = from; i < to; i++)
                yield return i;
        }

        // Do not change these methods to public!!
        #region Private Methods
        private static void ApplyDimensionsForwardAndBackward(Action<Form1.CellSeries> processing, int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            bool executeBelow = true;

            for (int row = 0; executeBelow && row < Form1.rowCount
                //&& !Form1.isRowCompleted[row]
                ; row++)
            {
                processing(new Form1.CellSeries(row, picture, Form1.Direction.Horizontal, leftColumn[row]));
                processing(new Form1.CellSeries(row, picture, Form1.Direction.HorizontalReverse, leftColumn[row]));
            }

            for (int col = 0; executeBelow && col < Form1.rowCount
                //&& !Form1.isColCompleted[col]
                ; col++)
            {
                processing(new Form1.CellSeries(col, picture, Form1.Direction.Vertical, upColumn[col]));
                processing(new Form1.CellSeries(col, picture, Form1.Direction.VerticalReverse, upColumn[col]));
            }
        }
        private static void ApplyDimensionsForward(Action<Form1.CellSeries> processing, int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            bool executeBelow = true;

            for (int row = 0; executeBelow && row < Form1.rowCount
                //&& !Form1.isRowCompleted[row]
                ; row++)
            {
                processing(new Form1.CellSeries(row, picture, Form1.Direction.Horizontal, leftColumn[row]));
            }

            for (int col = 0; executeBelow && col < Form1.rowCount
                //&& !Form1.isColCompleted[col]
                ; col++)
            {
                processing(new Form1.CellSeries(col, picture, Form1.Direction.Vertical, upColumn[col]));
            }
        }
        #endregion
    }

    public class FilledInfo
    {
        public int CellIndexStart { get; set; }
        public int CellIndexEnd { get { return CellIndexStart + Size - 1; } }
        public int Size { get; set; }
    }
}
