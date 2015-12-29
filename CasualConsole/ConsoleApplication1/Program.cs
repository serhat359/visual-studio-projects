using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CasualConsole;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
			

            Console.WriteLine("Press a key to exit");
            Console.Read();
        }

		private static void TestPivot()
		{
			DataTable table = GetTestDebtPivotTable();

			List<FooBar> foobars = new List<FooBar>()
            {
                new FooBar{ Text = "a", Value1 = new DateTime(2015,12,12), Value2 = 4},
                new FooBar{ Text = "b", Value1 = new DateTime(2015,12,12), Value2 = 2},
                new FooBar{ Text = "a", Value1 = new DateTime(2015,12,10), Value2 = 3},
            };

			table = DataUtil.Pivot(foobars, x => x.Value1, x => x.Value2, x => x.Sum().ToString(), "NULL", x => x.Month + "-" + x.Day);

			PrintDataTable(table);
		}

        private static DataTable GetTestDebtPivotTable()
        {
            List<Debt> debtList = new List<Debt>() {
                new Debt { From = "a", To = "b", When = 2, HowMuch = 4 },
                new Debt { From = "a", To = "c", When = 3, HowMuch = 2 },
                new Debt { From = "b", To = "a", When = 4, HowMuch = 1 },
                new Debt { From = "a", To = "b", When = 4, HowMuch = 3 },
                new Debt { From = "b", To = "a", When = 3, HowMuch = 1 },
                new Debt { From = "a", To = "b", When = 2, HowMuch = 1 },
                new Debt { From = "b", To = "a", When = 2, HowMuch = 2 },
            };

            Func<IEnumerable<int>, string> groupConcat = x => string.Join(",", x);

            DataTable table = DataUtil.Pivot(debtList, x => x.When, x => x.HowMuch, groupConcat, "0");

            return table;
        }

        private static void PrintDataTable(DataTable table)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                Console.Write(column.ColumnName + "\t");
            }
            Console.WriteLine();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                foreach (object item in row.ItemArray)
                {
                    Console.Write(item + "\t");
                }
                Console.WriteLine();
            }
        }

    }

    class Debt
    {
        public string From { get; set; }

        public string To { get; set; }

        public int When { get; set; }

        public int HowMuch { get; set; }
    }

    class FooBar
    {
        public string Text { get; set; }

        public DateTime Value1 { get; set; }

        public int Value2 { get; set; }
    }
}
