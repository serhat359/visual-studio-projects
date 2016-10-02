using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nobetci
{
    public partial class Form1 : Form
    {
        public Color holidayColor { get { return Color.Red; } }

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

            SetupNobetTable();

            SetupCalendar();
        }

        private void SetupCalendar()
        {
            const int year = 2016;
            const int month = 10;
            DateTime date = new DateTime(year, month, 1);

            // myCalendar
            myCalendar.Rows.Add(6);

            int firstDay = (int)date.DayOfWeek;
            firstDay = firstDay == 0 ? 7 : firstDay;

            for (int i = 0; i < DateTime.DaysInMonth(year, month); i++)
            {
                int thisDay = i + firstDay - 1;
                int row = thisDay / 7;
                int column = thisDay % 7;

                myCalendar.Rows[row].Cells[column].Value = i + 1;

                var dayOfWeek = new DateTime(year, month, i + 1).DayOfWeek;
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                    ToggleHoliday(row, column);
            }
        }

        private void SetupNobetTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("İsim", typeof(string));
            table.Columns.Add("1.", typeof(int));
            table.Columns.Add("2.", typeof(int));
            table.Columns.Add("3.", typeof(int));
            table.Columns.Add("4.", typeof(int));
            table.Columns.Add("5.", typeof(int));
            table.Columns.Add("Cum", typeof(int));
            table.Columns.Add("Cts", typeof(int));
            table.Columns.Add("Paz", typeof(int));
            table.Columns.Add("H.sonu", typeof(int));
            table.Columns.Add("Per", typeof(bool));

            table.Rows.Add("serhat", 15, 23);
            table.Rows.Add("ahmet", 28, 12);
            table.AcceptChanges();

            this.nobetTable.DataSource = table;
            this.nobetTable.EndEdit();
        }

        private void myCalendar_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ToggleHoliday(e.RowIndex, e.ColumnIndex);
        }

        private void ToggleHoliday(int row, int column)
        {
            var color = myCalendar.Rows[row].Cells[column].Style.ForeColor;

            if (color != holidayColor)
                myCalendar.Rows[row].Cells[column].Style.ForeColor = holidayColor;
            else
                myCalendar.Rows[row].Cells[column].Style.ForeColor = Color.Black;
        }

        private bool IsHoliday(int row, int column)
        {
            return myCalendar.Rows[row].Cells[column].Style.ForeColor == holidayColor;
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
            }

            Flip(ref isCellClicked);
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

        private void Flip(ref bool val)
        {
            val = !val;
        }

    }
}
