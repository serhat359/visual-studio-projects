﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data;

namespace TorrentSeedLeechCounter
{
    class ExpressionTreeMapperAs<T> where T : new()
    {
        public List<T> MapAll(IDataReader dataReader)
        {
            List<T> list = new List<T>();

            BlockExpression blockEx;
            Func<IDataRecord, T> converter = GetMapFunc(dataReader, out blockEx);

            try
            {
                while (dataReader.Read())
                {
                    T obj = converter(dataReader);
                    list.Add(obj);
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

            blockEx = block;

            return Expression.Lambda<Func<IDataRecord, T>>(block, paramExp).Compile();
        }
    }
}
