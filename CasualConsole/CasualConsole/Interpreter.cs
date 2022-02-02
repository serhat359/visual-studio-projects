using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringRange = CasualConsole.CustomRange<string>;

namespace CasualConsole
{
    public class Interpreter
    {
        internal static readonly HashSet<char> onlyChars = new HashSet<char>() { '(', ')', ',', ';', '{', '}' };
        internal static readonly HashSet<char> multiChars = new HashSet<char>() { '+', '-', '*', '/', '=', '?', ':' };
        internal static readonly HashSet<string> assignmentSet = new HashSet<string>() { "=", "+=", "-=", "*=", "/=" };
        internal static readonly HashSet<string> commaSet = new HashSet<string>() { "," };
        internal static readonly HashSet<string> plusMinusSet = new HashSet<string>() { "+", "-" };
        internal static readonly HashSet<string> equalsSet = new HashSet<string>() { "==", "!=" };
        internal static readonly IReadOnlyList<string> ternaryList = new string[] { "?", ":" };
        internal static readonly HashSet<string> asteriskSlashSet = new HashSet<string>() { "*", "/" };
        internal static readonly HashSet<string> notSet = new HashSet<string>() { "!" };

        internal Dictionary<string, CustomValue> variables = new Dictionary<string, CustomValue>();

        public object InterpretCode(string code)
        {
            var tokens = GetTokens(code).ToList();

            return InterpretTokens(tokens).value;
        }

