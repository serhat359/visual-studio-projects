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
            TestDifferentMappers();

            /*new DBConnection(DBConnection.Mode.Write)
                .RunUpsertQueryOneRow("INSERT INTO casual (name, phone) VALUES ('ahmet', '5358724473')");*/

            //new DBConnection(DBConnection.Mode.Read).RunUpsertQueryOneRow("UPDATE casual SET name='murat' WHERE name='serhat'");

            IMapper<Radical> mapper = new ExpressionTreeMapperAs<Radical>();
            var result = RunQuery(mapper, 10000);

            Console.WriteLine();
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        private static void TestDifferentMappers()
        {
            StringBuilder builder = new StringBuilder();

            List<MapperResult<Radical>> results = new List<MapperResult<Radical>>();

            IMapper<Radical>[] mappers = new IMapper<Radical>[] 
			{
				new MyMapper<Radical>(),
				new ActivatorNewMapper<Radical>(),
				new ExpressionNewMapper<Radical>(),
				new ConstructorNewMapper<Radical>(),
				new ExpressionTreeMapperNullCheck<Radical>(),
				new ExpressionTreeMapperAs<Radical>(),
				new RadicalMapper(),
				new RadicalMapperByIndex(),
			};

            int[] numberList = { 10000, 10000, 10000 };
            foreach (int number in numberList)
            {
                Console.WriteLine("Running for {0} elements", number);
                foreach (var mapper in mappers)
                {
                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start();

                    LinkedList<Radical> result;
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);

                    stopwatch.Stop();

                    string typeName = mapper.GetType().Name;

                    results.Add(new MapperResult<Radical> { ConsumedTime = stopwatch.ElapsedMilliseconds, MapperName = typeName, ResultingList = result });

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

            File.WriteAllText(@"C:\Users\Xhertas\Documents\Visual Studio 2010\mapperresults.txt", builder.ToString());
        }

        public static LinkedList<Radical> RunQuery(IMapper<Radical> mapper, int limit)
        {
            string query = string.Format("select * from radical limit {0}", limit);

            return new MapperTextlibrary.DBConnection(MapperTextlibrary.DBConnection.Mode.Read).RunSelectQuery(query, mapper);
        }
    }
}
