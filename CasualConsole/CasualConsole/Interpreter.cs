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
            '(', ')', ',', ';', '=', '+', '-'
        };
        private static readonly HashSet<string> commaSet = new HashSet<string>() { "," };
        private static readonly HashSet<string> plusMinusSet = new HashSet<string>() { "+", "-" };
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

            var testCases = new List<(string code, object value)>()
            {
                ("2", 2),
                ("-2", -2),
                ("(2)", 2),
                ("((2))", 2),
                ("\"Hello world\"", "Hello world"),
                ("'Hello world'", "Hello world"),
                ("('Hello world')", "Hello world"),
                ("var a = 2; a", 2),
                ("var b = 3", 3),
                ("b = 5", 5),
                ("b = (7)", 7),
                ("var c = (7) + 2 - 1", 8),
                ("var _ = 6; _", 6),
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
            };

            var interpreter = new Interpreter();
            foreach (var testCase in testCases)
            {
                var result = interpreter.InterpretCode(testCase.code);
                if (!object.Equals(result, testCase.value))
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
                var value = GetValueFromExpression(expression, true);
                variables.Add(variableName, value);
                return value;
            }

            bool isAssignment = tokens.Count > 1 && tokens[1] == "=";
            if (isAssignment)
            {
                // Assignment to existing variable
                var variableName = tokens[0];
                var expression = new StringRange(tokens, 2, tokens.Count);
                var value = GetValueFromExpression(expression, true);
                variables[variableName] = value;
                return value;
            }
            else
            {
                return GetValueFromExpression(tokens, true);
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

        private CustomValue AddOrSubtract(IEnumerable<(CustomValue value, bool isNegative)> values)
        {
            bool hasMinus = false;
            bool hasString = false;
            foreach (var value in values)
            {
                var valueType = value.value.type;
                if (valueType != ValueType.Number && valueType != ValueType.String)
                    throw new ArgumentException();

                if (value.isNegative)
                    hasMinus = true;
                if (valueType == ValueType.String)
                    hasString = true;
            }

            if (hasMinus && hasString)
                throw new Exception();

            if (!hasString)
            {
                int total = 0;
                foreach (var value in values)
                {
                    if (value.isNegative)
                        total -= (int)value.value.value;
                    else
                        total += (int)value.value.value;
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

        private CustomValue GetValueFromExpression(IReadOnlyList<string> expressionTokens, bool mayContainPlusMinus)
        {
            if (expressionTokens.Count == 1)
            {
                var token = expressionTokens[0];
                return GetValueFromSingleToken(token);
            }

            if (mayContainPlusMinus)
            {
                CustomRange<(List<string> list, bool isNegative)> plusExpressions = CustomRange.From(expressionTokens.SplitBy(plusMinusSet).ToList());
                if (plusExpressions.Count > 1)
                {
                    if (plusExpressions[0].list.Count == 0)
                        plusExpressions = plusExpressions.SkipSlice(1);
                    var expressionValues = plusExpressions.SelectFast(x => (value: GetValueFromExpression(x.list, false), x.isNegative));
                    return AddOrSubtract(expressionValues);
                }
                else
                {
                    return GetValueFromExpression(plusExpressions[0].list, false);
                }
            }

            if (expressionTokens[0] == "(" && expressionTokens[expressionTokens.Count - 1] == ")")
            {
                var newExpression = new StringRange(expressionTokens, 1, expressionTokens.Count - 1);
                return GetValueFromExpression(newExpression, true);
            }
            else if (IsVariableName(expressionTokens[0]) && expressionTokens[1] == "(")
            {
                var functionName = expressionTokens[0];
                var allExpression = new StringRange(expressionTokens, 2, expressionTokens.IndexOfParenthesesEnd(2));
                var expressions = allExpression.SplitBy(commaSet);
                var arguments = expressions.Select(expression => GetValueFromExpression(expression.list, true)).ToArray();
                var returnValue = CallFunction(functionName, arguments);
                return returnValue;
            }
            else
                throw new Exception();
        }

        private CustomValue GetValueFromSingleToken(string token)
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
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    int start = i;
                    i++;
                    while (i < content.Length && (char.IsLetterOrDigit(content[i]) || c == content[i]))
                        i++;

                    string token = content.Substring(start, i - start);
                    yield return token;
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
                else if (c == '/')
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
                else
                {
                    throw new Exception();
                }
            }
        }
    }

    static class InterpreterExtensions
    {
        public static IEnumerable<(List<string> list, bool isNegative)> SplitBy(this IEnumerable<string> tokens, HashSet<string> separator)
        {
            var list = new List<string>();
            var parenthesesCount = 0;
            bool isNegative = false;

            foreach (var token in tokens)
            {
                if (token == "(") parenthesesCount++;
                else if (token == ")") parenthesesCount--;

                if (parenthesesCount == 0 && separator.Contains(token))
                {
                    yield return (list, isNegative);
                    isNegative = token == "-";
                    list = new List<string>();
                }
                else
                {
                    list.Add(token);
                }
            }
            yield return (list, isNegative);
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
            return new CustomValue(int.Parse(s), ValueType.Number);
        }

        public static CustomValue FromNumber(int s)
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
    }

    class CustomRange
    {
        public static CustomRange<T> From<T>(IReadOnlyList<T> list)
        {
            return new CustomRange<T>(list, 0, list.Count);
        }
    }
}
