using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nobetci;
using System.Globalization;

namespace Nobetci
{
    public partial class Form1 : Form
    {
        public Color holidayColor { get { return Color.Red; } }
        const int nobetCount = 6;
        public List<int> nobetDays
        {
            get
            {
                return Enumerable.Range(1, nobetCount).ToList();
            }
        }

        int year = 0;
        int month = 0;
        DateTime date;
        const string friDay = "Cum";
        const string satDay = "Cts";
        const string sunDay = "Paz";
        const string weekEnd = "H.sonu";
        const string thuDay = "Per";

        public class TableIndex
        {
            public int RowIndex { get; set; }
            public int ColumnIndex { get; set; }
        }

        private bool isCellClicked;
        private TableIndex lastIndex;

        public Form1()
        {
            InitializeComponent();

            SetYearAndMonth();

            SetupNobetTable();

            SetupCalendar();

            this.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(month) + " " + year + " Nöbetleri";
        }

        private void SetYearAndMonth()
        {
            var result = new YearAndMonthForm().GetYearAndMonth();

            if (!result.IsSuccessFull)
                System.Environment.Exit(0);
            else
            {
                this.month = result.Month;
                this.year = result.Year;
                this.date = new DateTime(year, month, 1);
            }
        }

        private void SetupCalendar()
        {
            myCalendar.Rows.Add(6);

            int firstDay = (int)date.DayOfWeek;
            firstDay = firstDay == 0 ? 7 : firstDay;

            int daysInMoth = DateTime.DaysInMonth(year, month);

            for (int i = 0; i < daysInMoth + 3; i++)
            {
                int thisDay = i + firstDay - 1;
                int row = thisDay / 7;
                int column = thisDay % 7;

                myCalendar.Rows[row].Cells[column].Value = i + 1;

                var dayOfWeek = new DateTime(year, month, 1).AddDays(i).DayOfWeek;
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                    ToggleHoliday(row, column, i > daysInMoth);

                if (i >= daysInMoth)
                {
                    var color = myCalendar.Rows[row].Cells[column].Style.ForeColor;
                    myCalendar.Rows[row].Cells[column].Style.ForeColor = LighterIfNext(color, true);
                }
            }
        }

        private void SetupNobetTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("İsim", typeof(string));
            foreach (int day in nobetDays)
            {
                table.Columns.Add(day + ".", typeof(int));
            }
            table.Columns.Add(friDay, typeof(int));
            table.Columns.Add(satDay, typeof(int));
            table.Columns.Add(sunDay, typeof(int));
            table.Columns.Add(weekEnd, typeof(int));
            table.Columns.Add(thuDay, typeof(bool));

            table.Rows.Add("serhat", 16, 23);
            table.Rows.Add("ahmet", 6, 12);
            table.AcceptChanges();

            this.nobetTable.DataSource = table;
            this.nobetTable.EndEdit();
        }

