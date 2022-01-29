using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringRange = CasualConsole.CustomRange<string>;

namespace CasualConsole
{
    public class Interpreter
    {
        private static readonly HashSet<char> onlyChars = new HashSet<char>()
        {
            '(', ')', ',', ';', '=', '+', '-', '*', '/'
        };
        private static readonly HashSet<string> commaSet = new HashSet<string>() { "," };
        public static readonly HashSet<string> plusMinusSet = new HashSet<string>() { "+", "-" };
        public static readonly HashSet<string> asteriskSlashSet = new HashSet<string>() { "*", "/" };
        private static readonly HashSet<string> constantDefinedFunctions = new HashSet<string>()
        {
            "print"
        };

        private Dictionary<string, CustomValue> variables = new Dictionary<string, CustomValue>();

        public object InterpretCode(string code)
        {
            var tokens = GetTokens(code).ToList();

            return InterpretTokens(tokens).value;
        }

        public static void Test()
        {
            CustomValue.Test();
            ExpressionTree.Test();

            var testCases = new List<(string code, object value)>()
            {
                ("2", 2),
                ("-2", -2),
                ("(2)", 2),
                ("-(2)", -2),
                ("-(-2)", 2),
                ("+(-2)", -2),
                ("-(+2)", -2),
                ("((2))", 2),
                ("-(-(2))", 2),
                ("\"Hello world\"", "Hello world"),
                ("'Hello world'", "Hello world"),
                ("('Hello world')", "Hello world"),
                ("var a = 2; a", 2),
                ("var a2 = 2; var b2 = a2 = 5; b2", 5),
                ("var b = 3", 3),
                ("b = 5", 5),
                ("b = (7)", 7),
                ("var c = (7) + 2 - 1", 8),
                ("var _ = 6; _", 6),
                ("var _a = 7; _a", 7),
                ("var a_ = 8; a_", 8),
                ("var __ = 9; __", 9),
                ("var _a_ = 10; _a_", 10),
                ("var _aa_ = 11; _aa_", 11),
                ("// this is a comment \n var comment = 5", 5),
                ("/* this is another comment */ var   comment2   =   5", 5),
                ("returnValue(2)", 2),
                ("returnValue(5, 6)", 5),
                ("returnValue(7 + 2)", 9),
                ("returnValue(7 - 2)", 5),
                ("returnValue((5), 6)", 5),
                ("returnValue(5, (6))", 5),
                ("returnValue(\"hello\")", "hello"),
                ("returnValue('hello')", "hello"),
                ("returnValue(returnValue(2))", 2),
                ("returnValue(returnValue(returnValue(2)))", 2),
                ("2 + 3", 5),
                ("1 + 2", 3),
                ("1 + 2 + 3", 6),
                ("1 - 2 + 3", 2),
                ("1 + 2 - 3", 0),
                ("(1 + 2) + 3", 6),
                ("(1 - 2) - 3", -4),
                ("1 + (2 + 3)", 6),
                ("1 + (2 - 3)", 0),
                ("1 - (2 + 5)", -6),
                ("'' + 2 + 3", "23"),
                ("2 + 3 + ''", "23"),
                ("'' + (2 + 3)", "5"),
                ("(2 + 3) + ''", "5"),
                ("returnValue(2) + returnValue(3)", 5),
                ("returnValue(2) + 3", 5),
                ("returnValue(2) + (3)", 5),
                ("returnValue(2, 3) + (7)", 9),
                ("returnValue(2+1) + (7)", 10),
                ("returnValue(-2) + (7)", 5),
                ("(-2) + (7)", 5),
                ("-2 + (7)", 5),
                ("2 + returnValue(3)", 5),
                ("(2) + returnValue(3)", 5),
                ("'hello' + 'world'", "helloworld"),
                ("'2' + '3'", "23"),
                ("\"2\" + \"3\"", "23"),
                ("\"2\" + '3'", "23"),
                ("'2' + \"3\"", "23"),
                ("2 + '3'", "23"),
                ("'2' + 3", "23"),
                ("2 + 3", 5),
                ("2.0 + 3", 5),
                ("2.0 + 3.0", 5),
                ("2.0 - 3.0", -1),
                ("2.5 + 3.5", 6),
                ("2 * 5", 10),
                ("5 * 2", 10),
                ("5 / 2", 2.5),
                ("5 / 2 / 2", 1.25),
                ("5 / 2 * 2", 5),
                ("5 * 2 / 2", 5),
                ("5 * 2 + 2", 12),
                ("5 + 2 * 2", 9),
                ("(5 + 2) * 2", 14),
            };

            var interpreter = new Interpreter();
            foreach (var testCase in testCases)
            {
                var result = interpreter.InterpretCode(testCase.code);
                if (!InterpreterExtensions.Equals(result, testCase.value))
                {
                    throw new Exception();
                }
            }

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All Interpreter tests have passed!");
            Console.ForegroundColor = oldColor;
        }

