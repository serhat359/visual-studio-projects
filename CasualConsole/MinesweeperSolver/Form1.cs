using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinesweeperSolver
{
    public partial class Form1 : Form
    {
        enum Dir : byte
        {
            NONE,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        enum CellType : byte
        {
            UNKNOWN = 255,
            BOMB = 254,
            EMPTY = 0,
            ONE = 1,
            TWO = 2,
            THREE = 3,
            FOUR = 4,
            FIVE = 5,
            SIX = 6,
            SEVEN = 7,
            EIGHT = 8,

            MEANINGLESS = 128
        }

        const int taskdelayMillis = 90;
        const int size = 16;
        const int firstPixelOffset = 2;
        const int middlePixelOffset = 8;
        const int twoPixelOffset = 6;
        const int fourPixelOffset = 4;
        const int tenPixelOffset = 10;
        const int rowCount = 16;
        const int colCount = 30;
        const int w = 481; // each pixel is 16 x 16
        const int h = 257;
        const int pageX = 581;
        const int pageY = 165;
        CellType[,] arr = new CellType[rowCount, colCount];
        CellType[,] arrCopy = new CellType[rowCount, colCount];
        Bitmap b = new Bitmap(w, h);

        public Form1()
        {
            InitializeComponent();

            this.Paint += PaintEventHandler;
        }

        private void PaintEventHandler(object sender, PaintEventArgs e)
        {
            Graphics g2d = e.Graphics;
            var pen = new Pen(Color.Black);
            int xoff = 10;
            int yoff = 40;
            g2d.DrawRectangle(pen, new Rectangle(xoff, yoff, w, h));
            for (int row = 0; row < rowCount - 1; row++)
            {
                g2d.DrawLine(pen, new Point(xoff, yoff + (row + 1) * size), new Point(xoff + w - 1, yoff + (row + 1) * size));
            }

            for (int col = 0; col < colCount - 1; col++)
            {
                g2d.DrawLine(pen, new Point(xoff + (col + 1) * size, yoff), new Point(xoff + (col + 1) * size, yoff + h - 1));
            }

            Brush brushGetter(CellType x)
            {
                return x == CellType.UNKNOWN ? Brushes.Gray
                     : x == CellType.BOMB ? Brushes.Black
                     : x == CellType.ONE ? Brushes.Blue
                     : x == CellType.TWO ? Brushes.DarkGreen
                     : x == CellType.THREE ? Brushes.Red
                     : x == CellType.FOUR ? Brushes.MidnightBlue
                     : x == CellType.FIVE ? Brushes.DarkRed
                     : x == CellType.SIX ? Brushes.Turquoise
                     : x == CellType.SEVEN ? Brushes.Pink
                     : x == CellType.EIGHT ? Brushes.LightGray
                     : Brushes.White;
            }

            var font = new Font("Consolas", 11, FontStyle.Bold);

            for (int row = 0; row < rowCount; row++)
                for (int col = 0; col < colCount; col++)
                {
                    CellType cell = arr[row, col];
                    Brush brush = brushGetter(cell);

                    var locx = xoff + 1 + col * size;
                    var locy = yoff + 1 + row * size;

                    if (cell >= CellType.ONE && cell <= CellType.EIGHT)
                        g2d.DrawString(((int)cell).ToString(), font, brush, locx, locy);
                    else
                        g2d.FillRectangle(brush, new Rectangle(locx, locy, size - 1, size - 1));
                }
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(new Point(pageX, pageY), new Point(0, 0), new Size(w, h));
            }

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    var firstPixel = getPixel(firstPixelOffset, row, col);
                    var midPixel = getPixel(middlePixelOffset, row, col);
                    var twoPixel = getPixel(twoPixelOffset, row, col);
                    var fourPixel = getPixel(fourPixelOffset, row, col);
                    var tenPixel = getPixel(tenPixelOffset, row, col);
                    var fourOtherPixel = getPixel((13, 4), row, col);
                    if (tenPixel.R == 0 && tenPixel.G == 0 && tenPixel.B == 0)
                        arr[row, col] = CellType.UNKNOWN; // TODO remove this later, this is for capturing bombs
                    else if (firstPixel.R == 255)
                    {
                        if (midPixel.R == 255)
                            arr[row, col] = CellType.BOMB;
                        else
                            arr[row, col] = CellType.UNKNOWN;
                    }
                    else if (midPixel.B == 255)
                        arr[row, col] = CellType.ONE;
                    else if (twoPixel.G == 123 && twoPixel.B == 123)
                    {
                        if (twoPixel.R == 123)
                            arr[row, col] = CellType.EIGHT;
                        else
                            arr[row, col] = CellType.SIX;
                    }
                    else if (twoPixel.G == 123)
                        arr[row, col] = CellType.TWO;
                    else if (midPixel.R == 255)
                        arr[row, col] = CellType.THREE;
                    else if (midPixel.B == 123)
                        arr[row, col] = CellType.FOUR;
                    else if (twoPixel.R == 123)
                        arr[row, col] = CellType.FIVE;
                    else if (midPixel.R == 189 || firstPixel.R == 189)
                    {
                        if (fourPixel.R == 0 && fourPixel.G == 0 && fourPixel.B == 0 && fourOtherPixel.R == 0 && fourOtherPixel.G == 0 && fourOtherPixel.B == 0)
                            arr[row, col] = CellType.SEVEN;
                        else
                            arr[row, col] = CellType.EMPTY;
                    }
                    else
                        throw new Exception();
                }
            }

            this.Invalidate();
        }

        private Color getPixel(int offset, int row, int col)
        {
            return b.GetPixel(x: offset + col * size, y: offset + row * size);
        }

        private Color getPixel((int, int) offset, int row, int col)
        {
            return b.GetPixel(x: offset.Item1 + col * size, y: offset.Item2 + row * size);
        }

        private CellType getSafe(int row, int col)
        {
            if (row >= 0 && col >= 0 && row < rowCount && col < colCount)
                return arr[row, col];
            else
                return 0;
        }

        private void solveButton_Click(object sender, EventArgs e)
        {
            while (true) // This is for normal algorithms
            {
                CopyArray();

                bool markedBomb = false;
                bool clickedCell = false;

                TryMarkBombs(ref markedBomb);
                TryClickCell(ref clickedCell);

                if (clickedCell)
                {
                    Task.WaitAll(Task.Delay(taskdelayMillis));
                    captureButton_Click(null, null);
                }

                if (!clickedCell && !markedBomb)
                {
                    bool markedBombAdvanced = false;
                    bool clickedBombAdvanced = false;

                    TryMarkBombAdvanced(ref markedBombAdvanced);

                    if (!markedBombAdvanced)
                    {
                        TryClickCellAdvanced(ref clickedBombAdvanced);

                        if (clickedBombAdvanced)
                        {
                            Task.WaitAll(Task.Delay(taskdelayMillis));
                            captureButton_Click(null, null);
                        }

                        if (!clickedBombAdvanced)
                            break;
                    }
                }
            }

            this.Invalidate();
        }

        private void TryClickCell(ref bool clickedCell)
        {
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    var cell = arr[row, col];
                    if (cell >= CellType.ONE && (byte)cell < 200)
                    {
                        var cell1 = getSafe(row - 1, col - 1);
                        var cell2 = getSafe(row - 1, col);
                        var cell3 = getSafe(row - 1, col + 1);
                        var cell4 = getSafe(row, col - 1);
                        //var cell5 = getSafe(row, col);
                        var cell6 = getSafe(row, col + 1);
                        var cell7 = getSafe(row + 1, col - 1);
                        var cell8 = getSafe(row + 1, col);
                        var cell9 = getSafe(row + 1, col + 1);

                        int count = 0;
                        if (cell1 == CellType.BOMB) count++;
                        if (cell2 == CellType.BOMB) count++;
                        if (cell3 == CellType.BOMB) count++;
                        if (cell4 == CellType.BOMB) count++;
                        if (cell6 == CellType.BOMB) count++;
                        if (cell7 == CellType.BOMB) count++;
                        if (cell8 == CellType.BOMB) count++;
                        if (cell9 == CellType.BOMB) count++;

                        if (count == (int)cell)
                        {
                            if (cell1 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row - 1, col - 1); }
                            if (cell2 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row - 1, col); }
                            if (cell3 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row - 1, col + 1); }
                            if (cell4 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row, col - 1); }

                            if (cell6 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row, col + 1); }
                            if (cell7 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row + 1, col - 1); }
                            if (cell8 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row + 1, col); }
                            if (cell9 == CellType.UNKNOWN) { clickedCell = true; ClickCell(row + 1, col + 1); }
                        }
                    }
                }
            }
        }

        private void TryMarkBombs(ref bool markedBomb)
        {
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    var cell = arr[row, col];
                    if (cell >= CellType.ONE && (byte)cell < 200)
                    {
                        var cell1 = getSafe(row - 1, col - 1);
                        var cell2 = getSafe(row - 1, col);
                        var cell3 = getSafe(row - 1, col + 1);
                        var cell4 = getSafe(row, col - 1);
                        //var cell5 = getSafe(row, col);
                        var cell6 = getSafe(row, col + 1);
                        var cell7 = getSafe(row + 1, col - 1);
                        var cell8 = getSafe(row + 1, col);
                        var cell9 = getSafe(row + 1, col + 1);

                        int count = 0;
                        if (cell1 >= CellType.BOMB) count++;
                        if (cell2 >= CellType.BOMB) count++;
                        if (cell3 >= CellType.BOMB) count++;
                        if (cell4 >= CellType.BOMB) count++;
                        if (cell6 >= CellType.BOMB) count++;
                        if (cell7 >= CellType.BOMB) count++;
                        if (cell8 >= CellType.BOMB) count++;
                        if (cell9 >= CellType.BOMB) count++;

                        if (count == (int)cell)
                        {
                            if (cell1 == CellType.UNKNOWN) { MarkBomb(row - 1, col - 1, ref markedBomb); }
                            if (cell2 == CellType.UNKNOWN) { MarkBomb(row - 1, col, ref markedBomb); }
                            if (cell3 == CellType.UNKNOWN) { MarkBomb(row - 1, col + 1, ref markedBomb); }
                            if (cell4 == CellType.UNKNOWN) { MarkBomb(row, col - 1, ref markedBomb); }

                            if (cell6 == CellType.UNKNOWN) { MarkBomb(row, col + 1, ref markedBomb); }
                            if (cell7 == CellType.UNKNOWN) { MarkBomb(row + 1, col - 1, ref markedBomb); }
                            if (cell8 == CellType.UNKNOWN) { MarkBomb(row + 1, col, ref markedBomb); }
                            if (cell9 == CellType.UNKNOWN) { MarkBomb(row + 1, col + 1, ref markedBomb); }
                        }
                    }
                }
            }
        }

        private void TryMarkBombAdvanced(ref bool markedBombAdvanced)
        {
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    var cell = arr[row, col];
                    if (cell >= CellType.ONE && cell < CellType.MEANINGLESS)
                    {
                        var (count, success, dir, bombCount, isAdjacent) = GetUnknownCount(row, col);
                        if (success)
                        {
                            if (dir == Dir.LEFT || dir == Dir.RIGHT)
                            {
                                var (upCount, upSuccess, upDir, upBombCount, isUpAdjacent) = GetUnknownCount(row - 1, col);
                                var (downCount, downSuccess, downDir, downBombCount, isDownAdjacent) = GetUnknownCount(row + 1, col);
                                int missing = (int)cell - bombCount;

                                if (upSuccess && arr[row - 1, col] >= CellType.ONE && arr[row - 1, col] < CellType.MEANINGLESS)
                                {
                                    int upMissing = (int)arr[row - 1, col] - upBombCount;
                                    if (upMissing > missing)
                                    {
                                        MarkBomb(row - 2, col + (dir == Dir.LEFT ? -1 : 1), ref markedBombAdvanced);
                                    }
                                }
                                if (downSuccess && arr[row + 1, col] >= CellType.ONE && arr[row + 1, col] < CellType.MEANINGLESS)
                                {
                                    int downMissing = (int)arr[row + 1, col] - downBombCount;
                                    if (downMissing > missing)
                                    {
                                        MarkBomb(row + 2, col + (dir == Dir.LEFT ? -1 : 1), ref markedBombAdvanced);
                                    }
                                }
                            }
                            if (dir == Dir.UP || dir == Dir.DOWN)
                            {
                                var (leftCount, leftSuccess, leftDir, leftBombCount, isLeftAdjacent) = GetUnknownCount(row, col - 1);
                                var (rightCount, rightSuccess, rightDir, rightBombCount, isRightAdjacent) = GetUnknownCount(row, col + 1);
                                int missing = (int)cell - bombCount;

                                if (leftSuccess && arr[row, col - 1] >= CellType.ONE && arr[row, col - 1] < CellType.MEANINGLESS)
                                {
                                    int leftMissing = (int)arr[row, col - 1] - leftBombCount;
                                    if (leftMissing > missing)
                                    {
                                        MarkBomb(row + (dir == Dir.UP ? -1 : 1), col - 2, ref markedBombAdvanced);
                                    }
                                }
                                if (rightSuccess && arr[row, col + 1] >= CellType.ONE && arr[row, col + 1] < CellType.MEANINGLESS)
                                {
                                    int rightMissing = (int)arr[row, col + 1] - rightBombCount;
                                    if (rightMissing > missing)
                                    {
                                        MarkBomb(row + (dir == Dir.UP ? -1 : 1), col + 2, ref markedBombAdvanced);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TryClickCellAdvanced(ref bool clickedBombAdvanced)
        {
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    var cell = arr[row, col];
                    if (cell >= CellType.ONE && cell < CellType.MEANINGLESS)
                    {
                        var (unknownCount, success, dir, bombCount, isAdjacent) = GetUnknownCount(row, col);
                        int missing = (int)arr[row, col] - bombCount;
                        if (isAdjacent && missing == 1 && unknownCount == 2)
                        {
                            if (dir == Dir.LEFT || dir == Dir.RIGHT)
                            {
                                var (upCount, upSuccess, upDir, upBombCount, isUpAdjacent) = GetUnknownCount(row - 1, col);
                                var (downCount, downSuccess, downDir, downBombCount, isDownAdjacent) = GetUnknownCount(row + 1, col);

                                var upCell = getSafe(row - 1, col);
                                var downCell = getSafe(row + 1, col);
                                if (isUpAdjacent && ((int)upCell - upBombCount) == 1)
                                {
                                    ClickMultiCellAdjacent(row, col, dir, Dir.UP, ref clickedBombAdvanced);
                                }
                                if (isDownAdjacent && ((int)downCell - downBombCount) == 1)
                                {
                                    ClickMultiCellAdjacent(row, col, dir, Dir.DOWN, ref clickedBombAdvanced);
                                }
                            }
                            if (dir == Dir.UP || dir == Dir.DOWN)
                            {
                                var (leftCount, leftSuccess, leftDir, leftBombCount, isLeftAdjacent) = GetUnknownCount(row, col - 1);
                                var (rightCount, rightSuccess, rightDir, rightBombCount, isRightAdjacent) = GetUnknownCount(row, col + 1);

                                var leftCell = getSafe(row, col - 1);
                                var rightCell = getSafe(row, col + 1);
                                if (isLeftAdjacent && ((int)leftCell - leftBombCount) == 1)
                                {
                                    ClickMultiCellAdjacent(row, col, dir, Dir.LEFT, ref clickedBombAdvanced);
                                }
                                if (isRightAdjacent && ((int)rightCell - rightBombCount) == 1)
                                {
                                    ClickMultiCellAdjacent(row, col, dir, Dir.RIGHT, ref clickedBombAdvanced);
                                }
                            }
                        }

                        if (missing == 1 && unknownCount == 2)
                        {
                            var (otherRow, otherCol) = (row, col);

                            Action modifyCoordinate = () =>
                            {
                                switch (dir)
                                {
                                    case Dir.LEFT: otherCol--; break;
                                    case Dir.RIGHT: otherCol++; break;
                                    case Dir.UP: otherRow--; break;
                                    case Dir.DOWN: otherRow++; break;
                                    default:
                                    case Dir.NONE:
                                        break;
                                }
                            };

                            void tryClick(ref bool clickedBombAdvanced2)
                            {
                                var otherCell = getSafe(otherRow, otherCol);
                                if (otherCell == CellType.ONE)
                                {
                                    if (dir == Dir.UP)
                                    {
                                        CheckClickCell(otherRow - 1, otherCol - 1, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow - 1, otherCol, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow - 1, otherCol + 1, ref clickedBombAdvanced2);
                                    }
                                    else if (dir == Dir.DOWN)
                                    {
                                        CheckClickCell(otherRow + 1, otherCol - 1, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow + 1, otherCol, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow + 1, otherCol + 1, ref clickedBombAdvanced2);
                                    }
                                    else if (dir == Dir.LEFT)
                                    {
                                        CheckClickCell(otherRow - 1, otherCol - 1, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow, otherCol - 1, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow + 1, otherCol - 1, ref clickedBombAdvanced2);
                                    }
                                    else if (dir == Dir.RIGHT)
                                    {
                                        CheckClickCell(otherRow - 1, otherCol + 1, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow, otherCol + 1, ref clickedBombAdvanced2);
                                        CheckClickCell(otherRow + 1, otherCol + 1, ref clickedBombAdvanced2);
                                    }
                                }
                            }

                            modifyCoordinate();
                            tryClick(ref clickedBombAdvanced);
                            modifyCoordinate();
                            tryClick(ref clickedBombAdvanced);
                        }
                    }
                }
            }
        }

        // Returns unknown count, isAligned, direction, bomb count
        private (int, bool, Dir, int, bool) GetUnknownCount(int row, int col)
        {
            int count = 0;
            int leftCount = 0;
            int rightCount = 0;
            int upCount = 0;
            int downCount = 0;
            int bombCount = 0;

            var cell1 = getSafe(row - 1, col - 1);
            var cell2 = getSafe(row - 1, col);
            var cell3 = getSafe(row - 1, col + 1);
            var cell4 = getSafe(row, col - 1);
            //var cell5 = getSafe(row, col);
            var cell6 = getSafe(row, col + 1);
            var cell7 = getSafe(row + 1, col - 1);
            var cell8 = getSafe(row + 1, col);
            var cell9 = getSafe(row + 1, col + 1);

            if (cell1 == CellType.UNKNOWN) { count++; upCount++; leftCount++; }
            if (cell2 == CellType.UNKNOWN) { count++; upCount++; }
            if (cell3 == CellType.UNKNOWN) { count++; upCount++; rightCount++; }
            if (cell4 == CellType.UNKNOWN) { count++; leftCount++; }

            if (cell6 == CellType.UNKNOWN) { count++; rightCount++; }
            if (cell7 == CellType.UNKNOWN) { count++; downCount++; leftCount++; }
            if (cell8 == CellType.UNKNOWN) { count++; downCount++; }
            if (cell9 == CellType.UNKNOWN) { count++; downCount++; rightCount++; }

            if (cell1 == CellType.BOMB) bombCount++;
            if (cell2 == CellType.BOMB) bombCount++;
            if (cell3 == CellType.BOMB) bombCount++;
            if (cell4 == CellType.BOMB) bombCount++;

            if (cell6 == CellType.BOMB) bombCount++;
            if (cell7 == CellType.BOMB) bombCount++;
            if (cell8 == CellType.BOMB) bombCount++;
            if (cell9 == CellType.BOMB) bombCount++;

            if (count >= 2)
            {
                if (count == leftCount) return (count, true, Dir.LEFT, bombCount, cell4 == CellType.UNKNOWN);
                if (count == rightCount) return (count, true, Dir.RIGHT, bombCount, cell6 == CellType.UNKNOWN);
                if (count == upCount) return (count, true, Dir.UP, bombCount, cell2 == CellType.UNKNOWN);
                if (count == downCount) return (count, true, Dir.DOWN, bombCount, cell8 == CellType.UNKNOWN);
            }

            return (count, false, Dir.NONE, bombCount, false);
        }

        private void CopyArray()
        {
            for (int row = 0; row < rowCount; row++)
                for (int col = 0; col < colCount; col++)
                    arrCopy[row, col] = arr[row, col];
        }

        private void CheckClickCell(int row, int col, ref bool isClicked)
        {
            if (getSafe(row, col) == CellType.UNKNOWN)
            {
                isClicked = true;
                ClickCell(row, col);
            }
        }

        private void ClickCell(int row, int col)
        {
            //Console.WriteLine($"ClickCell {row} {col}");
            var oldPosition = MouseOperations.GetCursorPosition();
            MouseOperations.SetCursorPosition(pageX + 8 + col * size, pageY + 8 + row * size);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
            MouseOperations.SetCursorPosition(oldPosition);
        }

        // row and col are not actual positions
        private void ClickMultiCellAdjacent(int row, int col, Dir dirFirst, Dir dirSecond, ref bool clickedBombAdvanced)
        {
            var (rowNo1, colNo1) = (row, col);
            switch (dirFirst)
            {
                case Dir.LEFT: colNo1--; break;
                case Dir.RIGHT: colNo1++; break;
                case Dir.UP: rowNo1--; break;
                case Dir.DOWN: rowNo1++; break;
                case Dir.NONE:
                default: throw new Exception();
            }

            var (rowNo2, colNo2) = (rowNo1, colNo1);
            switch (dirSecond)
            {
                case Dir.LEFT: colNo2--; break;
                case Dir.RIGHT: colNo2++; break;
                case Dir.UP: rowNo2--; break;
                case Dir.DOWN: rowNo2++; break;
                case Dir.NONE:
                default: throw new Exception();
            }

            switch (dirSecond)
            {
                case Dir.LEFT: col--; break;
                case Dir.RIGHT: col++; break;
                case Dir.UP: row--; break;
                case Dir.DOWN: row++; break;
                case Dir.NONE:
                default: throw new Exception();
            }

            var cell1 = getSafe(row - 1, col - 1);
            var cell2 = getSafe(row - 1, col);
            var cell3 = getSafe(row - 1, col + 1);
            var cell4 = getSafe(row, col - 1);
            //var cell5 = getSafe(row, col);
            var cell6 = getSafe(row, col + 1);
            var cell7 = getSafe(row + 1, col - 1);
            var cell8 = getSafe(row + 1, col);
            var cell9 = getSafe(row + 1, col + 1);

            foreach (var (r, c) in new (int, int)[] { (row - 1, col - 1), (row - 1, col), (row - 1, col + 1), (row, col - 1), (row, col + 1), (row + 1, col - 1), (row + 1, col), (row + 1, col + 1) })
            {
                var cell = getSafe(r, c);
                if (cell == CellType.UNKNOWN && (r, c) != (rowNo1, colNo1) && (r, c) != (rowNo2, colNo2))
                {
                    clickedBombAdvanced = true;
                    ClickCell(r, c);
                }
            }
        }

        private void MarkBomb(int row, int col, ref bool marked)
        {
            if (arr[row, col] == CellType.UNKNOWN)
            {
                marked = true;
                //Console.WriteLine($"MarkBomb {row} {col}");
                arr[row, col] = CellType.BOMB;
                var oldPosition = MouseOperations.GetCursorPosition();
                MouseOperations.SetCursorPosition(pageX + 8 + col * size, pageY + 8 + row * size);
                MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightDown);
                MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp);
                MouseOperations.SetCursorPosition(oldPosition);
            }
        }
    }
}
