using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace MapperTextlibrary
{
	public class MyMapper<T> : IMapper<T> where T : new()
	{
		public LinkedList<T> MapAll(IDataReader dataReader)
		{
			List<PropertyInfo> properties = CommonMethods.GetCommonProperties<T>(dataReader);

			LinkedList<T> list = new LinkedList<T>();

			if (dataReader.Read())
			{
				do
				{
					T t = Map(dataReader, properties);

					list.AddLast(t);
				} while (dataReader.Read());
			}

			return list;
		}

		private T Map(IDataRecord record, List<PropertyInfo> properties)
		{
			T t = new T();

			foreach (PropertyInfo property in properties)
			{
				object value = record[property.Name];

				if (value != DBNull.Value)
					property.SetValue(t, value, null);
			}

			return t;
		}
	}
}
