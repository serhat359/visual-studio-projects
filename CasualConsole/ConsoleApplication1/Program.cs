using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CasualConsole;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestPivot();

            //TestRegex();

            //TestSplitWithCondition();

            TestIntersect();

            Console.WriteLine("Press a key to exit");
            Console.Read();
        }

        private static void TestIntersect()
        {
            int[] firstList = { 2, 5, 3, 7, 4, 8 };

            Debt[] secondList = { 
                                    new Debt{ From= "asd", To="x", HowMuch = 2, When = 3},
                                    new Debt{ From= "kjs", To="rrtu", HowMuch = 6, When = 2},
                                    new Debt{ From= "ret", To="cc", HowMuch = 9, When = 5}
                                };

            var intersected = Intersect(firstList, x => x, secondList, x => x.HowMuch);

            Console.WriteLine(string.Join(",", intersected.LeftSide));
            Console.WriteLine(string.Join(",", intersected.Intersection.Select(x => x.MatchedValue)));
            Console.WriteLine(string.Join(",", intersected.RightSide.Select(x => x.ToString())));
        }

        private static IntersectionResult<T, E, V> Intersect<T, E, V>(IEnumerable<T> source1, Func<T, V> source1Selector, IEnumerable<E> source2, Func<E, V> source2Selector)
        {
            Dictionary<V, T> source1Dic = source1.ToDictionary(source1Selector);

            List<Result<T, E, V>> matches = new List<Result<T, E, V>>();
            List<E> rightSide = new List<E>();

            foreach (E source2Item in source2)
            {
                V source2Value = source2Selector(source2Item);

                T source1Match;
                if (source1Dic.TryGetValue(source2Value, out source1Match))
                {
                    Result<T, E, V> match = new Result<T, E, V>()
                    {
                        LeftValue = source1Match,
                        MatchedValue = source2Value,
                        RightValue = source2Item,
                    };

                    matches.Add(match);

                    source1Dic.Remove(source2Value);
                }
                else
                {
                    rightSide.Add(source2Item);
                }
            }

            IntersectionResult<T, E, V> result = new IntersectionResult<T, E, V>
            {
                LeftSide = source1Dic.Values,
                Intersection = matches,
                RightSide = rightSide,
            };

            return result;
        }

        public class IntersectionResult<T, E, V>
        {
            public ICollection<T> LeftSide { get; set; }
            public ICollection<Result<T, E, V>> Intersection { get; set; }
            public ICollection<E> RightSide { get; set; }
        }

        public class Result<T, E, V>
        {
            public T LeftValue { get; set; }
            public E RightValue { get; set; }
            public V MatchedValue { get; set; }
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

    class Debt
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

    class FooBar
    {
        public string Text { get; set; }

        public DateTime Date { get; set; }

        public int Amount { get; set; }
    }
}
