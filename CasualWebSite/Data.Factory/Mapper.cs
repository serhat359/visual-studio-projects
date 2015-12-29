using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Data
{
    public class Mapper
    {
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
    }
}
