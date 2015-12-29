using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using MapperTextlibrary;
using System.Diagnostics;

namespace ConsoleProgram
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Dictionary<string, int> results = new Dictionary<string, int>();
			
			IMapper<Example>[] mappers = new IMapper<Example>[] 
			{ 
				new MyMapper<Example>(), 
				new ExpressionNewMapper<Example>(), 
				new ExpressionMapMapper<Example>(), 
				new ExampleMapper(),
			};

			foreach (var mapper in mappers)
			{
				Stopwatch stopwatch = new Stopwatch();

				stopwatch.Start();

				LinkedList<Example> result = RunQuery(mapper);

				stopwatch.Stop();

				string typeName = mapper.GetType().Name;

				Console.WriteLine("Result for {0}: {1}", typeName, stopwatch.ElapsedMilliseconds);
			}

			Console.WriteLine("Press a key to exit");
			Console.ReadKey();
		}

		public static LinkedList<Example> RunQuery(IMapper<Example> mapper)
		{
			string query = "select * from example";

			return new DBConnection(DBConnection.Mode.Read).RunSelectQuery(query, mapper);
		}
	}
}