        private void myCalendar_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ToggleHoliday(e.RowIndex, e.ColumnIndex);
        }

        private void ToggleHoliday(int row, int column)
        {
            ToggleHoliday(row, column, myCalendar[column, row].Style.ForeColor.G > 0);
        }

        private void ToggleHoliday(int row, int column, bool isNextMonth)
        {
            var color = myCalendar.Rows[row].Cells[column].Style.ForeColor;

            if (!IsHoliday(color))
                myCalendar.Rows[row].Cells[column].Style.ForeColor = LighterIfNext(holidayColor, isNextMonth);
            else
                myCalendar.Rows[row].Cells[column].Style.ForeColor = LighterIfNext(Color.Black, isNextMonth);
        }

        private Color LighterIfNext(Color color, bool isNextMonth)
        {
            return isNextMonth ? Color.FromArgb(color.R / 3 + 170, color.G / 3 + 170, color.B / 3 + 170) : color;
        }

        private bool IsHoliday(Color c)
        {
            return c.R == 255;
        }

        private bool IsHoliday(int row, int column)
        {
            return IsHoliday(myCalendar.Rows[row].Cells[column].Style.ForeColor);
        }

        private void nobetTable_CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (!isCellClicked)
            {
                lastIndex = new TableIndex { RowIndex = e.RowIndex, ColumnIndex = e.ColumnIndex };
            }
            else
            {
                TableIndex newIndex = new TableIndex { RowIndex = e.RowIndex, ColumnIndex = e.ColumnIndex };
                SwapCells(lastIndex, newIndex);
                SortRows(lastIndex.RowIndex, newIndex.RowIndex);
                CheckAllRows();
            }

            Flip(ref isCellClicked);
        }

        private void SortRows(params int[] rows)
        {
            foreach (int rowIndex in rows)
            {
                foreach (var item in GetNobets(rowIndex).OrderBy(x => x).WithIndex())
                {
                    this.nobetTable[item.Index + 1, rowIndex].Value = item.Value;
                }
            }
        }

        private void SwapCells(TableIndex lastIndex, TableIndex newIndex)
        {
            var lastCellValue = this.nobetTable[lastIndex.ColumnIndex, lastIndex.RowIndex].Value;
            var newCellValue = this.nobetTable[newIndex.ColumnIndex, newIndex.RowIndex].Value;

            if (lastCellValue != null && newCellValue != null && lastCellValue.GetType() == typeof(int) && newCellValue.GetType() == typeof(int))
            {
                int lastValue = (int)lastCellValue;
                int newValue = (int)newCellValue;

                this.nobetTable[lastIndex.ColumnIndex, lastIndex.RowIndex].Value = newValue;
                this.nobetTable[newIndex.ColumnIndex, newIndex.RowIndex].Value = lastValue;
            }
        }

        void nobetTable_CellEndEdit(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            SortRows(e.RowIndex);
            CheckAllRows();
        }

        private void CheckAllRows()
        {
            IEnumerable<int> rows = Enumerable.Range(0, nobetTable.Rows.Count);

            foreach (int rowIndex in rows)
            {
                ClearRowStyle(rowIndex);

                CheckFollowingDays(rowIndex);

                DoCountings(rowIndex);
            }
        }

        private void DoCountings(int rowIndex)
        {
            int friCount = 0;
            int satCount = 0;
            int sunCount = 0;
            int thuCount = 0;
            int weekEndCount = 0;

            foreach (int day in GetNobets(rowIndex))
            {
                DayOfWeek dayOfWeek = new DateTime(year, month, day).DayOfWeek;

                switch (dayOfWeek)
                {
                    case DayOfWeek.Thursday:
                        thuCount++;
                        break;
                    case DayOfWeek.Friday:
                        friCount++;
                        break;
                    case DayOfWeek.Saturday:
                        satCount++;
                        break;
                    case DayOfWeek.Sunday:
                        sunCount++;
                        break;
                    default:
                        break;
                }

                weekEndCount += GetWeekendWaste(day);
            }

            nobetTable[friDay, rowIndex].Value = friCount;
            nobetTable[satDay, rowIndex].Value = satCount;
            nobetTable[sunDay, rowIndex].Value = sunCount;
            nobetTable[weekEnd, rowIndex].Value = weekEndCount;
            nobetTable[thuDay, rowIndex].Value = thuCount > 0;
        }

        private int GetWeekendWaste(int day)
        {
            int weekendCount = 0;

            var cells = GetAllCalendarCells().SkipWhile(x => Extensions.CastInt(x.Value) != day).Take(2).ToArray();

            foreach (var cell in cells)
            {
                var color = cell.Style.ForeColor;
                bool isHoliday = IsHoliday(color);
                if (isHoliday)
                {
                    weekendCount++;
                }
            }

            return weekendCount;
        }

        private IEnumerable<DataGridViewCell> GetAllCalendarCells()
        {
            foreach (int rowIndex in Enumerable.Range(0, myCalendar.Rows.Count))
            {
                foreach (int columnIndex in Enumerable.Range(0, myCalendar.Columns.Count))
                {
                    DataGridViewCell cell = myCalendar[columnIndex, rowIndex];
                    yield return cell;
                }
            }
        }

        private void CheckFollowingDays(int rowIndex)
        {
            var days = GetNobets(rowIndex).Select((e, i) => new { index = i, value = e }).OrderBy(x => x.value).ToList();

            for (int i = 1; i < days.Count; i++)
            {
                if (days[i].value - days[i - 1].value <= 1)
                    Highlight(rowIndex, days[i].index + 1);
            }
        }

        private void ClearRowStyle(int rowIndex)
        {
            foreach (int columnIndex in nobetDays)
            {
                nobetTable[columnIndex, rowIndex].Style.ForeColor = Color.Black;
            }
        }

        private void Highlight(int rowIndex, int columnIndex)
        {
            nobetTable[columnIndex, rowIndex].Style.ForeColor = Color.Red;
        }

        private IEnumerable<int> GetNobets(int rowIndex)
        {
            foreach (int i in nobetDays)
            {
                var cellValue = nobetTable[i, rowIndex].Value;

                if (cellValue != null && typeof(int) == cellValue.GetType())
                    yield return (int)cellValue;
                else
                    break;
            }
        }

        private void Flip(ref bool val)
        {
            val = !val;
        }
    }
}
