using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CasualConsole
{
    public class Interpreter
    {
        private static readonly HashSet<char> onlyChars = new HashSet<char>()
        {
            '(', ')', ',', ';', '='
        };
        private static readonly HashSet<string> constantDefinedFunctions = new HashSet<string>()
        {
            "print"
        };

        private Dictionary<string, CustomValue> variables = new Dictionary<string, CustomValue>();

        public object InterpretCode(string code)
        {
            var tokens = GetTokens(code).ToArray();

            return InterpretTokens(tokens).value;
        }

        public static void Test()
        {
            CustomValue.Test();

            var testCases = new List<(string code, object value)>()
            {
                ("2", 2),
                ("(2)", 2),
                ("((2))", 2),
                ("\"Hello world\"", "Hello world"),
                ("'Hello world'", "Hello world"),
                ("('Hello world')", "Hello world"),
                ("var a = 2; a", 2),
                ("var b = 3", 3),
                ("b = 5", 5),
                ("b = (7)", 7),
                ("var _ = 6; _", 6),
                ("// this is a comment \n var comment = 5", 5),
                ("/* this is another comment */ var   comment2   =   5", 5),
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
        }

        private CustomValue InterpretTokens(string[] tokenSource)
        {
            var statements = tokenSource.GetStatements();
            foreach (var statement in statements)
            {
                bool isLastStatement = statement.end == tokenSource.Length;
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
                var expression = tokens.Skip(3).GetEnumerator().GetTokensUntilSemicolon().ToArray();
                var value = GetValueFromExpression(expression);
                variables.Add(variableName, value);
                return value;
            }

            bool isAssignment = tokens.Count > 1 && tokens[1] == "=";
            if (isAssignment)
            {
                // Assignment to existing variable
                var variableName = tokens[0];
                var expression = tokens.Skip(2).GetEnumerator().GetTokensUntilSemicolon().ToArray();
                var value = GetValueFromExpression(expression);
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
                default:
                    throw new Exception();
            }
        }

        private CustomValue HandlePrint(CustomValue[] arguments)
        {
            Console.WriteLine(arguments[0].value);
            return CustomValue.Null;
        }

        private CustomValue GetValueFromExpression(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
            {
                var token = expressionTokens[0];
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
            else if (IsVariableName(expressionTokens[0]) && expressionTokens[1] == "(")
            {
                var functionName = expressionTokens[0];
                var allExpression = expressionTokens.Skip(2).GetEnumerator().GetTokensUntilParantheses().ToArray();
                var expressions = allExpression.SplitByCommas();
                var arguments = expressions.Select(expression => GetValueFromExpression(expression)).ToArray();
                var returnValue = CallFunction(functionName, arguments);
                return returnValue;
            }
            else if (expressionTokens[0] == "(" && expressionTokens[expressionTokens.Count - 1] == ")")
            {
                var newExpression = new StringRange(expressionTokens, 1, expressionTokens.Count - 1);
                return GetValueFromExpression(newExpression);
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
        public static IEnumerable<string> GetTokensUntilSemicolon(this IEnumerator<string> tokenSource)
        {
            while (tokenSource.MoveNext())
            {
                var token = tokenSource.Current;
                if (token != ";")
                {
                    yield return token;
                    continue;
                }
                break;
            }
        }

        public static IEnumerable<string> GetTokensUntilParantheses(this IEnumerator<string> tokenSource)
        {
            while (tokenSource.MoveNext())
            {
                var token = tokenSource.Current;
                if (token != ")")
                {
                    yield return token;
                    continue;
                }
                break;
            }
        }

        public static IEnumerable<List<string>> SplitByCommas(this IEnumerable<string> tokens)
        {
            var list = new List<string>();
            foreach (var token in tokens)
            {
                if (token == ",")
                {
                    yield return list;
                    list = new List<string>();
                }
                else
                {
                    list.Add(token);
                }
            }
            yield return list;
        }

        public static IEnumerable<StringRange> GetStatements(this string[] tokens)
        {
            int index = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == ";")
                {
                    yield return new StringRange(tokens, index, i);
                    i++;
                    index = i;
                }
            }
            yield return new StringRange(tokens, index, tokens.Length);
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

    class StringRange : IReadOnlyList<string>
    {
        public readonly IReadOnlyList<string> array;
        public readonly int start;
        public readonly int end;

        public StringRange(IReadOnlyList<string> array, int start, int end)
        {
            this.array = array;
            this.start = start;
            this.end = end;
        }

        public string this[int index] => array[start + index];

        public int Count => end - start;

        public IEnumerator<string> GetEnumerator()
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
    }
}
