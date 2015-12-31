using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MapperTextlibrary;
using Model.Data;

namespace ConsoleProgram
{
	public class Program
	{
		public static void Main(string[] args)
		{
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

			foreach (var mapper in mappers)
			{
				Stopwatch stopwatch = new Stopwatch();

				stopwatch.Start();

				LinkedList<Example> result;
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);
				result = RunQuery(mapper);

				stopwatch.Stop();

				string typeName = mapper.GetType().Name;

				results.Add(new MapperResult { ConsumedTime = stopwatch.ElapsedMilliseconds, MapperName = typeName, ResultingList = result });

				Console.WriteLine("Result for {0}: {1}", typeName, stopwatch.ElapsedMilliseconds);
			}

			for (int i = 0; i < results.Count; i++)
			{
				for (int j = 0; j < results.Count; j++)
				{
					if (Enumerable.SequenceEqual(results[i].ResultingList, results[j].ResultingList))
					{
						//Console.BackgroundColor = ConsoleColor.Black;
						//Console.WriteLine("{0} and {1} are the same", i, j);
					}
					else
					{
						Console.BackgroundColor = ConsoleColor.Red;
						Console.WriteLine("{0} and {1} are not the same", i, j);
					}
				}
			}

			Console.WriteLine("Press a key to exit");
			Console.ReadKey();
		}

		public static LinkedList<Example> RunQuery(IMapper<Example> mapper)
		{
			string query = "select * from example limit 10000";

			return new DBConnection(DBConnection.Mode.Read).RunSelectQuery(query, mapper);
		}
	}
}
