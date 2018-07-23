using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleSsmsEcosystemAddin
{
    public class Dumper
    {
        public static void Dump<T>(T obj, Action<string> action)
        {
            Type type = obj.GetType();

            DumpProperties(obj, type, action);
            action(null);

            DumpFields(obj, type, action);
            action(null);

            DumpMethods(obj, type, action);
            action(null);
        }

        private static void DumpProperties<T>(T obj, Type type, Action<string> action)
        {
            var properties = type.GetProperties();

            action("Properties: ");
            foreach (var prop in properties)
            {
                action(prop.Name + ": " + prop.PropertyType.Name + " " + prop.GetValue(obj, null));
            }
        }
        
        private static void DumpFields<T>(T obj, Type type, Action<string> action)
        {
            var fields = type.GetFields();

            action("Fields: ");
            foreach (var field in fields)
            {
                action(field.Name + ": " + field.FieldType.Name + " " + field.GetValue(obj));
            }
        }

        private static void DumpMethods<T>(T obj, Type type, Action<string> action)
        {
            var methods = type.GetMethods();

            action("Methods: ");
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0)
                {
                    try
                    {
                        action(method.Name + ": " + method.Invoke(obj, null));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

    }
}
