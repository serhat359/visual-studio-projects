using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Data;

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