        public static void Test()
        {
            CustomValue.Test();
            ExpressionTreeMethods.Test();

            var testCases = new List<(string code, object value)>()
            {
                ("", null),
                ("    ", null),
                ("2", 2),
                ("-2", -2),
                ("(2)", 2),
                ("-(2)", -2),
                ("-(-2)", 2),
                ("+(-2)", -2),
                ("-(+2)", -2),
                ("((2))", 2),
                ("-(-(2))", 2),
                ("true", true),
                ("false", false),
                ("!true", false),
                ("!false", true),
                ("!(true)", false),
                ("!(false)", true),
                ("\"Hello world\"", "Hello world"),
                ("'Hello world'", "Hello world"),
                ("('Hello world')", "Hello world"),
                ("var aaa = 2", null),
                ("var aa = 2;", null),
                ("var a = 2; a", 2),
                ("var a2 = 2; var b2 = a2 = 5; b2", 5),
                ("var b = 3; b", 3),
                ("b = 5", 5),
                ("b = (7)", 7),
                ("var c = (7) + 2 - 1; c", 8),
                ("var _ = 6; _", 6),
                ("var _a = 7; _a", 7),
                ("var a_ = 8; a_", 8),
                ("var __ = 9; __", 9),
                ("var _a_ = 10; _a_", 10),
                ("var _aa_ = 11; _aa_", 11),
                ("var _2a_ = 12; _2a_", 12),
                ("var _b2 = 13; _b2", 13),
                ("var b345 = 14; b345", 14),
                ("var bbb = true; bbb", true),
                ("var bbbf = false; bbbf", false),
                ("var bool2 = false; !bool2", true),
                ("// this is a comment \n var comment = 5; comment", 5),
                ("/* this is another comment */ var   comment2   =   6; comment2", 6),
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
                ("2==2", true),
                ("2==3", false),
                ("returnValue(2) == 2", true),
                ("2+3 == 3+2", true),
                ("2 != null", true),
                ("2!=2", false),
                ("2!=3", true),
                ("returnValue(2) != 2", false),
                ("2+2 != 3+2", true),
                ("2 != null", true),
                ("(2 == 2)", true),
                ("!(2 == 2)", false),
                ("null == null", true),
                ("null != null", false),
                ("true == true == true", true),
                ("true == false == false", true),
                ("true == (false == false)", true),
                ("!true == !true", true),
                ("var f = 1; f += 2", 3),
                ("var g = 1; g -= 2", -1),
                ("var h = 3; h *= 4", 12),
                ("var i = 4.5; i /= 2", 2.25),
                ("var j = 'hello'; j += 2", "hello2"),
                ("var k = 'hello'; k += 2 + 3", "hello5"),
                ("var l = 'hello'; l += ' '; l += 'world'", "hello world"),
                ("true ? 2 : 5", 2),
                ("false ? 2 : 5", 5),
                ("true ? 2 : true ? 3 : 5", 2),
                ("false ? 2 : true ? 3 : 5", 3),
                ("false ? 2 : false ? 3 : 5", 5),
                ("true ? true ? 2 : 3 : 5", 2),
                ("returnValue(true) ? true ? 2 : 3 : 5", 2),
                ("true ? (true ? 2 : 3) : 5", 2),
                ("1 ? 1 ? 1 ? 1 : 1 : 1 : 1", 1),
                ("1 ? 1 : 1 ? 1 : 1 ? 1 : 1", 1),
                ("1 ? 2 : 5", 2),
                ("0 ? 2 : 5", 5),
                ("null ? 2 : 5", 5),
                ("'' ? 2 : 5", 5),
                ("'foo' ? 2 : 5", 2),
                ("var op11 = 2; var op12 = true ? 5 : op11 = 3; op11", 2), // Checking optimization
                ("var op21 = 2; var op22 = false ? 5 : op21 = 3; op21", 3), // Checking optimization
                ("{ var scope1 = null; }", null),
                ("var scope2 = 2; { scope2 = 3; } scope2", 3),
                ("var scopeif1 = 5; if(true){ scopeif1 = 8; } scopeif1", 8),
                ("var scopeif2 = 5; if(false){ scopeif2 = 8; } scopeif2", 5),
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
            var statementRanges = tokenSource.GetStatementRanges();
            foreach (var statementRange in statementRanges)
            {
                bool isLastStatement = statementRange.end == tokenSource.Count;
                var statement = StatementMethods.New(statementRange);
                var value = GetValueFromStatement(statement);
                if (isLastStatement)
                {
                    return value;
                }
            }
            return CustomValue.Null;
        }

        private CustomValue GetValueFromStatement(Statement statement)
        {
            return statement.Evaluate(this);
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

        private CustomValue TernaryExpression(List<(Operator operatorType, ExpressionTree tree)> expressions)
        {
            bool isValid = expressions.Count == 3
                && expressions[0].operatorType == Operator.None
                && expressions[1].operatorType == Operator.QuestionMark
                && expressions[2].operatorType == Operator.Colon;

            if (!isValid)
                throw new Exception();

            var conditionValue = expressions[0].tree.Evaluate(this);
            bool isTruthy = conditionValue.IsTruthy();

            if (isTruthy)
                return expressions[1].tree.Evaluate(this);
            else
                return expressions[2].tree.Evaluate(this);
        }

        private CustomValue CheckEqualsOrNot(List<(Operator operatorType, ExpressionTree tree)> trees)
        {
            if (trees[0].operatorType != Operator.None) throw new Exception();

            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.Evaluate(this)));

            var lastValue = values[0].value;
            for (int i = 1; i < values.Count; i++)
            {
                var value = values[i];
                bool result;
                if (value.operatorType == Operator.CheckEquals)
                    result = object.Equals(lastValue.value, value.value.value);
                else
                    result = !object.Equals(lastValue.value, value.value.value);
                lastValue = result ? CustomValue.True : CustomValue.False;
            }

            return lastValue;
        }

        internal CustomValue MultiplyOrDivide(IReadOnlyList<(Operator operatorType, ExpressionTree tree)> trees)
        {
            if (trees[0].operatorType != Operator.None) throw new Exception();

            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.Evaluate(this)));

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

