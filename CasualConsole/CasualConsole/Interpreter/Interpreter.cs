using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CasualConsole.Interpreter
{
    public class Interpreter
    {
        private static readonly HashSet<char> onlyChars = new HashSet<char>() { '(', ')', ',', ';', '{', '}', '[', ']' };
        private static readonly HashSet<char> multiChars = new HashSet<char>() { '+', '-', '*', '/', '%', '=', '?', ':', '<', '>', '&', '|', '!', '.' };
        private static readonly HashSet<string> assignmentSet = new HashSet<string>() { "=", "+=", "-=", "*=", "/=", "%=" };
        private static readonly HashSet<string> commaSet = new HashSet<string>() { "," };
        private static readonly HashSet<string> semicolonSet = new HashSet<string>() { ";" };
        private static readonly HashSet<string> plusMinusSet = new HashSet<string>() { "+", "-" };
        private static readonly HashSet<string> comparisonSet = new HashSet<string>() { "==", "!=", "<", ">", "<=", ">=" };
        private static readonly HashSet<string> andOrSet = new HashSet<string>() { "&&", "||", "??" };
        private static readonly HashSet<string> asteriskSlashSet = new HashSet<string>() { "*", "/", "%" };
        private static readonly HashSet<string> keywords = new HashSet<string>() { "this", "var", "let", "const", "if", "while", "for", "break", "continue", "function", "return", "true", "false", "null" };

        private static Expression trueExpression;
        private static Expression falseExpression;
        private static Expression nullExpression;

        private static InternalFunction printFunction;

        private static readonly Lazy<CustomValue> arrayPushFunction = new Lazy<CustomValue>(() =>
        {
            var func = new ArrayPushFunction();
            return CustomValue.FromFunction(func);
        });
        private static readonly Lazy<CustomValue> arrayPopFunction = new Lazy<CustomValue>(() =>
        {
            var func = new ArrayPopFunction();
            return CustomValue.FromFunction(func);
        });

        private Context defaultContext;

        public Interpreter()
        {
            var defaultvariables = new Dictionary<string, (CustomValue, AssignmentType)>();
            var defaultVariableScope = VariableScope.NewDefault(defaultvariables, true);
            var defaultThisOwner = CustomValue.Null;
            defaultContext = new Context(defaultVariableScope, defaultThisOwner);

            trueExpression = new CustomValueExpression(CustomValue.True);
            falseExpression = new CustomValueExpression(CustomValue.False);
            nullExpression = new CustomValueExpression(CustomValue.Null);

            printFunction = new InternalFunction("print");
        }

        public object InterpretCode(string code)
        {
            var tokens = GetTokens(code).ToList();

            return InterpretTokens(tokens).value;
        }

        private static void GetStatementRangesTest(bool verbose)
        {
            var testCases = new List<(string code, string[] statements)>
            {
                ("2", new[]{ "2" }),
                ("a = 2", new[]{ "a = 2" }),
                ("var a = 2; a", new[]{ "var a = 2;", "a" }),
                ("var a = 2;", new[]{ "var a = 2;" }),
                ("var a = 2; var b = 2;", new[]{ "var a = 2;", "var b = 2;" }),
                ("{}", new[]{ "{}" }),
                ("({})", new[]{ "({})" }),
                ("{}{}", new[]{ "{}","{}" }),
                ("{} var a = 2; {}", new[]{ "{}", "var a = 2;", "{}" }),
                ("{ var a = 2; }", new[]{ "{ var a = 2; }" }),
                ("function () {}", new[]{ "function () {}" }),
                ("() => {}", new[]{ "() => {}" }),
                ("(x,y) => {}", new[]{ "(x,y) => {}" }),
                ("function(){} + 1", new[]{ "function(){} + 1" }),
                ("function(){} + function(){} + function(){}", new[]{ "function(){} + function(){} + function(){}" }),
                ("function(){};function(){};", new[]{ "function(){};", "function(){};" }),
                ("var func2 = true ? function(){ return '1'; } : function(){ return '2'; }; func2()", new[]{ "var func2 = true ? function(){ return '1'; } : function(){ return '2'; };", "func2()" }),
                ("function customReturnConstant(){ return -8; } customReturnConstant()",new []{ "function customReturnConstant(){ return -8; }", "customReturnConstant()" }),
                ("for(;;){}", new[]{ "for(;;){}" }),
                ("while(){}", new[]{ "while(){}" }),
            };

            foreach (var testCase in testCases)
            {
                var tokens = GetTokens(testCase.code).ToList();
                var statements = GetStatementRanges(tokens).ToList();
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

            if (verbose)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All GetStatementRangesTest tests have passed!");
                Console.ForegroundColor = oldColor;
            }
        }

        public static void Test(bool verbose = true)
        {
            CustomValue.Test();

            GetStatementRangesTest(verbose);

            DoPositiveTests();
            DoNegativeTests();

            if (verbose)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All Interpreter tests have passed!");
                Console.ForegroundColor = oldColor;
            }
        }

        private static void DoPositiveTests()
        {
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
                ("2 + 3 + ''", "5"),
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
                ("(function(){ return true; })()", true),
                ("(function(){ return; })()", null),
                ("(function(){ return null; })()", null),
                ("(function(){ return 1; })()", 1),
                ("(function(x){ return x; })(2)", 2),
                ("(function(){ return abs; })()(-3)", 3),
                ("returnValue(abs)(-4)", 4),
                ("(function(){ return function(){ return 1; }; })()()", 1),
                ("function customReturnConstant(){ return -8; } customReturnConstant()", -8),
                ("function returnAddAll(x,y,z){ return x + y + z; } returnAddAll(1,2,3)", 6),
                ("var k1 = 2; function f_k1(){ k1 = 10; } f_k1(); k1", 10),
                ("var o1 = { name: 'Serhat', \"number\": 2, 'otherField': 23, otherMap : { field1: 2001 }, f:function(){} }; o1 != null", true),
                ("o1['name']", "Serhat"),
                ("o1.name", "Serhat"),
                ("o1['number']", 2),
                ("o1.number", 2),
                ("o1['otherField']", 23),
                ("o1.otherField", 23),
                ("o1['undefinedVariable']", null),
                ("o1.undefinedVariable", null),
                ("o1['otherMap']['field1']", 2001),
                ("o1.otherMap.field1", 2001),
                ("o1['otherMap']['newFieldOtherMap'] = 1256", 1256),
                ("o1['otherMap']['newFieldOtherMap']", 1256),
                ("o1.otherMap.newFieldOtherMap = 1257", 1257),
                ("o1.otherMap.newFieldOtherMap", 1257),
                ("o1['newField'] = 987", 987),
                ("o1['newField']", 987),
                ("o1.newField", 987),
                ("({  }) != null", true),
                ("(function(){ return { name: 'serhat' } })()['name']", "serhat"),
                ("({ f: function(){ return -29; } }) != null", true),
                ("({ f: function(){ return -29; } })['f'] != null", true),
                ("({ f: function(){ return -29; } })['f']()", -29),
                ("({ field: 2 })['field']", 2),
                ("var o2 = { fieldName: 'field' }; ({ field: 23 })[o2['fieldName']]", 23),
                ("var while2 = 1; while(while2 < 7) { while2 += 1;  if(while2 == 6) break; } while2", 6),
                ("var while3 = 0; while(true){ break; } while3", 0),
                ("var while4 = 0; while(true){ while(true){ while4 = 2; break; } while4 = 4; break; } while4", 4),
                ("var while5 = 0; var while6 = 1; while(true){ while6 += 1; if(while6 == 10) break; if(while6 % 2 == 0) continue; while5 += 5; } while5", 20),
                ("((x,y) => { return x + y; })(2,3)", 5),
                ("(() => { return -1; })()", -1),
                ("(x => { return -2; })()", -2),
                ("(() => -3)()", -3),
                ("(x => -4)()", -4),
                ("(x => -4 - 2)()", -6),
                ("var o3 = { 'number': 20 }; o3.number += 1", 21),
                ("var o4 = { 'number': 24 }; o4.number += 1; o4.number", 25),
                ("var plusplus1 = 2; ++plusplus1", 3),
                ("var plusplus2 = { number: 21 }; ++plusplus2.number", 22),
                ("var plusplus3 = { number: 25 }; ++plusplus3['number']", 26),
                ("var plusplus4 = { obj: { number: 28 } }; ++plusplus4.obj.number", 29),
                ("var plusplus5 = { obj2: { number: 30 } }; ++plusplus5['obj2'].number", 31),
                ("var plusplus6 = { number: -10 }; ++plusplus6.number; plusplus6.number", -9),
                ("var minusminus1 = { number: 28 }; --minusminus1.number", 27),
                ("var minusminus2 = { number: 29 }; --minusminus2['number']", 28),
                ("1 == 1 && 2 == 2", true),
                ("1 == 1 && 2 == 3", false),
                ("0 == 1 && 2 == 2", false),
                ("0 == 1 && 2 == 3", false),
                ("var o5 = { number: 29 }; o5 != null && o5.number == 29", true),
                ("var o6 = null; o6 != null && o6.number == 29", false), // Optimization check
                ("var o7 = { number: 32 }; o7 && o7.number == 32", true), // Truthy check
                ("var o8 = null; o8 != null && o8.number == 33", false), // Optimization check with truthy
                ("1 == 1 || 2 == 2", true),
                ("1 == 1 || 2 == 3", true),
                ("0 == 1 || 2 == 2", true),
                ("0 == 1 || 2 == 3", false),
                ("var x1 = -9; var modifier = function(){ x1 = -8; return false; }", null), // Preperation for next test
                ("var x2 = false && modifier(); x2 == false && x1 == -9", true), // Optimization check
                ("var x3 = true && modifier(); x3 == false && x1 == -8", true), // Optimization check
                ("var plusplus7 = 8; var plusplus8 = plusplus7++; plusplus8 == 8 && plusplus7 == 9", true),
                ("var plusplus9 = 10; 2 + plusplus9++", 12),
                ("var o9 = { number: 5 }; var n1 = o9['number']++; n1 == 5 && o9['number'] == 6", true),
                ("var o10 = { number: 6 }; var n2 = o10.number++; n2 == 6 && o10.number == 7", true),
                ("var plusplus10 = 9; 2 + plusplus10--", 11),
                ("var o11 = { number: 2 }; var n3 = o11['number']--; n3 == 2 && o11['number'] == 1", true),
                ("var o12 = { number: 3 }; var n4 = o12.number--; n4 == 3 && o12.number == 2", true),
                ("var arr1 = [1,2,3]", null),
                ("arr1.length", 3),
                ("arr1[0]", 1),
                ("arr1[1]", 2),
                ("arr1[2]", 3),
                ("arr1[1] = 5", 5),
                ("arr1[1]", 5),
                ("arr1.length = 4", 4),
                ("arr1.length", 4),
                ("arr1[3]", null),
                ("arr1.name", null),
                ("arr1.name = 'hello'", "hello"),
                ("arr1.name", "hello"),
                ("var o13 = { 'name': 'some name', nameGetter: function(){ return this.name; } }; o13.nameGetter()", "some name"),
                ("var indexer = function(i){ return this[i]; }; var arr2 = [4,5,6]; arr2.get = indexer; arr2.get(0)", 4),
                ("var arr3 = [];", null),
                ("arr3.length", 0),
                ("arr3.push(5)", 1),
                ("arr3.length", 1),
                ("arr3[0]", 5),
                ("arr3.length = 2", 2),
                ("arr3.push(12)", 3),
                ("arr3[0]", 5),
                ("arr3[1]", null),
                ("arr3[2]", 12),
                ("arr3.length = 0", 0),
                ("arr3.push(7)", 1),
                ("arr3[0]", 7),
                ("arr3 = [7,6,7];", null),
                ("arr3.length", 3),
                ("arr3.pop()", 7),
                ("arr3.length", 2),
                ("arr3.pop()", 6),
                ("arr3.length", 1),
                ("arr3.pop()", 7),
                ("arr3.length", 0),
                ("for(;;) break;", null),
                ("var for1 = 1; for(var i=0; i < 10; i++) for1 += 2; for1", 21),
                ("var total = 0; var a = [1,2,3,4]; for(var i = 0; i < a.length; i++) total += a[i]; total", 10),
                ("for(var i=0, j=10; ; i+=2, j++){ if(i==j) break; } i == 20 && j == 20", true),
                ("var total = 0; var o = { x1: 2, x2: 5, x4: 7 }; for(var x in o) total += o[x]; total", 14),
                ("var total = 0; for(var x of [1,2,3,4,5]) total += x; total", 15),
                ("var scopevar1 = 2; { var scopevar1 = 3; } scopevar1", 3),
                ("let scopelet1 = 2; { let scopelet1 = 3; } scopelet1", 2),
                ("const scopeconst1 = 2; { const scopeconst1 = 3; } scopeconst1", 2),
                ("for(const i = 0; i < 0; i++){}", null),
                ("for(const i of [1,2,3,4]){}", null),
                ("var scopevar2 = 5; (function(){ var scopevar2 = 8; })(); scopevar2", 5),
                ("for(var i = 0, j = 0; i < 5; i++){} i", 5),
                ("let li = 6; for(let li = 0; li < 5; li++){} li", 6),
                ("for(let li = 0, lj = 0; li < 5; li++, lj++){}", null), // Testing let with multi initialization
                ("var arr = []; for(var i = 0; i < 2; i++) arr.push(() => i); arr[0]()", 2),
                ("let i2 = 10; var arr = []; for(let i2 = 0; i2 < 2; i2++) arr.push(() => { return i2; }); arr[0]()", 0),
                ("var arr = []; for(let i2 = 0; i2 < 2; i2++) { let iset = () => { return i2 = 15; }; let iget = () => i2; arr.push({ iset: iset, iget: iget }); };", null),
                ("arr[1].iget()", 1),
                ("arr[1].iset()", 15),
                ("arr[1].iget()", 15),
                ("let i3 = 0; for(i3 of [1,2,3]){} i3", 3),
                ("let i4 = 6; for(let i4 of [1,2,3]){} i4", 6),
                ("`hello`", "hello"),
                ("`$`", "$"),
                ("`\\$`", "$"),
                ("`\\${number}`", "${number}"),
                ("`${2}`", "2"),
                ("`${2+3}`", "5"),
                ("`${(2+3)}`", "5"),
                ("`\n`", "\n"), // Allow new lines for backtick strings
                ("var number = 1; `${number}`", "1"),
                ("var number = 1; `foo ${number} baz`", "foo 1 baz"),
                ("var text = 'world'; `hello ${text}`", "hello world"),
                ("'' + null", "null"),
                ("null + ''", "null"),
                ("'' + true", "true"),
                ("'' + false", "false"),
                ("'' + 2", "2"),
                ("'' + 2.5", "2.5"),
                ("!2", false),
                ("!!2", true),
                ("!0", true),
                ("!!0", false),
                ("!''", true),
                ("!!''", false),
                ("!'a'", false),
                ("!!'a'", true),
                ("null ?? 2", 2),
                ("null ?? `hello`", "hello"),
                ("null || `hello`", "hello"),
                ("null || 2", 2),
                ("0 ?? 2", 0),
                ("0 || 2", 2),
                ("'' ?? 'hello'", ""),
                ("'' || 'hello'", "hello"),
                ("2 && 7", 7),
                ("2 && 0", 0),
                ("var name = 'name1'; ({ name }).name", "name1"),
                ("var n1 = 'name2'; (()=> this.n1)()", "name2"),
                ("var n2 = 'name3'; var o = {}; o.f = ()=>{ return this.n2; }; o.f()", "name3"),
                ("var n3 = 'name4'; function thisTester1(){ (function(){ this.n3 = 'name4-2'; })(); } thisTester1(); n3", "name4-2"),
                ("print()", null),
                ("[...[1,2,3]].length", 3),
                ("var arr = [2, ...[1,2,3]]; arr[0] == 2 && arr[1] == 1 && arr[2] == 2 && arr[3] == 3", true),
                ("var arr = [1,2,3]; var arr2 = [...arr]; arr.push(6); arr2.length", 3),
                ("(function(){ var total = 0; for(var x of arguments) total += x; return total; })(1,2,3,4)", 10),
                ("(function(){ var total = 0; for(var x of arguments) total += x; return total; })(2, ...[1,2,3,4,5], 1)", 18),
                ("(function(x,y,...rest){ var total = 0; for(var x of rest) total+=x; return total })(1,2,3,4,5)", 12),
            };

            var interpreter = new Interpreter();
            foreach (var (code, value) in testCases)
            {
                var result = interpreter.InterpretCode(code);
                if (!InterpreterExtensions.Equals(result, value))
                {
                    throw new Exception();
                }
            }
        }

        private static void DoNegativeTests()
        {
            var testCases = new List<string>()
            {
                "var",
                "int",
                "variable",
                "[",
                "]",
                "{",
                "}",
                "(",
                ")",
                "((2)",
                "if(true)",
                "while(true)",
                "var a = ",
                "true,",
                "var a = ()",
                "var 23a = 23",
                "var a-b = -2",
                "let l11 = 1; let l11 = 1;",
                "var l12 = 1; let l12 = 1;",
                "let l21 = 1; var l21 = 1;",
                "const c21 = 1; c21 = 2;",
                "for(const i = 0; i < 1; i++){}",
                "var var = 1",
                "var let = 1",
                "let var = 1",
                "let let = 1",
                "var true = 1",
                "var false = 1",
                "var null = 1",
                "`${`",
                "`${}`",
                "'\n'",
                "var a = 0; ++++a",
                "var a = 0; ++a++",
                "var a = 0; a++++",
                "function(){}",
                "...[1,2,3]",
                "function(x,,x2){}",
                "function(x,x,x){}",
                ("(function(...rest1, rest2){})"),
                ("(function(...rest1, ...rest2){})"),
            };
            var interpreter = new Interpreter();
            foreach (var code in testCases)
            {
                try
                {
                    var result = interpreter.InterpretCode(code);
                }
                catch (Exception e)
                {
                    continue;
                }

                throw new Exception("Expected error but there was no error");
            }
        }

        public static void Benchmark()
        {
            var stopwatch = new Stopwatch();
            while (true)
            {
                stopwatch.Restart();

                for (int i = 0; i < 200; i++)
                {
                    DoPositiveTests();
                }

                stopwatch.Stop();
                Console.WriteLine($"Elapsed millis: {stopwatch.ElapsedMilliseconds}");
            }
        }

        private CustomValue InterpretTokens(IReadOnlyList<string> tokenSource)
        {
            var statementRanges = GetStatementRanges(tokenSource);
            var statementRangesEnumerator = statementRanges.GetEnumerator();

            while (statementRangesEnumerator.MoveNext())
            {
                var statementRange = statementRangesEnumerator.Current;
                var statement = StatementMethods.New(statementRange);

                if (statement.Type == StatementType.ElseIfStatement || statement.Type == StatementType.ElseStatement)
                    throw new Exception();

                if (statement.Type == StatementType.IfStatement)
                {
                    CustomRange<string> nonIfElseStatementRange = null;
                    Statement nonIfElseStatement = null;

                    var ifStatement = (IfStatement)statement;
                    while (ifStatement.statementOfIf.Type == StatementType.IfStatement)
                    {
                        ifStatement = (IfStatement)ifStatement.statementOfIf;
                    }

                    while (statementRangesEnumerator.MoveNext())
                    {
                        var statementRangeAfterIf = statementRangesEnumerator.Current;
                        var statementAfterIf = StatementMethods.New(statementRangeAfterIf);
                        if (statementAfterIf.Type == StatementType.ElseIfStatement)
                        {
                            ifStatement.AddElseIf(statementAfterIf);
                        }
                        else if (statementAfterIf.Type == StatementType.ElseStatement)
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

                    var value = GetValueFromStatement(ifStatement, defaultContext);
                    if (statementRange.end == tokenSource.Count)
                    {
                        return value.Item1;
                    }

                    if (nonIfElseStatement != null)
                    {
                        value = GetValueFromStatement(nonIfElseStatement, defaultContext);
                        if (nonIfElseStatementRange.end == tokenSource.Count)
                        {
                            return value.Item1;
                        }
                    }
                }
                else
                {
                    var value = GetValueFromStatement(statement, defaultContext);
                    if (statementRange.end == tokenSource.Count)
                    {
                        return value.Item1;
                    }
                }
            }

            return CustomValue.Null;
        }

        private static IEnumerable<CustomRange<string>> GetStatementRanges(IReadOnlyList<string> tokens)
        {
            int index = 0;
            while (index < tokens.Count)
            {
                var newIndex = GetStatementEndIndex(tokens, index);
                if (newIndex <= index)
                    throw new Exception();
                yield return new CustomRange<string>(tokens, index, newIndex);
                index = newIndex;
            }
            yield break;
        }

        private static int GetStatementEndIndex(IReadOnlyList<string> tokens, int startingIndex)
        {
            bool hasFunction = false;
            for (int i = startingIndex; i < tokens.Count; i++)
            {
                if (tokens[i] == "(")
                {
                    var newIndex = tokens.IndexOfParenthesesEnd(i + 1);
                    i = newIndex;
                }

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

        private static IEnumerable<IReadOnlyList<string>> SplitBy(IReadOnlyList<string> tokens, HashSet<string> separator)
        {
            if (tokens.Count == 0)
                yield break;

            var index = 0;
            var parenthesesCount = 0;
            var bracketsCount = 0;
            var bracesCount = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token == "(") parenthesesCount++;
                else if (token == ")") parenthesesCount--;
                else if (token == "[") bracketsCount++;
                else if (token == "]") bracketsCount--;
                else if (token == "{") bracesCount++;
                else if (token == "}") bracesCount--;

                if (parenthesesCount == 0 && bracketsCount == 0 && bracesCount == 0 && separator.Contains(token))
                {
                    yield return new CustomRange<string>(tokens, index, i);
                    index = i + 1;
                }
            }
            yield return new CustomRange<string>(tokens, index, tokens.Count);
        }

        private (CustomValue value, bool isReturn, bool isBreak, bool isContinue) GetValueFromStatement(Statement statement, Context context)
        {
            return statement.EvaluateStatement(context);
        }

        private static CustomValue CallFunction(FunctionObject function, IReadOnlyList<CustomValue> arguments, Context context)
        {
            var functionParameterArguments = new Dictionary<string, (CustomValue, AssignmentType)>();
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var (argName, isRest) = function.Parameters[i];
                if (isRest)
                {
                    var restArrayCount = arguments.Count - i;
                    var restArray = new CustomValue[restArrayCount];
                    for (int j = 0; j < restArrayCount; j++)
                    {
                        restArray[j] = arguments[i + j];
                    }
                    functionParameterArguments[argName] = (CustomValue.FromArray(new CustomArray(restArray)), AssignmentType.Var);
                    break;
                }
                else
                {
                    var value = i < arguments.Count ? arguments[i] : CustomValue.Null;
                    functionParameterArguments[argName] = (value, AssignmentType.Var);
                }
            }
            functionParameterArguments["arguments"] = (CustomValue.FromArray(new CustomArray(arguments)), AssignmentType.Var);
            var newScope = VariableScope.NewWithInner(function.Scope, functionParameterArguments, isFunctionScope: true);
            var newContext = new Context(newScope, context.thisOwner);
            var (result, isReturn, isBreak, isContinue) = function.EvaluateStatement(newContext);
            if (isBreak || isContinue)
                throw new Exception();
            return result;
        }

        private static FunctionObject GetFunctionObject(CustomValue f)
        {
            if (f.type != ValueType.Function)
                throw new Exception();
            return (FunctionObject)f.value;
        }

        private static FunctionObject GetFunctionObject(string functionName, VariableScope variableScope)
        {
            switch (functionName)
            {
                case "print":
                    return printFunction;
            }

            if (variableScope.TryGetVariable(functionName, out var f))
            {
                return GetFunctionObject(f);
            }

            throw new Exception();
        }

        private static bool Compare(CustomValue first, CustomValue second, Operator operatorType)
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

        private static CustomValue AndOr(Expression firstTree, IReadOnlyList<(Operator operatorType, Expression tree)> trees, Context context)
        {
            var firstValue = firstTree.EvaluateExpression(context);

            CustomValue result = firstValue;

            for (int i = 0; i < trees.Count; i++)
            {
                var (operatorType, tree) = trees[i];
                if (operatorType == Operator.AndAnd)
                {
                    if (!result.IsTruthy())
                        continue;
                    var nextValue = tree.EvaluateExpression(context);
                    result = nextValue;
                }
                else if (operatorType == Operator.OrOr)
                {
                    if (result.IsTruthy())
                        continue;
                    var nextValue = tree.EvaluateExpression(context);
                    result = nextValue;
                }
                else if (operatorType == Operator.DoubleQuestion)
                {
                    if (result.value != null)
                        continue;
                    var nextValue = tree.EvaluateExpression(context);
                    result = nextValue;
                }
                else
                    throw new Exception();
            }

            return result;
        }

        private static CustomValue CompareTo(Expression firstTree, IReadOnlyList<(Operator operatorType, Expression tree)> trees, Context context)
        {
            var firstValue = firstTree.EvaluateExpression(context);
            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.EvaluateExpression(context)));

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

        private static CustomValue MultiplyOrDivide(Expression firstTree, IReadOnlyList<(Operator operatorType, Expression tree)> trees, Context context)
        {
            var firstValue = firstTree.EvaluateExpression(context);
            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.EvaluateExpression(context)));

            return MultiplyOrDivide(firstValue, values);
        }

        private static CustomValue MultiplyOrDivide(CustomValue firstValue, Operator operatorType, CustomValue value)
        {
            return MultiplyOrDivide(firstValue, new[] { (operatorType, value) });
        }

        private static CustomValue MultiplyOrDivide(CustomValue firstValue, IReadOnlyList<(Operator operatorType, CustomValue value)> values)
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

        private static CustomValue AddOrSubtract(Expression firstTree, IReadOnlyList<(Operator operatorType, Expression tree)> trees, Context context)
        {
            var firstValue = firstTree.EvaluateExpression(context);
            var values = trees.SelectFast(x => (x.operatorType, value: x.tree.EvaluateExpression(context)));

            return AddOrSubtract(firstValue, values);
        }

        private static CustomValue AddOrSubtract(CustomValue firstValue, Operator operatorType, CustomValue value)
        {
            return AddOrSubtract(firstValue, new[] { (operatorType, value) });
        }

        private static CustomValue AddOrSubtract(CustomValue firstValue, IReadOnlyList<(Operator operatorType, CustomValue value)> values)
        {
            var totalValue = firstValue;

            foreach (var (type, value) in values)
            {
                if (type == Operator.Plus)
                {
                    if (totalValue.type == ValueType.Number && value.type == ValueType.Number)
                    {
                        double totalNumber = (double)totalValue.value + (double)value.value;
                        totalValue = CustomValue.FromNumber(totalNumber);
                    }
                    else
                    {
                        string totalString = totalValue.ToString() + value.ToString();
                        totalValue = CustomValue.FromParsedString(totalString);
                    }
                }
                else if (type == Operator.Minus)
                {
                    if (totalValue.type == ValueType.Number && value.type == ValueType.Number)
                    {
                        double totalNumber = (double)totalValue.value - (double)value.value;
                        totalValue = CustomValue.FromNumber(totalNumber);
                    }
                    else
                        throw new Exception();
                }
                else
                    throw new Exception();
            }

            return totalValue;
        }

        private static CustomValue GetValueFromExpression(IReadOnlyList<string> expressionTokens, Context context)
        {
            if (expressionTokens.Count == 0)
                throw new Exception();

            var expressionTree = ExpressionMethods.New(expressionTokens);
            return expressionTree.EvaluateExpression(context);
        }

        private static CustomValue DoIndexingGet(CustomValue baseExpressionValue, CustomValue keyExpressionValue, Context context)
        {
            if (baseExpressionValue.type == ValueType.Map && keyExpressionValue.type == ValueType.String)
            {
                var map = (Dictionary<string, CustomValue>)baseExpressionValue.value;
                if (map.TryGetValue((string)keyExpressionValue.value, out var value))
                    return value;
                else
                    return CustomValue.Null;
            }
            else if (baseExpressionValue.type == ValueType.Array)
            {
                var array = (CustomArray)baseExpressionValue.value;
                if (keyExpressionValue.type == ValueType.Number)
                {
                    var index = (int)(double)keyExpressionValue.value;
                    if (index >= array.Length)
                        throw new Exception();
                    else if (index >= array.list.Count)
                        return CustomValue.Null;
                    else
                        return array.list[index];
                }
                else if (keyExpressionValue.type == ValueType.String)
                {
                    var fieldName = (string)keyExpressionValue.value;
                    if (fieldName == "length")
                        return CustomValue.FromNumber(array.Length);

                    if (array.map.TryGetValue(fieldName, out CustomValue mapValue))
                        return mapValue;
                    else
                        return CustomValue.Null;
                }
            }
            else if (baseExpressionValue.type == ValueType.Null)
            {
                var variableName = (string)keyExpressionValue.value;
                return context.variableScope.GetVariable(variableName);
            }

            throw new Exception();
        }

        private static CustomValue DoIndexingSet(CustomValue value, CustomValue baseExpressionValue, CustomValue keyExpressionValue, Context context)
        {
            if (baseExpressionValue.type == ValueType.Map && keyExpressionValue.type == ValueType.String)
            {
                var map = (Dictionary<string, CustomValue>)baseExpressionValue.value;
                var key = (string)keyExpressionValue.value;
                map[key] = value;
                return value;
            }
            else if (baseExpressionValue.type == ValueType.Array)
            {
                var array = (CustomArray)baseExpressionValue.value;
                if (keyExpressionValue.type == ValueType.Number)
                {
                    var index = (int)(double)keyExpressionValue.value;
                    array.list[index] = value;
                    return value;
                }
                else if (keyExpressionValue.type == ValueType.String)
                {
                    var fieldName = (string)keyExpressionValue.value;
                    if (fieldName == "length")
                    {
                        int newLength = (int)(double)value.value;
                        array.Length = newLength;
                        return value;
                    }
                    else
                    {
                        array.map[fieldName] = value;
                        return value;
                    }
                }
            }
            else if (baseExpressionValue.type == ValueType.Null)
            {
                var variableName = (string)keyExpressionValue.value;
                context.variableScope.SetVariable(variableName, value);
                return value;
            }

            throw new Exception();
        }

        private static CustomValue ApplyLValueOperation(Expression lValue, Func<CustomValue, CustomValue> operation, Context context, out CustomValue oldValue)
        {
            if (lValue is SingleTokenVariableExpression singleExpression)
            {
                var variableName = singleExpression.token;
                oldValue = context.variableScope.GetVariable(variableName);
                var newValue = operation(oldValue);
                context.variableScope.SetVariable(variableName, newValue);
                return newValue;
            }
            else if (lValue is IndexingExpression indexingExpression)
            {
                var baseExpressionValue = indexingExpression.baseExpression.EvaluateExpression(context);
                var keyExpressionValue = indexingExpression.keyExpression.EvaluateExpression(context);

                oldValue = DoIndexingGet(baseExpressionValue, keyExpressionValue, context);
                var newValue = operation(oldValue);
                return DoIndexingSet(newValue, baseExpressionValue, keyExpressionValue, context);
            }
            else if (lValue is DotAccessExpression dotAccessExpression)
            {
                var baseExpressionValue = dotAccessExpression.baseExpression.EvaluateExpression(context);
                var keyExpressionValue = dotAccessExpression.keyExpressionValue;

                oldValue = DoIndexingGet(baseExpressionValue, keyExpressionValue, context);
                var newValue = operation(oldValue);
                return DoIndexingSet(newValue, baseExpressionValue, keyExpressionValue, context);
            }
            else
            {
                throw new Exception();
            }
        }

        private static bool IsVariableName(string token)
        {
            char firstChar = token[0];
            return firstChar == '_' || char.IsLetter(firstChar);
        }

        private static bool IsNumber(string token)
        {
            char firstChar = token[0];
            return char.IsDigit(firstChar);
        }

        private static bool IsStaticString(string token)
        {
            char firstChar = token[0];
            return firstChar == '"' || firstChar == '\'' || firstChar == '`';
        }

        private static Operator ParseOperator(string token)
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
                case "&&": return Operator.AndAnd;
                case "||": return Operator.OrOr;
                case "??": return Operator.DoubleQuestion;
                default: throw new Exception();
            }
        }

        private static bool IsAssignmentType(string operatorToken, out AssignmentType type)
        {
            switch (operatorToken)
            {
                case "var":
                    type = AssignmentType.Var;
                    return true;
                case "let":
                    type = AssignmentType.Let;
                    return true;
                case "const":
                    type = AssignmentType.Const;
                    return true;
                default:
                    break;
            }
            type = AssignmentType.None;
            return false;
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
                else if (c == '!' && content[i + 1] == '!')
                {
                    i++;
                    yield return c.ToString();
                }
                else if (multiChars.Contains(c))
                {
                    int start = i;
                    i++;
                    while (i < content.Length && multiChars.Contains(content[i]))
                        i++;
                    string token = content.Substring(start, i - start);
                    yield return token;
                }
                else if (onlyChars.Contains(c))
                {
                    i++;
                    yield return c.ToString();
                }
                else if (c == '"' || c == '\'' || c == '`')
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

        interface FunctionObject : Statement
        {
            IReadOnlyList<(string paramName, bool isRest)> Parameters { get; }
            VariableScope Scope { get; }
            bool IsLambda { get; }
        }
        class CustomFunction : FunctionObject
        {
            private IReadOnlyList<(string paramName, bool isRest)> parameters;
            private Statement body;
            private VariableScope scope;
            private bool isLambda;

            public CustomFunction(IReadOnlyList<(string paramName, bool isRest)> parameters, Statement body, VariableScope scope, bool isLambda)
            {
                this.parameters = parameters;
                this.body = body;
                this.scope = scope;
                this.isLambda = isLambda;
            }

            public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

            public VariableScope Scope => scope;

            public bool IsLambda => isLambda;

            public StatementType Type => throw new Exception();

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                return body.EvaluateStatement(context);
            }
        }
        class InternalFunction : FunctionObject
        {
            private string functionName;

            public InternalFunction(string functionName)
            {
                this.functionName = functionName;
            }

            public IReadOnlyList<(string paramName, bool isRest)> Parameters => new[] { ("x", false) };

            public VariableScope Scope => null;

            public bool IsLambda => false;

            public StatementType Type => throw new NotImplementedException();

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                switch (functionName)
                {
                    case "print":
                        return (HandlePrint(context), false, false, false);
                }

                throw new Exception();
            }

            private CustomValue HandlePrint(Context context)
            {
                var printValue = context.variableScope.GetVariable(Parameters[0].paramName);
                if (printValue.type != ValueType.Null)
                    Console.WriteLine(printValue.ToString());
                return CustomValue.Null;
            }
        }
        class ArrayPushFunction : FunctionObject
        {
            public IReadOnlyList<(string paramName, bool isRest)> Parameters => new[] { ("x", false) };

            public StatementType Type => throw new Exception();

            public VariableScope Scope => null;

            public bool IsLambda => false;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                var thisArray = context.thisOwner;
                if (thisArray.type != ValueType.Array)
                    throw new Exception();
                var array = (CustomArray)thisArray.value;
                var pushValue = context.variableScope.GetVariable(Parameters[0].paramName);
                array.list.Add(pushValue);
                return (CustomValue.FromNumber(array.list.Count), false, false, false);
            }
        }
        class ArrayPopFunction : FunctionObject
        {
            public IReadOnlyList<(string paramName, bool isRest)> Parameters => new (string, bool)[] { };

            public StatementType Type => throw new Exception();

            public VariableScope Scope => null;

            public bool IsLambda => false;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                var thisArray = context.thisOwner;
                if (thisArray.type != ValueType.Array)
                    throw new Exception();
                var array = (CustomArray)thisArray.value;
                var returnValue = array.list[array.list.Count - 1];
                array.list.RemoveAt(array.list.Count - 1);
                return (returnValue, false, false, false);
            }
        }
        class CustomArray
        {
            internal List<CustomValue> list;
            internal Dictionary<string, CustomValue> map;

            public int Length
            {
                get
                {
                    return list.Count;
                }
                set
                {
                    var newLength = value;
                    if (newLength > list.Count)
                    {
                        var diff = newLength - list.Count;
                        for (int i = 0; i < diff; i++)
                        {
                            list.Add(CustomValue.Null);
                        }
                    }
                    else if (newLength < list.Count)
                    {
                        var diff = list.Count - newLength;
                        for (int i = 0; i < diff; i++)
                        {
                            list.RemoveAt(list.Count - 1);
                        }
                    }
                }
            }

            public CustomArray(IReadOnlyList<CustomValue> list)
            {
                this.list = list.ToList(); // TODO fix this later
                this.map = new Dictionary<string, CustomValue>();
            }

            public CustomArray(List<CustomValue> list)
            {
                this.list = list;
                this.map = new Dictionary<string, CustomValue>();
                this.map["push"] = arrayPushFunction.Value;
                this.map["pop"] = arrayPopFunction.Value;
            }
        }

        private struct CustomValue
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

            public static CustomValue FromStaticString(string s)
            {
                var parsedString = SingleTokenStringExpression.ParseString(s, null);
                return FromParsedString(parsedString);
            }

            public static CustomValue FromParsedString(string s)
            {
                return new CustomValue(s, ValueType.String);
            }

            internal static CustomValue FromFunction(FunctionObject func)
            {
                return new CustomValue(func, ValueType.Function);
            }

            internal static CustomValue FromMap(Dictionary<string, CustomValue> map)
            {
                return new CustomValue(map, ValueType.Map);
            }

            internal static CustomValue FromArray(CustomArray array)
            {
                return new CustomValue(array, ValueType.Array);
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
                    case ValueType.Map:
                        return true;
                    case ValueType.Function:
                        return true;
                    case ValueType.Array:
                        return true;
                    default:
                        throw new Exception();
                }
            }

            internal string ToString()
            {
                if (value == null)
                    return "null";
                if (value is bool b)
                    return b ? "true" : "false";
                return value.ToString();
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
                    var result = CustomValue.FromStaticString(stringTestCase.token);
                    if (!string.Equals(result.value, stringTestCase.value))
                    {
                        throw new Exception();
                    }
                }
            }
        }

        enum StatementType
        {
            LineStatement,
            BlockStatement,
            IfStatement,
            ElseIfStatement,
            ElseStatement,
            WhileStatement,
            ForStatement,
            ForInOfStatement,
            FunctionStatement,
        }

        enum ValueType
        {
            Null,
            Number,
            String,
            Bool,
            Function,
            Map,
            Array,
        }

        enum AssignmentType
        {
            None,
            Var,
            Let,
            Const,
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
            AndAnd,
            OrOr,
            DoubleQuestion,
        }

        enum Precedence : int
        {
            DoubleQuestionMark = 4,
            OrOr = 4,
            AndAnd = 5,
            EqualityCheck = 9,
            Comparison = 10,
            AddSubtract = 12,
            MultiplyDivide = 13,
            FunctionCall = 9999,
            Indexing = 9999,
            DotAccess = 9999,
            LambdaExpression = 9999,
            Increment = 9999,
        }

        private class CustomRange<T> : IReadOnlyList<T>
        {
            public readonly IReadOnlyList<T> array;
            public readonly int start;
            public readonly int end;

            public CustomRange(IReadOnlyList<T> array, int start, int end)
            {
                if (array is CustomRange<T> range)
                {
                    this.array = (List<T>)range.array;
                    this.start = range.start + start;
                    this.end = this.start + (end - start);
                }
                else
                {
                    this.array = array;
                    this.start = start;
                    this.end = end;
                }
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
            private Dictionary<string, (CustomValue, AssignmentType)> variables;
            private VariableScope innerScope;
            private bool isFunctionScope;

            private VariableScope(Dictionary<string, (CustomValue, AssignmentType)> variables, VariableScope innerScope, bool isFunctionScope)
            {
                this.variables = variables;
                this.innerScope = innerScope;
                this.isFunctionScope = isFunctionScope;
            }

            public bool TryGetVariable(string variableName, out CustomValue value)
            {
                if (variables.TryGetValue(variableName, out var valuePair))
                {
                    value = valuePair.Item1;
                    return true;
                }
                if (innerScope != null && innerScope.TryGetVariable(variableName, out value))
                {
                    return true;
                }
                value = CustomValue.Null;
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
                if (keywords.Contains(variableName))
                    throw new Exception();

                if (variables.TryGetValue(variableName, out var res))
                {
                    var (_, type) = res;
                    if (type == AssignmentType.Const)
                        throw new Exception();
                    variables[variableName] = (value, type);
                }
                else if (innerScope != null)
                {
                    innerScope.SetVariable(variableName, value);
                }
                else
                    throw new Exception();
            }

            public void AddVarVariable(string variableName, CustomValue value)
            {
                if (keywords.Contains(variableName))
                    throw new Exception();

                var scope = this;
                while (!scope.isFunctionScope)
                    scope = scope.innerScope;

                if (scope.variables.TryGetValue(variableName, out var res))
                {
                    var (_, type) = res;
                    if (type == AssignmentType.Var)
                        scope.variables[variableName] = (value, AssignmentType.Var);
                    else
                        throw new Exception();
                }
                else
                {
                    scope.variables[variableName] = (value, AssignmentType.Var);
                }
            }

            public void AddLetVariable(string variableName, CustomValue value)
            {
                if (keywords.Contains(variableName))
                    throw new Exception();

                var scope = this;
                scope.variables.Add(variableName, (value, AssignmentType.Let));
            }

            public void AddConstVariable(string variableName, CustomValue value)
            {
                if (keywords.Contains(variableName))
                    throw new Exception();

                var scope = this;
                scope.variables.Add(variableName, (value, AssignmentType.Const));
            }

            public static VariableScope NewDefault(Dictionary<string, (CustomValue, AssignmentType)> variables, bool isFunctionScope)
            {
                return new VariableScope(variables, null, isFunctionScope);
            }
            public static VariableScope NewWithInner(VariableScope innerScope, bool isFunctionScope)
            {
                var newVariables = new Dictionary<string, (CustomValue, AssignmentType)>();
                return NewWithInner(innerScope, newVariables, isFunctionScope);
            }
            public static VariableScope NewWithInner(VariableScope innerScope, Dictionary<string, (CustomValue, AssignmentType)> values, bool isFunctionScope)
            {
                return new VariableScope(values, innerScope, isFunctionScope);
            }
            public static VariableScope GetNewLoopScope(VariableScope scope, AssignmentType assignmentType, string variableName, CustomValue variableValue)
            {
                var loopScope = ScopeForLoop(scope, assignmentType);
                AssignVariable(loopScope, assignmentType, variableName, variableValue);
                return loopScope;
            }
            public static VariableScope GetNewLoopScopeCopy(VariableScope scope, AssignmentType assignmentType)
            {
                var newScope = ScopeForLoop(scope, assignmentType);

                foreach (var variable in scope.variables)
                {
                    var variableName = variable.Key;
                    var (variableValue, _) = variable.Value;
                    AssignVariable(newScope, assignmentType, variableName, variableValue);
                }

                return newScope;
            }
            private static VariableScope ScopeForLoop(VariableScope scope, AssignmentType assignmentType)
            {
                switch (assignmentType)
                {
                    case AssignmentType.None:
                        {
                            return scope;
                        }
                    case AssignmentType.Var:
                    case AssignmentType.Let:
                    case AssignmentType.Const:
                        {
                            var newScope = VariableScope.NewWithInner(scope, isFunctionScope: false);
                            return newScope;
                        }
                    default:
                        throw new Exception();
                }
            }
            private static void AssignVariable(VariableScope scope, AssignmentType assignmentType, string variableName, CustomValue variableValue)
            {
                switch (assignmentType)
                {
                    case AssignmentType.None:
                        scope.SetVariable(variableName, variableValue);
                        break;
                    case AssignmentType.Var:
                        scope.AddVarVariable(variableName, variableValue);
                        break;
                    case AssignmentType.Let:
                        scope.AddLetVariable(variableName, variableValue);
                        break;
                    case AssignmentType.Const:
                        scope.AddConstVariable(variableName, variableValue);
                        break;
                    default:
                        break;
                }
            }
        }
        class Context
        {
            public readonly VariableScope variableScope;
            public readonly CustomValue thisOwner;

            public Context(VariableScope variableScope, CustomValue thisOwner)
            {
                this.variableScope = variableScope;
                this.thisOwner = thisOwner;
            }
        }

        interface Expression
        {
            CustomValue EvaluateExpression(Context context);
        }
        static class ExpressionMethods
        {
            public static Expression New(IReadOnlyList<string> tokens)
            {
                Expression previousExpression = null;
                var index = 0;

                while (true)
                {
                    if (index == tokens.Count)
                        return previousExpression;

                    if (previousExpression == null)
                    {
                        var (expression, newIndex) = ReadExpression(tokens, index);
                        if (newIndex <= index)
                            throw new Exception();

                        index = newIndex;
                        previousExpression = expression;
                    }
                    else
                    {
                        var newToken = tokens[index];

                        if (newToken == ";")
                            return previousExpression;

                        if (assignmentSet.Contains(newToken))
                        {
                            var restTokens = new CustomRange<string>(tokens, index + 1, tokens.Count);
                            return new AssignmentExpression(previousExpression, newToken, restTokens, AssignmentType.None);
                        }

                        if (plusMinusSet.Contains(newToken)
                            || asteriskSlashSet.Contains(newToken)
                            || comparisonSet.Contains(newToken)
                            || andOrSet.Contains(newToken))
                        {
                            var newPrecedence = TreeExpression.GetPrecedence(newToken);

                            Expression nextExpression;
                            (nextExpression, index) = ReadExpression(tokens, index + 1);

                            AddToLastNode(ref previousExpression, newPrecedence, (expression, precedence) =>
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

                            var questionMarkExpressionTokens = new CustomRange<string>(tokens, questionMarkIndex + 1, colonIndex);
                            var questionMarkExpression = ExpressionMethods.New(questionMarkExpressionTokens);
                            var colonExpressionTokens = new CustomRange<string>(tokens, colonIndex + 1, tokens.Count);
                            var colonExpression = ExpressionMethods.New(colonExpressionTokens);

                            var ternaryExpression = new TernaryExpression(previousExpression, questionMarkExpression, colonExpression);
                            return ternaryExpression;
                        }

                        if (newToken == "=>")
                        {
                            var (functionBodyTokens, end) = ReadBodyTokensAndEnd(tokens, index);

                            AddToLastNode(ref previousExpression, Precedence.LambdaExpression, (expression, p) =>
                            {
                                if (expression is SingleTokenVariableExpression singleTokenVariableExpression)
                                {
                                    var parameterTokens = new string[] { singleTokenVariableExpression.token };
                                    return FunctionStatement.FromParametersAndBody(parameterTokens, functionBodyTokens, isLambda: true);
                                }
                                else
                                    throw new Exception();
                            });

                            index = end + 1;
                            continue;
                        }

                        if (newToken == "(")
                        {
                            // Function call
                            var end = tokens.IndexOfParenthesesEnd(index + 1);
                            if (end < 0)
                                throw new Exception();
                            var parameters = new CustomRange<string>(tokens, index + 1, end);

                            AddToLastNode(ref previousExpression, Precedence.FunctionCall, (expression, p) =>
                            {
                                return new FunctionCallExpression(expression, parameters);
                            });

                            index = end + 1;
                            continue;
                        }

                        if (newToken == "++" || newToken == "--")
                        {
                            var isInc = newToken == "++";
                            AddToLastNode(ref previousExpression, Precedence.Indexing, (expression, p) =>
                            {
                                return new PrePostIncDecExpression(expression, isPre: false, isInc: isInc);
                            });

                            index += 1;
                            continue;
                        }

                        if (newToken == "[")
                        {
                            // Indexing
                            var end = tokens.IndexOfBracketsEnd(index + 1);
                            if (end < 0)
                                throw new Exception();
                            var keyExpressionTokens = new CustomRange<string>(tokens, index + 1, end);

                            AddToLastNode(ref previousExpression, Precedence.Indexing, (expression, p) =>
                            {
                                if (expression is PrePostIncDecExpression prePostIncDecExpression)
                                {
                                    prePostIncDecExpression.expressionRest = new IndexingExpression(prePostIncDecExpression.expressionRest, keyExpressionTokens);
                                    return prePostIncDecExpression;
                                }
                                else
                                {
                                    return new IndexingExpression(expression, keyExpressionTokens);
                                }
                            });

                            index = end + 1;
                            continue;
                        }

                        if (newToken == ".")
                        {
                            // Dot access
                            var fieldName = tokens[index + 1];
                            if (!IsVariableName(fieldName))
                                throw new Exception();

                            AddToLastNode(ref previousExpression, Precedence.DotAccess, (expression, p) =>
                            {
                                if (expression is PrePostIncDecExpression prePostIncDecExpression)
                                {
                                    prePostIncDecExpression.expressionRest = new DotAccessExpression(prePostIncDecExpression.expressionRest, fieldName);
                                    return prePostIncDecExpression;
                                }
                                else
                                {
                                    return new DotAccessExpression(expression, fieldName);
                                }
                            });

                            index += 2;
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
                    var newExpression = new NotExpression(expressionRest);
                    return (newExpression, lastIndex);
                }
                if (token == "++" || token == "--")
                {
                    var isInc = token == "++";
                    var (expressionRest, lastIndex) = ReadExpression(tokens, index + 1);
                    var newExpression = new PrePostIncDecExpression(expressionRest, isPre: true, isInc: isInc);
                    return (newExpression, lastIndex);
                }

                if (token == "(")
                {
                    var newend = tokens.IndexOfParenthesesEnd(index + 1);
                    if (newend < 0)
                        throw new Exception();

                    bool isLambda = newend + 1 < tokens.Count && tokens[newend + 1] == "=>";
                    if (!isLambda)
                    {
                        var parenthesesTokens = new CustomRange<string>(tokens, index, newend + 1);
                        var newExpression = new ParenthesesExpression(parenthesesTokens);
                        return (newExpression, newend + 1);
                    }
                    else
                    {
                        var (functionBodyTokens, end) = ReadBodyTokensAndEnd(tokens, newend + 1);

                        var parameterTokens = new CustomRange<string>(tokens, index + 1, newend);
                        var functionExpression = FunctionStatement.FromParametersAndBody(parameterTokens, functionBodyTokens, isLambda);

                        return (functionExpression, end + 1);
                    }
                }
                if (token == "{")
                {
                    var bracesEnd = tokens.IndexOfBracesEnd(index + 1);
                    if (bracesEnd < 0)
                        throw new Exception();

                    var mapExpressionTokens = new CustomRange<string>(tokens, index, bracesEnd + 1);
                    var mapExpression = new MapExpression(mapExpressionTokens);
                    return (mapExpression, bracesEnd + 1);
                }
                if (token == "[")
                {
                    var bracketsEnd = tokens.IndexOfBracketsEnd(index + 1);
                    if (bracketsEnd < 0)
                        throw new Exception();

                    var arrayTokens = new CustomRange<string>(tokens, index, bracketsEnd + 1);
                    var arrayExpression = new ArrayExpression(arrayTokens);
                    return (arrayExpression, bracketsEnd + 1);
                }

                if (token == "true")
                {
                    return (trueExpression, index + 1);
                }
                if (token == "false")
                {
                    return (falseExpression, index + 1);
                }
                if (token == "null")
                {
                    return (nullExpression, index + 1);
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

                    var functionExpressionTokens = new CustomRange<string>(tokens, index, bracesEnd + 1);
                    var functionExpression = FunctionStatement.FromTokens(functionExpressionTokens, isLambda: false);
                    return (functionExpression, bracesEnd + 1);
                }
                if (IsNumber(token))
                {
                    return (new SingleTokenNumberExpression(token), index + 1);
                }
                if (IsStaticString(token))
                {
                    return (new SingleTokenStringExpression(token), index + 1);
                }
                if (IsVariableName(token))
                {
                    return (new SingleTokenVariableExpression(token), index + 1);
                }

                throw new Exception();
            }

            private static (IReadOnlyList<string>, int) ReadBodyTokensAndEnd(IReadOnlyList<string> tokens, int index)
            {
                var nextToken = tokens[index + 1];

                IReadOnlyList<string> functionBodyTokens;
                int end;
                if (nextToken == "{")
                {
                    end = tokens.IndexOfBracesEnd(index + 2);
                    if (end < 0)
                        throw new Exception();

                    functionBodyTokens = new CustomRange<string>(tokens, index + 1, end + 1);
                }
                else
                {
                    functionBodyTokens = new CustomRange<string>(tokens, index + 1, tokens.Count);
                    end = tokens.Count - 1;
                }

                return (functionBodyTokens, end);
            }

            private static void AddToLastNode(ref Expression previousExpression, Precedence precedence, Func<Expression, Precedence?, Expression> handler)
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

            public CustomValue EvaluateExpression(Context context)
            {
                switch (precedence)
                {
                    case Precedence.EqualityCheck:
                        return CompareTo(firstExpression, nextValues, context);
                    case Precedence.Comparison:
                        return CompareTo(firstExpression, nextValues, context);
                    case Precedence.AddSubtract:
                        return AddOrSubtract(firstExpression, nextValues, context);
                    case Precedence.MultiplyDivide:
                        return MultiplyOrDivide(firstExpression, nextValues, context);
                    case Precedence.AndAnd:
                        return AndOr(firstExpression, nextValues, context);
                    case Precedence.OrOr:
                        return AndOr(firstExpression, nextValues, context);
                    default:
                        break;
                }

                throw new Exception();
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
                    case "==": return Precedence.EqualityCheck;
                    case "!=": return Precedence.EqualityCheck;
                    case "<": return Precedence.Comparison;
                    case "<=": return Precedence.Comparison;
                    case ">": return Precedence.Comparison;
                    case ">=": return Precedence.Comparison;
                    case "&&": return Precedence.AndAnd;
                    case "||": return Precedence.OrOr;
                    case "??": return Precedence.DoubleQuestionMark;
                    default: throw new Exception();
                }
            }

            internal void AddExpression(string newToken, Expression nextExpression)
            {
                nextValues.Add((ParseOperator(newToken), nextExpression));
            }
        }
        class FunctionCallExpression : Expression
        {
            Expression lValue;
            List<(bool hasThreeDot, Expression expression)> expressionList;

            public FunctionCallExpression(Expression lValue, IReadOnlyList<string> parameterTokens)
            {
                this.lValue = lValue;
                var res = SplitBy(parameterTokens, commaSet);
                var expressionList = new List<(bool hasThreeDot, Expression expression)>();
                foreach (var item in res)
                {
                    if (item[0] == "...")
                        expressionList.Add((true, ExpressionMethods.New(new CustomRange<string>(item, 1, item.Count))));
                    else
                        expressionList.Add((false, ExpressionMethods.New(item)));
                }
                this.expressionList = expressionList;
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var arguments = new List<CustomValue>();

                foreach (var (hasThreeDot, expression) in expressionList)
                {
                    if (hasThreeDot)
                    {
                        var value = expression.EvaluateExpression(context);
                        if (value.type != ValueType.Array)
                            throw new Exception();
                        var array = (CustomArray)value.value;
                        foreach (var item in array.list)
                            arguments.Add(item);
                    }
                    else
                        arguments.Add(expression.EvaluateExpression(context));
                }

                FunctionObject function;
                if (lValue is SingleTokenVariableExpression variable)
                {
                    var functionName = variable.token;
                    function = GetFunctionObject(functionName, context.variableScope);
                }
                else
                {
                    var functionValue = lValue.EvaluateExpression(context);
                    function = GetFunctionObject(functionValue);
                }

                Context newContext;
                var isLambda = function.IsLambda;
                if (!isLambda)
                {
                    CustomValue newOwner;
                    if (lValue is DotAccessExpression dotAccessExpression)
                        newOwner = dotAccessExpression.baseExpression.EvaluateExpression(context);
                    else if (lValue is IndexingExpression indexingExpression)
                        newOwner = indexingExpression.baseExpression.EvaluateExpression(context);
                    else
                        newOwner = CustomValue.Null;

                    newContext = new Context(context.variableScope, newOwner);
                }
                else
                {
                    newContext = context;
                }

                return CallFunction(function, arguments, newContext);
            }
        }
        class IndexingExpression : Expression
        {
            internal Expression baseExpression;
            internal Expression keyExpression;

            public IndexingExpression(Expression baseExpression, IReadOnlyList<string> keyExpressionTokens)
            {
                this.baseExpression = baseExpression;
                this.keyExpression = ExpressionMethods.New(keyExpressionTokens);
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var ownerExpressionValue = baseExpression.EvaluateExpression(context);
                var keyExpressionValue = keyExpression.EvaluateExpression(context);

                return DoIndexingGet(ownerExpressionValue, keyExpressionValue, context);
            }
        }
        class DotAccessExpression : Expression
        {
            internal Expression baseExpression;
            internal CustomValue keyExpressionValue;

            public DotAccessExpression(Expression expression, string fieldName)
            {
                this.baseExpression = expression;
                this.keyExpressionValue = CustomValue.FromParsedString(fieldName);
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var ownerExpressionValue = baseExpression.EvaluateExpression(context);
                return DoIndexingGet(ownerExpressionValue, keyExpressionValue, context);
            }
        }
        class MapExpression : Expression
        {
            List<(string fieldName, Expression expression)> fieldExpressions;

            public MapExpression(IReadOnlyList<string> tokens)
            {
                tokens = new CustomRange<string>(tokens, 1, tokens.Count - 1);
                var res = SplitBy(tokens, commaSet);
                this.fieldExpressions = new List<(string fieldName, Expression expression)>();
                foreach (var item in res)
                {
                    var firstToken = item[0];
                    var fieldName = IsVariableName(firstToken) ? firstToken : (string)CustomValue.FromStaticString(firstToken).value;

                    Expression expression;
                    if (item.Count == 1)
                        expression = ExpressionMethods.New(new[] { firstToken });
                    else if (item[1] == ":")
                        expression = ExpressionMethods.New(new CustomRange<string>(item, 2, item.Count));
                    else
                        throw new Exception();

                    this.fieldExpressions.Add((fieldName, expression));
                }
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var map = new Dictionary<string, CustomValue>();
                foreach (var (fieldName, expression) in fieldExpressions)
                {
                    var fieldValue = expression.EvaluateExpression(context);
                    map.Add(fieldName, fieldValue);
                }
                return CustomValue.FromMap(map);
            }
        }
        class ArrayExpression : Expression
        {
            private List<(bool hasThreeDot, Expression expression)> expressionList;

            public ArrayExpression(CustomRange<string> tokens)
            {
                tokens = new CustomRange<string>(tokens, 1, tokens.Count - 1);
                var res = SplitBy(tokens, commaSet);
                expressionList = new List<(bool, Expression)>();
                foreach (var item in res)
                {
                    if (item[0] == "...")
                        expressionList.Add((true, ExpressionMethods.New(new CustomRange<string>(item, 1, item.Count))));
                    else
                        expressionList.Add((false, ExpressionMethods.New(item)));
                }
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var list = new List<CustomValue>();
                foreach (var (hasThreeDot, expression) in expressionList)
                {
                    if (hasThreeDot)
                    {
                        var value = expression.EvaluateExpression(context);
                        if (value.type != ValueType.Array)
                            throw new Exception();
                        var array = (CustomArray)value.value;
                        foreach (var item in array.list)
                            list.Add(item);
                    }
                    else
                        list.Add(expression.EvaluateExpression(context));
                }
                return CustomValue.FromArray(new CustomArray(list));
            }
        }
        class ParenthesesExpression : Expression
        {
            private Expression insideExpression;

            public ParenthesesExpression(IReadOnlyList<string> parenthesesTokens)
            {
                if (parenthesesTokens[0] != "(")
                    throw new Exception();
                this.insideExpression = ExpressionMethods.New(new CustomRange<string>(parenthesesTokens, 1, parenthesesTokens.Count - 1));
            }

            public CustomValue EvaluateExpression(Context context)
            {
                return insideExpression.EvaluateExpression(context);
            }
        }
        class SingleTokenNumberExpression : Expression
        {
            private CustomValue number;

            public SingleTokenNumberExpression(string token)
            {
                this.number = CustomValue.FromNumber(token);
            }

            public CustomValue EvaluateExpression(Context context)
            {
                return number;
            }
        }
        class SingleTokenStringExpression : Expression
        {
            private string token;

            public SingleTokenStringExpression(string token)
            {
                this.token = token;
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var parsedString = ParseString(token, context);
                return CustomValue.FromParsedString(parsedString);
            }

            public static string ParseString(string s, Context context)
            {
                if (s[0] != '"' && s[0] != '\'' && s[0] != '`')
                    throw new Exception();

                var isBacktickString = s[0] == '`';

                if (isBacktickString && context == null)
                    throw new Exception();

                char firstChar = s[0];

                var sb = new StringBuilder();
                for (int i = 1; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == firstChar)
                        break;
                    else if ((c == '\r' || c == '\n') && !isBacktickString)
                        throw new Exception();
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
                            case '$': sb.Append(isBacktickString ? c2 : throw new Exception()); break;
                            default: throw new Exception();
                        }
                    }
                    else if (c == '$' && s[i + 1] == '{')
                    {
                        var end = s.IndexOfBracesEnd(i + 2);
                        var substring = s.Substring(i + 2, end - (i + 2));
                        var subtokens = GetTokens(substring).ToList();
                        var subExpression = ExpressionMethods.New(subtokens);
                        var subExpressionValue = subExpression.EvaluateExpression(context);
                        var subExpressionString = subExpressionValue.ToString();
                        sb.Append(subExpressionString);
                        i = end;
                    }
                    else
                        sb.Append(c);
                }
                return sb.ToString();
            }
        }
        class SingleTokenVariableExpression : Expression
        {
            internal string token;

            public SingleTokenVariableExpression(string token)
            {
                this.token = token;
            }

            public CustomValue EvaluateExpression(Context context)
            {
                if (token == "this")
                    return context.thisOwner;
                return context.variableScope.GetVariable(token);
            }
        }
        class CustomValueExpression : Expression
        {
            private CustomValue value;

            public CustomValueExpression(CustomValue value)
            {
                this.value = value;
            }

            public CustomValue EvaluateExpression(Context context)
            {
                return value;
            }
        }
        class SinglePlusMinusExpression : Expression
        {
            private bool isMinus;
            private Expression expressionRest;

            public SinglePlusMinusExpression(string token, Expression expressionRest)
            {
                this.expressionRest = expressionRest;

                if (token == "-")
                    isMinus = true;
                else if (token == "+")
                    isMinus = false;
                else
                    throw new Exception();
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var rest = expressionRest.EvaluateExpression(context);
                if (rest.type != ValueType.Number)
                    throw new Exception();

                if (isMinus)
                    return CustomValue.FromNumber((double)rest.value * -1);
                else
                    return CustomValue.FromNumber((double)rest.value);
            }
        }
        class NotExpression : Expression
        {
            private Expression expressionRest;

            public NotExpression(Expression expressionRest)
            {
                this.expressionRest = expressionRest;
            }

            public CustomValue EvaluateExpression(Context context)
            {
                var rest = expressionRest.EvaluateExpression(context);
                bool restValue = rest.IsTruthy();
                return restValue ? CustomValue.False : CustomValue.True;
            }
        }
        class PrePostIncDecExpression : Expression
        {
            internal Expression expressionRest;
            bool isPre;
            bool isInc;

            public PrePostIncDecExpression(Expression expressionRest, bool isPre, bool isInc)
            {
                this.expressionRest = expressionRest;
                this.isPre = isPre;
                this.isInc = isInc;
            }

            public CustomValue EvaluateExpression(Context context)
            {
                CustomValue value = CustomValue.FromNumber(1);
                Func<CustomValue, CustomValue> operation = existingValue => AddOrSubtract(existingValue, isInc ? Operator.Plus : Operator.Minus, value);

                var newValue = ApplyLValueOperation(expressionRest, operation, context, out var oldValue);

                return isPre ? newValue : oldValue;
            }
        }
        class AssignmentExpression : Expression
        {
            public Expression lValue;
            public string assignmentOperator;
            public Expression rValue;
            public AssignmentType assignmentType;

            public AssignmentExpression(Expression lValue, string assignmentOperator, IReadOnlyList<string> rValueTokens, AssignmentType assignmentType)
            {
                if (!assignmentSet.Contains(assignmentOperator))
                    throw new Exception();

                this.lValue = lValue;
                this.assignmentOperator = assignmentOperator;
                this.rValue = ExpressionMethods.New(rValueTokens);
                this.assignmentType = assignmentType;
            }

            public AssignmentExpression(string variableName, string assignmentOperator, IReadOnlyList<string> rValueTokens, AssignmentType assignmentType)
                : this(new SingleTokenVariableExpression(variableName), assignmentOperator, rValueTokens, assignmentType)
            {

            }

            public CustomValue EvaluateExpression(Context context)
            {
                if (assignmentType != AssignmentType.None)
                {
                    var variableName = ((SingleTokenVariableExpression)lValue).token;
                    if (assignmentOperator != "=")
                        throw new Exception();

                    var value = rValue.EvaluateExpression(context);

                    switch (assignmentType)
                    {
                        case AssignmentType.Var:
                            context.variableScope.AddVarVariable(variableName, value);
                            break;
                        case AssignmentType.Let:
                            context.variableScope.AddLetVariable(variableName, value);
                            break;
                        case AssignmentType.Const:
                            context.variableScope.AddConstVariable(variableName, value);
                            break;
                        default:
                            throw new Exception();
                    }

                    return value;
                }
                else
                {
                    var value = rValue.EvaluateExpression(context);

                    Func<CustomValue, CustomValue> operation;

                    switch (assignmentOperator)
                    {
                        case "=":
                            operation = existingValue => value;
                            break;
                        case "+=":
                            operation = existingValue => AddOrSubtract(existingValue, Operator.Plus, value);
                            break;
                        case "-=":
                            operation = existingValue => AddOrSubtract(existingValue, Operator.Minus, value);
                            break;
                        case "*=":
                            operation = existingValue => MultiplyOrDivide(existingValue, Operator.Multiply, value);
                            break;
                        case "/=":
                            operation = existingValue => MultiplyOrDivide(existingValue, Operator.Divide, value);
                            break;
                        case "%=":
                            operation = existingValue => MultiplyOrDivide(existingValue, Operator.Modulus, value);
                            break;
                        default:
                            throw new Exception();
                    }

                    return ApplyLValueOperation(lValue, operation, context, out var _);
                }
            }

            public static AssignmentExpression FromVarStatement(IReadOnlyList<string> tokens, AssignmentType assignmentType)
            {
                var variableName = tokens[0];
                return new AssignmentExpression(variableName, tokens[1], new CustomRange<string>(tokens, 2, tokens.Count), assignmentType);
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

            public CustomValue EvaluateExpression(Context context)
            {
                var conditionValue = conditionExpression.EvaluateExpression(context);
                bool isTruthy = conditionValue.IsTruthy();

                if (isTruthy)
                    return questionMarkExpression.EvaluateExpression(context);
                else
                    return colonExpression.EvaluateExpression(context);
            }
        }

        interface Statement
        {
            (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context);
            StatementType Type { get; }
        }
        static class StatementMethods
        {
            public static Statement New(IReadOnlyList<string> tokens)
            {
                if (tokens[0] == "{")
                {
                    if (tokens[tokens.Count - 1] != "}")
                        throw new Exception();
                    return new BlockStatement(tokens);
                }
                else if (tokens[0] == "while")
                {
                    return new WhileStatement(tokens);
                }
                else if (tokens[0] == "for")
                {
                    return ForStatement.FromTokens(tokens);
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
            internal static (IReadOnlyList<string>, IReadOnlyList<string>) GetTokensConditionAndBody(IReadOnlyList<string> tokens, int conditionStartIndex)
            {
                var endOfParentheses = tokens.IndexOfParenthesesEnd(conditionStartIndex);
                if (endOfParentheses < 0)
                    throw new Exception();
                var conditionTokens = new CustomRange<string>(tokens, conditionStartIndex, endOfParentheses);

                var statementTokens = new CustomRange<string>(tokens, endOfParentheses + 1, tokens.Count);

                return (conditionTokens, statementTokens);
            }
            internal static (Expression, Statement) GetConditionAndBody(IReadOnlyList<string> tokens, int conditionStartIndex)
            {
                var (conditionTokens, statementTokens) = GetTokensConditionAndBody(tokens, conditionStartIndex);
                var conditionExpression = ExpressionMethods.New(conditionTokens);
                var statement = StatementMethods.New(statementTokens);

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
                    tokens = new CustomRange<string>(tokens, 0, tokens.Count - 1);
                }

                this.tokens = tokens;
            }

            public StatementType Type => StatementType.LineStatement;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                if (tokens.Count == 0) return (CustomValue.Null, false, false, false);

                if (IsAssignmentType(tokens[0], out var assignmentType))
                {
                    // Assignment to new variable
                    var assignmentTree = AssignmentExpression.FromVarStatement(new CustomRange<string>(tokens, 1, tokens.Count), assignmentType);
                    var value = assignmentTree.EvaluateExpression(context);
                    return (CustomValue.Null, false, false, false);
                }
                else if (tokens[0] == "return")
                {
                    if (tokens.Count == 1)
                        return (CustomValue.Null, true, false, false);
                    var returnExpression = ExpressionMethods.New(new CustomRange<string>(tokens, 1, tokens.Count));
                    var returnValue = returnExpression.EvaluateExpression(context);
                    return (returnValue, true, false, false);
                }
                else if (tokens[0] == "break")
                {
                    return (CustomValue.Null, false, true, false);
                }
                else if (tokens[0] == "continue")
                {
                    return (CustomValue.Null, false, false, true);
                }
                else if (tokens[0] == "function")
                {
                    if (!IsVariableName(tokens[1]))
                        throw new Exception();

                    var variableName = tokens[1];
                    var functionStatement = FunctionStatement.FromTokens(tokens, isLambda: false);
                    var function = functionStatement.EvaluateExpression(context);

                    context.variableScope.AddVarVariable(variableName, function);
                    return (CustomValue.Null, false, false, false);
                }

                CustomValue expressionValue = GetValueFromExpression(tokens, context);
                if (hasSemiColon)
                    return (CustomValue.Null, false, false, false);
                return (expressionValue, false, false, false);
            }
        }
        class BlockStatement : Statement
        {
            List<Statement> statements;

            public BlockStatement(IReadOnlyList<string> tokens)
            {
                if (tokens[0] != "{")
                    throw new Exception();
                tokens = new CustomRange<string>(tokens, 1, tokens.Count - 1);
                var statementRanges = GetStatementRanges(tokens);
                statements = statementRanges.Select(range => StatementMethods.New(range)).ToList();
            }

            public StatementType Type => StatementType.BlockStatement;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                var newScope = VariableScope.NewWithInner(context.variableScope, isFunctionScope: false);
                var newContext = new Context(newScope, context.thisOwner);
                foreach (var statement in statements)
                {
                    var (value, isReturn, isBreak, isContinue) = statement.EvaluateStatement(newContext);
                    if (isReturn)
                        return (value, true, false, false);
                    if (isBreak)
                        return (CustomValue.Null, false, true, false);
                    if (isContinue)
                        return (CustomValue.Null, false, false, true);
                }
                return (CustomValue.Null, false, false, false);
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

            public StatementType Type => StatementType.WhileStatement;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                while (true)
                {
                    var conditionValue = conditionExpression.EvaluateExpression(context);
                    if (!conditionValue.IsTruthy())
                        break;

                    var (value, isReturn, isBreak, isContinue) = statement.EvaluateStatement(context);
                    if (isReturn)
                        return (value, true, false, false);
                    if (isBreak)
                        break;
                    if (isContinue)
                        continue;
                }

                return (CustomValue.Null, false, false, false);
            }
        }
        class ForStatement : Statement
        {
            AssignmentType assignmentType;
            IReadOnlyList<Expression> initializationStatements;
            Expression conditionExpression;
            IReadOnlyList<Statement> iterationStatements;
            Statement bodyStatement;

            private ForStatement(AssignmentType assignmentType, IReadOnlyList<Expression> initializationStatements, Expression conditionExpression, IReadOnlyList<Statement> iterationStatements, Statement bodyStatement)
            {
                this.assignmentType = assignmentType;
                this.initializationStatements = initializationStatements;
                this.conditionExpression = conditionExpression;
                this.iterationStatements = iterationStatements;
                this.bodyStatement = bodyStatement;
            }

            public StatementType Type => StatementType.ForStatement;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                var newScope = VariableScope.NewWithInner(context.variableScope, isFunctionScope: false);
                var newContext = new Context(newScope, context.thisOwner);

                // Initialize
                foreach (var initializationStatement in initializationStatements)
                {
                    initializationStatement.EvaluateExpression(newContext);
                }

                while (true)
                {
                    var conditionValue = conditionExpression.EvaluateExpression(newContext);
                    if (!conditionValue.IsTruthy())
                        break;

                    var loopScope = VariableScope.GetNewLoopScopeCopy(newScope, assignmentType);
                    var loopContext = new Context(loopScope, newContext.thisOwner);

                    var (value, isReturn, isBreak, isContinue) = bodyStatement.EvaluateStatement(loopContext);
                    if (isReturn)
                        return (value, true, false, false);
                    if (isBreak)
                        break;
                    if (isContinue)
                    {
                        DoIteration(newContext);
                        continue;
                    }

                    DoIteration(newContext);
                }

                return (CustomValue.Null, false, false, false);
            }

            private void DoIteration(Context context)
            {
                foreach (var iterationStatement in iterationStatements)
                {
                    iterationStatement.EvaluateStatement(context);
                }
            }

            public static Statement FromTokens(IReadOnlyList<string> tokens)
            {
                if (tokens[1] != "(")
                    throw new Exception();
                var conditionStartIndex = 2;
                var (expressionTokens, statementTokens) = StatementMethods.GetTokensConditionAndBody(tokens, conditionStartIndex);
                var expressions = SplitBy(expressionTokens, semicolonSet).ToList();
                if (expressions.Count == 3)
                {
                    // Normal for loop
                    var allInitializationTokens = expressions[0];
                    var isNewAssignment = IsAssignmentType(allInitializationTokens[0], out var assignmentType);
                    var assignmentTokens = isNewAssignment ? new CustomRange<string>(allInitializationTokens, 1, allInitializationTokens.Count) : allInitializationTokens;
                    var initializationTokenGroup = SplitBy(assignmentTokens, commaSet).ToList();
                    var initializationStatements = initializationTokenGroup.SelectFast(x => AssignmentExpression.FromVarStatement(x, assignmentType));

                    var conditionTokens = expressions[1];
                    var conditionExpression = conditionTokens.Count > 0 ? ExpressionMethods.New(conditionTokens) : trueExpression;

                    var allIterationTokens = expressions[2];
                    var iterationTokenGroup = SplitBy(allIterationTokens, commaSet).ToList();
                    var iterationStatements = iterationTokenGroup.SelectFast(x => StatementMethods.New(x));

                    var bodyStatement = StatementMethods.New(statementTokens);

                    return new ForStatement(assignmentType, initializationStatements, conditionExpression, iterationStatements, bodyStatement);
                }
                else if (expressions.Count == 1)
                {
                    var parenthesesTokens = expressions[0];
                    var isNewAssignment = IsAssignmentType(parenthesesTokens[0], out var assignmentType);
                    var index = isNewAssignment ? 1 : 0;
                    var variableName = parenthesesTokens[index];
                    var operationType = parenthesesTokens[index + 1];
                    var restTokens = new CustomRange<string>(parenthesesTokens, index + 2, parenthesesTokens.Count);
                    var restExpression = ExpressionMethods.New(restTokens);
                    var bodyStatement = StatementMethods.New(statementTokens);

                    if (operationType == "in")
                    {
                        return new ForInOfStatement(true, assignmentType, variableName, restExpression, bodyStatement);
                    }
                    else if (operationType == "of")
                    {
                        return new ForInOfStatement(false, assignmentType, variableName, restExpression, bodyStatement);
                    }
                    throw new Exception();
                }
                throw new Exception();
            }
        }
        class ForInOfStatement : Statement
        {
            AssignmentType assignmentType;
            string variableName;
            Expression sourceExpression;
            Statement bodyStatement;
            bool isInStatement;

            public ForInOfStatement(bool isInStatement, AssignmentType assignmentType, string variableName, Expression sourceExpression, Statement bodyStatement)
            {
                this.assignmentType = assignmentType;
                this.variableName = variableName;
                this.sourceExpression = sourceExpression;
                this.bodyStatement = bodyStatement;
                this.isInStatement = isInStatement;
            }

            public StatementType Type => StatementType.ForInOfStatement;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                bool isOfStatement = !isInStatement;
                var scope = context.variableScope;
                var sourceValue = sourceExpression.EvaluateExpression(context);
                if (isOfStatement && sourceValue.type != ValueType.Array)
                    throw new Exception();
                if (isInStatement && sourceValue.type != ValueType.Map)
                    throw new Exception();

                IEnumerable<CustomValue> elements;
                if (isInStatement)
                {
                    var map = (Dictionary<string, CustomValue>)sourceValue.value;
                    var keys = map.Keys;
                    elements = keys.Select(key => CustomValue.FromParsedString(key));
                }
                else if (isOfStatement)
                {
                    var array = (CustomArray)sourceValue.value;
                    elements = array.list;
                }
                else
                    throw new Exception();

                foreach (var element in elements)
                {
                    var loopScope = VariableScope.GetNewLoopScope(scope, assignmentType, variableName, element);

                    var (value, isReturn, isBreak, isContinue) = bodyStatement.EvaluateStatement(new Context(loopScope, context.thisOwner));
                    if (isReturn)
                        return (value, true, false, false);
                    if (isBreak)
                        break;
                    if (isContinue)
                        continue;
                }

                return (CustomValue.Null, false, false, false);
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

            public StatementType Type => StatementType.IfStatement;

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

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                var conditionValue = conditionExpression.EvaluateExpression(context);
                if (conditionValue.IsTruthy())
                {
                    var (value, isReturn, isBreak, isContinue) = statementOfIf.EvaluateStatement(context);
                    if (isReturn)
                        return (value, true, false, false);
                    return (CustomValue.Null, false, isBreak, isContinue);
                }

                foreach (var elseIfStatement in elseIfStatements.Value)
                {
                    var elseIfCondition = elseIfStatement.condition.EvaluateExpression(context);
                    if (elseIfCondition.IsTruthy())
                    {
                        var (value, isReturn, isBreak, isContinue) = elseIfStatement.statement.EvaluateStatement(context);
                        if (isReturn)
                            return (value, true, false, false);
                        return (CustomValue.Null, false, isBreak, isContinue);
                    }
                }

                if (elseStatement != null)
                {
                    var (value, isReturn, isBreak, isContinue) = elseStatement.EvaluateStatement(context);
                    if (isReturn)
                        return (value, true, false, false);
                    return (CustomValue.Null, false, isBreak, isContinue);
                }

                return (CustomValue.Null, false, false, false);
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

            public StatementType Type => StatementType.ElseIfStatement;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                return statement.EvaluateStatement(context);
            }
        }
        class ElseStatement : Statement
        {
            Statement statement;

            public ElseStatement(IReadOnlyList<string> tokens)
            {
                if (tokens[0] != "else")
                    throw new Exception();
                statement = StatementMethods.New(new CustomRange<string>(tokens, 1, tokens.Count));
            }

            public StatementType Type => StatementType.ElseStatement;

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                return statement.EvaluateStatement(context);
            }
        }
        class FunctionStatement : Statement, Expression
        {
            List<(string paramName, bool isRest)> parametersList;
            Statement body;
            bool isLambda;

            private FunctionStatement(IReadOnlyList<string> parameters, Statement body, bool isLambda)
            {
                if (parameters.Count > 0 && parameters[0] == "(")
                    throw new Exception();
                this.body = body;
                this.isLambda = isLambda;

                // Prepare parameters
                var parametersSet = new HashSet<string>(); // Set is used to ensure uniqueness
                this.parametersList = new List<(string, bool)>();

                var parameterGroups = SplitBy(parameters, commaSet).ToList();
                for (int parameterIndex = 0; parameterIndex < parameterGroups.Count; parameterIndex++)
                {
                    var parameterGroup = parameterGroups[parameterIndex];

                    string parameter;
                    bool isRest;
                    if (parameterGroup.Count == 1)
                    {
                        parameter = parameterGroup[0];
                        isRest = false;
                    }
                    else if (parameterGroup.Count == 2 && parameterGroup[0] == "...")
                    {
                        parameter = parameterGroup[1];
                        isRest = true;
                        if (parameterIndex != parameterGroups.Count - 1)
                            throw new Exception();
                    }
                    else
                        throw new Exception();
                    if (!parametersSet.Add(parameter))
                        throw new Exception();
                    this.parametersList.Add((parameter, isRest));
                }
            }

            public StatementType Type => StatementType.FunctionStatement;

            public static FunctionStatement FromParametersAndBody(IReadOnlyList<string> parameterTokens, IReadOnlyList<string> bodyTokens, bool isLambda)
            {
                var body = StatementMethods.New(bodyTokens);
                return new FunctionStatement(parameterTokens, body, isLambda);
            }

            public static FunctionStatement FromTokens(IReadOnlyList<string> tokens, bool isLambda)
            {
                var parenthesesIndex = tokens.IndexOf("(", 0);
                var (parameters, bodyTokens) = StatementMethods.GetTokensConditionAndBody(tokens, parenthesesIndex + 1);
                var body = StatementMethods.New(bodyTokens);
                return new FunctionStatement(parameters, body, isLambda);
            }

            public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
            {
                var function = new CustomFunction(parametersList, body, context.variableScope, isLambda);
                return (CustomValue.FromFunction(function), false, false, false);
            }

            public CustomValue EvaluateExpression(Context context)
            {
                return EvaluateStatement(context).Item1;
            }
        }
    }
}

