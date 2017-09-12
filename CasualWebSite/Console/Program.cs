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

            IMapper<FaaliyetMSSQL> mapper = new ExpressionTreeMapperAs<FaaliyetMSSQL>();
            var result = RunQuery(mapper, 10000);
            
            // Closing, Do Not Delete!
            Console.WriteLine();
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        private static void TestDifferentMappers()
        {
            StringBuilder builder = new StringBuilder();

            List<MapperResult<FaaliyetMSSQL>> results = new List<MapperResult<FaaliyetMSSQL>>();

            IMapper<FaaliyetMSSQL>[] mappers = new IMapper<FaaliyetMSSQL>[] 
			{
				new ExpressionTreeMapperAs<FaaliyetMSSQL>(),
				new ExpressionTreeMapperNullCheck<FaaliyetMSSQL>(),
                new ExpressionMapperWithActivator<FaaliyetMSSQL>(),
                new ExpressionMapperWithInnerActivator<FaaliyetMSSQL>(),
				new MyMapper<FaaliyetMSSQL>(),
				new ActivatorNewMapper<FaaliyetMSSQL>(),
                new ActivatorNewMapperTemp<FaaliyetMSSQL>(),
				new ExpressionNewMapper<FaaliyetMSSQL>(),
				new ConstructorNewMapper<FaaliyetMSSQL>(),
				new FaaliyetMapper(),
				new FaaliyetMapperByIndex(),
			};

            bool report = true;
            int[] numberList = { 10000, 10000, 10000 };
            foreach (int number in numberList)
            {
                Console.WriteLine("Running for {0} elements", number);
                foreach (var mapper in EachValueMultiple(mappers))
                {
                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start();

                    LinkedList<FaaliyetMSSQL> result;
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);
                    result = RunQuery(mapper, number);

                    stopwatch.Stop();

                    string typeName = mapper.GetType().Name;

                    results.Add(new MapperResult<FaaliyetMSSQL> { ConsumedTime = stopwatch.ElapsedMilliseconds, MapperName = typeName, ResultingList = result });

                    builder.AppendFormat("{0}\t{1}\t{2}\r\n", typeName, number, stopwatch.ElapsedMilliseconds);

                    Console.WriteLine("Result for {0}: {1}", typeName, stopwatch.ElapsedMilliseconds);
                }

                if (report) {
                    report = false;
                    Console.WriteLine("Please ignore the results above, those values are calculated before the profiler kicks in.");
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            bool checkForIntegrity = true;
            if (checkForIntegrity)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    for (int j = i + 1; j < results.Count; j++)
                    {
                        if (!Enumerable.SequenceEqual(results[i].ResultingList, results[j].ResultingList))
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0} and {1} are not the same", results[i].MapperName, results[j].MapperName);
                        }
                    }
                }
            }

            File.WriteAllText(@"C:\Users\Xhertas\Documents\Visual Studio 2010\mapperresults.txt", builder.ToString());
        }

        public static IEnumerable<T> EachValueMultiple<T>(IEnumerable<T> values) {
            foreach (var item in values)
            {
                yield return item;
                yield return item;
                yield return item;
                yield return item;
            }
        }

        public static LinkedList<FaaliyetMSSQL> RunQuery(IMapper<FaaliyetMSSQL> mapper, int limit)
        {
            string query = string.Format("select top {0} * from faaliyet", limit);

            query = @"select top 10000
                	FaaliyetID,
                	AskerID,
                	BirlikID,
                	GelisTarih,
                	CikisTarih,
                	BelgeCikisTarih,
                	DurumID,
                	Aciklama,
                	GeldigiMerkez,
                	GidecegiMerkez,
                	SorumluTel,
                	IsKonvoy,
                	KonvoyID
                from faaliyet";

            return new MapperTextlibrary.DBConnection(MapperTextlibrary.DBConnection.Mode.MSSQL).RunSelectQuery(query, mapper);
        }
    }
}
