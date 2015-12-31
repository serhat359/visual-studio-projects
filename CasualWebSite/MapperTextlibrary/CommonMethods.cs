using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MapperTextlibrary
{
	public class CommonMethods
	{
		public static Func<T> GetNewInstanceFunc<T>()
		{
			return Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
		}

		public static List<PropertyInfo> GetCommonProperties<T>(IDataReader dataReader)
		{
			IEnumerable<string> columnNames = Enumerable.Range(0, dataReader.FieldCount).Select(dataReader.GetName);

			List<PropertyInfo> properties = columnNames.Select(x => typeof(T).GetProperty(x)).Where(x => x != null).ToList();

			return properties;
		}

	}
}
