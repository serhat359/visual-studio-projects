using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model.Data;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using MyConsole;
using System.Text.RegularExpressions;
using Data;
using MapperTextlibrary;

namespace ConsoleProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TestDifferentMappers();

            /*new DBConnection(DBConnection.Mode.Write)
                .RunUpsertQueryOneRow("INSERT INTO casual (name, phone) VALUES ('ahmet', '5358724473')");*/

            //new DBConnection(DBConnection.Mode.Read).RunUpsertQueryOneRow("UPDATE casual SET name='murat' WHERE name='serhat'");



            string radicalquery = "select * from radical order by strokes";
            
            List<Radical> radicalList = DataFactory.RunSelectQuery<Radical>(radicalquery);

            List<RadicalJS> radicalsForJs = new List<RadicalJS>();

            foreach (var radical in radicalList)
            {
                RadicalJS r = new RadicalJS {
                    Kanji = radical.kanji,
                    Strokes = (int)radical.strokes,
                    Radicals = radical.radicals.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(x => Int32.Parse(x)).ToList()
                };
                radicalsForJs.Add(r);
            }

            var jsonStr = JsonConvert.SerializeObject(radicalsForJs);

            List<char> buttons = new List<char>();

            for (int i = 1; i <= 253; i++)
            {
                char c;

                if (radicalList.Any(x => x.radicals.Contains("," + i + ",")))
                    c = 'P';
                else
                    c = 'I';

                buttons.Add(c);
            }

            foreach (char item in buttons)
            {
                //Console.Write(item);
            }


            string s = string.Join("", buttons);
            Console.WriteLine(s);

            Console.WriteLine();
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        private static void InsertRadicals()
        {
            string tempquery = "select * from radical_temp";

            List<RadicalTemp> radicalTempList = DataFactory.RunSelectQuery<RadicalTemp>(tempquery);

            foreach (var radicalTemp in radicalTempList)
            {
                foreach (var part in DivideParts(radicalTemp.json))
                {
                    Console.WriteLine(radicalTemp.id + "-" + part);

                    uint strokeCount = Convert.ToUInt32(Regex.Replace(part, "[^0-9]", ""));

                    var kanjis = part.Where(x => !IsDigit(x));

                    foreach (char kanji in kanjis)
                    {
                        string radicalquery = string.Format("select * from radical where kanji='{0}'", kanji);
                        List<Radical> radicals = DataFactory.RunSelectQuery<Radical>(radicalquery);

                        if (radicals.Any())
                        {
                            Radical radicalEx = radicals[0];
                            radicalEx.radicals += string.Format("{0},", radicalTemp.id);
                            string updateQuery = UpdateForRadical(radicalEx);
                            new Data.DBConnection(Data.DBConnection.Mode.Write).RunQueryUpdateOneRow(updateQuery);
                        }
                        else
                        {
                            Radical radicalNew = new Radical
                            {
                                kanji = kanji.ToString(),
                                strokes = strokeCount,
                                radicals = string.Format(",{0},", radicalTemp.id)
                            };

                            string insertQuery = InsertForRadical(radicalNew);
                            new Data.DBConnection(Data.DBConnection.Mode.Write).RunQueryUpdateOneRow(insertQuery);
                        }
                    }
                }
            }
        }

        private static string UpdateForRadical(Radical radicalEx)
        {
            string baseQuery = "update radical set radicals='{0}' where kanji='{1}'";
            return string.Format(baseQuery, radicalEx.radicals, radicalEx.kanji);
        }

        public static string InsertForRadical(Radical radicalNew)
        {
            string baseQuery = "INSERT INTO radical (kanji, strokes, radicals) VALUES ('{0}', {1}, '{2}')";
            return string.Format(baseQuery, radicalNew.kanji, radicalNew.strokes, radicalNew.radicals);
        }

        public static IEnumerable<string> DivideParts(string json)
        {
            RadicalJsonResult obj = JsonConvert.DeserializeObject<RadicalJsonResult>(json);

            string wholeStr = obj.results;

            int lastInt = 0;
            for (int i = 1; i < wholeStr.Length; i++)
            {
                if (IsDigit(wholeStr[i]) && !IsDigit(wholeStr[i - 1]))
                {
                    yield return wholeStr.Substring(lastInt, i - lastInt);
                    lastInt = i;
                }
            }

            yield return wholeStr.Substring(lastInt, wholeStr.Length - lastInt);
        }

        static bool IsDigit(char x)
        {
            return x >= '0' && x <= '9';
        }

        private static void TestDifferentMappers()
        {
            StringBuilder builder = new StringBuilder();

            List<MapperResult> results = new List<MapperResult>();

            IMapper<Example>[] mappers = new IMapper<Example>[] 
			{
				new MyMapper<Example>(),
				new ActivatorNewMapper<Example>(),
				new ExpressionNewMapper<Example>(), 
				new ConstructorNewMapper<Example>(),
				new ExpressionTreeMapperNullCheck<Example>(),
				new ExpressionTreeMapperAs<Example>(),
				new ExampleMapper(),
				new ExampleMapperByIndex(),
			};

            int[] numberList = { 100 };
            foreach (int number in numberList)
            {
                Console.WriteLine("Running for {0} elements", number);
                foreach (var mapper in mappers)
                {
                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start();

                    LinkedList<Example> result;
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);

                    stopwatch.Stop();

                    string typeName = mapper.GetType().Name;

                    results.Add(new MapperResult { ConsumedTime = stopwatch.ElapsedMilliseconds, MapperName = typeName, ResultingList = result });

                    builder.AppendFormat("{0}\t{1}\t{2}\r\n", typeName, number, stopwatch.ElapsedMilliseconds);

                    Console.WriteLine("Result for {0}: {1}", typeName, stopwatch.ElapsedMilliseconds);
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            bool checkForIntegrity = true;
            if (checkForIntegrity)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    for (int j = 0; j < results.Count; j++)
                    {
                        if (!Enumerable.SequenceEqual(results[i].ResultingList, results[j].ResultingList))
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0} and {1} are not the same", i, j);
                        }
                    }
                }
            }

            File.WriteAllText(@"C:\Users\Serhat\Documents\Visual Studio 2010\mapperresults.txt", builder.ToString());

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        public static LinkedList<Example> RunQuery(IMapper<Example> mapper, int number)
        {
            string query = string.Format("select * from example limit {0}", number);

            return new MapperTextlibrary.DBConnection(MapperTextlibrary.DBConnection.Mode.Read).RunSelectQuery(query, mapper);
        }
    }
}
