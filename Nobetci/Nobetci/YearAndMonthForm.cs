using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Nobetci
{
    public partial class YearAndMonthForm : Form
    {
        public class YearAndMonthResult
        {
            public bool IsSuccessFull { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
        }

        public YearAndMonthForm()
        {
            InitializeComponent();
        }

        public YearAndMonthResult GetYearAndMonth()
        {
            int month = 0;
            int year = 0;
            bool isButtonClicked = false;

            var months = DateTimeFormatInfo.CurrentInfo.MonthNames.Where(x => x.Length != 0).Select((name, index) => new { name = name, value = index + 1 }).ToList();

            this.comboBoxMonth.DataSource = months;
            this.comboBoxMonth.DisplayMember = "name";
            this.comboBoxMonth.ValueMember = "value";
            this.comboBoxMonth.SelectedIndex = DateTime.Today.Month;

            this.comboBoxYear.DataSource = Enumerable.Range(DateTime.Today.Year - 5, 11).ToList();
            this.comboBoxYear.SelectedIndex = 5;

            this.okButton.Click += (sender, e) =>
            {
                month = (int)this.comboBoxMonth.SelectedValue;
                year = (int)this.comboBoxYear.SelectedValue;
                isButtonClicked = true;
                this.Close();
            };

            this.ShowDialog();

            return new YearAndMonthResult { IsSuccessFull = isButtonClicked, Month = month, Year = year };
        }
    }
}
