using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace CasualConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TestPivot();

            //TestRegex();

            //TestSplitWithCondition();

            //TestIntersect();

            Dummy d = new Dummy(3, "ahmet");

            string exprname = Ext.NameOf(() => d.StringProperty);

            Console.WriteLine(exprname);

            MyThread<int> fibThread = MyThread.DoInThread(() =>
            {
                return Fib(1);
            });

            int fibResult = fibThread.Await();

            Console.WriteLine(fibResult);

            // Closing, Do Not Delete!
            Console.WriteLine("Program has terminated, press a key to exit");
            Console.ReadKey();
        }

        private static int Fib(int x)
        {
            switch (x)
            {
                case 0:
                case 1:
                    return x;
                default:
                    return Fib(x - 1) + Fib(x - 2);
            }

        }

        private static void PrintArray<T>(IEnumerable<T> arr)
        {
            foreach (var item in arr)
            {
                Console.Write(item + "/");
            }
        }

        private static void Dump<T>(T obj)
        {
            DumpProperties(obj);
            Console.WriteLine();

            DumpFields(obj);
            Console.WriteLine();

            DumpMethods(obj);
            Console.WriteLine();
        }

        private static void DumpProperties<T>(T obj)
        {
            var properties = typeof(T).GetProperties();

            Console.WriteLine("Properties: ");
            foreach (var prop in properties)
            {
                Console.WriteLine(prop.Name + ": " + prop.PropertyType.Name + " " + prop.GetValue(obj, null));
            }
        }

        private static void DumpFields<T>(T obj)
        {
            var fields = typeof(T).GetFields();

            Console.WriteLine("Fields: ");
            foreach (var field in fields)
            {
                Console.WriteLine(field.Name + ": " + field.FieldType.Name + " " + field.GetValue(obj));
            }
        }

        private static void DumpMethods<T>(T obj)
        {
            var methods = typeof(T).GetMethods();

            Console.WriteLine("Methods: ");
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0)
                {
                    try
                    {
                        Console.WriteLine(method.Name + ": " + method.Invoke(obj, null));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private static void TestSplitWithCondition()
        {
            string text = @"a,[a,c,b],c,d,[e,x]";

            Func<string, int, bool> splitCond = (e, i) =>
            {
                string leftPart = e.Substring(0, i);

                int bracketIndex = leftPart.LastIndexOfAny(new char[] { '[', ']' });

                if (bracketIndex < 0)
                    return true;
                else if (e[bracketIndex] == ']')
                    return true;
                else if (e[bracketIndex] == '[')
                    return false;
                else
                    throw new Exception();
            };

            string[] splitted = SplitWithCondition(text, ',', splitCond);
        }

        private static string[] SplitWithCondition(string text, char splitChar, Func<string, int, bool> condition)
        {
            List<int> matchIndexes = new List<int>();

            for (int lastFound = 0, index = text.IndexOf(splitChar, lastFound); index >= 0; index = text.IndexOf(splitChar, lastFound))
            {
                bool isValid = condition(text, index);

                if (isValid)
                {
                    matchIndexes.Add(index);
                }

                lastFound = index + 1;
            }

            string[] result = new string[matchIndexes.Count + 1];
            int matchBefore = 0;
            for (int i = 0; i < matchIndexes.Count; i++)
            {
                int currentMatch = matchIndexes[i];
                result[i] = text.Substring(matchBefore, currentMatch - matchBefore);
                matchBefore = currentMatch + 1; // length of split character
            }
            result[matchIndexes.Count] = text.Substring(matchBefore);

            return result;
        }

        private static void TestRegex()
        {
            string pattern = "<[a-z]+><[0-8]>";
            string text = "<hey><4><youthere><2>";

            Regex regex = new Regex(pattern);

            string replaced = regex.Replace(text, new MatchEvaluator(a => new string('*', a.Length)));

            var matchCollection = Regex.Matches(text, pattern);

            Match x = matchCollection[0];
            Match x2 = matchCollection[1];

            Group y = x.Groups[0];
            Group y2 = x2.Groups[0];

            Capture z = y.Captures[0];
            Capture z2 = y2.Captures[0];

            foreach (var match in matchCollection)
            {
                Group group = match as Group;

                //foreach (var capture in captures)
                //{
                //    string resultString = capture.ToString();
                //}

                //foreach (var group in match.Groups)
                //{
                //    string grouoString = group.ToString();
                //}

                //string nextvalue = match.NextMatch().Value;
            }
        }

        private static void TestPivot()
        {
            DataTable table = GetTestDebtPivotTable();

            List<FooBar> foobars = new List<FooBar>()
            {
                new FooBar{ Text = "a", Date = new DateTime(2015,12,12), Amount = 4},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,12), Amount = 2},
                new FooBar{ Text = "a", Date = new DateTime(2015,12,10), Amount = 3},
                new FooBar{ Text = "a", Date = new DateTime(2015,12,10), Amount = 3},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,12), Amount = 5},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,10), Amount = 1},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,10), Amount = 4},
            };

            table = DataUtil.PivotAll(foobars, x => x.Date, x => x.Amount, x => x.Average().ToString(), "0", x => x.Month + "-" + x.Day);

            //PrintDataTable(table);

            table = DataUtil.Pivot(foobars, x => x.Text, x => x.Date, x => x.Amount, x => x.Average(), 0, x => x.ToString("MM-dd"));

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

            DataTable table = DataUtil.PivotAll(debtList, x => x.When, x => x.HowMuch, groupConcat, "0");

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

    public class Debt
    {
        public string From { get; set; }

        public string To { get; set; }

        public int When { get; set; }

        public int HowMuch { get; set; }

        public override string ToString()
        {
            return string.Format("From: {0}, To: {1}, When: {2}, HowMuch: {3}", From, To, When, HowMuch);
        }
    }

    public class FooBar
    {
        public string Text { get; set; }

        public DateTime Date { get; set; }

        public int Amount { get; set; }
    }

    public class Dummy : IEquatable<Dummy>
    {
        public int index;
        public string text;
        public string StringProperty { get; set; }

        public Dummy()
        {
        }

        public Dummy(int x, string text)
        {
            this.index = x;
            this.text = text;
        }

        public override string ToString()
        {
            return "Dummy " + index + " and " + text;
        }

        public bool Equals(Dummy dummyObj)
        {
            return dummyObj.text == this.text && dummyObj.index == this.index;
        }
    }

}
