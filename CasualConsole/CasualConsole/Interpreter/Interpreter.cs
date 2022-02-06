using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringRange = CasualConsole.Interpreter.CustomRange<string>;

namespace CasualConsole.Interpreter
{
    public class Interpreter
    {
        internal static readonly HashSet<char> onlyChars = new HashSet<char>() { '(', ')', ',', ';', '{', '}' };
        internal static readonly HashSet<char> multiChars = new HashSet<char>() { '+', '-', '*', '/', '%', '=', '?', ':', '<', '>' };
        internal static readonly HashSet<string> assignmentSet = new HashSet<string>() { "=", "+=", "-=", "*=", "/=", "%=" };
        internal static readonly HashSet<string> commaSet = new HashSet<string>() { "," };
        internal static readonly HashSet<string> plusMinusSet = new HashSet<string>() { "+", "-" };
        internal static readonly HashSet<string> comparisonSet = new HashSet<string>() { "==", "!=", "<", ">", "<=", ">=" };
        internal static readonly IReadOnlyList<string> ternaryList = new string[] { "?", ":" };
        internal static readonly HashSet<string> asteriskSlashSet = new HashSet<string>() { "*", "/", "%" };
        internal static readonly HashSet<string> notSet = new HashSet<string>() { "!" };

        internal static Expression trueExpression;
        internal static Expression falseExpression;
        internal static Expression nullExpression;

        private Dictionary<string, CustomValue> defaultvariables = new Dictionary<string, CustomValue>();
        internal VariableScope variableScope;

        public Interpreter()
        {
            defaultvariables = new Dictionary<string, CustomValue>();
            variableScope = VariableScope.NewFromExisting(defaultvariables);

            trueExpression = new CustomValueExpression(CustomValue.True);
            falseExpression = new CustomValueExpression(CustomValue.False);
            nullExpression = new CustomValueExpression(CustomValue.Null);
        }

        public object InterpretCode(string code)
        {
            var tokens = GetTokens(code).ToList();

            return InterpretTokens(tokens).value;
        }

        public static void GetStatementRangesTest()
        {
            var testCases = new List<(string code, string[] statements)>
            {
                ("2", new[]{ "2" }),
                ("a = 2", new[]{ "a = 2" }),
                ("var a = 2; a", new[]{ "var a = 2;", "a" }),
                ("var a = 2;", new[]{ "var a = 2;" }),
                ("var a = 2; var b = 2;", new[]{ "var a = 2;", "var b = 2;" }),
                ("{}", new[]{ "{}" }),
                ("{}{}", new[]{ "{}","{}" }),
                ("{} var a = 2; {}", new[]{ "{}", "var a = 2;", "{}" }),
                ("{ var a = 2; }", new[]{ "{ var a = 2; }" }),
                ("function () {}", new[]{ "function () {}" }),
                ("function(){} + 1", new[]{ "function(){} + 1" }),
                ("function(){} + function(){} + function(){}", new[]{ "function(){} + function(){} + function(){}" }),
                ("function(){};function(){};", new[]{ "function(){};", "function(){};" }),
                ("var func2 = true ? function(){ return '1'; } : function(){ return '2'; }; func2()", new[]{ "var func2 = true ? function(){ return '1'; } : function(){ return '2'; };", "func2()" }),
                ("function customReturnConstant(){ return -8; } customReturnConstant()",new []{ "function customReturnConstant(){ return -8; }", "customReturnConstant()" }),
            };

            foreach (var testCase in testCases)
            {
                var tokens = GetTokens(testCase.code).ToList();
                var statements = tokens.GetStatementRanges().ToList();
                if (statements.Count != testCase.statements.Length)
                    throw new Exception();
                for (int i = 0; i < statements.Count; i++)
                {
                    var statement = string.Join(" ", statements[i]);
                    var testCaseStatement = testCase.statements[i];

                    if (statement.Replace(" ", "") != testCaseStatement.Replace(" ", ""))
                        throw new Exception();
                }
            }

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All GetStatementRangesTest tests have passed!");
            Console.ForegroundColor = oldColor;
        }

