using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace MapperTextlibrary
{
	public class ExpressionTreeMapperNullCheck<T> : IMapper<T> where T : new()
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

            var paramExp = Expression.Parameter(typeof(IDataRecord), "record");
			var dbNullParamExpression = Expression.Constant(DBNull.Value);

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

				var columnNameExp = Expression.Constant(column.i);
				var propertyExp = Expression.MakeIndex(paramExp, indexerInfo, new[] { columnNameExp });
				var convertExp = Expression.Convert(propertyExp, property.PropertyType);
				var bindExp = Expression.Assign(Expression.Property(targetExp, property), convertExp);
				var testExp = Expression.NotEqual(propertyExp, dbNullParamExpression);
				var ifExpression = Expression.IfThen(testExp, bindExp);

				exps.Add(ifExpression);
			}

			exps.Add(targetExp);

			var block = Expression.Block(new[] { targetExp }, exps);

			return Expression.Lambda<Func<IDataRecord, T>>(block, paramExp).Compile();
		}
	}
}