static class InterpreterExtensions
{
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
        return IndexOfPairsEnd(source, startIndex, "(", ")");
    }

    public static int IndexOfBracesEnd(this IReadOnlyList<string> source, int startIndex)
    {
        return IndexOfPairsEnd(source, startIndex, "{", "}");
    }

    public static int IndexOfBracesEnd(this string source, int startIndex)
    {
        return IndexOfPairsEnd(source, startIndex, '{', '}');
    }

    public static int IndexOfBracketsEnd(this IReadOnlyList<string> source, int startIndex)
    {
        return IndexOfPairsEnd(source, startIndex, "[", "]");
    }

    private static int IndexOfPairsEnd(this IReadOnlyList<string> source, int startIndex, string first, string last)
    {
        int count = 0;
        for (int i = startIndex; i < source.Count; i++)
        {
            string currentElement = source[i];
            if (currentElement == last)
            {
                if (count == 0)
                    return i;

                count--;
                if (count < 0)
                    throw new Exception();
            }
            else if (currentElement == first)
            {
                count++;
            }
        }
        return -1;
    }

    private static int IndexOfPairsEnd(this string source, int startIndex, char first, char last)
    {
        int count = 0;
        for (int i = startIndex; i < source.Length; i++)
        {
            char currentElement = source[i];
            if (currentElement == last)
            {
                if (count == 0)
                    return i;

                count--;
                if (count < 0)
                    throw new Exception();
            }
            else if (currentElement == first)
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
        if (result == null)
            return expected == null;
        if (expected == null)
            return false;

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