        public static void Test()
        {
            CustomValue.Test();

            GetStatementRangesTest();

            var testCases = new List<(string code, object value)>()
            {
                ("var returnValue = function(x){ return x; };", null),
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
                ("5 % 1", 0),
                ("5 % 2", 1),
                ("var mod1 = 5; mod1 %= 2", 1),
                ("2==2", true),
                ("2==3", false),
                ("2 + 2 == 2 + 2", true),
                ("2 * 2 == 2 * 2", true),
                ("1+1*1+1*1+1*1+1*1+1*1+1*1+1*1", 8),
                ("2*2+1 == 2*2+1", true),
                ("1*2+3*4 == 1*2+3*4", true),
                ("1*2+3*4 == 5*6+7*8", false),
                ("1*2+3*4 == 5+6*7+8", false),
                ("5+6*7+8 == 5+6*7+8", true),
                ("1 * 2 + 3 * returnValue(4) == 14", true),
                ("1 * returnValue(2) + returnValue(3) * returnValue(4) == 14", true),
                ("returnValue(1) * returnValue(2) + returnValue(3) * returnValue(4) == 14", true),
                ("returnValue(2) == 2", true),
                ("2 == returnValue(2)", true),
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
                ("2 < 3", true),
                ("2 <= 3", true),
                ("2 > 3", false),
                ("2 >= 3", false),
                ("2 >= 2", true),
                ("2 <= 2", true),
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
                ("var scopeif3 = 5; if(true) scopeif3 = 8; scopeif3", 8),
                ("var scopeif4 = 5; if(true) if(true) scopeif4 = 8; scopeif4", 8),
                ("var scopeif5 = 5; if(true) if(true) { scopeif5 += 1; scopeif5 += 1; } scopeif5", 7),
                ("var scopeifelse1 = 6; if(false){ scopeifelse1 = 7; } else { scopeifelse1 = 8; }  scopeifelse1", 8),
                ("var elseif1 = 1; if(false) elseif1 = 10; else if (false) elseif1 = 11; else if (true) elseif1 = 12; else elseif1 = 13; elseif1", 12),
                ("var elseif2 = 1; if(true) if(true) if(false) elseif2 = 2; else if (false) elseif2 = 3; else if (true) elseif2 = 4; else elseif2 = 5; elseif2", 4),
                ("var elseif3 = 1; if(true) if(false) elseif3 = 5; else elseif3 = 7; elseif3", 7),
                ("var elseif4 = 1; if(true) if(true) if(false) elseif4 = 2; else if (false) elseif4 = 3; else if (true) elseif4 = 4; else elseif4 = 5; elseif4",4),
                ("var elseif5 = 1; if(true) if(false) { if(false) elseif5 = 2; } else if (false) elseif5 = 3; else if (true) elseif5 = 4; else elseif5 = 5; elseif5", 4),
                ("var while1 = 1; while(while1 < 5) while1 += 1; while1", 5),
                ("var customReturnConstantVar = function(){ return -10; }; customReturnConstantVar()", -10),
                ("var func1 = function(){}; func1()", null),
                ("var func2 = true ? function(){ return '1'; } : function(){ return '2'; }; func2()", "1"),
                ("var func3 = function(){ return 8; }; func3()", 8),
                ("var func4 = function(){ if(true) { return 9; } }; func4()", 9),
                ("var func5 = function(){ if(false) { return 9; } }; func5()", null),
                ("var abs = function(x){ if(x < 0) return -x; else return x; };", null),
                ("abs(1)", 1),
                ("abs(0)", 0),
                ("abs(-1)", 1),
                ("abs(-15)", 15),
                ("(abs)(-16)", 16),
                ("var gcd = function(a,b) { a = abs(a); b = abs(b); if (b > a) {var temp = a; a = b; b = temp;} while (true) { if (b == 0) return a; a %= b; if (a == 0) return b; b %= a; } }; gcd(24,60)", 12),
                ("returnValue(true)", true),
                ("(returnValue)(true)", true),
                ("(returnValue)(1) + (returnValue)(2)", 3),
                ("(function(){})()", null),
                ("(function(){ return 1; })()", 1),
                ("(function(x){ return x; })(2)", 2),
                ("(function(){ return abs; })()(-3)", 3),
                ("returnValue(abs)(-4)", 4),
                ("(function(){ return function(){ return 1; }; })()()", 1),
                ("function customReturnConstant(){ return -8; } customReturnConstant()", -8),
                ("function returnAddAll(x,y,z){ return x + y + z; } returnAddAll(1,2,3)", 6),
                ("var k1 = 2; function f_k1(){ k1 = 10; } f_k1(); k1", 10),
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
            var statementRangesEnumerator = statementRanges.GetEnumerator();

            while (statementRangesEnumerator.MoveNext())
            {
                var statementRange = statementRangesEnumerator.Current;
                var statement = StatementMethods.New(statementRange, false);

                if (statement.IsElseIfStatement || statement.IsElseStatement)
                    throw new Exception();

                if (statement.IsIfStatement)
                {
                    StringRange nonIfElseStatementRange = null;
                    Statement nonIfElseStatement = null;

                    var ifStatement = (IfStatement)statement;
                    while (ifStatement.statementOfIf.IsIfStatement)
                    {
                        ifStatement = (IfStatement)ifStatement.statementOfIf;
                    }

                    while (statementRangesEnumerator.MoveNext())
                    {
                        var statementRangeAfterIf = statementRangesEnumerator.Current;
                        var statementAfterIf = StatementMethods.New(statementRangeAfterIf, false);
                        if (statementAfterIf.IsElseIfStatement)
                        {
                            ifStatement.AddElseIf(statementAfterIf);
                        }
                        else if (statementAfterIf.IsElseStatement)
                        {
                            ifStatement.SetElse(statementAfterIf);
                            break;
                        }
                        else
                        {
                            nonIfElseStatement = statementAfterIf;
                            nonIfElseStatementRange = statementRangeAfterIf;
                            break;
                        }
                    }

                    var value = GetValueFromStatement(ifStatement, variableScope);
                    if (statementRange.end == tokenSource.Count)
                    {
                        return value.Item1;
                    }

                    if (nonIfElseStatement != null)
                    {
                        value = GetValueFromStatement(nonIfElseStatement, variableScope);
                        if (nonIfElseStatementRange.end == tokenSource.Count)
                        {
                            return value.Item1;
                        }
                    }
                }
                else
                {
                    var value = GetValueFromStatement(statement, variableScope);
                    if (statementRange.end == tokenSource.Count)
                    {
                        return value.Item1;
                    }
                }
            }

            return CustomValue.Null;
        }

        private (CustomValue, bool) GetValueFromStatement(Statement statement, VariableScope variableScope)
        {
            return statement.EvaluateStatement(this, variableScope);
        }

        private CustomValue CallFunction(string functionName, CustomValue[] arguments, VariableScope variableScope)
        {
            if (variableScope.TryGetVariable(functionName, out var f))
            {
                return CallFunction(f, arguments, variableScope);
            }

            switch (functionName)
            {
                case "print":
                    return HandlePrint(arguments);
                default:
                    throw new Exception();
            }
        }

        internal CustomValue CallFunction(CustomValue f, CustomValue[] arguments, VariableScope variableScope)
        {
            if (f.type == ValueType.String)
                return CallFunction((string)f.value, arguments, variableScope);
            if (f.type != ValueType.Function)
                throw new Exception();

            var function = (CustomFunction)f.value;
            var functionParameterArguments = new Dictionary<string, CustomValue>();
            for (int i = 0; i < function.parameters.Count; i++)
            {
                var argName = function.parameters[i];
                var value = i < arguments.Length ? arguments[i] : CustomValue.Null;
                functionParameterArguments[argName] = value;
            }
            var newScope = VariableScope.NewWithInner(variableScope, functionParameterArguments);
            var (result, isReturn) = function.body.EvaluateStatement(this, newScope);
            return result;
        }

        private CustomValue HandlePrint(CustomValue[] arguments)
        {
            Console.WriteLine(arguments[0].value);
            return CustomValue.Null;
        }

        private bool Compare(CustomValue first, CustomValue second, Operator operatorType)
        {
            if (first.type != second.type)
                throw new Exception();

            Func<CustomValue, CustomValue, int> comparer;
            switch (first.type)
            {
                case ValueType.Number:
                    comparer = (f1, f2) => ((double)f1.value).CompareTo((double)f2.value);
                    break;
                case ValueType.String:
                    comparer = (f1, f2) => ((string)f1.value).CompareTo((string)f2.value);
                    break;
                default:
                    throw new Exception();
            }

            switch (operatorType)
            {
                case Operator.GreaterThan:
                    return comparer(first, second) > 0;
                case Operator.LessThan:
                    return comparer(first, second) < 0;
                case Operator.GreaterThanOrEqual:
                    return comparer(first, second) >= 0;
                case Operator.LessThanOrEqual:
                    return comparer(first, second) <= 0;
                default:
                    throw new Exception();
            }
        }

        internal CustomValue CompareTo(Expression firstTree, IReadOnlyList<(Operator operatorType, Expression tree)> trees, VariableScope variableScope)
        {
            var firstValue = firstTree.EvaluateExpression(this, variableScope);
            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.EvaluateExpression(this, variableScope)));