        private CustomValue InterpretTokens(IReadOnlyList<string> tokenSource)
        {
            var statements = tokenSource.GetStatements();
            foreach (var statement in statements)
            {
                bool isLastStatement = statement.end == tokenSource.Count;
                var value = InterpretExpression(statement);
                if (isLastStatement)
                {
                    return value;
                }
            }
            return CustomValue.Null;
        }

        private CustomValue InterpretExpression(IReadOnlyList<string> tokens)
        {
            if (tokens.Count == 0) return CustomValue.Null;

            var firstToken = tokens[0];
            if (firstToken == "var")
            {
                // Assignment to new variable
                var variableName = tokens[1];
                var shouldBeEquals = tokens[2];
                if (shouldBeEquals != "=")
                {
                    throw new Exception();
                }

                var expression = new StringRange(tokens, 3, tokens.Count);
                var value = InterpretExpression(expression);
                variables.Add(variableName, value);
                return value;
            }

            bool isAssignment = tokens.Count > 1 && tokens[1] == "=";
            if (isAssignment)
            {
                // Assignment to existing variable
                var variableName = tokens[0];
                var expression = new StringRange(tokens, 2, tokens.Count);
                var value = InterpretExpression(expression);
                variables[variableName] = value;
                return value;
            }
            else
            {
                return GetValueFromExpression(tokens);
            }
        }

        private CustomValue CallFunction(string functionName, CustomValue[] arguments)
        {
            switch (functionName)
            {
                case "print":
                    return HandlePrint(arguments);
                case "returnValue":
                    return HandleReturnValue(arguments);
                default:
                    throw new Exception();
            }
        }

        private CustomValue HandlePrint(CustomValue[] arguments)
        {
            Console.WriteLine(arguments[0].value);
            return CustomValue.Null;
        }

        private CustomValue HandleReturnValue(CustomValue[] arguments)
        {
            return arguments[0];
        }

        private CustomValue MultiplyOrDivide(IReadOnlyList<(Operator operatorType, ExpressionTree tree)> trees)
        {
            if (trees[0].operatorType != Operator.None) throw new Exception();

            var values = trees.SelectFast(x => (x.operatorType, value: EvaluateTree(x.tree)));

            double total = 0;
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (i == 0 && value.operatorType == Operator.None)
                    total = (double)value.value.value;
                else if (value.operatorType == Operator.Multiply)
                    total *= (double)value.value.value;
                else if (value.operatorType == Operator.Divide)
                    total /= (double)value.value.value;
                else
                    throw new Exception();
            }

            return CustomValue.FromNumber(total);
        }

        private CustomValue AddOrSubtract(IReadOnlyList<(Operator operatorType, ExpressionTree tree)> trees)
        {
            bool hasMinus = false;
            bool hasString = false;

            var values = trees.SelectFast(x => (x.operatorType, value: EvaluateTree(x.tree)));

            foreach (var value in values)
            {
                var valueType = value.value.type;
                if (valueType != ValueType.Number && valueType != ValueType.String)
                    throw new ArgumentException();

                if (value.operatorType == Operator.Minus)
                    hasMinus = true;
                if (valueType == ValueType.String)
                    hasString = true;
            }

            if (hasMinus && hasString)
                throw new Exception();

            if (!hasString)
            {
                double total = 0;
                foreach (var value in values)
                {
                    if (value.operatorType == Operator.Minus)
                        total -= (double)value.value.value;
                    else
                        total += (double)value.value.value;
                }
                return CustomValue.FromNumber(total);
            }
            else
            {
                // String concat
                var sb = new StringBuilder();
                foreach (var value in values)
                {
                    sb.Append(value.value.value.ToString());
                }
                return CustomValue.FromParsedString(sb.ToString());
            }
        }

