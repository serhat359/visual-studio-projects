using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualConsoleCore.Interpreter;

public class Interpreter
{
    private static readonly HashSet<char> onlyChars = new HashSet<char>() { '(', ')', ',', ';', '{', '}', '[', ']' };
    private static readonly string[] onlyCharStrings;
    private static readonly HashSet<string> operators = new HashSet<string>() { "+", "-", "*", "/", "%", "=", "?", ":", "<", ">", "<=", ">=", "&&", "||", "??", "!", "!=", ".", "==", "+=", "-=", "*=", "/=", "%=", "??=", "||=", "&&=", "=>", "++", "--", "...", "?.", "?.[", "?.(" };
    private static readonly HashSet<string> assignmentSet = new HashSet<string>() { "=", "+=", "-=", "*=", "/=", "%=", "&&=", "||=", "??=" };
    private static readonly HashSet<string> regularOperatorSet = new HashSet<string>() { "+", "-", "*", "/", "%", "==", "!=", "<", ">", "<=", ">=", "&&", "||", "??" };
    private static readonly HashSet<string> keywords = new HashSet<string>() { "this", "var", "let", "const", "if", "else", "while", "for", "break", "continue", "function", "class", "async", "await", "return", "yield", "true", "false", "null", "new" };
    private static readonly Dictionary<char, Dictionary<char, HashSet<char>>> operatorsCompiled;
    private static readonly Dictionary<char, int> hexToint = new Dictionary<char, int>() { { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 }, { '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 }, { 'A', 10 }, { 'B', 11 }, { 'C', 12 }, { 'D', 13 }, { 'E', 14 }, { 'F', 15 }, { 'a', 10 }, { 'b', 11 }, { 'c', 12 }, { 'd', 13 }, { 'e', 14 }, { 'f', 15 }, };

    private static readonly Expression trueExpression;
    private static readonly Expression falseExpression;
    private static readonly Expression nullExpression;
    private static readonly Expression thisExpression;

    static Interpreter()
    {
        trueExpression = new CustomValueExpression(CustomValue.True);
        falseExpression = new CustomValueExpression(CustomValue.False);
        nullExpression = new CustomValueExpression(CustomValue.Null);
        thisExpression = new ThisExpression();

        operatorsCompiled = operators.GroupBy(x => x[0]).ToDictionary(x => x.Key, x => x.Where(y => y.Length > 1).GroupBy(y => y[1]).ToDictionary(y => y.Key, y => y.Where(z => z.Length > 2).Select(z => z[2]).ToHashSet()));

        // Setup onlyCharStrings
        onlyCharStrings = new string[128];
        foreach (var c in onlyChars)
        {
            onlyCharStrings[c] = c.ToString();
        }
    }

    private Context defaultContext;

    public Interpreter()
    {
        var defaultvariables = new Dictionary<string, (CustomValue, AssignmentType)>();
        var defaultVariableScope = VariableScope.NewDefault(defaultvariables, true);
        var defaultThisOwner = CustomValue.Null;
        defaultContext = new Context(defaultVariableScope, defaultThisOwner);

        CustomValue printFunctionCustomValue = CustomValue.FromFunction(new PrintFunction());
        defaultvariables["console"] = (CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>()
        {
            { "log", printFunctionCustomValue },
        }), AssignmentType.Const);
        defaultvariables["print"] = (printFunctionCustomValue, AssignmentType.Const);

        defaultvariables["Math"] = (CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>
        {
            { "random", CustomValue.FromFunction(new MathRandomFunction()) },
            { "floor", CustomValue.FromFunction(new MathFloorFunction()) }
        }), AssignmentType.Const);

        defaultvariables["Array"] = (CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter> {
            { "prototype", CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>()
                {
                    { "push", CustomValue.FromFunction(new ArrayPushFunction()) },
                    { "pop", CustomValue.FromFunction(new ArrayPopFunction()) },
                    { "map", CustomValue.FromFunction(new ArrayMapFunction()) },
                    { "filter", CustomValue.FromFunction(new ArrayFilterFunction()) },
                    { "join", CustomValue.FromFunction(new ArrayJoinFunction()) },
                })
            },
        }), AssignmentType.Const);

        defaultvariables["Function"] = (CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter> {
            { "prototype", CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>()
                {
                    { "call", CustomValue.FromFunction(new FunctionCallFunction()) },
                    { "apply", CustomValue.FromFunction(new FunctionApplyFunction()) },
                })
            },
        }), AssignmentType.Const);

        defaultvariables["String"] = (CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter> {
            { "prototype", CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>()
                {
                    { "charAt", CustomValue.FromFunction(new CharAtFunction()) },
                    { "charCodeAt", CustomValue.FromFunction(new CharCodeAtFunction()) },
                })
            },
        }), AssignmentType.Const);

        defaultvariables["Generator"] = (CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter> {
            { "prototype", CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>()
                {
                    { "next", CustomValue.FromFunction(new GeneratorNextFunction()) },
                })
            },
        }), AssignmentType.Const);

        defaultvariables["AsyncGenerator"] = (CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter> {
            { "prototype", CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>()
                {
                    { "next", CustomValue.FromFunction(new AsyncGeneratorNextFunction()) },
                })
            },
        }), AssignmentType.Const);

        defaultvariables["Proxy"] = (CustomValue.FromClass(new ProxyClassObject()), AssignmentType.Const);
    }

    public object InterpretCode(string code)
    {
        var tokens = GetTokens(code, 16);

        CustomValue value = CustomValue.Null;
        bool isReturn;
        bool isBreak;
        bool isContinue;

        var statements = GetStatements(tokens.ToSegment());

        for (int i = 0; i < statements.count; i++)
        {
            (value, isReturn, isBreak, isContinue) = statements.array[i].EvaluateStatement(defaultContext);
            if (isReturn || isBreak || isContinue)
                throw new Exception();
        }

        return value.value;
    }

    public static void Test(bool verbose = true)
    {
        CustomValue.Test();

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
        var testCases = new List<(string code, object? value)>()
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
            ("var aaa;", null),
            ("aaa == null", true),
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
            ("arr1[4]", null),
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
            ("arr3 = [7,6,7]; arr3.length", 3),
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
            ("`$${2}`", "$2"),
            ("`\\$${2}`", "$2"),
            ("`${2+3}`", "5"),
            ("`${(2+3)}`", "5"),
            ("`\n`", "\n"), // Allow new lines for backtick strings
            ("`hello ${'world'}`", "hello world"),
            ("`hello ${`world`}`", "hello world"),
            ("`hello ${`w${`orld`}`}`", "hello world"),
            ("`${`}` + `}`}`", "}}"),
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
            ("print()", null),
            ("[...[1,2,3]].length", 3),
            ("var arr = [2, ...[1,2,3]]; arr[0] == 2 && arr[1] == 1 && arr[2] == 2 && arr[3] == 3", true),
            ("var arr = [1,2,3]; var arr2 = [...arr]; arr.push(6); arr2.length", 3),
            ("(function(){ var total = 0; for(var x of arguments) total += x; return total; })(1,2,3,4)", 10),
            ("(function(){ var total = 0; for(var x of arguments) total += x; return total; })(2, ...[1,2,3,4,5], 1)", 18),
            ("(function(x,y,...rest){ var total = 0; for(var x of rest) total+=x; return total })(1,2,3,4,5)", 12),
            ("var o = { age: 26 }; -o.age", -26),
            ("var o = { age: 27 }; -o[`age`]", -27),
            ("-(x => x)(2)", -2),
            ("(function(){ return null; })() == null", true),
            ("(async function(){ return null; })() == null", false),
            ("(() => null)() == null", true),
            ("(async () => null)() == null", false),
            ("(async x => null)() == null", false),
            ("(async (x) => null)() == null", false),
            ("(async (x,y) => null)() == null", false),
            ("(() => null) == null", false),
            ("(x => null) == null", false),
            ("((x,y) => null) == null", false),
            ("(async () => null) == null", false),
            ("(async x => null) == null", false),
            ("(async (x,y) => null) == null", false),
            ("var asyncFunc1 = async x => x; var prom1 = asyncFunc1(2); await prom1", 2),
            ("await (async x => x)(2)", 2),
            ("await 2", 2),
            ("await await 2", 2),
            ("var { name1, age1 } = { name1:'serhat', age1: 25 }; name1 == 'serhat' && age1 == 25", true),
            ("var [ n1, n2, n3, n4, n5 ] = [ 5,6,7 ]; n1 == 5 && n2 == 6 && n3 == 7 && n4 == null && n5 == null", true),
            ("(()=>-2)()", -2),
            ("var ff = ()=> { var elseif1 = 1; if(false) elseif1 = 10; else if (false) elseif1 = 11; else if (true) elseif1 = 12; else elseif1 = 13; return elseif1; }; ff()", 12),
            ("var o = null;", null),
            ("o?.name", null),
            ("o?.['name']", null),
            ("o?.name?.name", null),
            ("o?.['name']?.['name']", null),
            ("o?.name.name", null),
            ("o?.['name']['name']", null),
            ("o?.name.name()", null),
            ("o?.['name']['name']()", null),
            ("o?.name.name().name", null),
            ("o?.['name']['name']()['name']", null),
            ("o?.[2]", null),
            ("o = [1,2,3]; o?.[2]", 3),
            ("o = { name: 'Serhat' }; o.name", "Serhat"),
            ("o?.name", "Serhat"),
            ("o = null;", null),
            ("o?.()", null),
            ("o?.name()", null),
            ("o?.name?.()", null),
            ("o = {}; o.name?.()", null),
            ("var potentiallyNullObj = null; var x = 0; var prop = potentiallyNullObj?.[x++]; x", 0),
            ("var f = x => `asd${x}asd`; var arr = [f(1),f(2),f(3)]; arr[0] == 'asd1asd' && arr[1] == 'asd2asd' && arr[2] == 'asd3asd'", true),
            ("Array.prototype.popTwice = function(){ this.pop(); this.pop(); }; var arr = [1,2,3]; arr.popTwice(); arr.length", 1),
            ("Array.prototype.pushTwice = function(x){ this.push(x); this.push(x); }; var arr = [1,2,3]; arr.pushTwice(9); arr.length == 5 && arr[3] == 9 && arr[4] == 9", true),
            ("var f = function(x, y, z){ return this.name + (x + y); }; var o = { name: 'Serhat' }; f.call(o, 1, 2)", "Serhat3"),
            ("var f = function(x, y, z){ return this.name + (x + y); }; var o = { name: 'Serhat' }; f.apply(o, [1, 2])", "Serhat3"),
            ("'hello'.charAt(0)", "h"),
            ("'hello'.charAt(2)", "l"),
            ("var o = { name: 'Serhat', age: 30 }; var name; var age; ({ name, age } = o); name == 'Serhat' && age == 30", true),
            ("var xx1; var xx2; ([xx1, xx2] = [5,6]); xx1 == 5 && xx2 == 6", true),
            ("var o1 = { i1: 1 }; var o2 = { i2: 2, i3: 3 }; var o3 = { ...o1, ...o2 }; o3.i1 == 1 && o3.i2 == 2 && o3.i3 == 3", true),
            ("({ key: 1, key: 2 }).key", 2),
            ("var o = { name: \"Serhat\", age: 30 }; var { age: agenew } = o; agenew", 30),
            ("function f(x,x,x){ return x; } ", null),
            ("f(1,2,3)", 3),
            ("var o = { name: 'Serhat', getName(){ return this.name; } }; o.getName()", "Serhat"),
            ("var o = { async n(){ return 2; } }; await o.n()", 2),
            ("function* f(){};", null),
            ("var emptyNext = (function*(){})().next(); emptyNext.value == null && emptyNext.done == true", true),
            ("var numbersGen = (function*(){ yield 1; yield 2; })(); numbersGen != true", true),
            ("var { value, done } = numbersGen.next(); value == 1 && done == false", true),
            ("var { value, done } = numbersGen.next(); value == 2 && done == false", true),
            ("var { value, done } = numbersGen.next(); value == null && done == true", true),
            ("var gen = function*(){ for(var i = 0; i < arguments.length; i++) {  yield arguments[i]; yield arguments[i]; } }; var genFirst = gen(1,2,3).next(); genFirst.value == 1", true),
            ("function* f(){ yield 1; yield 2; } var arr = []; for(var x of f()) arr.push(x); arr.length", 2),
            ("function* f(){ yield 1; yield 2; } [...f()].length", 2),
            ("function* f(){ yield 1; yield 2; } function c(){ return arguments.length; } c(...f()) ", 2),
            ("function* f(){ yield 1; yield 2; } function* c(){ yield* f(); yield* [1,2,3]; } [...c()].length", 5),
            ("var f = async function*(){ yield 1; yield 2 }; var gen = f(); var val = gen.next(); (await val).value == 1", true),
            ("var arr = []; for await (let x of f()) arr.push(x); arr.length", 2),
            ("var arr = []; for(let i = 0; i < 10; i++){ i++; arr.push(i); } arr.length", 5),
            ("async function* asd1(){ yield 1; yield 2; yield 3; }; async function* asd2(){ yield 1; for await (let x of asd1()) yield x; }; var arr = []; for await (let x of asd2()) arr.push(x); arr.length", 4),
            ("\"\\u0041\"", "A"),
            ("\"\\u0041\\u0041\\u0041\"", "AAA"),
            ("'\\u0041\\u0041\\u0041'", "AAA"),
            ("`\\u0041\\u0041\\u0041`", "AAA"),
            ("\"\\u003C/script\\u003E\"", "</script>"),
            ("\"\\uD83D\\uDC4C\"", "👌"),
            ("var o = { get val(){ return 2; } }; o.val", 2),
            ("var o = { name:'Serhat', get gname(){ return this.name; } }; o.gname", "Serhat"),
            ("var x = 2; var o = { set val(value){ x = value; } }; o.val = 10; x", 10),
            ("var x = 2; var o = { set val(value){ x = value; }, get val(){ return x; } }; x = 25; o.val", 25),
            ("(function(x,){return x})(12,)", 12),
            ("var { name, } = { name: 'Serhat', }; name", "Serhat"),
            ("var [x,] = [12,]; x", 12),
            ("(function(a,b,...rest){ return rest.length })()", 0),
            ("class Rectangle { constructor(height, width) { this.height = height; this.width = width; } get area() { return this.calcArea(); } calcArea() { return this.height * this.width; } } new Rectangle(10,20).height", 10),
            ("new Rectangle(10,20).area", 200),
            ("new Rectangle(10,20).calcArea()", 200),
            ("Rectangle != null", true),
            ("Rectangle.prototype.calcArea != null", true),
            ("Rectangle.prototype.isSquare = function(){ return this.width == this.height; }; new Rectangle(20,20).isSquare()", true),
            ("var r = new Rectangle(10,20); r.calcArea = 'custom'; r.calcArea", "custom"),
            ("var proxy = new Proxy([], {}); proxy.push(12); proxy.length", 1),
            ("var proxy = new Proxy({}, {}); proxy.name = 'Serhat'; proxy.name", "Serhat"),
            ("var proxy = new Proxy({ name(){ return 'Serhat' } }, {}); proxy.name()", "Serhat"),
            ("var proxy = new Proxy({}, { get(obj, prop){ return prop } }); proxy.customProp", "customProp"),
            ("var proxy = new Proxy({ name: 'Serhat' }, { get(obj, prop){ return obj[prop] ?? '23' } }); proxy.name", "Serhat"),
            ("proxy.customProp", "23"),
            ("var proxy = new Proxy({ name: 'Serhat', age: 23 }, { set(obj, prop, value){ if(prop == 'age' && value < 20) obj[prop] = 20; else obj[prop] = value } }); proxy.name = 'new name'", "new name"),
            ("proxy.age = 30", 30),
            ("proxy.age", 30),
            ("proxy.age = 15", 15),
            ("proxy.age", 20),
            ("'A'.charCodeAt(0)", 65),
            ("'Aa'.charCodeAt(1)", 97),
            ("Math.floor(3.2)", 3),
            ("Math.floor(-3.2)", -4),
            ("var ran = Math.random(); ran >=0 && ran < 1", true),
            ("'text'.length", 4),
            ("'hello'.length", 5),
            ("var total = 0; for (let c of 'hello') total++; total", 5),
            ("var k = null; k ??= 2; k", 2),
            ("var k = 2; var n = null; k ??= n=2; n", null), // Checking optimization
            ("var k = 0; k ||= 2; k", 2),
            ("var k = 2; var n = null; k ||= n=2; n", null), // Checking optimization
            ("var k = 0; k &&= 2; k", 0),
            ("var k = ''; k &&= 2; k", ""),
            ("var k = 'hello'; k &&= 2; k", 2),
            ("var k = ''; var n = null; k &&= n=2; n", null), // Checking optimization
            ("var o = { name:'thisName', getName(){ return (() => this.name)(); } }; o.getName()", "thisName"),
            ("var [x, ...y] = [1,2,3,4]; y.length", 3),
            ("[1,2,3].map(x => x + 1).length", 3),
            ("[1,2,3].filter(x => x < 3).length", 2),
            ("var sum = function(arr){ let total = 0; for (let x of arr) total += x; return total }; sum([1,2,3].map(x => x + 1))", 9),
            ("var arr = [1,2,3]; arr.map = null; arr.map", null),
            ("[1,2,3].join(',')", "1,2,3"),
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
            "var a = null; this.a",
            "(function(){ return this.a })()",
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
            "var a =0 ;(++a)++",
            "var a =0 ;(a++)++",
            "function(){}",
            "x,y => {}",
            "x => ",
            "...[1,2,3]",
            "function(x,,x2){}",
            "function(x,x,x){}",
            "(function(...rest1, rest2){})",
            "(function(...rest1, ...rest2){})",
            "var async = 1;",
            "async function(){}",
            "async",
            "async 2",
            "async ()",
            "var i = 12; var o = null; o.i",
            "function vvv() print('hello')",
            "return 1;",
            "{ return 1; }",
            "break;",
            "{ break; }",
            "continue;",
            "{ continue; }",
            "else if(true) { ; }",
            "else { ; }",
            "if(false) {} else {} else {}",
            "if(false) {} else {} else if(true) {}",
            "var a = null; a.name?.length",
            "var o = { val: 2, get val(){} }",
            "var o = { val: 2, set val(x){} }",
            "var o = { get val(){}, get val(){} }",
            "var o = { set val(){}, set val(){} }",
            "(function(,){})",
            "(function(12,,){})",
            "(function(){})(,)",
            "(function(){})(12,,)",
            "({ , })",
            "({ age:2 ,, })",
            "[,]",
            "[12,,]",
            "var {x,,} = { age:2 }",
            "class X{} class X{} ",
            "true + true",
        };
        var interpreter = new Interpreter();
        foreach (var code in testCases)
        {
            try
            {
                var result = interpreter.InterpretCode(code);
            }
            catch (Exception)
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

    private static Slice<Statement> GetStatements(ArraySegment<string> tokens)
    {
        var statements = new Slice<Statement>(3);
        int index = 0;
        while (index < tokens.Count)
        {
            var (newIndex, statement) = GetStatement(tokens, index, expectingElse: false);
            if (newIndex <= index)
                throw new Exception();
            statements.Add(statement);
            index = newIndex;
        }
        return statements;
    }

    private static (int, Statement) GetStatement(ArraySegment<string> tokens, int startingIndex, bool expectingElse)
    {
        var token = tokens[startingIndex];

        switch (token)
        {
            case "{":
                {
                    var newIndex = tokens.IndexOfBracesEnd(startingIndex + 1);
                    if (newIndex < 0)
                        throw new Exception();
                    return (newIndex + 1, new BlockStatement(tokens[startingIndex..(newIndex + 1)]));
                }
            case "function":
                {
                    var paranBegin = tokens.IndexOf("(", startingIndex);
                    if (paranBegin < 0)
                        throw new Exception();
                    var paranEnd = tokens.IndexOfParenthesesEnd(paranBegin + 1);
                    if (paranEnd < 0)
                        throw new Exception();
                    var braceBegin = tokens.IndexOf("{", paranEnd + 1);
                    if (braceBegin < 0)
                        throw new Exception();
                    var braceEnd = tokens.IndexOfBracesEnd(braceBegin + 1);
                    if (braceEnd < 0)
                        throw new Exception();
                    return (braceEnd + 1, new LineStatement(tokens[startingIndex..(braceEnd + 1)]));
                }
            case "class":
                {
                    var braceBegin = tokens.IndexOf("{", startingIndex);
                    if (braceBegin < 0)
                        throw new Exception();
                    var braceEnd = tokens.IndexOfBracesEnd(braceBegin + 1);
                    if (braceEnd < 0)
                        throw new Exception();
                    var classtokens = tokens[startingIndex..(braceEnd + 1)];
                    return (braceEnd + 1, new LineStatement(tokens[startingIndex..(braceEnd + 1)]));
                }
            case "if":
                {
                    var paranBegin = tokens.IndexOf("(", startingIndex);
                    if (paranBegin < 0)
                        throw new Exception();
                    var paranEnd = tokens.IndexOfParenthesesEnd(paranBegin + 1);
                    if (paranEnd < 0)
                        throw new Exception();

                    var (subEnd, subStatement) = GetStatement(tokens, paranEnd + 1, expectingElse: false);
                    var condition = ExpressionMethods.New(tokens[(paranBegin + 1)..paranEnd]);
                    var ifStatement = new IfStatement(condition, subStatement);
                    while (subEnd < tokens.Count && tokens[subEnd] == "else")
                    {
                        bool isElseIf = tokens[subEnd + 1] == "if";
                        if (isElseIf)
                        {
                            var (elseIfEnd, elseIfStatement) = GetStatement(tokens, subEnd, expectingElse: true);
                            ifStatement.AddElseIf(elseIfStatement);
                            subEnd = elseIfEnd;
                            continue;
                        }
                        else
                        {
                            var (elseEnd, elseStatement) = GetStatement(tokens, subEnd + 1, expectingElse: true);
                            ifStatement.SetElse(elseStatement);
                            subEnd = elseEnd;
                            break;
                        }
                    }
                    return (subEnd, ifStatement);
                }
            case "while":
                {
                    var paranBegin = tokens.IndexOf("(", startingIndex);
                    if (paranBegin < 0)
                        throw new Exception();
                    var paranEnd = tokens.IndexOfParenthesesEnd(paranBegin + 1);
                    if (paranEnd < 0)
                        throw new Exception();

                    var (subEnd, subStatement) = GetStatement(tokens, paranEnd + 1, expectingElse: false);
                    var condition = ExpressionMethods.New(tokens[(paranBegin + 1)..paranEnd]);
                    var whileStatement = new WhileStatement(condition, subStatement);
                    return (subEnd, whileStatement);
                }
            case "for":
                {
                    var paranBegin = tokens.IndexOf("(", startingIndex);
                    if (paranBegin < 0)
                        throw new Exception();
                    var paranEnd = tokens.IndexOfParenthesesEnd(paranBegin + 1);
                    if (paranEnd < 0)
                        throw new Exception();

                    var (subEnd, subStatement) = GetStatement(tokens, paranEnd + 1, expectingElse: false);
                    var forStatement = ForStatement.FromTokens(tokens[startingIndex..(paranEnd + 1)], subStatement);
                    return (subEnd, forStatement);
                }
        }

        if (token == "else" && !expectingElse)
            throw new Exception();

        int firstIndex = startingIndex;
        while (true)
        {
            if (startingIndex == tokens.Count)
                return (startingIndex, StatementMethods.New(tokens[firstIndex..startingIndex]));
            if (tokens[startingIndex] == ";")
                return (startingIndex + 1, StatementMethods.New(tokens[firstIndex..(startingIndex + 1)]));
            if (tokens[startingIndex] == "(")
            {
                startingIndex = tokens.IndexOfParenthesesEnd(startingIndex + 1);
                if (startingIndex < 0)
                    throw new Exception();
                continue;
            }
            if (tokens[startingIndex] == "{")
            {
                startingIndex = tokens.IndexOfBracesEnd(startingIndex + 1);
                if (startingIndex < 0)
                    throw new Exception();
                continue;
            }
            startingIndex++;
        }
    }

    private static IEnumerable<ArraySegment<string>> SplitBy(ArraySegment<string> tokens, string separator, bool allowTrailing)
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

            if (parenthesesCount == 0 && bracketsCount == 0 && bracesCount == 0 && separator == token)
            {
                yield return tokens[index..i];
                index = i + 1;
            }
        }
        if (!allowTrailing || index < tokens.Count)
        {
            yield return tokens[index..];
        }
    }

    private static CustomValue CallFunction(FunctionObject function, List<CustomValue> arguments, CustomValue thisOwner)
    {
        var functionParameterArguments = new Dictionary<string, (CustomValue, AssignmentType)>();
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            var (argName, isRest) = function.Parameters[i];
            if (isRest)
            {
                var restArrayCount = arguments.Count - i;
                if (restArrayCount < 0)
                    restArrayCount = 0;
                var restArray = new List<CustomValue>(restArrayCount);
                for (int j = 0; j < restArrayCount; j++)
                {
                    restArray.Add(arguments[i + j]);
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
        if (!function.IsLambda)
            functionParameterArguments["arguments"] = (CustomValue.FromArray(new CustomArray(arguments)), AssignmentType.Var);
        var newScope = VariableScope.NewWithInner(function.Scope, functionParameterArguments, isFunctionScope: true);
        var newContext = new Context(newScope, thisOwner);
        var (result, isReturn, isBreak, isContinue) = function.EvaluateStatement(newContext);
        if (isBreak || isContinue)
            throw new Exception();
        return result;
    }

    private static CustomValue EvaluateFunctionCall(Context context, CustomValue functionValue, List<(bool hasThreeDot, Expression expression)> expressionList, CustomValue newOwner)
    {
        if (functionValue.type != ValueType.Function)
            throw new Exception("variable is not a function");
        FunctionObject function = (FunctionObject)functionValue.value;

        return EvaluateFunctionCall(context, function, expressionList, newOwner);
    }

    private static CustomValue EvaluateFunctionCall(Context context, FunctionObject function, List<(bool hasThreeDot, Expression expression)> expressionList, CustomValue newOwner)
    {
        var arguments = new List<CustomValue>();
        foreach (var (hasThreeDot, expression) in expressionList)
        {
            if (hasThreeDot)
            {
                var value = expression.EvaluateExpression(context);
                foreach (var item in value.AsMultiValue())
                    arguments.Add(item);
            }
            else
                arguments.Add(expression.EvaluateExpression(context));
        }

        CustomValue owner = function.IsLambda ? context.thisOwner : newOwner;

        return CallFunction(function, arguments, owner);
    }

    private static bool Compare(CustomValue first, Operator operatorType, CustomValue second)
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

    private static CustomValue AndOr(CustomValue firstValue, Operator operatorType, Expression tree, Context context)
    {
        if (operatorType == Operator.AndAnd)
        {
            if (!firstValue.IsTruthy())
                return CustomValue.False;
            else
                return tree.EvaluateExpression(context);
        }
        else if (operatorType == Operator.OrOr)
        {
            if (firstValue.IsTruthy())
                return CustomValue.True;
            else
                return tree.EvaluateExpression(context);
        }
        else if (operatorType == Operator.DoubleQuestion)
        {
            if (firstValue.value != null)
                return firstValue;
            else
                return tree.EvaluateExpression(context);
        }
        else
            throw new Exception();
    }

    private static CustomValue CompareTo(CustomValue firstValue, Operator operatorType, Expression tree, Context context)
    {
        var value = tree.EvaluateExpression(context);

        bool result;
        if (operatorType == Operator.CheckEquals)
            result = object.Equals(firstValue.value, value.value);
        else if (operatorType == Operator.CheckNotEquals)
            result = !object.Equals(firstValue.value, value.value);
        else if (operatorType == Operator.LessThan
            || operatorType == Operator.GreaterThan
            || operatorType == Operator.LessThanOrEqual
            || operatorType == Operator.GreaterThanOrEqual)
            result = Compare(firstValue, operatorType, value);
        else
            throw new Exception();

        return result ? CustomValue.True : CustomValue.False;
    }

    private static CustomValue MultiplyOrDivide(CustomValue firstValue, Operator operatorType, CustomValue value)
    {
        double total = (double)firstValue.value;
        if (operatorType == Operator.Multiply)
            total *= (double)value.value;
        else if (operatorType == Operator.Divide)
            total /= (double)value.value;
        else if (operatorType == Operator.Modulus)
            total %= (int)(double)value.value;
        else
            throw new Exception();
        return CustomValue.FromNumber(total);
    }

    private static CustomValue AddOrSubtract(CustomValue firstValue, Operator operatorType, CustomValue value)
    {
        if (operatorType == Operator.Plus)
        {
            if (firstValue.type == ValueType.Number && value.type == ValueType.Number)
            {
                double totalNumber = (double)firstValue.value + (double)value.value;
                return CustomValue.FromNumber(totalNumber);
            }
            else if (firstValue.type == ValueType.String || value.type == ValueType.String)
            {
                string totalString = firstValue.ToString() + value.ToString();
                return CustomValue.FromParsedString(totalString);
            }
            else
                throw new Exception();
        }
        else if (operatorType == Operator.Minus)
        {
            if (firstValue.type == ValueType.Number && value.type == ValueType.Number)
            {
                double totalNumber = (double)firstValue.value - (double)value.value;
                return CustomValue.FromNumber(totalNumber);
            }
            else
                throw new Exception();
        }
        else
            throw new Exception();
    }

    private static Expression GetExpression(ArraySegment<string> expressionTokens)
    {
        if (expressionTokens.Count == 0)
            throw new Exception();

        var expressionTree = ExpressionMethods.New(expressionTokens);
        return expressionTree;
    }

    private static CustomValue DoIndexingGet(CustomValue baseExpressionValue, CustomValue keyExpressionValue, Context context)
    {
        if (baseExpressionValue.type == ValueType.Null)
            throw new Exception();

        if (baseExpressionValue.type == ValueType.Map && keyExpressionValue.type == ValueType.String)
        {
            if (baseExpressionValue.value is ProxyObjectInstance proxy)
            {
                return proxy.DoIndexingGet(keyExpressionValue, context);
            }

            var baseObject = baseExpressionValue.GetBaseObject();
            var key = (string)keyExpressionValue.value;

            static CustomValue GetValue(ValueOrGetterSetter value, CustomValue baseExpressionValue)
            {
                if (value is CustomValue customValue)
                {
                    return customValue;
                }
                else
                {
                    var getterSetter = (GetterSetter)value;
                    return CallFunction(getterSetter.GetGetter(), new List<CustomValue>(), baseExpressionValue);
                }
            }

            var map = baseObject.properties;
            if (map.TryGetValue(key, out var value))
            {
                return GetValue(value, baseExpressionValue);
            }
            else if (baseObject.name != "")
            {
                // Not a regular object, an instance of a class
                var classInfo = context.variableScope.GetVariable(baseObject.name);
                var classObject = (IClassInfoObject)classInfo.value;
                if (classObject.TryGetValueMethod(key, out var method))
                {
                    return GetValue(method, baseExpressionValue);
                }
            }
        }
        else if (baseExpressionValue.type == ValueType.Array)
        {
            var array = (CustomArray)baseExpressionValue.value;
            if (keyExpressionValue.type == ValueType.Number)
            {
                var index = (int)(double)keyExpressionValue.value;
                if (index >= array.Length)
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
            }
        }
        else if (baseExpressionValue.type == ValueType.String)
        {
            var str = (string)baseExpressionValue.value;
            if (keyExpressionValue.type == ValueType.String)
            {
                var fieldName = (string)keyExpressionValue.value;
                if (fieldName == "length")
                    return CustomValue.FromNumber(str.Length);
            }
        }
        else if (baseExpressionValue.type == ValueType.Class && keyExpressionValue.type == ValueType.String)
        {
            var classObject = (IClassInfoObject)baseExpressionValue.value;
            if ((string)keyExpressionValue.value == "prototype")
                return classObject.Prototype;
        }

        if (keyExpressionValue.type == ValueType.String)
        {
            var prototype = GetPrototype(baseExpressionValue.type, context);
            if (prototype.type != ValueType.Null)
            {
                var prototypeMap = prototype.GetAsMap();
                if (prototypeMap.TryGetValue((string)keyExpressionValue.value, out var value))
                {
                    return (CustomValue)value;
                }
            }
        }

        return CustomValue.Null;
    }

    private static CustomValue GetPrototype(ValueType type, Context context)
    {
        switch (type)
        {
            case ValueType.Array:
                {
                    var v = context.variableScope.GetVariable("Array");
                    return v.GetAsMap().TryGetValue("prototype", out var value) ? (CustomValue)value : CustomValue.Null;
                }
            case ValueType.Function:
                {
                    var v = context.variableScope.GetVariable("Function");
                    return v.GetAsMap().TryGetValue("prototype", out var value) ? (CustomValue)value : CustomValue.Null;
                }
            case ValueType.String:
                {
                    var v = context.variableScope.GetVariable("String");
                    return v.GetAsMap().TryGetValue("prototype", out var value) ? (CustomValue)value : CustomValue.Null;
                }
            case ValueType.Generator:
                {
                    var v = context.variableScope.GetVariable("Generator");
                    return v.GetAsMap().TryGetValue("prototype", out var value) ? (CustomValue)value : CustomValue.Null;
                }
            case ValueType.AsyncGenerator:
                {
                    var v = context.variableScope.GetVariable("AsyncGenerator");
                    return v.GetAsMap().TryGetValue("prototype", out var value) ? (CustomValue)value : CustomValue.Null;
                }
            default:
                return CustomValue.Null;
        }
    }

    private static CustomValue DoIndexingSet(CustomValue value, CustomValue baseExpressionValue, CustomValue keyExpressionValue, Context context)
    {
        if (baseExpressionValue.type == ValueType.Map && keyExpressionValue.type == ValueType.String)
        {
            if (baseExpressionValue.value is ProxyObjectInstance proxy)
            {
                return proxy.DoIndexingSet(value, keyExpressionValue, context);
            }

            var map = baseExpressionValue.GetAsMap();
            var key = (string)keyExpressionValue.value;
            if (map.TryGetValue(key, out var previousValue))
            {
                if (previousValue is GetterSetter getterSetter)
                {
                    CallFunction(getterSetter.GetSetter(), new List<CustomValue> { value }, baseExpressionValue);
                }
                else
                {
                    map[key] = value;
                }
            }
            else
            {
                map[key] = value;
            }
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

        throw new Exception();
    }

    private static CustomValue ApplyLValueOperation(Expression lValue, Func<CustomValue?, CustomValue> operation, bool needOldValue, Context context, out CustomValue? oldValue)
    {
        if (lValue is SingleTokenVariableExpression singleExpression)
        {
            var variableName = singleExpression.token;
            oldValue = needOldValue ? context.variableScope.GetVariable(variableName) : null;
            var newValue = operation(oldValue);
            context.variableScope.SetVariable(variableName, newValue);
            return newValue;
        }
        else if (lValue is Op18Expression op18)
        {
            var (lastOperator, expressions) = op18.nextValues[^1];
            if (lastOperator == Operator.MemberAccess || lastOperator == Operator.ComputedMemberAccess)
            {
                var baseExpressionValue = op18.EvaluateAllButLast(context);
                var keyExpressionValue = ((Expression)expressions).EvaluateExpression(context);

                oldValue = needOldValue ? DoIndexingGet(baseExpressionValue, keyExpressionValue, context) : null;
                var newValue = operation(oldValue);
                return DoIndexingSet(newValue, baseExpressionValue, keyExpressionValue, context);
            }

            throw new Exception();
        }
        else
        {
            throw new Exception();
        }
    }

    private static FunctionStatement ExtractProperty(ArraySegment<string> item, ref string fieldName, ref bool isGetProp, ref bool isSetProp)
    {
        FunctionStatement expression;
        if (item[1] == "(")
        {
            expression = FunctionStatement.FromTokens(item, isLambda: false, isAsync: false, isGenerator: false);
        }
        else if (item[0] == "async" && item[2] == "(")
        {
            expression = FunctionStatement.FromTokens(item, isLambda: false, isAsync: true, isGenerator: false);
            fieldName = item[1];
        }
        else if (item[0] == "*" && item[2] == "(")
        {
            expression = FunctionStatement.FromTokens(item, isLambda: false, isAsync: false, isGenerator: true);
            fieldName = item[1];
        }
        else if (item[0] == "async" && item[1] == "*" && item[3] == "(")
        {
            expression = FunctionStatement.FromTokens(item, isLambda: false, isAsync: true, isGenerator: true);
            fieldName = item[2];
        }
        else if (item[0] == "get" && item[2] == "(")
        {
            expression = FunctionStatement.FromTokens(item, isLambda: false, isAsync: false, isGenerator: false);
            fieldName = item[1];
            isGetProp = true;
        }
        else if (item[0] == "set" && item[2] == "(")
        {
            expression = FunctionStatement.FromTokens(item, isLambda: false, isAsync: false, isGenerator: false);
            fieldName = item[1];
            isSetProp = true;
        }
        else
            throw new Exception();
        return expression;
    }

    private static List<(bool hasThreeDot, Expression expression)> GetArguments(ArraySegment<string> parameters)
    {
        var paramsSplit = SplitBy(parameters, ",", allowTrailing: true);
        var expressionList = new List<(bool hasThreeDot, Expression expression)>();
        foreach (var item in paramsSplit)
        {
            if (item[0] == "...")
                expressionList.Add((true, ExpressionMethods.New(item[1..])));
            else
                expressionList.Add((false, ExpressionMethods.New(item)));
        }
        return expressionList;
    }

    private static IEnumerable<CustomValue> StringAsEnumerable(string str)
    {
        return str.Select(c => CustomValue.FromParsedString(c.ToString()));
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
        return firstChar == '"' || firstChar == '\'';
    }

    private static bool IsStaticTemplateString(string token)
    {
        char firstChar = token[0];
        return firstChar == '`';
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

    private static Slice<string> GetTokens(string content, int initialSliceSize)
    {
        var tokens = new Slice<string>(initialSliceSize);
        int i = 0;
        for (; i < content.Length;)
        {
            if (SkipWhiteSpace(content, ref i))
                continue;
            if (SkipComment(content, ref i))
                continue;

            tokens.Add(ReadToken(content, ref i, skipWhitespaceAndComment: false));
        }
        return tokens;
    }

    private static bool SkipWhiteSpace(string content, ref int i)
    {
        if (i >= content.Length)
            return false;
        bool isWhiteSpace = char.IsWhiteSpace(content[i]);
        if (!isWhiteSpace)
            return false;

        i++;
        while (i < content.Length)
        {
            if (char.IsWhiteSpace(content[i]))
                i++;
            else
                break;
        }
        return true;
    }

    private static bool SkipComment(string content, ref int i)
    {
        if (i >= content.Length)
            return false;
        bool isComment = content[i] == '/' && i + 1 < content.Length && (content[i + 1] == '/' || content[i + 1] == '*');
        if (!isComment)
            return false;

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
        return true;
    }

    private static string ReadToken(string content, ref int i, bool skipWhitespaceAndComment)
    {
        if (skipWhitespaceAndComment)
        {
            while (true)
            {
                if (SkipWhiteSpace(content, ref i))
                    continue;
                if (SkipComment(content, ref i))
                    continue;
                break;
            }
        }

        char c = content[i];
        if (char.IsLetter(c) || c == '_')
        {
            int start = i;
            i++;
            while (i < content.Length && (char.IsLetterOrDigit(content[i]) || '_' == content[i]))
                i++;

            return content[start..i];
        }
        else if (char.IsDigit(c))
        {
            int start = i;
            i++;
            while (i < content.Length && (char.IsDigit(content[i]) || content[i] == '.'))
                i++;

            return content[start..i];
        }
        else if (operatorsCompiled.TryGetValue(c, out var level1Map))
        {
            int start = i;
            i++;

            if (i < content.Length && level1Map.TryGetValue(content[i], out var level2Set))
            {
                i++;
                if (i < content.Length && level2Set.Contains(content[i]))
                {
                    i++;
                }
            }
            return content[start..i];
        }
        else if (onlyChars.Contains(c))
        {
            i++;
            return onlyCharStrings[c];
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
            return content[start..i];
        }
        else if (c == '`')
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
                else if (content[i] == '$' && content[i + 1] == '{')
                {
                    i += 2;
                    var tempToken = ReadToken(content, ref i, skipWhitespaceAndComment: true);
                    while (tempToken != "}")
                        tempToken = ReadToken(content, ref i, skipWhitespaceAndComment: true);
                }
                else
                    i++;
            }
            return content[start..i];
        }
        else
        {
            throw new Exception();
        }
    }

    // Defines info about a class, NOT an instance
    interface IClassInfoObject
    {
        FunctionObject? Constructor { get; }
        CustomValue Prototype { get; }
        bool TryGetValueMethod(string key, out ValueOrGetterSetter valueOrGetterSetter);
    }
    class ClassObject : IClassInfoObject
    {
        public FunctionObject? Constructor { get; }
        public CustomValue Prototype { get; }
        private Dictionary<string, ValueOrGetterSetter> Methods { get; }

        public ClassObject(FunctionObject? constructor, Dictionary<string, ValueOrGetterSetter> methods)
        {
            this.Constructor = constructor;
            this.Methods = methods;
            this.Prototype = CustomValue.FromMap(methods);
        }

        public bool TryGetValueMethod(string key, out ValueOrGetterSetter valueOrGetterSetter)
        {
            return Methods.TryGetValue(key, out valueOrGetterSetter);
        }
    }
    class ProxyClassObject : IClassInfoObject
    {
        private FunctionObject constructor = new ProxyClassConstructor();

        public FunctionObject? Constructor => constructor;
        public CustomValue Prototype => throw new Exception();

        public bool TryGetValueMethod(string key, out ValueOrGetterSetter valueOrGetterSetter)
        {
            throw new Exception();
        }
    }
    class BaseObject
    {
        internal readonly string name;
        internal readonly Dictionary<string, ValueOrGetterSetter> properties;

        public BaseObject(string name, Dictionary<string, ValueOrGetterSetter> properties)
        {
            this.name = name;
            this.properties = properties;
        }
    }
    class ProxyObjectInstance
    {
        internal CustomValue target;
        private FunctionObject? getHandler;
        private FunctionObject? setHandler;

        public ProxyObjectInstance(CustomValue target, FunctionObject? getHandler, FunctionObject? setHandler)
        {
            this.target = target;
            this.getHandler = getHandler;
            this.setHandler = setHandler;
        }

        public CustomValue DoIndexingGet(CustomValue keyExpressionValue, Context context)
        {
            if (getHandler == null)
                return Interpreter.DoIndexingGet(target, keyExpressionValue, context);
            else
                return CallFunction(getHandler, new List<CustomValue> { target, keyExpressionValue }, CustomValue.Null);
        }

        public CustomValue DoIndexingSet(CustomValue value, CustomValue keyExpressionValue, Context context)
        {
            if (setHandler == null)
            {
                return Interpreter.DoIndexingSet(value, target, keyExpressionValue, context);
            }
            else
            {
                CallFunction(setHandler, new List<CustomValue> { target, keyExpressionValue, value }, CustomValue.Null);
                return value;
            }
        }
    }
    interface FunctionObject : Statement
    {
        IReadOnlyList<(string paramName, bool isRest)> Parameters { get; }
        VariableScope? Scope { get; }
        bool IsLambda { get; }
    }
    class CustomFunction : FunctionObject
    {
        private IReadOnlyList<(string paramName, bool isRest)> parameters;
        private Statement body;
        private VariableScope scope;
        private bool isLambda;
        private bool isAsync;
        private bool isGenerator;

        public CustomFunction(IReadOnlyList<(string paramName, bool isRest)> parameters, Statement body, VariableScope scope, bool isLambda, bool isAsync, bool isGenerator)
        {
            this.parameters = parameters;
            this.body = body;
            this.scope = scope;
            this.isLambda = isLambda;
            this.isAsync = isAsync;
            this.isGenerator = isGenerator;
        }

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope Scope => scope;

        public bool IsLambda => isLambda;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            if (!isGenerator)
            {
                if (!isAsync)
                {
                    return body.EvaluateStatement(context);
                }
                else
                {
                    Task<CustomValue> promiseTask = Task.Run(() => body.EvaluateStatement(context).value);
                    CustomValue promise = CustomValue.FromPromise(promiseTask);
                    return (promise, false, false, false);
                }
            }
            else
            {
                if (!isAsync)
                {
                    var generator = new Generator(body, context);
                    CustomValue generatorValue = CustomValue.FromGenerator(generator);
                    return (generatorValue, false, false, false);
                }
                else
                {
                    var asyncGenerator = new AsyncGenerator(body, context);
                    CustomValue asyncGeneratorValue = CustomValue.FromAsyncGenerator(asyncGenerator);
                    return (asyncGeneratorValue, false, false, false);
                }
            }
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class PrintFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("x", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var printValue = context.variableScope.GetVariable(Parameters[0].paramName);
            if (printValue.type != ValueType.Null)
                Console.WriteLine(printValue.ToString());
            return (CustomValue.Null, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class FunctionCallFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("thisOwner", false), ("args", true) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisOwner = context.variableScope.GetVariable(Parameters[0].paramName);
            var args = context.variableScope.GetVariable(Parameters[1].paramName);
            var argsList = ((CustomArray)args.value).list;

            var returnValue = CallFunction((FunctionObject)context.thisOwner.value, argsList, thisOwner);

            return (returnValue, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class FunctionApplyFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("thisOwner", false), ("args", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisOwner = context.variableScope.GetVariable(Parameters[0].paramName);
            var args = context.variableScope.GetVariable(Parameters[1].paramName);
            var argsList = ((CustomArray)args.value).list;

            var returnValue = CallFunction((FunctionObject)context.thisOwner.value, argsList, thisOwner);

            return (returnValue, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class MathRandomFunction : FunctionObject
    {
        public IReadOnlyList<(string paramName, bool isRest)> Parameters => Array.Empty<(string, bool)>();

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var randomDouble = Random.Shared.NextDouble();
            return (CustomValue.FromNumber(randomDouble), false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class MathFloorFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("number", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var number = context.variableScope.GetVariable(Parameters[0].paramName);
            var num = (double)number.value;
            return (CustomValue.FromNumber(Math.Floor(num)), false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class GeneratorNextFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = Array.Empty<(string, bool)>();

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisOwner = context.thisOwner;
            var generator = (Generator)thisOwner.value;
            var value = generator.Next();
            return (value, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class AsyncGeneratorNextFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = Array.Empty<(string, bool)>();

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisOwner = context.thisOwner;
            var asyncGenerator = (AsyncGenerator)thisOwner.value;
            var value = asyncGenerator.Next();
            return (CustomValue.FromPromise(value), false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class CharAtFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("x", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisString = context.thisOwner;
            if (thisString.type != ValueType.String)
                throw new Exception();

            var str = (string)thisString.value;

            var indexValue = context.variableScope.GetVariable(Parameters[0].paramName);
            if (indexValue.type != ValueType.Number)
                throw new Exception();

            int index = (int)(double)indexValue.value;

            var newValue = CustomValue.FromParsedString(str[index].ToString());
            return (newValue, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class CharCodeAtFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("x", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisString = context.thisOwner;
            if (thisString.type != ValueType.String)
                throw new Exception();

            var str = (string)thisString.value;

            var indexValue = context.variableScope.GetVariable(Parameters[0].paramName);
            if (indexValue.type != ValueType.Number)
                throw new Exception();

            int index = (int)(double)indexValue.value;

            var newValue = (int)str[index];
            return (CustomValue.FromNumber(newValue), false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class ArrayPushFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("x", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

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

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class ArrayPopFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = Array.Empty<(string, bool)>();

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisArray = context.thisOwner;
            if (thisArray.type != ValueType.Array)
                throw new Exception();
            var array = (CustomArray)thisArray.value;
            var returnValue = array.list[^1];
            array.list.RemoveAt(array.list.Count - 1);
            return (returnValue, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class ArrayMapFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("f", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisArray = context.thisOwner.GetAsArray();
            var f = context.variableScope.GetVariable(Parameters[0].paramName).GetAsFunction();
            var list = thisArray.list;

            var res = list.Select(x => CallFunction(f, new List<CustomValue> { x }, CustomValue.Null)).ToList();
            var newList = CustomValue.FromArray(new CustomArray(res));
            return (newList, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class ArrayFilterFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("f", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisArray = context.thisOwner.GetAsArray();
            var f = context.variableScope.GetVariable(Parameters[0].paramName).GetAsFunction();
            var list = thisArray.list;

            var res = list.Where(x => CallFunction(f, new List<CustomValue> { x }, CustomValue.Null).IsTruthy()).ToList();
            var newList = CustomValue.FromArray(new CustomArray(res));
            return (newList, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class ArrayJoinFunction : FunctionObject
    {
        private static (string paramName, bool isRest)[] parameters = new[] { ("separator", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var thisArray = context.thisOwner.GetAsArray();
            var separator = (string)context.variableScope.GetVariable(Parameters[0].paramName).value;
            var res = string.Join(separator, thisArray.list.Select(x => x.ToString()));
            return (CustomValue.FromParsedString(res), false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class ProxyClassConstructor : FunctionObject
    {
        private IReadOnlyList<(string paramName, bool isRest)> parameters = new[] { ("target", false), ("options", false) };

        public IReadOnlyList<(string paramName, bool isRest)> Parameters => parameters;

        public VariableScope? Scope => null;

        public bool IsLambda => false;

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var target = context.variableScope.GetVariable(Parameters[0].paramName);
            var options = context.variableScope.GetVariable(Parameters[1].paramName);
            var optionsProperties = options.GetBaseObject().properties;
            var hasGetHandler = optionsProperties.TryGetValue("get", out var getSettings);
            var hasSetHandler = optionsProperties.TryGetValue("set", out var setSettings);

            FunctionObject? getHandler = hasGetHandler ? (FunctionObject)(((CustomValue)getSettings).value) : null;
            FunctionObject? setHandler = hasSetHandler ? (FunctionObject)(((CustomValue)setSettings).value) : null;

            var newProxy = new ProxyObjectInstance(target, getHandler, setHandler);

            var value = CustomValue.FromProxy(newProxy);
            return (value, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
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

        public CustomArray(List<CustomValue> list)
        {
            this.list = list;
            this.map = new Dictionary<string, CustomValue>();
        }
    }

    class Generator : IEnumerable<CustomValue>
    {
        IEnumerator<CustomValue> enumerator;

        public Generator(Statement statement, Context context)
        {
            this.enumerator = statement.AsEnumerable(context).GetEnumerator();
        }

        internal CustomValue Next()
        {
            bool success = this.enumerator.MoveNext();
            if (success)
            {
                var map = new Dictionary<string, ValueOrGetterSetter>
                {
                    ["value"] = this.enumerator.Current,
                    ["done"] = CustomValue.False
                };
                return CustomValue.FromMap(map);
            }
            else
            {
                return CustomValue.GeneratorDone;
            }
        }

        public IEnumerator<CustomValue> GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return enumerator;
        }
    }
    class AsyncGenerator : IEnumerable<Task<(bool, CustomValue)>>
    {
        IEnumerator<Task<(bool, CustomValue)>> enumerator;

        public AsyncGenerator(Statement statement, Context context)
        {
            this.enumerator = Promisify(statement.AsEnumerable(context)).GetEnumerator();
        }

        private static IEnumerable<Task<(bool, CustomValue)>> Promisify(IEnumerable<CustomValue> source)
        {
            var enumerator = source.GetEnumerator();
            while (true)
            {
                yield return Task.Run(() =>
                {
                    var success = enumerator.MoveNext();
                    if (success)
                        return (true, enumerator.Current);
                    else
                        return (false, CustomValue.Null);
                });
            }
        }

        internal Task<CustomValue> Next()
        {
            if (!enumerator.MoveNext())
                throw new Exception();

            var task = enumerator.Current;
            var newTask = Task.Run(async () =>
            {
                var (success, value) = await task;
                if (success)
                {
                    var map = new Dictionary<string, ValueOrGetterSetter>
                    {
                        ["value"] = value,
                        ["done"] = CustomValue.False
                    };
                    return CustomValue.FromMap(map);
                }
                else
                {
                    return CustomValue.GeneratorDone;
                }
            });
            return newTask;
        }

        public IEnumerator<Task<(bool, CustomValue)>> GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return enumerator;
        }
    }

    interface ValueOrGetterSetter
    {

    }
    private struct CustomValue : ValueOrGetterSetter
    {
        public object value;
        public ValueType type;

        public static readonly CustomValue Null = new CustomValue(null!, ValueType.Null);
        public static readonly CustomValue True = new CustomValue(true, ValueType.Bool);
        public static readonly CustomValue False = new CustomValue(false, ValueType.Bool);
        public static readonly CustomValue GeneratorDone = CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>
        {
            ["value"] = CustomValue.Null,
            ["done"] = CustomValue.True,
        });

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
            var parsedString = SingleTokenStringExpression.ParseStaticString(s);
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

        internal static CustomValue FromClass(IClassInfoObject func)
        {
            return new CustomValue(func, ValueType.Class);
        }

        internal static CustomValue FromMap(Dictionary<string, ValueOrGetterSetter> map, string className = "")
        {
            return new CustomValue(new BaseObject(className, map), ValueType.Map);
        }

        internal static CustomValue FromProxy(ProxyObjectInstance proxy)
        {
            return new CustomValue(proxy, ValueType.Map);
        }

        internal static CustomValue FromArray(CustomArray array)
        {
            return new CustomValue(array, ValueType.Array);
        }

        internal static CustomValue FromPromise(Task<CustomValue> promise)
        {
            return new CustomValue(promise, ValueType.Promise);
        }

        internal static CustomValue FromGenerator(Generator generator)
        {
            return new CustomValue(generator, ValueType.Generator);
        }

        internal static CustomValue FromAsyncGenerator(AsyncGenerator generator)
        {
            return new CustomValue(generator, ValueType.AsyncGenerator);
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
                    return true;
            }
        }

        internal IEnumerable<CustomValue> AsMultiValue()
        {
            if (this.type == ValueType.Array)
                return ((CustomArray)this.value).list;
            else if (this.type == ValueType.Generator)
                return (Generator)this.value;
            else
                throw new Exception();
        }

        internal FunctionObject GetAsFunction()
        {
            if (type != ValueType.Function)
                throw new Exception();
            return (FunctionObject)this.value;
        }

        internal CustomArray GetAsArray()
        {
            if (type != ValueType.Array)
                throw new Exception();
            return (CustomArray)this.value;
        }

        internal Dictionary<string, ValueOrGetterSetter> GetAsMap()
        {
            return ((BaseObject)value).properties;
        }

        internal BaseObject GetBaseObject()
        {
            return (BaseObject)value;
        }

        public override string ToString()
        {
            if (value is null)
                return "null";
            if (value is bool b)
                return b ? "true" : "false";
            return value.ToString()!;
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
    private struct GetterSetter : ValueOrGetterSetter
    {
        private readonly FunctionObject? getter;
        private readonly FunctionObject? setter;

        public GetterSetter(FunctionObject? getter, FunctionObject? setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public FunctionObject GetGetter()
        {
            return getter ?? throw new Exception("no getter was defined");
        }

        public FunctionObject GetSetter()
        {
            return setter ?? throw new Exception("no setter was defined");
        }

        public GetterSetter WithNewGetter(FunctionObject f)
        {
            if (getter != null)
                throw new Exception("getter was already defined");
            return new GetterSetter(f, setter);
        }

        public GetterSetter WithNewSetter(FunctionObject f)
        {
            if (setter != null)
                throw new Exception("setter was already defined");
            return new GetterSetter(getter, f);
        }
    }

    enum ValueType
    {
        Null,
        Number,
        String,
        Bool,
        Function,
        Class,
        Map,
        Array,
        Promise,
        Generator,
        AsyncGenerator,
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
        MemberAccess,
        ComputedMemberAccess,
        FunctionCall,
        ConditionalMemberAccess,
        ConditionalComputedMemberAccess,
        ConditionalFunctionCall,
        New,
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
        PostfixIncrement = 16,
        FunctionCall = 18,
        Indexing = 18,
        DotAccess = 18,
        LambdaExpression = 9999,
    }

    class VariableScope
    {
        private Dictionary<string, (CustomValue, AssignmentType)> variables;
        private VariableScope? innerScope;
        private bool isFunctionScope;

        private VariableScope(Dictionary<string, (CustomValue, AssignmentType)> variables, VariableScope? innerScope, bool isFunctionScope)
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
            while (!scope!.isFunctionScope)
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
        public static VariableScope NewWithInner(VariableScope? innerScope, Dictionary<string, (CustomValue, AssignmentType)> values, bool isFunctionScope)
        {
            return new VariableScope(values, innerScope, isFunctionScope);
        }
        public static VariableScope GetNewLoopScope(VariableScope scope, AssignmentType assignmentType, string variableName, CustomValue variableValue)
        {
            var loopScope = ScopeForLoop(scope, assignmentType);
            loopScope.AssignVariable(assignmentType, variableName, variableValue);
            return loopScope;
        }
        public static VariableScope GetNewLoopScopeCopy(VariableScope scope, AssignmentType assignmentType)
        {
            var newScope = ScopeForLoop(scope, assignmentType);

            foreach (var variable in scope.variables)
            {
                var variableName = variable.Key;
                var (variableValue, _) = variable.Value;
                newScope.AssignVariable(assignmentType, variableName, variableValue);
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
        public void AssignVariable(AssignmentType assignmentType, string variableName, CustomValue variableValue)
        {
            switch (assignmentType)
            {
                case AssignmentType.None:
                    this.SetVariable(variableName, variableValue);
                    break;
                case AssignmentType.Var:
                    this.AddVarVariable(variableName, variableValue);
                    break;
                case AssignmentType.Let:
                    this.AddLetVariable(variableName, variableValue);
                    break;
                case AssignmentType.Const:
                    this.AddConstVariable(variableName, variableValue);
                    break;
                default:
                    break;
            }
        }
        public void ApplyToScope(VariableScope scope)
        {
            foreach (var item in variables)
                scope.variables[item.Key] = item.Value;
        }
    }
    record Context
    {
        public readonly VariableScope variableScope;
        public readonly CustomValue thisOwner;

        public Context(VariableScope variableScope, CustomValue thisOwner)
        {
            this.variableScope = variableScope;
            this.thisOwner = thisOwner;
        }
    }

    interface HasRestExpression : Expression
    {
        Expression ExpressionRest { get; set; }
    }
    interface Expression
    {
        CustomValue EvaluateExpression(Context context);
    }
    static class ExpressionMethods
    {
        public static Expression New(ArraySegment<string> tokens)
        {
            var (previousExpression, index) = ReadExpression(tokens, 0);
            if (index <= 0)
                throw new Exception();

            while (true)
            {
                if (index == tokens.Count)
                    return previousExpression;

                var newToken = tokens[index];

                if (assignmentSet.Contains(newToken))
                {
                    var restTokens = tokens[(index + 1)..];
                    return new AssignmentExpression(previousExpression, newToken, restTokens, AssignmentType.None);
                }

                if (regularOperatorSet.Contains(newToken))
                {
                    var newPrecedence = TreeExpression.GetPrecedence(newToken);

                    Expression nextExpression;
                    (nextExpression, index) = ReadExpression(tokens, index + 1);

                    AddToLastNode(ref previousExpression, newPrecedence, (expression, precedence) =>
                    {
                        if (precedence != newPrecedence)
                        {
                            var newTree = new TreeExpression(newPrecedence, expression);
                            newTree.AddExpression(newToken, nextExpression);
                            return newTree;
                        }
                        else
                        {
                            ((TreeExpression)expression).AddExpression(newToken, nextExpression);
                            return null!;
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

                    var questionMarkExpressionTokens = tokens[(questionMarkIndex + 1)..colonIndex];
                    var questionMarkExpression = ExpressionMethods.New(questionMarkExpressionTokens);
                    var colonExpressionTokens = tokens[(colonIndex + 1)..];
                    var colonExpression = ExpressionMethods.New(colonExpressionTokens);

                    var ternaryExpression = new TernaryExpression(previousExpression, questionMarkExpression, colonExpression);
                    return ternaryExpression;
                }

                if (newToken == "=>")
                {
                    // Read operator
                    // Handle lambda, async: false
                    var (functionBodyTokens, end) = ReadBodyTokensAndEnd(tokens, index);

                    AddToLastNode(ref previousExpression, Precedence.LambdaExpression, (expression, p) =>
                    {
                        if (expression is SingleTokenVariableExpression singleTokenVariableExpression)
                            return FunctionStatement.FromParametersAndBody(singleTokenVariableExpression.token, functionBodyTokens, isLambda: true, isAsync: false, isGenerator: false);
                        else
                            throw new Exception();
                    });

                    index = end + 1;
                    continue;
                }

                if (newToken == "++" || newToken == "--")
                {
                    // Postfix increment
                    var isInc = newToken == "++";
                    AddToLastNode(ref previousExpression, Precedence.PostfixIncrement, (expression, p) =>
                    {
                        return new PrePostIncDecExpression(expression, isPre: false, isInc: isInc);
                    });

                    index += 1;
                    continue;
                }

                if (newToken == "(" || newToken == "?.(")
                {
                    // Function call
                    var isConditional = newToken == "?.(";
                    var @operator = isConditional ? Operator.ConditionalFunctionCall : Operator.FunctionCall;

                    var end = tokens.IndexOfParenthesesEnd(index + 1);
                    if (end < 0)
                        throw new Exception();
                    var parameters = tokens[(index + 1)..end];

                    AddToLastNode(ref previousExpression, Precedence.FunctionCall, (expression, p) =>
                    {
                        var expressionList = GetArguments(parameters);

                        if (expression is HasRestExpression hasRestExpression)
                        {
                            var newExpression = new Op18Expression(hasRestExpression.ExpressionRest);
                            newExpression.AddExpression(@operator, expressionList);

                            hasRestExpression.ExpressionRest = newExpression;
                            return hasRestExpression;
                        }
                        else if (expression is Op18Expression op18)
                        {
                            op18.AddExpression(@operator, expressionList);
                            return expression;
                        }
                        else
                        {
                            var newExpression = new Op18Expression(expression);
                            newExpression.AddExpression(@operator, expressionList);
                            return newExpression;
                        }
                    });

                    index = end + 1;
                    continue;
                }

                if (newToken == "[" || newToken == "?.[")
                {
                    // Indexing
                    var isConditional = newToken == "?.[";
                    var @operator = isConditional ? Operator.ConditionalComputedMemberAccess : Operator.ComputedMemberAccess;

                    var end = tokens.IndexOfBracketsEnd(index + 1);
                    if (end < 0)
                        throw new Exception();
                    var keyExpressionTokens = tokens[(index + 1)..end];

                    AddToLastNode(ref previousExpression, Precedence.Indexing, (expression, p) =>
                    {
                        var keyExpression = ExpressionMethods.New(keyExpressionTokens);

                        if (expression is HasRestExpression hasRestExpression)
                        {
                            var newExpression = new Op18Expression(hasRestExpression.ExpressionRest);
                            newExpression.AddExpression(@operator, keyExpression);

                            hasRestExpression.ExpressionRest = newExpression;
                            return hasRestExpression;
                        }
                        else if (expression is Op18Expression op18)
                        {
                            op18.AddExpression(@operator, keyExpression);
                            return expression;
                        }
                        else
                        {
                            var newExpression = new Op18Expression(expression);
                            newExpression.AddExpression(@operator, keyExpression);
                            return newExpression;
                        }
                    });

                    index = end + 1;
                    continue;
                }

                if (newToken == "." || newToken == "?.")
                {
                    // Dot access
                    var isConditional = newToken == "?.";
                    var @operator = isConditional ? Operator.ConditionalMemberAccess : Operator.MemberAccess;

                    var fieldName = tokens[index + 1];
                    if (!IsVariableName(fieldName))
                        throw new Exception();

                    AddToLastNode(ref previousExpression, Precedence.DotAccess, (expression, p) =>
                    {
                        var nextExpression = new CustomValueExpression(CustomValue.FromParsedString(fieldName));

                        if (expression is HasRestExpression hasRestExpression)
                        {
                            var newExpression = new Op18Expression(hasRestExpression.ExpressionRest);
                            newExpression.AddExpression(@operator, nextExpression);

                            hasRestExpression.ExpressionRest = newExpression;
                            return hasRestExpression;
                        }
                        else if (expression is Op18Expression op18)
                        {
                            op18.AddExpression(@operator, nextExpression);
                            return expression;
                        }
                        else
                        {
                            var newExpression = new Op18Expression(expression);
                            newExpression.AddExpression(@operator, nextExpression);
                            return newExpression;
                        }
                    });

                    index += 2;
                    continue;
                }

                throw new Exception();
            }

            throw new Exception();
        }

        public static (Expression, int) ReadExpression(ArraySegment<string> tokens, int index)
        {
            var token = tokens[index];
            if (token == "await")
            {
                var (expressionRest, lastIndex) = ReadExpression(tokens, index + 1);
                var newExpression = new AwaitExpression(expressionRest);
                return (newExpression, lastIndex);
            }
            if (token == "new")
            {
                var className = tokens[1];
                var paranBegin = 2;
                if (tokens[paranBegin] != "(")
                    throw new Exception();
                var paranEnd = tokens.IndexOfParenthesesEnd(paranBegin + 1);
                if (paranEnd < 0)
                    throw new Exception();
                var newExpression = new NewClassInstanceExpression(className, tokens[(paranBegin + 1)..paranEnd]);
                return (newExpression, paranEnd + 1);
            }
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
                // Prefix increment
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
                    var parenthesesTokens = tokens[index..(newend + 1)];
                    var newExpression = new ParenthesesExpression(parenthesesTokens);
                    return (newExpression, newend + 1);
                }
                else
                {
                    // Read expression
                    // Lambda, async: false
                    var (functionBodyTokens, end) = ReadBodyTokensAndEnd(tokens, newend + 1);

                    var parameterTokens = tokens[(index + 1)..newend];
                    var functionExpression = FunctionStatement.FromParametersAndBody(parameterTokens, functionBodyTokens, isLambda, isAsync: false, isGenerator: false);

                    return (functionExpression, end + 1);
                }
            }
            if (token == "{")
            {
                var braceEndIndex = tokens.IndexOfBracesEnd(index + 1);
                if (braceEndIndex < 0)
                    throw new Exception();

                if (braceEndIndex + 1 < tokens.Count && tokens[braceEndIndex + 1] == "=")
                {
                    // Map assignment without var, let or const
                    var mapExpression = MapAssignmentExpression.FromBody(tokens, braceEndIndex, AssignmentType.None);
                    return (mapExpression, tokens.Count);
                }
                else
                {
                    var mapExpressionTokens = tokens[index..(braceEndIndex + 1)];
                    var mapExpression = new MapExpression(mapExpressionTokens);
                    return (mapExpression, braceEndIndex + 1);
                }
            }
            if (token == "[")
            {
                var bracketsEndIndex = tokens.IndexOfBracketsEnd(index + 1);
                if (bracketsEndIndex < 0)
                    throw new Exception();

                if (bracketsEndIndex + 1 < tokens.Count && tokens[bracketsEndIndex + 1] == "=")
                {
                    // Array assignment without var, let or const
                    var arrayExpression = ArrayAssignmentExpression.FromBody(tokens, bracketsEndIndex, AssignmentType.None);
                    return (arrayExpression, tokens.Count);
                }
                else
                {
                    var arrayTokens = tokens[index..(bracketsEndIndex + 1)];
                    var arrayExpression = new ArrayExpression(arrayTokens);
                    return (arrayExpression, bracketsEndIndex + 1);
                }
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
            if (tokens[index] == "function" || (tokens[index] == "async" && tokens[index + 1] == "function"))
            {
                bool isAsync = token == "async";
                if (isAsync)
                {
                    tokens = tokens[1..];
                }

                bool isGenerator = false;
                if (tokens[index + 1] == "*")
                {
                    isGenerator = true;
                }
                int tokenShiftIndex = isGenerator ? 1 : 0;

                if (tokens[index + 1 + tokenShiftIndex] != "(")
                    throw new Exception();
                var parenthesesEnd = tokens.IndexOfParenthesesEnd(index + 2 + tokenShiftIndex);
                if (parenthesesEnd < 0)
                    throw new Exception();
                if (tokens[parenthesesEnd + 1] != "{")
                    throw new Exception();
                var bracesEnd = tokens.IndexOfBracesEnd(parenthesesEnd + 2);
                if (bracesEnd < 0)
                    throw new Exception();

                var functionExpressionTokens = tokens[index..(bracesEnd + 1)];
                var functionExpression = FunctionStatement.FromTokens(functionExpressionTokens, isLambda: false, isAsync, isGenerator);
                return (functionExpression, isAsync ? bracesEnd + 2 : bracesEnd + 1);
            }
            if (token == "async")
            {
                // Async lambda here

                var lambdaIndex = tokens.IndexOf("=>", 1);
                if (lambdaIndex < 0)
                    throw new Exception();

                var paramTokens = tokens[(index + 1)..lambdaIndex];
                if (paramTokens.Count <= 0)
                    throw new Exception();

                ArraySegment<string> parameters;
                if (paramTokens[0] == "(")
                {
                    if (paramTokens[^1] != ")")
                        throw new Exception();

                    parameters = paramTokens[1..^1];
                }
                else if (paramTokens.Count == 1)
                {
                    parameters = paramTokens;
                }
                else
                    throw new Exception();

                var (body, end) = ReadBodyTokensAndEnd(tokens, lambdaIndex);
                var function = FunctionStatement.FromParametersAndBody(parameters, body, isLambda: true, isAsync: true, isGenerator: false);

                return (function, end + 1);
            }
            if (IsNumber(token))
            {
                return (new SingleTokenNumberExpression(token), index + 1);
            }
            if (IsStaticString(token))
            {
                return (new SingleTokenStringExpression(token), index + 1);
            }
            if (IsStaticTemplateString(token))
            {
                return (new SingleTokenStringTemplateExpression(token), index + 1);
            }
            if (IsVariableName(token))
            {
                if (token == "this")
                    return (thisExpression, index + 1);
                return (new SingleTokenVariableExpression(token), index + 1);
            }

            throw new Exception();
        }

        private static (ArraySegment<string>, int) ReadBodyTokensAndEnd(ArraySegment<string> tokens, int index)
        {
            var nextToken = tokens[index + 1];

            ArraySegment<string> functionBodyTokens;
            int end;
            if (nextToken == "{")
            {
                end = tokens.IndexOfBracesEnd(index + 2);
                if (end < 0)
                    throw new Exception();

                functionBodyTokens = tokens[(index + 1)..(end + 1)];
            }
            else
            {
                functionBodyTokens = tokens[(index + 1)..];
                end = tokens.Count - 1;
            }

            return (functionBodyTokens, end);
        }

        private static void AddToLastNode(ref Expression previousExpression, Precedence precedence, Func<Expression, Precedence?, Expression> handler)
        {
            if (previousExpression is TreeExpression treeExpression && precedence >= treeExpression.precedence)
            {
                TreeExpression lowestTreeExpression = treeExpression;

                while (lowestTreeExpression.nextValues[^1].expression is TreeExpression subTree && precedence > subTree.precedence)
                {
                    lowestTreeExpression = subTree;
                }

                var (treeLastElementOperator, treeLastElementExpression) = lowestTreeExpression.nextValues[^1];

                if (precedence == treeExpression.precedence)
                {
                    var newExpression = handler(lowestTreeExpression, lowestTreeExpression.precedence);
                }
                else
                {
                    var newExpression = handler(treeLastElementExpression, lowestTreeExpression.precedence);
                    lowestTreeExpression.nextValues[^1] = (treeLastElementOperator, newExpression);
                }
            }
            else
            {
                previousExpression = handler(previousExpression, null);
            }
        }
    }
    class TreeExpression : Expression
    {
        internal Precedence precedence;
        private Expression firstExpression;
        internal List<(Operator operatorToken, Expression expression)> nextValues;

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
                    {
                        var value = firstExpression.EvaluateExpression(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = CompareTo(value, op, expression, context);
                        }
                        return value;
                    }
                case Precedence.Comparison:
                    {
                        var value = firstExpression.EvaluateExpression(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = CompareTo(value, op, expression, context);
                        }
                        return value;
                    }
                case Precedence.AddSubtract:
                    {
                        var value = firstExpression.EvaluateExpression(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = AddOrSubtract(value, op, expression.EvaluateExpression(context));
                        }
                        return value;
                    }
                case Precedence.MultiplyDivide:
                    {
                        var value = firstExpression.EvaluateExpression(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = MultiplyOrDivide(value, op, expression.EvaluateExpression(context));
                        }
                        return value;
                    }
                case Precedence.AndAnd:
                    {
                        var value = firstExpression.EvaluateExpression(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = AndOr(value, op, expression, context);
                        }
                        return value;
                    }
                case Precedence.OrOr:
                    {
                        var value = firstExpression.EvaluateExpression(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = AndOr(value, op, expression, context);
                        }
                        return value;
                    }
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
    class Op18Expression : Expression
    {
        private Expression firstExpression;
        internal List<(Operator operatorToken, object)> nextValues;

        public Op18Expression(Expression firstExpression)
        {
            this.firstExpression = firstExpression;
            this.nextValues = new List<(Operator operatorToken, object)>();
        }

        internal void AddExpression(Operator @operator, List<(bool hasThreeDot, Expression expression)> expressionList)
        {
            nextValues.Add((@operator, expressionList));
        }

        internal void AddExpression(Operator @operator, Expression nextExpression)
        {
            nextValues.Add((@operator, nextExpression));
        }

        public CustomValue EvaluateAllButLast(Context context)
        {
            return Evaluate(context, nextValues.Count - 1);
        }

        public CustomValue EvaluateExpression(Context context)
        {
            return Evaluate(context, nextValues.Count);
        }

        private CustomValue Evaluate(Context context, int count)
        {
            var secondLastValue = CustomValue.Null;
            var lastValue = firstExpression.EvaluateExpression(context);
            for (int i = 0; i < count; i++)
            {
                var (op, expressions) = nextValues[i];
                switch (op)
                {
                    case Operator.ConditionalMemberAccess:
                        {
                            if (lastValue.type == ValueType.Null)
                                return CustomValue.Null;
                            else
                                op = Operator.MemberAccess;
                        }
                        break;
                    case Operator.ConditionalComputedMemberAccess:
                        {
                            if (lastValue.type == ValueType.Null)
                                return CustomValue.Null;
                            else
                                op = Operator.ComputedMemberAccess;
                        }
                        break;
                    case Operator.ConditionalFunctionCall:
                        {
                            if (lastValue.type == ValueType.Null)
                                return CustomValue.Null;
                            else
                                op = Operator.FunctionCall;
                        }
                        break;
                }

                switch (op)
                {
                    case Operator.MemberAccess:
                        {
                            var keyExpressionValue = ((Expression)expressions).EvaluateExpression(context);
                            secondLastValue = lastValue;
                            lastValue = DoIndexingGet(lastValue, keyExpressionValue, context);
                        }
                        break;
                    case Operator.ComputedMemberAccess:
                        {
                            var keyExpressionValue = ((Expression)expressions).EvaluateExpression(context);
                            secondLastValue = lastValue;
                            lastValue = DoIndexingGet(lastValue, keyExpressionValue, context);
                        }
                        break;
                    case Operator.FunctionCall:
                        {
                            var expressionList = (List<(bool hasThreeDot, Expression expression)>)expressions;
                            var oldSecondLastValue = secondLastValue;
                            secondLastValue = lastValue;

                            if (oldSecondLastValue.type == ValueType.Map && oldSecondLastValue.value is ProxyObjectInstance proxy)
                                oldSecondLastValue = proxy.target;
                            lastValue = EvaluateFunctionCall(context, lastValue, expressionList, oldSecondLastValue);
                        }
                        break;
                    default:
                        throw new Exception();
                }
            }

            return lastValue;
        }

        public Expression GetSecondLastExpression()
        {
            var count = nextValues.Count;
            return count > 1 ? (Expression)nextValues[count - 2].Item2 : firstExpression;
        }
    }
    class MapExpression : Expression
    {
        List<(string fieldName, Expression expression, bool hasThreeDot, bool isGetProp, bool isSetProp)> fieldExpressions;

        public MapExpression(ArraySegment<string> tokens)
        {
            tokens = tokens[1..^1];
            var res = SplitBy(tokens, ",", allowTrailing: true);
            this.fieldExpressions = new List<(string fieldName, Expression expression, bool hasThreeDot, bool isGetProp, bool isSetProp)>();
            foreach (var item in res)
            {
                var firstToken = item[0];
                var fieldName = IsVariableName(firstToken) ? firstToken : (string)CustomValue.FromStaticString(firstToken).value;

                Expression expression;
                bool hasThreeDot = false;
                bool isGetProp = false;
                bool isSetProp = false;
                if (item.Count == 1)
                    expression = ExpressionMethods.New(item);
                else if (item[1] == ":")
                    expression = ExpressionMethods.New(item[2..]);
                else if (item[0] == "...")
                {
                    hasThreeDot = true;
                    expression = ExpressionMethods.New(item[1..]);
                }
                else
                    expression = Interpreter.ExtractProperty(item, ref fieldName, ref isGetProp, ref isSetProp);

                this.fieldExpressions.Add((fieldName, expression, hasThreeDot, isGetProp, isSetProp));
            }
        }

        public CustomValue EvaluateExpression(Context context)
        {
            var map = new Dictionary<string, ValueOrGetterSetter>();
            foreach (var (fieldName, expression, hasThreeDot, isGetProp, isSetProp) in fieldExpressions)
            {
                var fieldValue = expression.EvaluateExpression(context);
                if (hasThreeDot)
                {
                    var valueMap = fieldValue.GetAsMap();
                    foreach (var pair in valueMap)
                        map[pair.Key] = pair.Value;
                }
                else if (isGetProp)
                {
                    var f = (FunctionObject)fieldValue.value;
                    if (map.TryGetValue(fieldName, out var oldVal))
                    {
                        if (oldVal is GetterSetter getterSetter)
                        {
                            map[fieldName] = getterSetter.WithNewGetter(f);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        map[fieldName] = new GetterSetter(getter: f, setter: null);
                    }
                }
                else if (isSetProp)
                {
                    var f = (FunctionObject)fieldValue.value;
                    if (map.TryGetValue(fieldName, out var oldVal))
                    {
                        if (oldVal is GetterSetter getterSetter)
                        {
                            map[fieldName] = getterSetter.WithNewSetter(f);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        map[fieldName] = new GetterSetter(getter: null, setter: f);
                    }
                }
                else
                {
                    map[fieldName] = fieldValue;
                }
            }
            return CustomValue.FromMap(map);
        }
    }
    class ArrayExpression : Expression
    {
        private List<(bool hasThreeDot, Expression expression)> expressionList;

        public ArrayExpression(ArraySegment<string> tokens)
        {
            tokens = tokens[1..^1];
            var res = SplitBy(tokens, ",", allowTrailing: true);
            expressionList = new List<(bool, Expression)>();
            foreach (var item in res)
            {
                if (item[0] == "...")
                    expressionList.Add((true, ExpressionMethods.New(item[1..])));
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
                    foreach (var item in value.AsMultiValue())
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

        public ParenthesesExpression(ArraySegment<string> parenthesesTokens)
        {
            if (parenthesesTokens[0] != "(")
                throw new Exception();
            this.insideExpression = ExpressionMethods.New(parenthesesTokens[1..^1]);
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
        private CustomValue value;

        public SingleTokenStringExpression(string token)
        {
            var parsedString = ParseStaticString(token);
            this.value = CustomValue.FromParsedString(parsedString);
        }

        public CustomValue EvaluateExpression(Context context)
        {
            return value;
        }

        public static string ParseStaticString(string s)
        {
            char firstChar = s[0];

            var sb = new StringBuilder();
            for (int i = 1; i < s.Length; i++)
            {
                char c = s[i];
                if (c == firstChar)
                    break;
                else if (c == '\r' || c == '\n')
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
                        case 'u':
                            {
                                int res = (hexToint[s[i + 1]] << 12) + (hexToint[s[i + 2]] << 8) + (hexToint[s[i + 3]] << 4) + hexToint[s[i + 4]];
                                sb.Append((char)res);
                                i += 4;
                            }
                            break;
                        default: throw new Exception();
                    }
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
    class SingleTokenStringTemplateExpression : Expression
    {
        private List<object> parts; // Each part can be either a String or an Expression

        public SingleTokenStringTemplateExpression(string token)
        {
            parts = new List<object>();

            for (int i = 1; i < token.Length;)
            {
                if (token[i] == '`')
                    break;
                else if (token[i] == '$' && token[i + 1] == '{')
                {
                    var subtokens = new Slice<string>(8);
                    i += 2;
                    while (true)
                    {
                        var t = ReadToken(token, ref i, skipWhitespaceAndComment: true);
                        if (t == "}")
                            break;
                        subtokens.Add(t);
                    }

                    var subExpression = ExpressionMethods.New(subtokens.ToSegment());
                    parts.Add(subExpression);
                    continue;
                }

                var sb = new StringBuilder();
                while (token[i] != '`' && !(token[i] == '$' && token[i + 1] == '{'))
                {
                    if (token[i] == '\\')
                    {
                        i++;
                        char c2 = token[i];
                        switch (c2)
                        {
                            case '"': sb.Append(c2); break;
                            case '\'': sb.Append(c2); break;
                            case '\\': sb.Append(c2); break;
                            case 't': sb.Append('\t'); break;
                            case 'r': sb.Append('\r'); break;
                            case 'n': sb.Append('\n'); break;
                            case '$': sb.Append(c2); break;
                            case 'u':
                                {
                                    int res = (hexToint[token[i + 1]] << 12) + (hexToint[token[i + 2]] << 8) + (hexToint[token[i + 3]] << 4) + hexToint[token[i + 4]];
                                    sb.Append((char)res);
                                    i += 4;
                                }
                                break;
                            default: throw new Exception();
                        }
                    }
                    else
                        sb.Append(token[i]);
                    i++;
                }
                parts.Add(sb.ToString());
            }
        }

        public CustomValue EvaluateExpression(Context context)
        {
            var sb = new StringBuilder();
            foreach (var item in parts)
            {
                if (item is string s)
                {
                    sb.Append(s);
                }
                else
                {
                    var value = ((Expression)item).EvaluateExpression(context);
                    sb.Append(value.ToString());
                }
            }
            return CustomValue.FromParsedString(sb.ToString());
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
            return context.variableScope.GetVariable(token);
        }
    }
    class ThisExpression : Expression
    {
        public CustomValue EvaluateExpression(Context context)
        {
            return context.thisOwner;
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
    class SinglePlusMinusExpression : HasRestExpression
    {
        private bool isMinus;
        private Expression expressionRest;

        public Expression ExpressionRest { get { return expressionRest; } set { expressionRest = value; } }

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
    class NotExpression : HasRestExpression
    {
        private Expression expressionRest;

        public Expression ExpressionRest { get { return expressionRest; } set { expressionRest = value; } }

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
    class PrePostIncDecExpression : HasRestExpression
    {
        private Expression expressionRest;
        bool isPre;
        bool isInc;

        public Expression ExpressionRest { get { return expressionRest; } set { expressionRest = value; } }

        public PrePostIncDecExpression(Expression expressionRest, bool isPre, bool isInc)
        {
            this.expressionRest = expressionRest;
            this.isPre = isPre;
            this.isInc = isInc;
        }

        public CustomValue EvaluateExpression(Context context)
        {
            CustomValue value = CustomValue.FromNumber(1);
            Func<CustomValue?, CustomValue> operation = existingValue => AddOrSubtract(existingValue.Value, isInc ? Operator.Plus : Operator.Minus, value);

            var newValue = ApplyLValueOperation(expressionRest, operation, needOldValue: true, context, out var oldValue);

            return isPre ? newValue : oldValue.Value;
        }
    }
    class AwaitExpression : HasRestExpression
    {
        private Expression expressionRest;

        public Expression ExpressionRest { get { return expressionRest; } set { expressionRest = value; } }

        public AwaitExpression(Expression expressionRest)
        {
            this.expressionRest = expressionRest;
        }

        public CustomValue EvaluateExpression(Context context)
        {
            var rest = expressionRest.EvaluateExpression(context);

            while (rest.type == ValueType.Promise)
            {
                var task = (Task<CustomValue>)rest.value;
                rest = task.Result;
            }

            return rest;
        }
    }
    class NewClassInstanceExpression : Expression
    {
        private string className;
        private List<(bool hasThreeDot, Expression expression)> expressionList;

        public NewClassInstanceExpression(string className, ArraySegment<string> tokens)
        {
            this.className = className;
            expressionList = GetArguments(tokens);
        }

        public CustomValue EvaluateExpression(Context context)
        {
            var classObject = context.variableScope.GetVariable(className);
            if (classObject.type != ValueType.Class)
                throw new Exception();

            var cl = (IClassInfoObject)classObject.value;
            var newObject = CustomValue.FromMap(new Dictionary<string, ValueOrGetterSetter>(), className);
            if (cl.Constructor != null)
            {
                var constructorReturn = EvaluateFunctionCall(context, cl.Constructor, expressionList, newObject);
                if (constructorReturn.type == ValueType.Null)
                    return newObject;
                else
                    return constructorReturn;
            }

            return newObject;
        }
    }
    class MapAssignmentExpression : Expression
    {
        public (string sourceName, string targetName)[] variableNames;
        public Expression rValue;
        public AssignmentType assignmentType;

        public MapAssignmentExpression((string sourceName, string targetName)[] variableNames, Expression rValue, AssignmentType assignmentType)
        {
            this.variableNames = variableNames;
            this.rValue = rValue;
            this.assignmentType = assignmentType;
        }

        public CustomValue EvaluateExpression(Context context)
        {
            CustomValue mapValue = rValue.EvaluateExpression(context);
            if (mapValue.type != ValueType.Map)
                throw new Exception();
            var underlyingMap = mapValue.GetAsMap();
            foreach (var (sourceName, targetName) in variableNames)
            {
                if (!underlyingMap.TryGetValue(sourceName, out var value))
                {
                    context.variableScope.AssignVariable(assignmentType, targetName, CustomValue.Null);
                }
                else
                {
                    if (value is CustomValue customValue)
                    {
                        context.variableScope.AssignVariable(assignmentType, targetName, customValue);
                    }
                    else
                    {
                        var getterSetter = (GetterSetter)value;
                        var res = CallFunction(getterSetter.GetGetter(), new List<CustomValue>(), mapValue);
                        context.variableScope.AssignVariable(assignmentType, targetName, res);
                    }
                }
            }

            return CustomValue.Null;
        }

        public static MapAssignmentExpression FromBody(ArraySegment<string> tokens, int braceEndIndex, AssignmentType assignmentType)
        {
            var variableGroups = SplitBy(tokens[1..braceEndIndex], ",", allowTrailing: true);
            var variableNames = variableGroups.Select(x =>
            {
                if (x.Count == 1)
                    return (x[0], x[0]);
                if (x.Count == 3 && x[1] == ":")
                    return (x[0], x[2]);
                throw new Exception();
            }).ToArray();

            var rValueTokens = tokens[(braceEndIndex + 2)..];
            var rvalueExpression = ExpressionMethods.New(rValueTokens);
            return new MapAssignmentExpression(variableNames, rvalueExpression, assignmentType);
        }
    }
    class ArrayAssignmentExpression : Expression
    {
        public (string varName, bool isRest)[] variableNames;
        public Expression rValue;
        public AssignmentType assignmentType;

        public ArrayAssignmentExpression((string, bool)[] variableNames, Expression rValue, AssignmentType assignmentType)
        {
            this.variableNames = variableNames;
            this.rValue = rValue;
            this.assignmentType = assignmentType;
        }

        public CustomValue EvaluateExpression(Context context)
        {
            CustomValue mapValue = rValue.EvaluateExpression(context);
            if (mapValue.type != ValueType.Array)
                throw new Exception();
            var underlyingArray = ((CustomArray)mapValue.value).list;

            for (int i = 0; i < variableNames.Length; i++)
            {
                var (varName, isRest) = variableNames[i];
                if (!isRest)
                {
                    var value = i < underlyingArray.Count ? underlyingArray[i] : CustomValue.Null;
                    context.variableScope.AssignVariable(assignmentType, varName, value);
                }
                else
                {
                    var values = underlyingArray.Skip(i).ToList();
                    var value = CustomValue.FromArray(new CustomArray(values));
                    context.variableScope.AssignVariable(assignmentType, varName, value);
                }
            }

            return CustomValue.Null;
        }

        public static ArrayAssignmentExpression FromBody(ArraySegment<string> tokens, int bracketsEndIndex, AssignmentType assignmentType)
        {
            var variableGroups = SplitBy(tokens[1..bracketsEndIndex], ",", allowTrailing: true);
            var variableNames = variableGroups.Select(x =>
            {
                if (x.Count == 1)
                    return (x[0], false);
                if (x.Count == 2 && x[0] == "...")
                    return (x[1], true);
                throw new Exception();
            }).ToArray();

            var rValueTokens = tokens[(bracketsEndIndex + 2)..];
            var rvalueExpression = ExpressionMethods.New(rValueTokens);
            return new ArrayAssignmentExpression(variableNames, rvalueExpression, assignmentType);
        }
    }
    class AssignmentExpression : Expression
    {
        public Expression lValue;
        public string assignmentOperator;
        public Expression rValue;
        public AssignmentType assignmentType;

        public AssignmentExpression(Expression lValue, string assignmentOperator, Expression valueExpression, AssignmentType assignmentType)
        {
            if (!assignmentSet.Contains(assignmentOperator))
                throw new Exception();

            this.lValue = lValue;
            this.assignmentOperator = assignmentOperator;
            this.rValue = valueExpression;
            this.assignmentType = assignmentType;
        }

        public AssignmentExpression(Expression lValue, string assignmentOperator, ArraySegment<string> rValueTokens, AssignmentType assignmentType)
            : this(lValue, assignmentOperator, ExpressionMethods.New(rValueTokens), assignmentType)
        {
        }

        public AssignmentExpression(string variableName, string assignmentOperator, Expression valueExpression, AssignmentType assignmentType)
            : this(new SingleTokenVariableExpression(variableName), assignmentOperator, valueExpression, assignmentType)
        {
        }

        public AssignmentExpression(string variableName, string assignmentOperator, ArraySegment<string> rValueTokens, AssignmentType assignmentType)
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
                Func<CustomValue?, CustomValue> operation;
                bool needOldValue;

                switch (assignmentOperator)
                {
                    case "=":
                        operation = existingValue => rValue.EvaluateExpression(context);
                        needOldValue = false;
                        break;
                    case "+=":
                        operation = existingValue => AddOrSubtract(existingValue.Value, Operator.Plus, rValue.EvaluateExpression(context));
                        needOldValue = true;
                        break;
                    case "-=":
                        operation = existingValue => AddOrSubtract(existingValue.Value, Operator.Minus, rValue.EvaluateExpression(context));
                        needOldValue = true;
                        break;
                    case "*=":
                        operation = existingValue => MultiplyOrDivide(existingValue.Value, Operator.Multiply, rValue.EvaluateExpression(context));
                        needOldValue = true;
                        break;
                    case "/=":
                        operation = existingValue => MultiplyOrDivide(existingValue.Value, Operator.Divide, rValue.EvaluateExpression(context));
                        needOldValue = true;
                        break;
                    case "%=":
                        operation = existingValue => MultiplyOrDivide(existingValue.Value, Operator.Modulus, rValue.EvaluateExpression(context));
                        needOldValue = true;
                        break;
                    case "&&=":
                        operation = existingValue => !existingValue.Value.IsTruthy() ? existingValue.Value : rValue.EvaluateExpression(context);
                        needOldValue = true;
                        break;
                    case "||=":
                        operation = existingValue => existingValue.Value.IsTruthy() ? existingValue.Value : rValue.EvaluateExpression(context);
                        needOldValue = true;
                        break;
                    case "??=":
                        operation = existingValue => existingValue.Value.type != ValueType.Null ? existingValue.Value : rValue.EvaluateExpression(context);
                        needOldValue = true;
                        break;
                    default:
                        throw new Exception();
                }

                return ApplyLValueOperation(lValue, operation, needOldValue, context, out var _);
            }
        }

        public static Expression FromVarStatement(ArraySegment<string> tokens, AssignmentType assignmentType)
        {
            if (tokens.Count == 1 || tokens[1] == "=")
            {
                var variableName = tokens[0];
                if (tokens.Count == 1)
                    return new AssignmentExpression(variableName, "=", nullExpression, assignmentType);
                else
                    return new AssignmentExpression(variableName, tokens[1], tokens[2..], assignmentType);
            }
            else if (tokens[0] == "{")
            {
                var braceEndIndex = tokens.IndexOf("}", 1);
                if (braceEndIndex < 1)
                    throw new Exception();
                if (tokens[braceEndIndex + 1] != "=")
                    throw new Exception();

                return MapAssignmentExpression.FromBody(tokens, braceEndIndex, assignmentType);
            }
            else if (tokens[0] == "[")
            {
                var bracketEndIndex = tokens.IndexOf("]", 1);
                if (bracketEndIndex < 1)
                    throw new Exception();
                if (tokens[bracketEndIndex + 1] != "=")
                    throw new Exception();

                return ArrayAssignmentExpression.FromBody(tokens, bracketEndIndex, assignmentType);
            }
            else
            {
                throw new Exception();
            }
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
        IEnumerable<CustomValue> AsEnumerable(Context context);
    }
    static class StatementMethods
    {
        public static Statement New(ArraySegment<string> tokens)
        {
            switch (tokens[0])
            {
                case "{":
                    {
                        if (tokens[^1] != "}")
                            throw new Exception();
                        return new BlockStatement(tokens);
                    }
                case "else":
                    {
                        if (tokens[1] == "if")
                            return new ElseIfStatement(tokens);
                        else
                            throw new Exception();
                    }
                case "yield":
                    return new YieldStatement(tokens);
                default:
                    return new LineStatement(tokens);
            }
        }
        internal static (ArraySegment<string>, ArraySegment<string>) GetTokensConditionAndBody(ArraySegment<string> tokens, int conditionStartIndex)
        {
            var endOfParentheses = tokens.IndexOfParenthesesEnd(conditionStartIndex);
            if (endOfParentheses < 0)
                throw new Exception();
            var conditionTokens = tokens[conditionStartIndex..endOfParentheses];

            var statementTokens = tokens[(endOfParentheses + 1)..];

            return (conditionTokens, statementTokens);
        }
        internal static (Expression, Statement) GetConditionAndBody(ArraySegment<string> tokens, int conditionStartIndex)
        {
            var (conditionTokens, statementTokens) = GetTokensConditionAndBody(tokens, conditionStartIndex);
            var conditionExpression = ExpressionMethods.New(conditionTokens);
            var statement = StatementMethods.New(statementTokens);

            return (conditionExpression, statement);
        }
    }
    class LineStatement : Statement
    {
        Func<Context, (CustomValue value, bool isReturn, bool isBreak, bool isContinue)> eval;

        public LineStatement(ArraySegment<string> tokens)
        {
            if (tokens[^1] == ";")
            {
                tokens = tokens[..^1];
            }

            if (tokens.Count == 0)
            {
                eval = context => (CustomValue.Null, false, false, false);
            }
            else if (IsAssignmentType(tokens[0], out var assignmentType))
            {
                // Assignment to new variable
                var assignmentTree = AssignmentExpression.FromVarStatement(tokens[1..], assignmentType);

                eval = context =>
                {
                    var value = assignmentTree.EvaluateExpression(context);
                    return (CustomValue.Null, false, false, false);
                };
            }
            else if (tokens[0] == "return")
            {
                if (tokens.Count == 1)
                {
                    eval = context => (CustomValue.Null, true, false, false);
                }
                else
                {
                    var returnExpression = ExpressionMethods.New(tokens[1..]);
                    eval = context =>
                    {
                        var returnValue = returnExpression.EvaluateExpression(context);
                        return (returnValue, true, false, false);
                    };
                }
            }
            else if (tokens[0] == "break")
            {
                eval = context => (CustomValue.Null, false, true, false);
            }
            else if (tokens[0] == "continue")
            {
                eval = context => (CustomValue.Null, false, false, true);
            }
            else if (tokens[0] == "function" || (tokens[0] == "async" && tokens[1] == "function"))
            {
                bool isAsync = tokens[0] == "async";
                if (isAsync)
                    tokens = tokens[1..];

                bool isGenerator = false;
                if (tokens[1] == "*")
                {
                    isGenerator = true;
                }
                int tokenShiftIndex = isGenerator ? 1 : 0;

                if (!IsVariableName(tokens[1 + tokenShiftIndex]))
                    throw new Exception();

                var variableName = tokens[1 + tokenShiftIndex];
                var functionStatement = FunctionStatement.FromTokens(tokens, isLambda: false, isAsync: isAsync, isGenerator: isGenerator);

                eval = context =>
                {
                    var function = functionStatement.EvaluateExpression(context);
                    context.variableScope.AddVarVariable(variableName, function);
                    return (CustomValue.Null, false, false, false);
                };
            }
            else if (tokens[0] == "class")
            {
                var className = tokens[1];
                if (!IsVariableName(className))
                    throw new Exception();
                if (tokens[2] != "{")
                    throw new Exception();
                if (tokens[^1] != "}")
                    throw new Exception();

                var classBodyTokens = tokens[3..^1];
                FunctionStatement? constructor = null;
                var methodList = new List<(FunctionStatement, string fieldName, bool isGetProp, bool isSetProp)>();
                int bodyIndex = 0;
                while (bodyIndex < classBodyTokens.Count)
                {
                    if (classBodyTokens[bodyIndex] == "constructor")
                    {
                        if (constructor != null)
                            throw new Exception("The constructor was already defined");

                        var braceBegin = classBodyTokens.IndexOf("{", bodyIndex + 1);
                        if (braceBegin < 0)
                            throw new Exception();
                        var braceEnd = classBodyTokens.IndexOfBracesEnd(braceBegin + 1);
                        if (braceEnd < 0)
                            throw new Exception();
                        constructor = FunctionStatement.FromTokens(classBodyTokens[bodyIndex..(braceEnd + 1)], isLambda: false, isAsync: false, isGenerator: false);
                        bodyIndex = braceEnd + 1;
                    }
                    else
                    {
                        // handle functions
                        var braceBegin = classBodyTokens.IndexOf("{", bodyIndex + 1);
                        if (braceBegin < 0)
                            throw new Exception();
                        var braceEnd = classBodyTokens.IndexOfBracesEnd(braceBegin + 1);
                        if (braceEnd < 0)
                            throw new Exception();
                        bool isGetProp = false;
                        bool isSetProp = false;
                        var functionTokens = classBodyTokens[bodyIndex..(braceEnd + 1)];
                        string fieldName = functionTokens[0];
                        var res = Interpreter.ExtractProperty(functionTokens, ref fieldName, ref isGetProp, ref isSetProp);
                        methodList.Add((res, fieldName, isGetProp, isSetProp));
                        bodyIndex = braceEnd + 1;
                    }
                }

                var classStatement = new ClassStatement(constructor, methodList);
                eval = context =>
                {
                    var newClass = classStatement.EvaluateExpression(context);
                    context.variableScope.AddConstVariable(className, newClass);
                    return (CustomValue.Null, false, false, false);
                };
            }
            else
            {
                var expression = GetExpression(tokens);

                eval = context =>
                {
                    var expressionValue = expression.EvaluateExpression(context);
                    return (expressionValue, false, false, false);
                };
            }
        }

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            return eval(context);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            eval(context);
            yield break;
        }
    }
    class BlockStatement : Statement
    {
        Slice<Statement> statements;

        public BlockStatement(ArraySegment<string> tokens)
        {
            if (tokens[0] != "{")
                throw new Exception();
            tokens = tokens[1..^1];
            statements = GetStatements(tokens);
        }

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var newScope = VariableScope.NewWithInner(context.variableScope, isFunctionScope: false);
            var newContext = new Context(newScope, context.thisOwner);

            for (int i = 0; i < statements.count; i++)
            {
                var (value, isReturn, isBreak, isContinue) = statements.array[i].EvaluateStatement(newContext);
                if (isReturn)
                    return (value, true, false, false);
                if (isBreak)
                    return (CustomValue.Null, false, true, false);
                if (isContinue)
                    return (CustomValue.Null, false, false, true);
            }

            return (CustomValue.Null, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            for (int i = 0; i < statements.count; i++)
                foreach (var value in statements.array[i].AsEnumerable(context))
                    yield return value;
        }
    }
    class WhileStatement : Statement
    {
        Expression conditionExpression;
        Statement statement;

        public WhileStatement(Expression conditionExpression, Statement statement)
        {
            this.conditionExpression = conditionExpression;
            this.statement = statement;
        }

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

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            while (true)
            {
                var conditionValue = conditionExpression.EvaluateExpression(context);
                if (!conditionValue.IsTruthy())
                    break;

                foreach (var value in statement.AsEnumerable(context))
                    yield return value;
            }
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

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            // Body of this function is duplicated into "AsEnumerable" method, when changed make sure to change that method too
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
                if (assignmentType == AssignmentType.Let)
                    loopScope.ApplyToScope(newScope);

                if (isReturn)
                    return (value, true, false, false);
                if (isBreak)
                    break;

                // Do iteration
                foreach (var iterationStatement in iterationStatements)
                {
                    iterationStatement.EvaluateStatement(newContext);
                }
            }

            return (CustomValue.Null, false, false, false);
        }

        public static Statement FromTokens(ArraySegment<string> nonBodyTokens, Statement bodyStatement)
        {
            bool isAwaitFor = false;
            if (nonBodyTokens[1] == "await")
            {
                nonBodyTokens = nonBodyTokens[1..];
                isAwaitFor = true;
            }

            if (nonBodyTokens[1] != "(")
                throw new Exception();
            var expressionTokens = nonBodyTokens[2..^1];
            var expressions = SplitBy(expressionTokens, ";", allowTrailing: false).ToList();
            if (expressions.Count == 3)
            {
                // Normal for loop
                if (isAwaitFor)
                    throw new Exception();

                var allInitializationTokens = expressions[0];
                AssignmentType assignmentType = AssignmentType.None;
                var isNewAssignment = allInitializationTokens.Count > 0 && IsAssignmentType(allInitializationTokens[0], out assignmentType);
                var assignmentTokens = isNewAssignment ? allInitializationTokens[1..] : allInitializationTokens;
                var initializationTokenGroup = SplitBy(assignmentTokens, ",", allowTrailing: false).ToList();
                var initializationStatements = initializationTokenGroup.SelectFast(x => AssignmentExpression.FromVarStatement(x, assignmentType));

                var conditionTokens = expressions[1];
                var conditionExpression = conditionTokens.Count > 0 ? ExpressionMethods.New(conditionTokens) : trueExpression;

                var allIterationTokens = expressions[2];
                var iterationTokenGroup = SplitBy(allIterationTokens, ",", allowTrailing: false).ToList();
                var iterationStatements = iterationTokenGroup.SelectFast(StatementMethods.New);

                return new ForStatement(assignmentType, initializationStatements, conditionExpression, iterationStatements, bodyStatement);
            }
            else if (expressions.Count == 1)
            {
                var parenthesesTokens = expressions[0];
                var isNewAssignment = IsAssignmentType(parenthesesTokens[0], out var assignmentType);
                var index = isNewAssignment ? 1 : 0;
                var variableName = parenthesesTokens[index];
                var operationType = parenthesesTokens[index + 1];
                var restTokens = parenthesesTokens[(index + 2)..];
                var restExpression = ExpressionMethods.New(restTokens);

                if (operationType == "in")
                {
                    return new ForInOfStatement(true, assignmentType, variableName, restExpression, bodyStatement, isAwaitFor);
                }
                else if (operationType == "of")
                {
                    return new ForInOfStatement(false, assignmentType, variableName, restExpression, bodyStatement, isAwaitFor);
                }
                throw new Exception();
            }
            throw new Exception();
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
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

                foreach (var value in bodyStatement.AsEnumerable(loopContext))
                    yield return value;

                if (assignmentType == AssignmentType.Let)
                    loopScope.ApplyToScope(newScope);

                // Do iteration
                foreach (var iterationStatement in iterationStatements)
                {
                    iterationStatement.EvaluateStatement(newContext);
                }
            }
        }
    }
    class ForInOfStatement : Statement
    {
        AssignmentType assignmentType;
        string variableName;
        Expression sourceExpression;
        Statement bodyStatement;
        bool isInStatement;
        bool isAwait;

        public ForInOfStatement(bool isInStatement, AssignmentType assignmentType, string variableName, Expression sourceExpression, Statement bodyStatement, bool isAwait)
        {
            this.assignmentType = assignmentType;
            this.variableName = variableName;
            this.sourceExpression = sourceExpression;
            this.bodyStatement = bodyStatement;
            this.isInStatement = isInStatement;
            this.isAwait = isAwait;
        }

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var (scope, elements) = GetElementsAndContext(context);

            if (!isAwait)
            {
                foreach (var element in (IEnumerable<CustomValue>)elements)
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
            }
            else
            {
                foreach (var task in (IEnumerable<Task<(bool, CustomValue)>>)elements)
                {
                    var (success, element) = task.Result;
                    if (!success)
                        break;

                    var loopScope = VariableScope.GetNewLoopScope(scope, assignmentType, variableName, element);

                    var (value, isReturn, isBreak, isContinue) = bodyStatement.EvaluateStatement(new Context(loopScope, context.thisOwner));
                    if (isReturn)
                        return (value, true, false, false);
                    if (isBreak)
                        break;
                    if (isContinue)
                        continue;
                }
            }

            return (CustomValue.Null, false, false, false);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            var (scope, elements) = GetElementsAndContext(context);

            if (!isAwait)
            {
                foreach (var element in (IEnumerable<CustomValue>)elements)
                {
                    var loopScope = VariableScope.GetNewLoopScope(scope, assignmentType, variableName, element);

                    foreach (var value in bodyStatement.AsEnumerable(new Context(loopScope, context.thisOwner)))
                    {
                        yield return value;
                    }
                }
            }
            else
            {
                foreach (var task in (IEnumerable<Task<(bool, CustomValue)>>)elements)
                {
                    var (success, element) = task.Result;
                    if (!success)
                        break;

                    var loopScope = VariableScope.GetNewLoopScope(scope, assignmentType, variableName, element);

                    foreach (var value in bodyStatement.AsEnumerable(new Context(loopScope, context.thisOwner)))
                    {
                        yield return value;
                    }
                }
            }
        }

        private (VariableScope, object) GetElementsAndContext(Context context)
        {
            VariableScope scope;

            bool isOfStatement = !isInStatement;
            scope = context.variableScope;
            var sourceValue = sourceExpression.EvaluateExpression(context);
            if (isInStatement)
            {
                var map = sourceValue.GetAsMap();
                var keys = map.Keys;
                return (scope, keys.Select(CustomValue.FromParsedString));
            }
            else if (isOfStatement)
            {
                if (sourceValue.type == ValueType.String && !isAwait)
                {
                    var str = (string)sourceValue.value;
                    return (scope, StringAsEnumerable(str));
                }
                else if (sourceValue.type == ValueType.Array && !isAwait)
                {
                    var array = (CustomArray)sourceValue.value;
                    return (scope, array.list);
                }
                else if (sourceValue.type == ValueType.Generator && !isAwait)
                {
                    var generator = (Generator)sourceValue.value;
                    return (scope, generator);
                }
                else if (sourceValue.type == ValueType.AsyncGenerator && isAwait)
                {
                    var asyncGenerator = (AsyncGenerator)sourceValue.value;
                    return (scope, asyncGenerator);
                }
                else
                    throw new Exception();
            }
            else
                throw new Exception();
        }
    }
    class IfStatement : Statement
    {
        Expression conditionExpression;
        internal Statement statementOfIf;
        internal List<(Expression condition, Statement statement)> elseIfStatements = new();
        Statement? elseStatement;

        public IfStatement(Expression conditionExpression, Statement statementOfIf)
        {
            this.conditionExpression = conditionExpression;
            this.statementOfIf = statementOfIf;
        }

        internal void AddElseIf(Statement statementAfterIf)
        {
            if (elseStatement != null)
                throw new Exception();
            var elseIf = (ElseIfStatement)statementAfterIf;
            elseIfStatements.Add((elseIf.condition, elseIf.statement));
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

            foreach (var elseIfStatement in elseIfStatements)
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

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            var conditionValue = conditionExpression.EvaluateExpression(context);
            if (conditionValue.IsTruthy())
                return statementOfIf.AsEnumerable(context);

            foreach (var elseIfStatement in elseIfStatements)
            {
                var elseIfCondition = elseIfStatement.condition.EvaluateExpression(context);
                if (elseIfCondition.IsTruthy())
                    return elseIfStatement.statement.AsEnumerable(context);
            }

            if (elseStatement != null)
                return elseStatement.AsEnumerable(context);

            throw new Exception();
        }
    }
    class ElseIfStatement : Statement
    {
        internal Expression condition;
        internal Statement statement;

        public ElseIfStatement(ArraySegment<string> tokens)
        {
            if (tokens[2] != "(")
                throw new Exception();
            var conditionStartIndex = 3;
            var (condition, statement) = StatementMethods.GetConditionAndBody(tokens, conditionStartIndex);
            this.condition = condition;
            this.statement = statement;
        }

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            return statement.EvaluateStatement(context);
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            return statement.AsEnumerable(context);
        }
    }
    class FunctionStatement : Statement, Expression
    {
        IReadOnlyList<(string paramName, bool isRest)> parametersList;
        Statement body;
        bool isLambda;
        bool isAsync;
        bool isGenerator;

        private FunctionStatement(IReadOnlyList<(string paramName, bool isRest)> parametersList, Statement body, bool isLambda, bool isAsync, bool isGenerator)
        {
            if (!isLambda && body is LineStatement)
                throw new Exception();
            this.body = body;
            this.isLambda = isLambda;
            this.isAsync = isAsync;
            this.isGenerator = isGenerator;
            this.parametersList = parametersList;
        }

        private FunctionStatement(string singleParameter, Statement body, bool isLambda, bool isAsync, bool isGenerator) :
            this(GetParameterList(singleParameter), body, isLambda, isAsync, isGenerator)
        {
        }

        private FunctionStatement(ArraySegment<string> parameters, Statement body, bool isLambda, bool isAsync, bool isGenerator) :
            this(GetParameterList(parameters), body, isLambda, isAsync, isGenerator)
        {
        }

        private static IReadOnlyList<(string paramName, bool isRest)> GetParameterList(string singleParameter)
        {
            // Prepare parameters
            return new (string, bool)[] { (singleParameter, false) };
        }

        private static IReadOnlyList<(string paramName, bool isRest)> GetParameterList(ArraySegment<string> parameters)
        {
            if (parameters.Count > 0 && parameters[0] == "(")
                throw new Exception();

            // Prepare parameters
            var parametersList = new List<(string, bool)>();

            var parameterGroups = SplitBy(parameters, ",", allowTrailing: true).ToList();
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
                parametersList.Add((parameter, isRest));
            }
            return parametersList;
        }

        public static FunctionStatement FromParametersAndBody(string singleParameter, ArraySegment<string> bodyTokens, bool isLambda, bool isAsync, bool isGenerator)
        {
            var body = StatementMethods.New(bodyTokens);
            return new FunctionStatement(singleParameter, body, isLambda, isAsync, isGenerator);
        }

        public static FunctionStatement FromParametersAndBody(ArraySegment<string> parameterTokens, ArraySegment<string> bodyTokens, bool isLambda, bool isAsync, bool isGenerator)
        {
            var body = StatementMethods.New(bodyTokens);
            return new FunctionStatement(parameterTokens, body, isLambda, isAsync, isGenerator);
        }

        public static FunctionStatement FromTokens(ArraySegment<string> tokens, bool isLambda, bool isAsync, bool isGenerator)
        {
            var parenthesesIndex = tokens.IndexOf("(", 0);
            if (parenthesesIndex < 0)
                throw new Exception();
            var (parameters, bodyTokens) = StatementMethods.GetTokensConditionAndBody(tokens, parenthesesIndex + 1);
            var body = StatementMethods.New(bodyTokens);
            return new FunctionStatement(parameters, body, isLambda, isAsync, isGenerator);
        }

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var function = new CustomFunction(parametersList, body, context.variableScope, isLambda, isAsync, isGenerator);
            return (CustomValue.FromFunction(function), false, false, false);
        }

        public CustomValue EvaluateExpression(Context context)
        {
            return EvaluateStatement(context).Item1;
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new Exception();
        }
    }
    class ClassStatement : Statement, Expression
    {
        private FunctionStatement? constructor;
        private List<(FunctionStatement, string fieldName, bool isGetProp, bool isSetProp)> methodList;

        public ClassStatement(FunctionStatement? constructor, List<(FunctionStatement, string fieldName, bool isGetProp, bool isSetProp)> methodList)
        {
            this.constructor = constructor;
            this.methodList = methodList;
        }

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            var methods = new Dictionary<string, ValueOrGetterSetter>();
            foreach (var (fStatement, name, isGet, isSet) in methodList)
            {
                var f = fStatement.EvaluateExpression(context);
                if (isGet || isSet)
                {
                    var func = (FunctionObject)f.value;
                    if (methods.TryGetValue(name, out var getterSetter))
                    {
                        if (isGet)
                            methods[name] = ((GetterSetter)getterSetter).WithNewGetter(func);
                        else if (isSet)
                            methods[name] = ((GetterSetter)getterSetter).WithNewSetter(func);
                    }
                    else
                    {
                        methods.Add(name, new GetterSetter(isGet ? func : null, isSet ? func : null));
                    }
                }
                else
                {
                    methods.Add(name, f);
                }
            }

            var newClass = new ClassObject(constructor == null ? null : (FunctionObject)constructor.EvaluateExpression(context).value, methods);
            return (CustomValue.FromClass(newClass), false, false, false);
        }

        public CustomValue EvaluateExpression(Context context)
        {
            return EvaluateStatement(context).Item1;
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            throw new NotImplementedException();
        }
    }
    class YieldStatement : Statement
    {
        Expression expression;
        bool isMultiYield = false;

        public YieldStatement(ArraySegment<string> tokens)
        {
            tokens = tokens[1..]; // Remove "yield"

            if (tokens[^1] == ";")
            {
                tokens = tokens[..^1];
            }
            if (tokens[0] == "*")
            {
                isMultiYield = true;
                tokens = tokens[1..];
            }

            this.expression = ExpressionMethods.New(tokens);
        }

        public (CustomValue value, bool isReturn, bool isBreak, bool isContinue) EvaluateStatement(Context context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CustomValue> AsEnumerable(Context context)
        {
            if (!isMultiYield)
                yield return expression.EvaluateExpression(context);
            else
            {
                var multiValue = expression.EvaluateExpression(context);
                foreach (var value in multiValue.AsMultiValue())
                    yield return value;
            }
        }
    }

    struct Slice<T>
    {
        internal T[] array;
        internal int count;

        public Slice(int initialSize)
        {
            this.array = new T[initialSize];
            this.count = 0;
        }

        public void Add(T item)
        {
            if (count == array.Length)
            {
                var newArray = new T[array.Length * 2];
                Array.Copy(array, 0, newArray, 0, array.Length);
                array = newArray;
            }

            array[count++] = item;
        }

        public ArraySegment<T> ToSegment()
        {
            return ((ArraySegment<T>)array)[..count];
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

    public static int IndexOfParenthesesEnd(this ArraySegment<string> source, int startIndex)
    {
        return IndexOfPairsEnd(source, startIndex, "(", ")");
    }

    public static int IndexOfBracesEnd(this ArraySegment<string> source, int startIndex)
    {
        return IndexOfPairsEnd(source, startIndex, "{", "}");
    }

    public static int IndexOfBracketsEnd(this ArraySegment<string> source, int startIndex)
    {
        return IndexOfPairsEnd(source, startIndex, "[", "]");
    }

    private static int IndexOfPairsEnd(this ArraySegment<string> source, int startIndex, string first, string last)
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

    public static new bool Equals(object? result, object? expected)
    {
        if (result is null)
            return expected is null;
        if (expected is null)
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