            var lastValue = firstValue;
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                bool result;
                if (value.operatorType == Operator.CheckEquals)
                    result = object.Equals(lastValue.value, value.value.value);
                else if (value.operatorType == Operator.CheckNotEquals)
                    result = !object.Equals(lastValue.value, value.value.value);
                else if (value.operatorType == Operator.LessThan
                    || value.operatorType == Operator.GreaterThan
                    || value.operatorType == Operator.LessThanOrEqual
                    || value.operatorType == Operator.GreaterThanOrEqual)
                    result = Compare(lastValue, value.value, value.operatorType);
                else
                    throw new Exception();

                lastValue = result ? CustomValue.True : CustomValue.False;
            }

            return lastValue;
        }

        internal CustomValue MultiplyOrDivide(Expression firstTree, IReadOnlyList<(Operator operatorType, Expression tree)> trees, VariableScope variableScope)
        {
            var firstValue = firstTree.EvaluateExpression(this, variableScope);
            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.EvaluateExpression(this, variableScope)));

            return MultiplyOrDivide(firstValue, values);
        }

        internal CustomValue MultiplyOrDivide(CustomValue firstValue, Operator operatorType, CustomValue value)
        {
            return MultiplyOrDivide(firstValue, new[] { (operatorType, value) });
        }

        internal CustomValue MultiplyOrDivide(CustomValue firstValue, IReadOnlyList<(Operator operatorType, CustomValue value)> values)
        {
            double total = (double)firstValue.value;
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (i == 0 && value.operatorType == Operator.None)
                    total = (double)value.value.value;
                else if (value.operatorType == Operator.Multiply)
                    total *= (double)value.value.value;
                else if (value.operatorType == Operator.Divide)
                    total /= (double)value.value.value;
                else if (value.operatorType == Operator.Modulus)
                    total %= (int)(double)value.value.value;
                else
                    throw new Exception();
            }

            return CustomValue.FromNumber(total);
        }

        internal CustomValue AddOrSubtract(Expression firstTree, IReadOnlyList<(Operator operatorType, Expression tree)> trees, VariableScope variableScope)
        {
            var firstValue = firstTree.EvaluateExpression(this, variableScope);
            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.EvaluateExpression(this, variableScope)));

            return AddOrSubtract(firstValue, values);
        }

        internal CustomValue AddOrSubtract(CustomValue firstValue, Operator operatorType, CustomValue value)
        {
            return AddOrSubtract(firstValue, new[] { (operatorType, value) });
        }

        internal CustomValue AddOrSubtract(CustomValue firstValue, IReadOnlyList<(Operator operatorType, CustomValue value)> values)
        {
            bool hasMinus = false;
            bool hasString = false;

            if (firstValue.type == ValueType.String)
                hasString = true;

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
                double total = (double)firstValue.value;
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
                sb.Append(firstValue.value.ToString());
                foreach (var value in values)
                {
                    sb.Append(value.value.value.ToString());
                }
                return CustomValue.FromParsedString(sb.ToString());
            }
        }

        internal CustomValue GetValueFromExpression(IReadOnlyList<string> expressionTokens, VariableScope variableScope)
        {
            if (expressionTokens.Count == 0)
                throw new Exception();

            var expressionTree = ExpressionMethods.New(expressionTokens);
            return expressionTree.EvaluateExpression(this, variableScope);
        }

        internal static bool IsVariableName(string token)
        {
            char firstChar = token[0];
            return firstChar == '_' || char.IsLetter(firstChar);
        }

        internal static bool IsNumber(string token)
        {
            char firstChar = token[0];
            return char.IsDigit(firstChar);
        }

        internal static bool IsStaticString(string token)
        {
            char firstChar = token[0];
            return firstChar == '"' || firstChar == '\'';
        }

        internal static Operator ParseOperator(string token)
        {
            switch (token)
            {
                case "+": return Operator.Plus;
                case "-": return Operator.Minus;
                case "*": return Operator.Multiply;
                case "/": return Operator.Divide;
                case "%": return Operator.Modulus;
                case "==": return Operator.CheckEquals;
                case "!=": return Operator.CheckNotEquals;
                case "<": return Operator.LessThan;
                case "<=": return Operator.LessThanOrEqual;
                case ">": return Operator.GreaterThan;
                case ">=": return Operator.GreaterThanOrEqual;
                default: throw new Exception();
            }
        }

        private static IEnumerable<string> GetTokens(string content)
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
            if (tokens.Count == 0)
                yield break;

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
            while (index < tokens.Count)
            {
                var newIndex = GetStatementEndIndex(tokens, index);
                if (newIndex <= index)
                    throw new Exception();
                yield return new StringRange(tokens, index, newIndex);
                index = newIndex;
            }
            yield break;
        }

        public static int GetStatementEndIndex(IReadOnlyList<string> tokens, int startingIndex)
        {
            bool hasFunction = false;
            for (int i = startingIndex; i < tokens.Count; i++)
            {
                if (tokens[i] == ";")
                {
                    i++;
                    return i;
                }
                else if (tokens[i] == "function")
                {
                    if (tokens[i + 1] == "(")
                        hasFunction = true;
                }
                else if (tokens[i] == "{")
                {
                    int braceCount = 1;
                    i++;
                    while (true)
                    {
                        if (tokens[i] == "{") braceCount++;
                        else if (tokens[i] == "}") braceCount--;

                        if (braceCount == 0)
                        {
                            if (!hasFunction)
                            {
                                return i + 1;
                            }
                            else
                            {
                                break;
                            }
                        }

                        i++;
                    }
                }
            }
            return tokens.Count;
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

        public static int IndexOfBracesEnd(this IReadOnlyList<string> source, int startIndex)
        {
            int count = 0;
            for (int i = startIndex; i < source.Count; i++)
            {
                string currentElement = source[i];
                if (currentElement == "}")
                {
                    if (count == 0)
                        return i;

                    count--;
                    if (count < 0)
                        throw new Exception();
                }
                else if (currentElement == "{")
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

    class CustomFunction
    {
        internal IReadOnlyList<string> parameters;
        internal Statement body;

        public CustomFunction(IReadOnlyList<string> parameters, Statement body)
        {
            this.parameters = parameters;
            this.body = body;
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

        internal static CustomValue FromFunction(CustomFunction func)
        {
            return new CustomValue(func, ValueType.Function);
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
        Function,
    }

    enum Operator
    {
        None,
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulus,
        CheckEquals,
        CheckNotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Not,
        QuestionMark,
        Colon,
    }

    enum Precedence : int
    {
        Comparison = 1,
        AddSubtract = 2,
        MultiplyDivide = 3,
        FunctionCall = 9999,
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
    }

    class VariableScope
    {
        private Dictionary<string, CustomValue> variables;
        private VariableScope innerScope;

        private VariableScope(Dictionary<string, CustomValue> variables, VariableScope innerScope)
        {
            this.variables = variables;
            this.innerScope = innerScope;
        }

        public bool TryGetVariable(string variableName, out CustomValue value)
        {
            if (variables.TryGetValue(variableName, out value))
            {
                return true;
            }
            if (innerScope != null && innerScope.TryGetVariable(variableName, out value))
            {
                return true;
            }
            return false;
        }

        public CustomValue GetVariable(string variableName)
        {
            if (TryGetVariable(variableName, out var value))
            {
                return value;
            }
            else
                throw new Exception($"variable not defined: {variableName}");
        }

        public void SetVariable(string variableName, CustomValue value)
        {
            if (variables.ContainsKey(variableName))
            {
                variables[variableName] = value;
            }
            else if (innerScope != null)
            {
                innerScope.SetVariable(variableName, value);
            }
            else
                throw new Exception();
        }

        public void AddVariable(string variableName, CustomValue value)
        {
            variables.Add(variableName, value);
        }

        public static VariableScope NewFromExisting(Dictionary<string, CustomValue> variables)
        {
            return new VariableScope(variables, null);
        }
        public static VariableScope NewWithInner(VariableScope innerScope)
        {
            var newVariables = new Dictionary<string, CustomValue>();
            return NewWithInner(innerScope, newVariables);
        }
        public static VariableScope NewWithInner(VariableScope innerScope, Dictionary<string, CustomValue> values)
        {
            return new VariableScope(values, innerScope);
        }
    }

    interface Expression
    {
        CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope);
    }
    static class ExpressionMethods
    {
        public static Expression New(IReadOnlyList<string> tokens)
        {
            Expression previousExpression = null;
            var index = 0;

            void addToLastNode(Precedence precedence, Func<Expression, Precedence?, Expression> handler)
            {
                if (previousExpression is TreeExpression treeExpression && precedence >= treeExpression.precedence)
                {
                    TreeExpression lowestTreeExpression = treeExpression;

                    while (lowestTreeExpression.nextValues[lowestTreeExpression.nextValues.Count - 1].Item2 is TreeExpression subTree
                        && precedence > subTree.precedence)
                    {
                        lowestTreeExpression = subTree;
                    }

                    var treeElementIndex = lowestTreeExpression.nextValues.Count - 1;
                    var (treeLastElementOperator, treeLastElementExpression) = lowestTreeExpression.nextValues[treeElementIndex];

                    if (precedence == treeExpression.precedence)
                    {
                        var newExpression = handler(lowestTreeExpression, lowestTreeExpression.precedence);
                    }
                    else
                    {
                        var newExpression = handler(treeLastElementExpression, lowestTreeExpression.precedence);
                        lowestTreeExpression.nextValues[treeElementIndex] = (treeLastElementOperator, newExpression);
                    }
                }
                else
                {
                    var newExpression = handler(previousExpression, null);
                    previousExpression = newExpression;
                }
            };

            while (true)
            {
                if (index == tokens.Count)
                    return previousExpression;

                if (previousExpression == null)
                {
                    var (expression, newIndex) = ReadExpression(tokens, index);
                    if (newIndex <= index)
                        throw new Exception();

                    if (newIndex == tokens.Count)
                        return expression;

                    index = newIndex;
                    previousExpression = expression;
                }
                else
                {
                    var newToken = tokens[index];

                    if (newToken == ";")
                        return previousExpression;

                    if (Interpreter.assignmentSet.Contains(newToken))
                    {
                        var restTokens = new StringRange(tokens, index + 1, tokens.Count);
                        return new AssignmentExpression(previousExpression, newToken, restTokens, false);
                    }

                    if (Interpreter.plusMinusSet.Contains(newToken)
                        || Interpreter.asteriskSlashSet.Contains(newToken)
                        || Interpreter.comparisonSet.Contains(newToken))
                    {
                        var newPrecedence = TreeExpression.GetPrecedence(newToken);

                        Expression nextExpression;
                        (nextExpression, index) = ReadExpression(tokens, index + 1);

                        addToLastNode(newPrecedence, (expression, precedence) =>
                        {
                            if (precedence == null || !(precedence == newPrecedence))
                            {
                                var newTree = new TreeExpression(newPrecedence, expression);
                                newTree.AddExpression(newToken, nextExpression);
                                return newTree;
                            }
                            else
                            {
                                ((TreeExpression)expression).AddExpression(newToken, nextExpression);
                                return null;
                            }
                        });

                        continue;
                    }

                    if (newToken == "?")
                    {
                        var questionMarkIndex = index;
                        var count = 1;
                        int colonIndex = index + 1;
                        for (; colonIndex < tokens.Count; colonIndex++)
                        {
                            var ternaryToken = tokens[colonIndex];
                            if (ternaryToken == "?")
                                count++;
                            if (ternaryToken == ":")
                                count--;

                            if (count == 0)
                                break;
                        }

                        var questionMarkExpressionTokens = new StringRange(tokens, questionMarkIndex + 1, colonIndex);
                        var questionMarkExpression = ExpressionMethods.New(questionMarkExpressionTokens);
                        var colonExpressionTokens = new StringRange(tokens, colonIndex + 1, tokens.Count);
                        var colonExpression = ExpressionMethods.New(colonExpressionTokens);

                        var ternaryExpression = new TernaryExpression(previousExpression, questionMarkExpression, colonExpression);
                        return ternaryExpression;
                    }

                    if (newToken == "(")
                    {
                        // Function call
                        var end = tokens.IndexOfParenthesesEnd(index + 1);
                        var parameters = new StringRange(tokens, index + 1, end);

                        addToLastNode(Precedence.FunctionCall, (expression, p) =>
                        {
                            return new FunctionCallExpression(expression, parameters);
                        });

                        index = end + 1;
                        continue;
                    }

                    throw new Exception();
                }
            }

            throw new Exception();
        }

        public static (Expression, int) ReadExpression(IReadOnlyList<string> tokens, int index)
        {
            var token = tokens[index];
            if (token == "-" || token == "+")
            {
                var (expressionRest, lastIndex) = ReadExpression(tokens, index + 1);
                var newExpression = new SinglePlusMinusExpression(token, expressionRest);
                return (newExpression, lastIndex);
            }
            if (token == "!")
            {
                var (expressionRest, lastIndex) = ReadExpression(tokens, index + 1);
                var newExpression = new NotExpression(token, expressionRest);
                return (newExpression, lastIndex);
            }

            if (token == "(")
            {
                var newend = tokens.IndexOfParenthesesEnd(index + 1);
                if (newend < 0)
                    throw new Exception();

                var parenthesesTokens = new StringRange(tokens, index, newend + 1);
                var newExpression = new ParenthesesExpression(parenthesesTokens);
                return (newExpression, newend + 1);
            }

            if (token == "true")
            {
                return (Interpreter.trueExpression, index + 1);
            }
            if (token == "false")
            {
                return (Interpreter.falseExpression, index + 1);
            }
            if (token == "null")
            {
                return (Interpreter.nullExpression, index + 1);
            }
            if (token == "function")
            {
                if (tokens[index + 1] != "(")
                    throw new Exception();
                var parenthesesEnd = tokens.IndexOfParenthesesEnd(index + 2);
                if (parenthesesEnd < 0)
                    throw new Exception();
                if (tokens[parenthesesEnd + 1] != "{")
                    throw new Exception();
                var bracesEnd = tokens.IndexOfBracesEnd(parenthesesEnd + 2);
                if (bracesEnd < 0)
                    throw new Exception();

                var functionExpressionTokens = new StringRange(tokens, index, bracesEnd + 1);
                var functionExpression = new FunctionStatement(functionExpressionTokens);
                return (functionExpression, bracesEnd + 1);
            }
            if (Interpreter.IsNumber(token))
            {
                return (new SingleTokenNumberExpression(token), index + 1);
            }
            if (Interpreter.IsStaticString(token))
            {
                return (new SingleTokenStringExpression(token), index + 1);
            }
            if (Interpreter.IsVariableName(token))
            {
                return (new SingleTokenVariableExpression(token), index + 1);
            }

            throw new Exception();
        }
    }
    class TreeExpression : Expression
    {
        internal Precedence precedence;
        private Expression firstExpression;
        internal List<(Operator operatorToken, Expression)> nextValues;

        public TreeExpression(Precedence precedence, Expression firstExpression)
        {
            this.precedence = precedence;
            this.firstExpression = firstExpression;
            this.nextValues = new List<(Operator operatorToken, Expression)>();
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            switch (precedence)
            {
                case Precedence.Comparison:
                    return interpreter.CompareTo(firstExpression, nextValues, variableScope);
                case Precedence.AddSubtract:
                    return interpreter.AddOrSubtract(firstExpression, nextValues, variableScope);
                case Precedence.MultiplyDivide:
                    return interpreter.MultiplyOrDivide(firstExpression, nextValues, variableScope);
                default:
                    break;
            }

            throw new NotImplementedException();
        }

        public static Precedence GetPrecedence(string operatorToken)
        {
            switch (operatorToken)
            {
                case "+": return Precedence.AddSubtract;
                case "-": return Precedence.AddSubtract;
                case "*": return Precedence.MultiplyDivide;
                case "/": return Precedence.MultiplyDivide;
                case "%": return Precedence.MultiplyDivide;
                case "==": return Precedence.Comparison;
                case "!=": return Precedence.Comparison;
                case "<": return Precedence.Comparison;
                case "<=": return Precedence.Comparison;
                case ">": return Precedence.Comparison;
                case ">=": return Precedence.Comparison;
                default: throw new Exception();
            }
        }

        internal void AddExpression(string newToken, Expression nextExpression)
        {
            nextValues.Add((Interpreter.ParseOperator(newToken), nextExpression));
        }
    }
    class FunctionCallExpression : Expression
    {
        Expression lValue;
        IReadOnlyList<string> parameterTokens;

        public FunctionCallExpression(Expression lValue, IReadOnlyList<string> parameterTokens)
        {
            this.lValue = lValue;
            this.parameterTokens = parameterTokens;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            var expressions = parameterTokens.SplitBy(Interpreter.commaSet);
            var arguments = expressions.Select(expression => interpreter.GetValueFromExpression(expression.list, variableScope)).ToArray();
            var function = lValue.EvaluateExpression(interpreter, variableScope);
            CustomValue returnValue = interpreter.CallFunction(function, arguments, variableScope);
            return returnValue;
        }
    }
    class ParenthesesExpression : Expression
    {
        private IReadOnlyList<string> parenthesesTokens;

        public ParenthesesExpression(IReadOnlyList<string> parenthesesTokens)
        {
            this.parenthesesTokens = parenthesesTokens;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            var insideExpression = ExpressionMethods.New(new StringRange(parenthesesTokens, 1, parenthesesTokens.Count - 1));
            return insideExpression.EvaluateExpression(interpreter, variableScope);
        }
    }
    class SingleTokenNumberExpression : Expression
    {
        private string token;

        public SingleTokenNumberExpression(string token)
        {
            this.token = token;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            return CustomValue.FromNumber(token);
        }
    }
    class SingleTokenStringExpression : Expression
    {
        private string token;

        public SingleTokenStringExpression(string token)
        {
            this.token = token;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            return CustomValue.FromString(token);
        }
    }
    class SingleTokenVariableExpression : Expression
    {
        internal string token;

        public SingleTokenVariableExpression(string token)
        {
            this.token = token;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            return variableScope.GetVariable(token);
        }
    }
    class CustomValueExpression : Expression
    {
        private CustomValue value;

        public CustomValueExpression(CustomValue value)
        {
            this.value = value;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            return value;
        }
    }
    class SinglePlusMinusExpression : Expression
    {
        private string token;
        private Expression expressionRest;

        public SinglePlusMinusExpression(string token, Expression expressionRest)
        {
            this.token = token;
            this.expressionRest = expressionRest;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            var rest = expressionRest.EvaluateExpression(interpreter, variableScope);
            if (rest.type != ValueType.Number)
                throw new Exception();

            if (token == "-")
                return CustomValue.FromNumber((double)rest.value * -1);
            else if (token == "+")
                return CustomValue.FromNumber((double)rest.value);
            else
                throw new Exception();
        }
    }
    class NotExpression : Expression
    {
        private string token;
        private Expression expressionRest;

        public NotExpression(string token, Expression expressionRest)
        {
            this.token = token;
            this.expressionRest = expressionRest;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            var rest = expressionRest.EvaluateExpression(interpreter, variableScope);
            if (rest.type != ValueType.Bool)
                throw new Exception();

            bool restValue = (bool)rest.value;
            return restValue ? CustomValue.False : CustomValue.True;
        }
    }
    class AssignmentExpression : Expression
    {
        public Expression lValue;
        public string assignmentOperator;
        public Expression rValue;
        public bool hasVar;

        public AssignmentExpression(Expression lValue, string assignmentOperator, IReadOnlyList<string> rValueTokens, bool hasVar)
        {
            if (!Interpreter.assignmentSet.Contains(assignmentOperator))
                throw new Exception();

            this.lValue = lValue;
            this.assignmentOperator = assignmentOperator;
            this.rValue = ExpressionMethods.New(rValueTokens);
            this.hasVar = hasVar;
        }

        public AssignmentExpression(string variableName, string assignmentOperator, IReadOnlyList<string> rValueTokens, bool hasVar)
            : this(new SingleTokenVariableExpression(variableName), assignmentOperator, rValueTokens, hasVar)
        {

        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            var variableName = ((SingleTokenVariableExpression)lValue).token;

            if (hasVar)
            {
                if (assignmentOperator != "=")
                    throw new Exception();

                var value = rValue.EvaluateExpression(interpreter, variableScope);
                variableScope.AddVariable(variableName, value);
                return value;
            }
            else
            {
                var value = rValue.EvaluateExpression(interpreter, variableScope);
                var assignmentToken = assignmentOperator;

                switch (assignmentToken)
                {
                    case "=":
                        variableScope.SetVariable(variableName, value);
                        break;
                    case "+=":
                        {
                            var existingValue = variableScope.GetVariable(variableName);
                            value = interpreter.AddOrSubtract(existingValue, Operator.Plus, value);
                            variableScope.SetVariable(variableName, value);
                        }
                        break;
                    case "-=":
                        {
                            var existingValue = variableScope.GetVariable(variableName);
                            value = interpreter.AddOrSubtract(existingValue, Operator.Minus, value);
                            variableScope.SetVariable(variableName, value);
                        }
                        break;
                    case "*=":
                        {
                            var existingValue = variableScope.GetVariable(variableName);
                            value = interpreter.MultiplyOrDivide(existingValue, Operator.Multiply, value);
                            variableScope.SetVariable(variableName, value);
                        }
                        break;
                    case "/=":
                        {
                            var existingValue = variableScope.GetVariable(variableName);
                            value = interpreter.MultiplyOrDivide(existingValue, Operator.Divide, value);
                            variableScope.SetVariable(variableName, value);
                        }
                        break;
                    case "%=":
                        {
                            var existingValue = variableScope.GetVariable(variableName);
                            value = interpreter.MultiplyOrDivide(existingValue, Operator.Modulus, value);
                            variableScope.SetVariable(variableName, value);
                        }
                        break;
                    default:
                        throw new Exception();
                }
                return value;
            }
        }

        public static AssignmentExpression FromVarStatement(IReadOnlyList<string> tokens)
        {
            return new AssignmentExpression(tokens[0], tokens[1], new StringRange(tokens, 2, tokens.Count), true);
        }
    }
    class TernaryExpression : Expression
    {
        private Expression conditionExpression;
        private Expression questionMarkExpression;
        private Expression colonExpression;

        public TernaryExpression(Expression conditionExpression, Expression questionMarkExpression, Expression colonExpression)
        {
            this.conditionExpression = conditionExpression;
            this.questionMarkExpression = questionMarkExpression;
            this.colonExpression = colonExpression;
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            var conditionValue = conditionExpression.EvaluateExpression(interpreter, variableScope);
            bool isTruthy = conditionValue.IsTruthy();

            if (isTruthy)
                return questionMarkExpression.EvaluateExpression(interpreter, variableScope);
            else
                return colonExpression.EvaluateExpression(interpreter, variableScope);
        }
    }

    interface Statement
    {
        (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope variableScope);
        bool IsIfStatement { get; }
        bool IsElseIfStatement { get; }
        bool IsElseStatement { get; }
    }
    static class StatementMethods
    {
        public static Statement New(IReadOnlyList<string> tokens, bool isFunction)
        {
            if (tokens[0] == "{")
            {
                if (tokens[tokens.Count - 1] != "}")
                    throw new Exception();
                return new BlockStatement(tokens, isFunction);
            }
            else if (tokens[0] == "while")
            {
                return new WhileStatement(tokens);
            }
            else if (tokens[0] == "if")
            {
                return new IfStatement(tokens);
            }
            else if (tokens[0] == "else")
            {
                if (tokens[1] == "if")
                    return new ElseIfStatement(tokens);
                else
                    return new ElseStatement(tokens);
            }
            else
                return new LineStatement(tokens);
        }
        internal static (IReadOnlyList<string>, Statement) GetConditionTokensAndBody(IReadOnlyList<string> tokens, int conditionStartIndex, bool isFunction)
        {
            var endOfParentheses = tokens.IndexOfParenthesesEnd(conditionStartIndex);
            if (endOfParentheses < 0)
                throw new Exception();
            var conditionTokens = new StringRange(tokens, conditionStartIndex, endOfParentheses);

            var statementTokens = new StringRange(tokens, endOfParentheses + 1, tokens.Count);
            var statement = StatementMethods.New(statementTokens, isFunction);

            return (conditionTokens, statement);
        }
        internal static (Expression, Statement) GetConditionAndBody(IReadOnlyList<string> tokens, int conditionStartIndex)
        {
            var (conditionTokens, statement) = GetConditionTokensAndBody(tokens, conditionStartIndex, isFunction: false);
            var conditionExpression = ExpressionMethods.New(conditionTokens);

            return (conditionExpression, statement);
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

        public bool IsIfStatement => false;

        public bool IsElseIfStatement => false;

        public bool IsElseStatement => false;

        public (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope variableScope)
        {
            if (tokens.Count == 0) return (CustomValue.Null, false);

            if (tokens[0] == "var")
            {
                // Assignment to new variable
                var assignmentTree = AssignmentExpression.FromVarStatement(new StringRange(tokens, 1, tokens.Count));
                var value = assignmentTree.EvaluateExpression(interpreter, variableScope);
                return (CustomValue.Null, false);
            }
            else if (tokens[0] == "return")
            {
                var returnExpression = ExpressionMethods.New(new StringRange(tokens, 1, tokens.Count));
                var returnValue = returnExpression.EvaluateExpression(interpreter, variableScope);
                return (returnValue, true);
            }
            else if (tokens[0] == "function" && Interpreter.IsVariableName(tokens[1]))
            {
                var variableName = tokens[1];
                var functionStatement = new FunctionStatement(tokens);
                var function = functionStatement.EvaluateExpression(interpreter, variableScope);

                variableScope.AddVariable(variableName, function);
                return (CustomValue.Null, false);
            }

            CustomValue expressionValue = interpreter.GetValueFromExpression(tokens, variableScope);
            if (hasSemiColon)
                return (CustomValue.Null, false);
            return (expressionValue, false);
        }
    }
    class BlockStatement : Statement
    {
        List<Statement> statements;
        bool isFunction;

        public BlockStatement(IReadOnlyList<string> tokens, bool isFunction)
        {
            if (tokens[0] != "{")
                throw new Exception();
            tokens = new StringRange(tokens, 1, tokens.Count - 1);
            var statementRanges = tokens.GetStatementRanges();
            statements = statementRanges.Select(range => StatementMethods.New(range, isFunction)).ToList();
            this.isFunction = isFunction;
        }

        public bool IsIfStatement => false;

        public bool IsElseIfStatement => false;

        public bool IsElseStatement => false;

        public (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope innerScope)
        {
            var variableScope = VariableScope.NewWithInner(innerScope);
            foreach (var statement in statements)
            {
                var (value, isReturn) = statement.EvaluateStatement(interpreter, variableScope);
                if (isReturn)
                    return (value, true);
            }
            return (CustomValue.Null, false);
        }
    }
    class WhileStatement : Statement
    {
        Expression conditionExpression;
        Statement statement;

        public WhileStatement(IReadOnlyList<string> tokens)
        {
            if (tokens[1] != "(")
                throw new Exception();
            var conditionStartIndex = 2;
            var (expression, statement) = StatementMethods.GetConditionAndBody(tokens, conditionStartIndex);
            this.conditionExpression = expression;
            this.statement = statement;
        }

        public bool IsIfStatement => false;

        public bool IsElseIfStatement => false;

        public bool IsElseStatement => false;

        public (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope variableScope)
        {
            var returnValue = CustomValue.Null;

            while (true)
            {
                var conditionValue = conditionExpression.EvaluateExpression(interpreter, variableScope);
                if (!conditionValue.IsTruthy())
                    break;

                var (value, isReturn) = statement.EvaluateStatement(interpreter, variableScope);
                if (isReturn)
                    return (value, true);
            }

            return (returnValue, false);
        }
    }
    class IfStatement : Statement
    {
        Expression conditionExpression;
        internal Statement statementOfIf;
        internal Lazy<List<(Expression condition, Statement statement)>> elseIfStatements = new Lazy<List<(Expression, Statement)>>(() => new List<(Expression, Statement)>());
        Statement elseStatement;

        public IfStatement(IReadOnlyList<string> tokens)
        {
            if (tokens[1] != "(")
                throw new Exception();
            var conditionStartIndex = 2;
            var (expression, statement) = StatementMethods.GetConditionAndBody(tokens, conditionStartIndex);
            this.conditionExpression = expression;
            this.statementOfIf = statement;
        }

        public bool IsIfStatement => true;

        public bool IsElseIfStatement => false;

        public bool IsElseStatement => false;

        internal void AddElseIf(Statement statementAfterIf)
        {
            var elseIf = (ElseIfStatement)statementAfterIf;
            elseIfStatements.Value.Add((elseIf.condition, elseIf.statement));
        }

        internal void SetElse(Statement statementAfterIf)
        {
            if (elseStatement != null)
                throw new Exception();
            elseStatement = statementAfterIf;
        }

        public (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope variableScope)
        {
            var returnValue = CustomValue.Null;

            var conditionValue = conditionExpression.EvaluateExpression(interpreter, variableScope);
            if (conditionValue.IsTruthy())
            {
                var (value, isReturn) = statementOfIf.EvaluateStatement(interpreter, variableScope);
                if (isReturn)
                    return (value, true);
                return (returnValue, false);
            }

            foreach (var elseIfStatement in elseIfStatements.Value)
            {
                var elseIfCondition = elseIfStatement.condition.EvaluateExpression(interpreter, variableScope);
                if (elseIfCondition.IsTruthy())
                {
                    var (value, isReturn) = elseIfStatement.statement.EvaluateStatement(interpreter, variableScope);
                    if (isReturn)
                        return (value, true);
                    return (returnValue, false);
                }
            }

            if (elseStatement != null)
            {
                var (value, isReturn) = elseStatement.EvaluateStatement(interpreter, variableScope);
                if (isReturn)
                    return (value, true);
                return (returnValue, false);
            }

            return (returnValue, false);
        }
    }
    class ElseIfStatement : Statement
    {
        internal Expression condition;
        internal Statement statement;

        public ElseIfStatement(IReadOnlyList<string> tokens)
        {
            if (tokens[2] != "(")
                throw new Exception();
            var conditionStartIndex = 3;
            var (condition, statement) = StatementMethods.GetConditionAndBody(tokens, conditionStartIndex);
            this.condition = condition;
            this.statement = statement;
        }

        public bool IsIfStatement => false;

        public bool IsElseIfStatement => true;

        public bool IsElseStatement => false;

        public (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope variableScope)
        {
            return statement.EvaluateStatement(interpreter, variableScope);
        }
    }
    class ElseStatement : Statement
    {
        Statement statement;

        public ElseStatement(IReadOnlyList<string> tokens)
        {
            if (tokens[0] != "else")
                throw new Exception();
            statement = StatementMethods.New(new StringRange(tokens, 1, tokens.Count), isFunction: false);
        }

        public bool IsIfStatement => false;

        public bool IsElseIfStatement => false;

        public bool IsElseStatement => true;

        public (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope variableScope)
        {
            return statement.EvaluateStatement(interpreter, variableScope);
        }
    }
    class FunctionStatement : Statement, Expression
    {
        IReadOnlyList<string> tokens;

        public FunctionStatement(IReadOnlyList<string> tokens)
        {
            this.tokens = tokens;
        }

        public bool IsIfStatement => false;

        public bool IsElseIfStatement => false;

        public bool IsElseStatement => false;

        public (CustomValue, bool) EvaluateStatement(Interpreter interpreter, VariableScope variableScope)
        {
            var parenthesesIndex = tokens.IndexOf("(", 0);
            var (parameters, body) = StatementMethods.GetConditionTokensAndBody(tokens, parenthesesIndex + 1, isFunction: true);

            var parametersMap = new Dictionary<string, int>(); // Map is used to ensure uniqueness
            var parametersList = new List<string>();
            for (int i = 0; i < parameters.Count; i++)
            {
                if (i % 2 == 1)
                {
                    if (parameters[i] != ",")
                        throw new Exception();
                }
                else
                {
                    var parameterIndex = i / 2;
                    parametersMap.Add(parameters[i], parameterIndex);
                    parametersList.Add(parameters[i]);
                }
            }

            var function = new CustomFunction(parametersList, body);
            return (CustomValue.FromFunction(function), false);
        }

        public CustomValue EvaluateExpression(Interpreter interpreter, VariableScope variableScope)
        {
            return EvaluateStatement(interpreter, variableScope).Item1;
        }
    }
}
