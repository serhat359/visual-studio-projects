using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MapperTextlibrary;
using Model.Data;
using System.IO;
using System.Text;

namespace ConsoleProgram
{
	public class Program
	{
		public static void Main(string[] args)
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

			return new DBConnection(DBConnection.Mode.Read).RunSelectQuery(query, mapper);
		}
	}
}
