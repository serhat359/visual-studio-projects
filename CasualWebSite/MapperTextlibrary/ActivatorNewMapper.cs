using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

namespace MapperTextlibrary
{
	public class ActivatorNewMapper<T> : IMapper<T> where T : new()
	{
		public LinkedList<T> MapAll(IDataReader dataReader)
		{
			List<PropertyInfo> properties = CommonMethods.GetCommonProperties<T>(dataReader);

			LinkedList<T> list = new LinkedList<T>();

			Type typeOfT = typeof(T);

			while (dataReader.Read())
			{
				T t = Map(dataReader, properties, (T)Activator.CreateInstance(typeOfT));

				list.AddLast(t);
			}

			return list;
		}

		private T Map(IDataRecord record, List<PropertyInfo> properties, T newInstance)
		{
			T t = newInstance;

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
