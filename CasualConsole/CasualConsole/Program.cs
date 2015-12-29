using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CasualConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            // Closing, Do Not Delete!
            Console.WriteLine("Program has terminated");
            Console.ReadKey();
        }

        public static void Dump<T>(T obj)
        {
            DumpProperties(obj);
            Console.WriteLine();

            DumpFields(obj);
            Console.WriteLine();

            DumpMethods(obj);
            Console.WriteLine();
        }

        private static void DumpProperties<T>(T obj)
        {
            var properties = typeof(T).GetProperties();

            Console.WriteLine("Properties: ");
            foreach (var prop in properties)
            {
                Console.WriteLine(prop.Name + ": " + prop.PropertyType.Name + " " + prop.GetValue(obj, null));
            }
        }

        private static void DumpFields<T>(T obj)
        {
            var fields = typeof(T).GetFields();

            Console.WriteLine("Fields: ");
            foreach (var field in fields)
            {
                Console.WriteLine(field.Name + ": " + field.FieldType.Name + " " + field.GetValue(obj));
            }
        }

        private static void DumpMethods<T>(T obj)
        {
            var methods = typeof(T).GetMethods();

            Console.WriteLine("Methods: ");
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0)
                {
                    try
                    {
                        Console.WriteLine(method.Name + ": " + method.Invoke(obj, null));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }

    public class Dummy
    {
        public int index;
        public string text;
        public string stringProp { get; set; }

        public Dummy(int x, string text)
        {
            this.index = x;
            this.text = text;
        }

        public override string ToString()
        {
            return "Dummy " + index + " and " + text;
        }
    }
}