        private CustomValue GetValueFromExpression(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
            {
                var token = expressionTokens[0];
                return GetValueFromSingleToken(token);
            }

            var expressionTree = ExpressionTree.New(expressionTokens);
            return EvaluateTree(expressionTree);
        }

        internal CustomValue GetValueFromSingleToken(string token)
        {
            if (IsVariableName(token))
            {
                if (variables.TryGetValue(token, out var value))
                {
                    return value;
                }
                else
                    throw new Exception($"variable not defined: {token}");
            }
            else if (IsNumber(token))
            {
                return CustomValue.FromNumber(token);
            }
            else if (IsStaticString(token))
            {
                return CustomValue.FromString(token);
            }
            else
                throw new Exception();
        }

        internal CustomValue GetValueFromMultipleTokens(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return GetValueFromSingleToken(expressionTokens[0]);

            if (IsVariableName(expressionTokens[0]) && expressionTokens[1] == "(")
            {
                var functionName = expressionTokens[0];
                var allExpression = new StringRange(expressionTokens, 2, expressionTokens.IndexOfParenthesesEnd(2));
                var expressions = allExpression.SplitBy(commaSet);
                var arguments = expressions.Select(expression => GetValueFromExpression(expression.list)).ToArray();
                var returnValue = CallFunction(functionName, arguments);
                return returnValue;
            }

            throw new Exception();
        }

        internal CustomValue EvaluateTree(ExpressionTree tree)
        {
            if (tree.tokens != null)
            {
                return GetValueFromMultipleTokens(tree.tokens);
            }

            var expressions = tree.expressions.Value;

            if (expressions.Count == 1)
            {
                var operation = expressions[0].operatorType;
                var subTree = expressions[0].tree;
                var subValue = EvaluateTree(subTree);
                if (operation == Operator.None)
                    return subValue;
                if (subValue.type == ValueType.Number && (operation == Operator.Minus || operation == Operator.Plus))
                {
                    if (operation == Operator.Plus)
                        return subValue;
                    else if (operation == Operator.Minus)
                        return CustomValue.FromNumber(-1 * (double)subValue.value);
                    else
                        throw new Exception();
                }
                throw new Exception();
            }

            Operator operatorType = expressions[1].operatorType;
            if (operatorType == Operator.Plus || operatorType == Operator.Minus)
            {
                return AddOrSubtract(expressions);
            }
            if (operatorType == Operator.Multiply || operatorType == Operator.Divide)
            {
                return MultiplyOrDivide(expressions);
            }
            throw new Exception();
        }

        private bool IsVariableName(string token)
        {
            char firstChar = token[0];
            return firstChar == '_' || char.IsLetter(firstChar);
        }

        private bool IsNumber(string token)
        {
            char firstChar = token[0];
            return char.IsDigit(firstChar);
        }

        private bool IsStaticString(string token)
        {
            char firstChar = token[0];
            return firstChar == '"' || firstChar == '\'';
        }

        private IEnumerable<string> GetTokens(string content)
        {
            int i = 0;
            for (; i < content.Length;)
            {
                char c = content[i];
                if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    i++;
                    while (i < content.Length && (char.IsLetterOrDigit(content[i]) || '_' == content[i]))
                        i++;

                    string token = content.Substring(start, i - start);
                    yield return token;
                }
                else if (char.IsDigit(c))
                {
                    int start = i;
                    i++;
                    while (i < content.Length && (char.IsDigit(content[i]) || content[i] == '.'))
                        i++;

                    string token = content.Substring(start, i - start);
                    yield return token;
                }
                else if (c == '/' && i + 1 < content.Length && (content[i + 1] == '/' || content[i + 1] == '*'))
                {
                    // Handle comment
                    i++;
                    char c2 = content[i];
                    if (c2 == '/')
                    {
                        i++;
                        while (content[i] != '\n')
                            i++;
                    }
                    else if (c2 == '*')
                    {
                        i++;
                        while (true)
                        {
                            if (content[i] == '*' && content[i + 1] == '/')
                                break;
                            i++;
                        }
                        i += 2;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (onlyChars.Contains(c))
                {
                    i++;
                    yield return c.ToString();
                }
                else if (c == '"' || c == '\'')
                {
                    int start = i;
                    i++;
                    while (true)
                    {
                        if (content[i] == '\\')
                            i += 2;
                        else if (content[i] == c)
                        {
                            i++;
                            break;
                        }
                        else
                            i++;
                    }
                    string token = content.Substring(start, i - start);
                    yield return token;
                }
                else if (char.IsWhiteSpace(c))
                {
                    i++;
                    while (i < content.Length && char.IsWhiteSpace(content[i]))
                        i++;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
    }

    static class InterpreterExtensions
    {
        public static IEnumerable<(IReadOnlyList<string> list, string operatorToken)> SplitBy(this IReadOnlyList<string> tokens, HashSet<string> separator)
        {
            var index = 0;
            var parenthesesCount = 0;
            string operatorToken = null;

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token == "(") parenthesesCount++;
                else if (token == ")") parenthesesCount--;

                if (parenthesesCount == 0 && separator.Contains(token))
                {
                    yield return (new StringRange(tokens, index, i), operatorToken);
                    operatorToken = token;
                    index = i + 1;
                }
            }
            yield return (new StringRange(tokens, index, tokens.Count), operatorToken);
        }

        public static IEnumerable<StringRange> GetStatements(this IReadOnlyList<string> tokens)
        {
            int index = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == ";")
                {
                    yield return new StringRange(tokens, index, i);
                    i++;
                    index = i;
                }
            }
            yield return new StringRange(tokens, index, tokens.Count);
        }

