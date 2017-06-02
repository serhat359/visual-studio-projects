using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace CasualConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Dummy d = new Dummy(3, "ahmet");

            string exprname = Ext.NameOf(() => d.StringProperty);

            Console.WriteLine(exprname);

            byte a = 250;
            byte b = 255;

            var x = a * b;

            Console.WriteLine(a * b);

            // Closing, Do Not Delete!
            Console.WriteLine("Program has terminated, press a key to exit");
            Console.ReadKey();
        }

        static IEnumerable<int> filter(List<int> list)
        {
            foreach (var item in list)
            {
                if (item < 10)
                    yield return item;
            }
        }

        static IEnumerable<string> GetWords()
        {
            yield return "ahmet";
            yield return "mehmet";
            yield return "süleyman";
        }

        static int DoOrDie(Func<int> action, string errorMessage)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                throw new Exception(errorMessage, e);
            }
        }

        static void PrintArray<GenericType>(IEnumerable<GenericType> arr)
        {
            foreach (var item in arr)
            {
                Console.Write(item + "/");
            }
        }

        private static Action GetPrinterForInstance(IEnumerable<object> elems)
        {

            List<Expression> expList = new List<Expression>();

            var printMethodInfo = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });

            foreach (var elem in elems)
            {
                var paramExp = Expression.Constant(elem, typeof(object));
                expList.Add(Expression.Call(printMethodInfo, paramExp));
            }

            var blockExp = Expression.Block(expList);

            return Expression.Lambda<Action>(blockExp).Compile();
        }

        private static Action<IEnumerable<int>> GetPrinter()
        {
            Expression<Action<int>> printExp = (x) => Console.WriteLine(x);

            var paramExp = Expression.Parameter(typeof(IEnumerable<int>));

            var loopVar = Expression.Variable(typeof(int));

            var printInvokeExp = Expression.Invoke(printExp, loopVar);

            var foreachExp = ForEach(paramExp, loopVar, printInvokeExp);

            return Expression.Lambda<Action<IEnumerable<int>>>(foreachExp, paramExp).Compile();
        }

        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        private static Func<T> GetNewInstancer<T>() where T : new()
        {
            return Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }

        private static Action<T> GetIntPrinter<T>()
        {
            var input = Expression.Parameter(typeof(T));

            Expression<Action<T>> printMethodExp = (x) => System.Console.WriteLine(x);

            var printStatement = Expression.Invoke(printMethodExp, input);

            var printExpression = Expression.Lambda<Action<T>>(printStatement, input);

            return printExpression.Compile();
        }

        private static Func<int, int, int> GetMax()
        {
            /*
                 math max
                 (x,y) => { if(x > y) return x; else return y; } 
            */

            List<Expression> expList = new List<Expression>();

            var paramX = Expression.Parameter(typeof(int));
            var paramY = Expression.Parameter(typeof(int));

            var resultVar = Expression.Variable(typeof(int));

            var ifExp = Expression.IfThenElse(Expression.GreaterThan(paramX, paramY), Expression.Assign(resultVar, paramX), Expression.Assign(resultVar, paramY));

            expList.Add(ifExp);
            expList.Add(resultVar);

            var allExpsBody = Expression.Block(new[] { resultVar }, expList);

            var finalExp = Expression.Lambda<Func<int, int, int>>(allExpsBody, paramX, paramY);

            return finalExp.Compile();
        }

        private static void TestEquality()
        {
            List<Dummy> first = new List<Dummy> { new Dummy(1, "asd") };
            List<Dummy> second = new List<Dummy> { new Dummy(1, "asd") };

            if (Enumerable.SequenceEqual(first, second))
            {
                Console.WriteLine("same");
            }
            else
            {
                Console.WriteLine("not the same");
            }
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

    public class Dummy : IEquatable<Dummy>
    {
        public int index;
        public string text;
        public string StringProperty { get; set; }

        public Dummy()
        {
        }

        public Dummy(int x, string text)
        {
            this.index = x;
            this.text = text;
        }

        public override string ToString()
        {
            return "Dummy " + index + " and " + text;
        }

        public bool Equals(Dummy dummyObj)
        {
            return dummyObj.text == this.text && dummyObj.index == this.index;
        }
    }
}