        internal CustomValue AddOrSubtract(IReadOnlyList<(Operator operatorType, ExpressionTree tree)> trees)
        {
            bool hasMinus = false;
            bool hasString = false;

            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.Evaluate(this)));

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

        internal CustomValue GetValueFromExpression(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 0)
                throw new Exception();
            else if (expressionTokens.Count == 1)
            {
                var token = expressionTokens[0];
                return GetValueFromSingleToken(token);
            }

            var expressionTree = ExpressionTreeMethods.New(expressionTokens);
            return expressionTree.Evaluate(this);
        }

        internal CustomValue GetValueFromSingleToken(string token)
        {
            if (token == "true")
                return CustomValue.True;
            else if (token == "false")
                return CustomValue.False;
            else if (token == "null")
                return CustomValue.Null;
            else if (IsVariableName(token))
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

        internal CustomValue EvaluateTreeExpressions(List<(Operator operatorType, ExpressionTree tree)> expressions)
        {
            if (expressions.Count == 1)
            {
                var operation = expressions[0].operatorType;
                var subTree = expressions[0].tree;
                var subValue = subTree.Evaluate(this);
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
                else if (operation == Operator.Not)
                {
                    bool res = (bool)subValue.value;
                    return res ? CustomValue.False : CustomValue.True;
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
            if (operatorType == Operator.CheckEquals || operatorType == Operator.CheckNotEquals)
            {
                return CheckEqualsOrNot(expressions);
            }
            if (operatorType == Operator.QuestionMark)
            {
                return TernaryExpression(expressions);
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
                else if (multiChars.Contains(c))
                {
                    int start = i;
                    i++;
                    while (multiChars.Contains(content[i]))
                        i++;
                    string token = content.Substring(start, i - start);
                    yield return token;
                }
                else if (onlyChars.Contains(c))
                {
                    i++;
                    yield return c.ToString();
                }
                else if (c == '=')
                {
                    if (content[i + 1] == '=')
                    {
                        i += 2;
                        yield return "==";
                    }
                    else
                    {
                        i++;
                        yield return "=";
                    }
                }
                else if (c == '!')
                {
                    if (content[i + 1] == '=')
                    {
                        i += 2;
                        yield return "!=";
                    }
                    else
                    {
                        i++;
                        yield return "!";
                    }
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

        public static IEnumerable<(IReadOnlyList<string> list, string operatorToken)> SplitByArray(this IReadOnlyList<string> tokens, IReadOnlyList<string> separator)
        {
            var index = 0;
            var parenthesesCount = 0;
            string operatorToken = null;
            string firstSeparator = separator[0];
            string secondSeparator = separator[1];
            string nextSeparator = firstSeparator;
            var separatorCount = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token == "(") parenthesesCount++;
                else if (token == ")") parenthesesCount--;

                if (token == firstSeparator && nextSeparator == secondSeparator) separatorCount++;
                if (token == secondSeparator && nextSeparator == secondSeparator) separatorCount--;

                if (parenthesesCount == 0 && token == nextSeparator && (token == firstSeparator || (token == secondSeparator && separatorCount == -1)))
                {
                    yield return (new StringRange(tokens, index, i), operatorToken);
                    operatorToken = token;
                    index = i + 1;
                    if (nextSeparator == secondSeparator)
                        break;
                    nextSeparator = secondSeparator;
                }
            }
            yield return (new StringRange(tokens, index, tokens.Count), operatorToken);
        }

        public static IEnumerable<StringRange> GetStatementRanges(this IReadOnlyList<string> tokens)
        {
            int index = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == ";")
                {
                    yield return new StringRange(tokens, index, i);
                    index = i + 1;
                }
                else if (tokens[i] == "{")
                {
                    i++;
                    int braceCount = 1;
                    while (true)
                    {
                        if (tokens[i] == "{") braceCount++;
                        else if (tokens[i] == "}") braceCount--;

                        if (braceCount == 0)
                            break;

                        i++;
                    }
                    i++;
                    yield return new StringRange(tokens, index, i);
                    index = i;
                }
            }

            if (tokens.Count - index > 0)
                yield return new StringRange(tokens, index, tokens.Count);
            else
                yield break;
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
            if (result == null && expected == null)
                return true;

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
        public static readonly CustomValue True = new CustomValue(true, ValueType.Bool);
        public static readonly CustomValue False = new CustomValue(false, ValueType.Bool);

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

        internal bool IsTruthy()
        {
            switch (type)
            {
                case ValueType.Null:
                    return false;
                case ValueType.Number:
                    return ((double)value) != 0;
                case ValueType.String:
                    return !string.IsNullOrEmpty((string)value);
                case ValueType.Bool:
                    return (bool)value;
                default:
                    throw new Exception();
            }
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
        Bool,
    }

    enum Operator
    {
        None,
        Plus,
        Minus,
        Multiply,
        Divide,
        CheckEquals,
        CheckNotEquals,
        Not,
        QuestionMark,
        Colon,
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

    interface ExpressionTree
    {
        CustomValue Evaluate(Interpreter interpreter);
    }

    static class ExpressionTreeMethods
    {
        public static ExpressionTree NewAssignmentExpressionTree(IReadOnlyList<string> expressionTokens, bool hasVar)
        {
            return new ExpressionTreeAssignment(expressionTokens, hasVar);
        }

        public static ExpressionTree New(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            if (Interpreter.assignmentSet.Contains(expressionTokens[1]))
            {
                return new ExpressionTreeAssignment(expressionTokens, false);
            }

            var tree = new ExpressionTreeList();

            var split = expressionTokens.SplitByArray(Interpreter.ternaryList).ToList();
            if (split.Count == 1)
                return ExpressionTreeMethods.NewNoTernary(expressionTokens);

            foreach (var splitExpression in split)
            {
                Operator operatorType = Operator.None;
                if (splitExpression.operatorToken == "?")
                    operatorType = Operator.QuestionMark;
                else if (splitExpression.operatorToken == ":")
                    operatorType = Operator.Colon;

                var subTree = ExpressionTreeMethods.New(splitExpression.list);
                tree.expressions.Value.Add((operatorType, subTree));
            }
            return tree;
        }

        public static ExpressionTree NewNoTernary(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            var tree = new ExpressionTreeList();

            var split = expressionTokens.SplitBy(Interpreter.equalsSet);
            foreach (var splitExpression in split)
            {
                Operator operatorType = Operator.None;
                if (splitExpression.operatorToken == "==")
                    operatorType = Operator.CheckEquals;
                else if (splitExpression.operatorToken == "!=")
                    operatorType = Operator.CheckNotEquals;

                var subTree = ExpressionTreeMethods.NewNoEquals(splitExpression.list);
                tree.expressions.Value.Add((operatorType, subTree));
            }
            return tree;
        }

        public static ExpressionTree NewNoEquals(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            var tree = new ExpressionTreeList();

            var split = expressionTokens.SplitBy(Interpreter.plusMinusSet);
            foreach (var splitExpression in split)
            {
                if (splitExpression.list.Count == 0)
                    continue;

                Operator operatorType = Operator.None;
                if (splitExpression.operatorToken == "+")
                    operatorType = Operator.Plus;
                else if (splitExpression.operatorToken == "-")
                    operatorType = Operator.Minus;

                var subTree = ExpressionTreeMethods.NewNoPlus(splitExpression.list);
                tree.expressions.Value.Add((operatorType, subTree));
            }
            return tree;
        }

        public static ExpressionTree NewNoPlus(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            var tree = new ExpressionTreeList();

            var split = expressionTokens.SplitBy(Interpreter.asteriskSlashSet);
            foreach (var splitExpression in split)
            {
                Operator operatorType = Operator.None;
                if (splitExpression.operatorToken == "*")
                    operatorType = Operator.Multiply;
                else if (splitExpression.operatorToken == "/")
                    operatorType = Operator.Divide;

                var subTree = ExpressionTreeMethods.NewNoAsterisk(splitExpression.list);
                tree.expressions.Value.Add((operatorType, subTree));
            }
            return tree;
        }

        public static ExpressionTree NewNoAsterisk(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            var tree = new ExpressionTreeList();

            var split = expressionTokens.SplitBy(Interpreter.notSet);
            foreach (var splitExpression in split)
            {
                if (splitExpression.list.Count == 0)
                    continue;

                Operator operatorType = Operator.None;
                if (splitExpression.operatorToken == "!")
                    operatorType = Operator.Not;

                var subTree = ExpressionTreeMethods.NewNoNot(splitExpression.list);
                tree.expressions.Value.Add((operatorType, subTree));
            }
            return tree;
        }

        // Can still contain parentheses
        public static ExpressionTree NewNoNot(IReadOnlyList<string> expressionTokens)
        {
            if (expressionTokens.Count == 1)
                return New(expressionTokens[0]);

            if (expressionTokens[0] == "(")
                return ExpressionTreeMethods.New(CustomRange.From(expressionTokens).StripSides());
            else
                return ExpressionTreeMethods.NewStripped(expressionTokens);
        }

        // Should not contain parentheses
        private static ExpressionTree NewStripped(IReadOnlyList<string> expressionTokens)
        {
            var tree = new ExpressionTreeTokens(expressionTokens);
            return tree;
        }

        private static ExpressionTree New(string singleToken)
        {
            var tree = new ExpressionTreeToken(singleToken);
            return tree;
        }

        static ExpressionTree New(CustomValue value)
        {
            var tree = new ExpressionTreeCustomValue(value);
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
                var tree = ExpressionTreeMethods.New(testCase.tokens);
                var result = tree.Evaluate(interpreter);
                if (!InterpreterExtensions.Equals(result.value, testCase.value))
                {
                    throw new Exception();
                }
            }
        }
    }

    class ExpressionTreeAssignment : ExpressionTree
    {
        public string variableName;
        public string assignmentOperator;
        public ExpressionTree rValue;
        public bool hasVar;

        public ExpressionTreeAssignment(IReadOnlyList<string> tokens, bool hasVar)
        {
            if (!Interpreter.assignmentSet.Contains(tokens[1]))
                throw new Exception();

            variableName = tokens[0];
            assignmentOperator = tokens[1];
            rValue = ExpressionTreeMethods.New(new StringRange(tokens, 2, tokens.Count));
            this.hasVar = hasVar;
        }

        public CustomValue Evaluate(Interpreter interpreter)
        {
            var variables = interpreter.variables;
            if (hasVar)
            {
                if (assignmentOperator != "=")
                    throw new Exception();

                var value = rValue.Evaluate(interpreter);
                interpreter.variables.Add(variableName, value);
                return value;
            }
            else
            {
                var value = rValue.Evaluate(interpreter);
                var assignmentToken = assignmentOperator;

                switch (assignmentToken)
                {
                    case "=":
                        variables[variableName] = value;
                        break;
                    case "+=":
                        {
                            var existingValue = variables[variableName];
                            value = interpreter.AddOrSubtract(new (Operator, ExpressionTree)[] { (Operator.None, new ExpressionTreeCustomValue(existingValue)), (Operator.Plus, new ExpressionTreeCustomValue(value)) });
                            variables[variableName] = value;
                        }
                        break;
                    case "-=":
                        {
                            var existingValue = variables[variableName];
                            value = interpreter.AddOrSubtract(new (Operator, ExpressionTree)[] { (Operator.None, new ExpressionTreeCustomValue(existingValue)), (Operator.Minus, new ExpressionTreeCustomValue(value)) });
                            variables[variableName] = value;
                        }
                        break;
                    case "*=":
                        {
                            var existingValue = variables[variableName];
                            value = interpreter.MultiplyOrDivide(new (Operator, ExpressionTree)[] { (Operator.None, new ExpressionTreeCustomValue(existingValue)), (Operator.Multiply, new ExpressionTreeCustomValue(value)) });
                            variables[variableName] = value;
                        }
                        break;
                    case "/=":
                        {
                            var existingValue = variables[variableName];
                            value = interpreter.MultiplyOrDivide(new (Operator, ExpressionTree)[] { (Operator.None, new ExpressionTreeCustomValue(existingValue)), (Operator.Divide, new ExpressionTreeCustomValue(value)) });
                            variables[variableName] = value;
                        }
                        break;
                    default:
                        throw new Exception();
                }
                return value;
            }
        }
    }

    class ExpressionTreeList : ExpressionTree
    {
        public Lazy<List<(Operator operatorType, ExpressionTree tree)>> expressions = new Lazy<List<(Operator, ExpressionTree)>>();

        public CustomValue Evaluate(Interpreter interpreter)
        {
            return interpreter.EvaluateTreeExpressions(expressions.Value);
        }
    }
    class ExpressionTreeTokens : ExpressionTree
    {
        IReadOnlyList<string> tokens;

        public ExpressionTreeTokens(IReadOnlyList<string> tokens)
        {
            this.tokens = tokens;
        }

        public CustomValue Evaluate(Interpreter interpreter)
        {
            return interpreter.GetValueFromMultipleTokens(tokens);
        }
    }
    class ExpressionTreeToken : ExpressionTree
    {
        string token;

        public ExpressionTreeToken(string token)
        {
            this.token = token;
        }

        public CustomValue Evaluate(Interpreter interpreter)
        {
            return interpreter.GetValueFromSingleToken(token);
        }
    }
    class ExpressionTreeCustomValue : ExpressionTree
    {
        CustomValue customValue;

        public ExpressionTreeCustomValue(CustomValue customValue)
        {
            this.customValue = customValue;
        }

        public CustomValue Evaluate(Interpreter interpreter)
        {
            return customValue;
        }
    }

    interface Statement
    {
        CustomValue Evaluate(Interpreter interpreter);
    }
    static class StatementMethods
    {
        public static Statement New(IReadOnlyList<string> tokens)
        {
            if (tokens[0] == "{")
            {
                if (tokens[tokens.Count - 1] != "}") throw new Exception();
                return new BlockStatement(tokens);
            }
            else if (tokens[0] == "if")
            {
                return new IfStatement(tokens);
            }
            else
                return new LineStatement(tokens);
        }
        static Statement NewLineStatement(IReadOnlyList<string> tokens)
        {
            return new LineStatement(tokens);
        }
        static Statement NewBlockStatement(IReadOnlyList<string> tokens)
        {
            return new BlockStatement(tokens);
        }
    }
    class LineStatement : Statement
    {
        IReadOnlyList<string> tokens;
        bool hasSemiColon;

        public LineStatement(IReadOnlyList<string> tokens)
        {
            if (tokens[tokens.Count - 1] == ";")
            {
                hasSemiColon = true;
                tokens = new StringRange(tokens, 0, tokens.Count - 1);
            }

            this.tokens = tokens;
        }

        public CustomValue Evaluate(Interpreter interpreter)
        {
            if (tokens.Count == 0) return CustomValue.Null;

            var firstToken = tokens[0];
            if (firstToken == "var")
            {
                // Assignment to new variable
                var assignmentTree = ExpressionTreeMethods.NewAssignmentExpressionTree(new StringRange(tokens, 1, tokens.Count), hasVar: true);
                var value = assignmentTree.Evaluate(interpreter);
                return CustomValue.Null;
            }

            CustomValue expressionValue = interpreter.GetValueFromExpression(tokens);
            if (hasSemiColon)
                return CustomValue.Null;
            return expressionValue;
        }
    }
    class BlockStatement : Statement
    {
        List<Statement> statements;

        public BlockStatement(IReadOnlyList<string> tokens)
        {
            if (tokens[0] != "{")
                throw new Exception();
            tokens = new StringRange(tokens, 1, tokens.Count - 1);
            var statementRanges = tokens.GetStatementRanges();
            statements = statementRanges.Select(range => StatementMethods.New(range)).ToList();
        }

        public CustomValue Evaluate(Interpreter interpreter)
        {
            foreach (var statement in statements)
            {
                statement.Evaluate(interpreter);
            }
            return CustomValue.Null;
        }
    }
    class IfStatement : Statement
    {
        ExpressionTree conditionExpression;
        Statement statementOfIf;
        Lazy<List<(ExpressionTree condition, Statement statement)>> elseIfStatements = new Lazy<List<(ExpressionTree, Statement)>>(() => new List<(ExpressionTree, Statement)>());
        Statement elseStatement;

        public IfStatement(IReadOnlyList<string> tokens)
        {
            if (tokens[1] != "(")
                throw new Exception();
            var conditionStartIndex = 2;
            var endOfParentheses = tokens.IndexOfParenthesesEnd(conditionStartIndex);
            if (endOfParentheses < 0)
                throw new Exception();
            var conditionTokens = new StringRange(tokens, conditionStartIndex, endOfParentheses);
            conditionExpression = ExpressionTreeMethods.New(conditionTokens);

            var statementTokens = new StringRange(tokens, endOfParentheses + 1, tokens.Count);
            statementOfIf = StatementMethods.New(statementTokens);
        }

        public CustomValue Evaluate(Interpreter interpreter)
        {
            var returnValue = CustomValue.Null;

            var conditionValue = conditionExpression.Evaluate(interpreter);
            if (conditionValue.IsTruthy())
            {
                statementOfIf.Evaluate(interpreter);
                return returnValue;
            }

            foreach (var elseIfStatement in elseIfStatements.Value)
            {
                var elseIfCondition = elseIfStatement.condition.Evaluate(interpreter);
                if (elseIfCondition.IsTruthy())
                {
                    elseIfStatement.statement.Evaluate(interpreter);
                    return returnValue;
                }
            }

            if (elseStatement != null)
            {
                elseStatement.Evaluate(interpreter);
            }

            return returnValue;
        }
    }
}
