using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CasualConsole;

namespace ConsoleApplication1
{
    class DataUtil
    {
        private static Dictionary<string, List<object>> keyToValues = new Dictionary<string, List<object>>();

        public static DataTable Pivot<T, C, V>(ICollection<T> elements, Expression<Func<T, C>> columnNameProperty, Expression<Func<T, V>> valueProperty, Func<IEnumerable<V>, string> reduceMethod, string defaultvalue, Func<C, string> valueFormat = null)
        {
            List<PropertyInfo> otherProperties = GetOtherProperties<T, C, V>(columnNameProperty, valueProperty);

            Func<T, C> columnNameFunc = columnNameProperty.Compile();
            Func<T, V> valueFunc = valueProperty.Compile();

            List<C> distinctColumnValuesForTable = elements.Select(columnNameFunc).Distinct().ToList();

            DataTable table = PrepareTableWithHeader<C, V>(valueFormat, otherProperties, distinctColumnValuesForTable);

            var valueDic = GetValuesForTable<T, C, V>(elements, otherProperties, columnNameFunc, valueFunc);

            AddValuesToTable<C, V>(reduceMethod, defaultvalue, distinctColumnValuesForTable, table, valueDic);

            return table;
        }

        private static void AddValuesToTable<C, V>(Func<IEnumerable<V>, object> reduceMethod, string defaultvalue, List<C> distinctColumnValues, DataTable table, Dictionary<string, Dictionary<C, List<V>>> valueDic)
        {
            foreach (var item in valueDic)
            {
                string key = item.Key;
                IEnumerable<object> otherValues = keyToValues[key];

                IEnumerable<object> pivotValues = distinctColumnValues.Select(x =>
                {
                    Dictionary<C, List<V>> valuePairs = item.Value;

                    List<V> values = valuePairs.GetExistingOrDefault(x);

                    object value = values != null && values.Any() ? reduceMethod(values) : defaultvalue;

                    return value;
                });

                object[] rowValues = otherValues.Concat(pivotValues).ToArray();

                table.Rows.Add(rowValues);
            }
        }

        private static Dictionary<string, Dictionary<C, List<V>>> GetValuesForTable<T, C, V>(ICollection<T> elements, List<PropertyInfo> otherProperties, Func<T, C> columnNameFunc, Func<T, V> valueFunc)
        {
            var valueDic = new Dictionary<string, Dictionary<C, List<V>>>();

            foreach (T item in elements)
            {
                string propKey = GetKey(item, otherProperties);
                C columnValue = columnNameFunc(item);
                V value = valueFunc(item);

                List<V> valueList = valueDic.GetValueAssuring(propKey).GetValueAssuring(columnValue);
                valueList.Add(value);
            }

            return valueDic;
        }

        private static string GetKey<T>(T item, List<PropertyInfo> otherProperties)
        {
            List<object> values = otherProperties.Select(prop => prop.GetValue(item, null)).ToList();

            string key = string.Join("^!", values);

            keyToValues[key] = values;

            return key;
        }

        private static DataTable PrepareTableWithHeader<C, V>(Func<C, string> valueFormat, List<PropertyInfo> otherProperties, List<C> distinctColumnValues)
        {
            DataTable table = new DataTable();
            foreach (var property in otherProperties)
            {
                table.Columns.Add(property.Name, property.PropertyType);
            }

            Type typeOfValue = typeof(object);
            foreach (C value in distinctColumnValues)
            {
                string columnName = valueFormat != null ? valueFormat(value) : value.ToString();
                table.Columns.Add(columnName, typeOfValue);
            }
            return table;
        }

        private static List<PropertyInfo> GetOtherProperties<T, C, V>(Expression<Func<T, C>> columnNameProperty, Expression<Func<T, V>> valueProperty)
        {
            PropertyInfo[] allProperties = typeof(T).GetProperties();

            MemberExpression columnNameMember = columnNameProperty.Body as MemberExpression;
            MemberExpression valueMember = valueProperty.Body as MemberExpression;

            if (columnNameMember == null) throw new ArgumentException("Expression is not a member expression", "columnNameProperty");
            if (valueMember == null) throw new ArgumentException("Expression is not a member expression", "valueProperty");

            PropertyInfo columnNamePropertyInfo = columnNameMember.Member as PropertyInfo;
            PropertyInfo valuePropertyInfo = valueMember.Member as PropertyInfo;

            List<PropertyInfo> otherProperties = allProperties.Where(x => x.Name != columnNamePropertyInfo.Name && x.Name != valuePropertyInfo.Name).ToList();
            return otherProperties;
        }

    }
}
