using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq.Expressions;

namespace MapperTextlibrary
{
	public class ExpressionNewMapper<T> : IMapper<T> where T : new()
	{
		public LinkedList<T> MapAll(IDataReader dataReader)
		{
			List<PropertyInfo> properties = CommonMethods.GetCommonProperties<T>(dataReader);

			LinkedList<T> list = new LinkedList<T>();

            Func<T> newInstancer = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();

			while (dataReader.Read())
			{
				T t = Map(dataReader, properties, newInstancer);

				list.AddLast(t);
			}

			return list;
		}

		private T Map(IDataRecord record, List<PropertyInfo> properties, Func<T> newInstancer)
		{
			T t = newInstancer();

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
