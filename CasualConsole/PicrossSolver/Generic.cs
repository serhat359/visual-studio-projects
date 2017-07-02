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
