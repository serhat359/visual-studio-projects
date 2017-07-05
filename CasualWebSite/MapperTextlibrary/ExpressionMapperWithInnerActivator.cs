using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;

namespace MapperTextlibrary
{
    public class ExpressionMapperWithInnerActivator<T> : IMapper<T> where T : new()
    {
        public LinkedList<T> MapAll(IDataReader dataReader)
        {
            LinkedList<T> list = new LinkedList<T>();

            BlockExpression blockEx;
            Func<IDataRecord, T> converter = GetMapFunc(dataReader, out blockEx);

            try
            {
                while (dataReader.Read())
                {
                    T obj = converter(dataReader);
                    list.AddLast(obj);
                }
            }
            catch
            {
                string expectedTypes = string.Join(" , ", Enumerable.Range(0, dataReader.FieldCount).Select(x => dataReader.GetDataTypeName(x)));

                string receivedTypes = string.Join(" , ", Enumerable.Range(0, dataReader.FieldCount).Select(x => dataReader[x]).Select(x => x.GetType().ToString().Replace("System.", "")));

                string receivedValues = string.Join(" , ", Enumerable.Range(0, dataReader.FieldCount).Select(x => dataReader[x]));

                throw;
            }

            return list;
        }

        private Func<IDataRecord, T> GetMapFunc(IDataReader dataReader, out BlockExpression blockEx)
        {
            var exps = new List<Expression>();

            var paramExp = Expression.Parameter(typeof(IDataRecord), "record");

            var typeOfT = typeof(T);
            var targetExp = Expression.Variable(typeOfT);
            var typeExpr = Expression.Constant(typeOfT);

            var methodInfo = typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type) });
            var callExpr = Expression.Call(methodInfo, typeExpr);
            var castExpr = Expression.Convert(callExpr, typeOfT);

            exps.Add(Expression.Assign(targetExp, castExpr));

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

            blockEx = block;

            return Expression.Lambda<Func<IDataRecord, T>>(block, paramExp).Compile();
        }
    }
}
