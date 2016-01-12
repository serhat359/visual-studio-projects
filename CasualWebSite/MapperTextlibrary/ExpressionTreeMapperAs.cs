using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data;

namespace MapperTextlibrary
{
	public class ExpressionTreeMapperAs<T> : IMapper<T> where T : new()
	{
		public LinkedList<T> MapAll(IDataReader dataReader)
		{
			LinkedList<T> list = new LinkedList<T>();

			Func<IDataRecord, T> converter = GetMapFunc(dataReader);

			while (dataReader.Read())
			{
				T obj = converter(dataReader);
				list.AddLast(obj);
			}

			return list;
		}

		private Func<IDataRecord, T> GetMapFunc(IDataReader dataReader)
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

			var block = Expression.Block(new[] { targetExp }, exps);

			return Expression.Lambda<Func<IDataRecord, T>>(block, paramExp).Compile();
		}
	}
}
