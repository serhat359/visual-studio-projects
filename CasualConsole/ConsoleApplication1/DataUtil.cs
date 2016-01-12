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
        public static DataTable PivotAll<T, C, V>(ICollection<T> elements, Expression<Func<T, C>> columnNameProperty, Expression<Func<T, V>> valueProperty, Func<IEnumerable<V>, string> reduceMethod, string defaultvalue, Func<C, string> valueFormat = null)
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

        public static DataTable Pivot<T, RH, CH, V, O>(IEnumerable<T> sourceElements, Expression<Func<T, RH>> rowProperty, Func<T, CH> columnProperty, Func<T, V> valueProperty, Func<IEnumerable<V>, O> valueReducer, O defaultValue, Func<CH, string> columnNameFormatter) where CH : IComparable<CH>
        {
            Func<T, RH> rowSelector = rowProperty.Compile();
            List<CH> distinctColumnNames = sourceElements.Select(columnProperty).Distinct().ToList();

            PropertyInfo columnMember = (rowProperty.Body as MemberExpression).Member as PropertyInfo;

            DataTable table = new DataTable();
            table.Columns.Add(columnMember.Name, columnMember.PropertyType);
            foreach (CH item in distinctColumnNames)
            {
                table.Columns.Add(columnNameFormatter(item), typeof(O));
            }

            var groupedBy = sourceElements.GroupBy(rowSelector).ToList();

            foreach (var groupPair in groupedBy)
            {
                RH rowHeader = groupPair.Key;
                List<T> groupedSourceElements = groupPair.ToList();

                object[] rowValues = new object[distinctColumnNames.Count + 1];
                rowValues[0] = rowHeader;

                int i = 1;
                foreach (CH columnHeader in distinctColumnNames)
                {
                    IEnumerable<V> values = groupedSourceElements.Where(x => columnProperty(x).CompareTo(columnHeader) == 0).Select(valueProperty);
                    O outputValue = values.Any() ? valueReducer(values) : defaultValue;
                    rowValues[i] = outputValue;
                    i++;
                }

                table.Rows.Add(rowValues);
            }

            return table;
        }

        #region Private Methods
        private static Dictionary<string, List<object>> keyToValues = new Dictionary<string, List<object>>();

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
        #endregion
    }
}
