using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CasualConsole
{
    public class ExpressionCodes
    {

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

        private static Action<T> GetIntPrinter<T>()
        {
            var input = Expression.Parameter(typeof(T));

            Expression<Action<T>> printMethodExp = (x) => System.Console.WriteLine(x);

            var printStatement = Expression.Invoke(printMethodExp, input);

            var printExpression = Expression.Lambda<Action<T>>(printStatement, input);

            return printExpression.Compile();
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

        private static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
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

    }
}
