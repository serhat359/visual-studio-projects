using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicrossSolver
{
    public static class Generic
    {
        public static void processGeneric(int[,] picture, int[][] upColumn, int[][] leftColumn)
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

        // Do not change this method to public!!
        private static void ApplyDimensionsForwardAndBackward(Action<Form1.CellSeries> processing, int[,] picture, int[][] upColumn, int[][] leftColumn)
        {
            bool executeBelow = true;

            for (int row = 0; executeBelow && row < Form1.rowCount; row++)
            {
                processing(new Form1.CellSeries(row, picture, Form1.Direction.Horizontal, leftColumn[row]));
                processing(new Form1.CellSeries(row, picture, Form1.Direction.HorizontalReverse, leftColumn[row]));
            }

            for (int col = 0; executeBelow && col < Form1.rowCount; col++)
            {
                processing(new Form1.CellSeries(col, picture, Form1.Direction.Vertical, upColumn[col]));
                processing(new Form1.CellSeries(col, picture, Form1.Direction.VerticalReverse, upColumn[col]));
            }
        }
    }
}
