using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using System.Data;
using MapperTextlibrary;
using MySql.Data.Types;

namespace MapperTextlibrary
{
	public class ExampleMapper : IMapper<Example>
	{
		public LinkedList<Example> MapAll(IDataReader reader)
		{
			LinkedList<Example> exampleList = new LinkedList<Example>();

			while (reader.Read()) {
				exampleList.AddLast(Map(reader));
			}

			return exampleList;
		}

		public Example Map(IDataRecord record){
			
			Example example = new Example();
			example.id = (UInt32)record["id"];
			example.name = (string)record["name"];
			example.largetext = (string)record["largetext"];
			example.date = (DateTime)record["date"];
			example.money = (double)record["money"];
			example.code = record["code"] as string;
			example.insertdate = record["insertdate"] as Nullable<DateTime>;
			example.isgood = (bool)record["isgood"];
			example.somecount = record["somecount"] as Nullable<long>;

			return example;
		}
	}
}
