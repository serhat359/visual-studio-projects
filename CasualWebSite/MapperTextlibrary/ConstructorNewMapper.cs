using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace MapperTextlibrary
{
    public class ConstructorNewMapper<T> : IMapper<T> where T : new()
    {
        public LinkedList<T> MapAll(IDataReader dataReader)
        {
            List<PropertyInfo> properties = CommonMethods.GetCommonProperties<T>(dataReader);

            LinkedList<T> list = new LinkedList<T>();

            var constructorInfo = typeof(T).GetConstructor(Type.EmptyTypes);

            while (dataReader.Read())
            {
                T t = Map(dataReader, properties, (T)constructorInfo.Invoke(null));

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
