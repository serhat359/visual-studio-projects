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
        public Form1()
        {
            InitializeComponent();

            //this.nobetTable.AllowUserToAddRows = true;
            //this.nobetTable.AllowUserToDeleteRows = true;
            //this.nobetTable.AutoGenerateColumns = true;

            DataTable table = new DataTable();
            table.Columns.Add("İsim", typeof(string));
            table.Columns.Add("1.", typeof(int));
            table.Columns.Add("2.", typeof(int));

            table.Rows.Add("serhat", 15, 23);
            table.Rows.Add("ahmet", 28, 12);
            table.AcceptChanges();

            this.nobetTable.DataSource = table;
            this.nobetTable.EndEdit();
            
            

        }
    }
}
