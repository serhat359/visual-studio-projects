using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using System.Data;
using MySql.Data.Types;

namespace MapperTextlibrary
{
	public class ExampleMapperByIndex : IMapper<Example>
	{
		public LinkedList<Example> MapAll(IDataReader reader)
		{
			LinkedList<Example> exampleList = new LinkedList<Example>();

			while (reader.Read())
			{
				exampleList.AddLast(Map(reader));
			}

			return exampleList;
		}

		public Example Map(IDataRecord record)
		{
			Example example = new Example();
			example.id = (UInt32)record[0]; // id
			example.name = (string)record[1]; // name
			example.largetext = (string)record[2]; // largetext
			example.date = (DateTime)record[3]; // date
			example.money = (double)record[4]; // money
			example.code = record[5] as string; // code
			example.insertdate = record[6] as Nullable<DateTime>; // insertdate
			example.isgood = (bool)record[7]; // isgood
			example.somecount = record[8] as Nullable<long>; // somecount

			return example;
		}
	}
}
