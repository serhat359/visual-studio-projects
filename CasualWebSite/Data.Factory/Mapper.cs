using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace Data
{
    public class Mapper
    {
        [Obsolete]
        public static List<T> MapAll<T>(IDataReader dataReader) where T : new()
        {
            List<PropertyInfo> properties = GetCommonProperties<T>(dataReader);

            List<T> list = new List<T>();

            while (dataReader.Read())
            {
                T t = Map<T>(dataReader, properties);

                list.Add(t);
            }

            return list;
        }

        [Obsolete]
        public static T MapSingle<T>(IDataReader dataReader) where T : class, new()
        {
            List<PropertyInfo> properties = GetCommonProperties<T>(dataReader);

            if (dataReader.Read())
                return Map<T>(dataReader, properties);
            else
                return null;
        }

        private static List<PropertyInfo> GetCommonProperties<T>(IDataReader dataReader) where T : new()
        {
            IEnumerable<string> columnNames = Enumerable.Range(0, dataReader.FieldCount).Select(dataReader.GetName);

            List<PropertyInfo> properties = columnNames.Select(x => typeof(T).GetProperty(x)).Where(x => x != null).ToList();

            return properties;
        }

        private static T Map<T>(IDataRecord record, List<PropertyInfo> properties) where T : new()
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

		public static List<T> MapAllByExpression<T>(IDataReader dataReader)
		{
			List<T> list = new List<T>();

			Func<IDataRecord, T> converter = GetMapFunc<T>(dataReader);

			while (dataReader.Read())
			{
				T obj = converter(dataReader);
				list.Add(obj);
			}

			return list;
		}

		private static Func<IDataRecord, T> GetMapFunc<T>(IDataReader dataReader)
		{
			var exps = new List<Expression>();

			var paramExp = Expression.Parameter(typeof(IDataRecord), "o7thDR");

			var targetExp = Expression.Variable(typeof(T));
			exps.Add(Expression.Assign(targetExp, Expression.New(targetExp.Type)));

			//does int based lookup
			var indexerInfo = typeof(IDataRecord).GetProperty("Item", new[] { typeof(int) });

			var columnNames = Enumerable.Range(0, dataReader.FieldCount).Select(i => new { i, name = dataReader.GetName(i) });

			foreach (var column in columnNames)
			{
				var property = targetExp.Type.GetProperty(column.name);
				if (property == null)
					continue;

				var columnIndexExp = Expression.Constant(column.i);
				var propertyExp = Expression.MakeIndex(paramExp, indexerInfo, new[] { columnIndexExp });

				UnaryExpression convertExp;
				if (property.PropertyType == typeof(string) || Nullable.GetUnderlyingType(property.PropertyType) != null)
					convertExp = Expression.TypeAs(propertyExp, property.PropertyType);
				else
					convertExp = Expression.Convert(propertyExp, property.PropertyType);

				var bindExp = Expression.Assign(Expression.Property(targetExp, property), convertExp);

				exps.Add(bindExp);
			}

			exps.Add(targetExp);

			var bodyExp = Expression.Block(new[] { targetExp }, exps);

			return Expression.Lambda<Func<IDataRecord, T>>(bodyExp, paramExp).Compile();
		}
    }
}