        public static int IndexOf<T>(this IReadOnlyList<T> source, T element, int startIndex) where T : IEquatable<T>
        {
            for (int i = startIndex; i < source.Count; i++)
            {
                T currentElement = source[i];
                if (currentElement.Equals(element))
                    return i;
            }
            return -1;
        }

        public static int IndexOfParenthesesEnd(this IReadOnlyList<string> source, int startIndex)
        {
            int count = 0;
            for (int i = startIndex; i < source.Count; i++)
            {
                string currentElement = source[i];
                if (currentElement == ")")
                {
                    if (count == 0)
                        return i;

                    count--;
                    if (count < 0)
                        throw new Exception();
                }
                else if (currentElement == "(")
                {
                    count++;
                }
            }
            return -1;
        }

        public static IReadOnlyList<E> SelectFast<T, E>(this IReadOnlyList<T> source, Func<T, E> converter)
        {
            var newArr = new E[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                newArr[i] = converter(source[i]);
            }
            return newArr;
        }

        public static bool Equals(object result, object expected)
        {
            Type resultType = result.GetType();
            Type expectedType = expected.GetType();

            if (resultType == typeof(int) && expectedType == typeof(double))
            {
                return object.Equals((double)(int)result, expected);
            }
            else if (resultType == typeof(double) && expectedType == typeof(int))
            {
                return object.Equals(result, (double)(int)expected);
            }
            else
            {
                return object.Equals(result, expected);
            }
        }
    }

    struct CustomValue
    {
        public object value;
        public ValueType type;

        public static readonly CustomValue Null = new CustomValue(null, ValueType.Null);

        public CustomValue(object value, ValueType type)
        {
            this.value = value;
            this.type = type;
        }

        public static CustomValue FromNumber(string s)
        {
            return new CustomValue(double.Parse(s), ValueType.Number);
        }

        public static CustomValue FromNumber(double s)
        {
            return new CustomValue(s, ValueType.Number);
        }

        public static CustomValue FromString(string s)
        {
            if (s[0] != '"' && s[0] != '\'')
                throw new Exception();

            char firstChar = s[0];

            var sb = new StringBuilder();
            for (int i = 1; i < s.Length; i++)
            {
                char c = s[i];
                if (c == firstChar)
                    break;
                else if (c == '\\')
                {
                    i++;
                    char c2 = s[i];
                    switch (c2)
                    {
                        case '"': sb.Append(c2); break;
                        case '\'': sb.Append(c2); break;
                        case '\\': sb.Append(c2); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case 'n': sb.Append('\n'); break;
                        default: throw new Exception();
                    }
                }
                else
                    sb.Append(c);
            }
            return new CustomValue(sb.ToString(), ValueType.String);
        }

        public static CustomValue FromParsedString(string s)
        {
            return new CustomValue(s, ValueType.String);
        }

        internal static void Test()
        {
            var stringTestCases = new List<(string token, string value)>()
            {
                ("\"2\"", "2"),
                ("\"Hello world\"", "Hello world"),
                ("\"foo\"", "foo"),
                ("\"\\\"foo\\\"\"", "\"foo\""),
                ("\"\\t\"", "\t"),
            };

            foreach (var stringTestCase in stringTestCases)
            {
                var result = CustomValue.FromString(stringTestCase.token);
                if (!string.Equals(result.value, stringTestCase.value))
                {
                    throw new Exception();
                }
            }
        }
    }

    enum ValueType
    {
        Null,
        Number,
        String,
    }

    enum Operator
    {
        None,
        Plus,
        Minus,
        Multiply,
        Divide,
    }

    class CustomRange<T> : IReadOnlyList<T>
    {
        public readonly IReadOnlyList<T> array;
        public readonly int start;
        public readonly int end;

        public CustomRange(IReadOnlyList<T> array, int start, int end)
        {
            this.array = array;
            this.start = start;
            this.end = end;
        }

        public T this[int index] => array[start + index];

        public int Count => end - start;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = start; i < end; i++)
            {
                yield return array[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CustomRange<T> SkipSlice(int skipCount)
        {
            return new CustomRange<T>(this, skipCount, this.Count);
        }

        public CustomRange<T> StripSides()
        {
            return new CustomRange<T>(this, 1, this.Count - 1);
        }
    }

    class CustomRange
    {
        public static CustomRange<T> From<T>(IReadOnlyList<T> list)
        {
            return new CustomRange<T>(list, 0, list.Count);
        }
    }

    class ExpressionTree
    {
        public Lazy<List<(Operator operatorType, ExpressionTree tree)>> expressions = new Lazy<List<(Operator, ExpressionTree)>>();
        public IReadOnlyList<string> tokens;

        private ExpressionTree()
        {

        }

        public static ExpressionTree New(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            var tree = new ExpressionTree();

            var plusMinusSplit = expressionTokens.SplitBy(Interpreter.plusMinusSet);
            foreach (var splitExpression in plusMinusSplit)
            {
                if (splitExpression.list.Count == 0)
                    continue;

                Operator operatorType = Operator.None;
                if (splitExpression.operatorToken == "+")
                    operatorType = Operator.Plus;
                else if (splitExpression.operatorToken == "-")
                    operatorType = Operator.Minus;

                var subTree = ExpressionTree.NewNoPlus(splitExpression.list);
                tree.expressions.Value.Add((operatorType, subTree));
            }
            return tree;
        }

        public static ExpressionTree NewNoPlus(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            var tree = new ExpressionTree();

            var asteriskSlashSplit = expressionTokens.SplitBy(Interpreter.asteriskSlashSet);
            foreach (var splitExpression in asteriskSlashSplit)
            {
                Operator operatorType = Operator.None;
                if (splitExpression.operatorToken == "*")
                    operatorType = Operator.Multiply;
                else if (splitExpression.operatorToken == "/")
                    operatorType = Operator.Divide;

                var subTree = ExpressionTree.NewNoAsterisk(splitExpression.list);
                tree.expressions.Value.Add((operatorType, subTree));
            }
            return tree;
        }

        // Can still contain parentheses
        public static ExpressionTree NewNoAsterisk(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            if (expressionTokens[0] == "(")
                return ExpressionTree.New(CustomRange.From(expressionTokens).StripSides());
            else
                return ExpressionTree.NewStripped(expressionTokens);
        }

        // Should not contain parentheses
        private static ExpressionTree NewStripped(IReadOnlyList<string> expressionTokens)
        {
            var tree = new ExpressionTree();
            tree.tokens = expressionTokens;
            return tree;
        }

        private static ExpressionTree New(string singleToken)
        {
            var tree = new ExpressionTree();
            tree.tokens = new[] { singleToken };
            return tree;
        }

        internal static void Test()
        {
            var testCases = new List<(string[] tokens, object value)>()
            {
                (new []{ "2" }, 2),
                (new []{ "(", "4", ")" }, 4),
                (new []{ "2", "+", "3" }, 5),
            };

            var interpreter = new Interpreter();
            foreach (var testCase in testCases)
            {
                var tree = ExpressionTree.New(testCase.tokens);
                var result = interpreter.EvaluateTree(tree);
                if (!InterpreterExtensions.Equals(result.value, testCase.value))
                {
                    throw new Exception();
                }
            }
        }
    }
}
