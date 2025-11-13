using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CasualConsoleCore.Interpreter;

public class NewInterpreter
{
    private readonly Dictionary<string, CustomValue> variables;

    private static Expression nullExpression;
    private static Expression trueExpression;
    private static Expression falseExpression;

    private static readonly HashSet<string> operators = new HashSet<string>() { "+", "-", "*", "/", "%", "=", "?", ":", "<", ">", "<=", ">=", "&&", "||", "??", "!", "!=", ".", "==", "+=", "-=", "*=", "/=", "%=", "??=", "||=", "&&=", "=>", "++", "--", "...", "?.", "?.[", "?.(" };
    private static readonly Dictionary<char, Dictionary<char, HashSet<char>>> operatorsCompiled;
    private static readonly Dictionary<char, int> hexToint = new Dictionary<char, int>() { { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 }, { '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 }, { 'A', 10 }, { 'B', 11 }, { 'C', 12 }, { 'D', 13 }, { 'E', 14 }, { 'F', 15 }, { 'a', 10 }, { 'b', 11 }, { 'c', 12 }, { 'd', 13 }, { 'e', 14 }, { 'f', 15 }, };
    private static readonly Dictionary<char, string> onlyChars = new Dictionary<char, string>() {
        { ',', "," },
        { ';', ";" },
        { '(', "(" },
        { ')', ")" },
        { '{', "{" },
        { '}', "}" },
        { '[', "[" },
        { ']', "]" },
    };

    static NewInterpreter()
    {
        nullExpression = new ValueExpression(CustomValue.Null);
        trueExpression = new ValueExpression(CustomValue.True);
        falseExpression = new ValueExpression(CustomValue.False);

        operatorsCompiled = operators.GroupBy(x => x[0]).ToDictionary(x => x.Key, x => x.Where(y => y.Length > 1).GroupBy(y => y[1]).ToDictionary(y => y.Key, y => y.Where(z => z.Length > 2).Select(z => z[2]).ToHashSet()));
    }

    public NewInterpreter()
    {
        variables = new();
    }

    public object? RunCode(ReadOnlySpan<char> code)
    {
        var context = new Context(variables);
        var expressions = GetStatements(code);

        CustomValue lastValue = CustomValue.Null;
        ResultType resultType;
        foreach (var expression in expressions)
        {
            (lastValue, resultType) = expression.Run(context);
            if (resultType != ResultType.Normal)
                throw new Exception();
        }

        return lastValue.value;
    }

    private static List<Statement> GetStatements(ReadOnlySpan<char> code)
    {
        var tokenizer = new Tokenizer(code);

        var statements = new List<Statement>();

        while (ReadStatement(ref tokenizer, out var expr))
        {
            statements.Add(expr);
        }

        return statements;
    }

    private static bool ReadStatement(ref Tokenizer tokenizer, [NotNullWhen(true)] out Statement? statement)
    {
        if (!tokenizer.TryReadToken(out var firstToken))
        {
            statement = null;
            return false;
        }

        return ReadStatement(firstToken, ref tokenizer, out statement);
    }

    private static bool ReadStatement(ReadOnlySpan<char> firstToken, ref Tokenizer tokenizer, [NotNullWhen(true)] out Statement? statement)
    {
        switch (firstToken)
        {
            case "var":
                statement = VarAssignmentExpression.New(ref tokenizer);
                return true;
            case "while":
                var (ifConditionExpression, ifBodyStatement) = ReadIfOnce(ref tokenizer);
                var whileStatement = new WhileStatement(ifConditionExpression, ifBodyStatement);
                statement = whileStatement;
                return true;
            case "if":
                var (conditionExpression, bodyStatement) = ReadIfOnce(ref tokenizer);
                var ifStatement = new IfStatement(conditionExpression, bodyStatement);

                while (true)
                {
                    var maybeElseToken = tokenizer.ReadToken();
                    if (!maybeElseToken.SequenceEqual("else"))
                    {
                        tokenizer.GiveBack(maybeElseToken);
                        statement = ifStatement;
                        return true;
                    }
                    var maybeIfToken = tokenizer.ReadToken();
                    if (maybeIfToken.SequenceEqual("if"))
                    {
                        ifStatement.AddElseIf(ReadIfOnce(ref tokenizer));
                        continue;
                    }
                    else
                    {
                        if (!ReadStatement(maybeIfToken, ref tokenizer, out var elseBodyStatement))
                            throw new Exception();
                        ifStatement.SetElse(elseBodyStatement);
                        statement = ifStatement;
                        return true;
                    }
                }
            case "{":
                statement = new BlockStatement(firstToken, ref tokenizer);
                return true;
            case "break":
                if (!tokenizer.ReadToken().SequenceEqual(";"))
                    throw new Exception();
                statement = BreakStatement.instance;
                return true;
            case "continue":
                if (!tokenizer.ReadToken().SequenceEqual(";"))
                    throw new Exception();
                statement = ContinueStatement.instance;
                return true;
            case "function":
                var functionName = tokenizer.ReadToken();
                if (!IsVariableName(functionName))
                    throw new Exception();
                var f = ReadFunction(ref tokenizer); // Read function statement
                statement = new FunctionStatement(functionName.ToString(), f);
                return true;
            case "return":
                var semiColon = tokenizer.ReadToken();
                if (semiColon.SequenceEqual(";"))
                {
                    statement = ReturnStatementEmpty.instance;
                    return true;
                }
                tokenizer.GiveBack(semiColon);
                var expr = ReadExpression(ref tokenizer);
                statement = new ReturnStatement(expr);
                return true;
        }

        statement = ReadExpression(firstToken, ref tokenizer);
        return true;
    }

    private static (Expression condition, Statement body) ReadIfOnce(ref Tokenizer tokenizer)
    {
        // if must be read here
        var t1 = tokenizer.ReadToken();
        if (!t1.SequenceEqual("(")) throw new Exception();
        var conditionExpression = ReadExpression(ref tokenizer);
        var t2 = tokenizer.ReadToken();
        if (!t2.SequenceEqual(")")) throw new Exception();
        if (!ReadStatement(ref tokenizer, out var bodyStatement))
            throw new Exception();
        return (conditionExpression, bodyStatement);
    }

    private static bool TryReadExpression(ref Tokenizer tokenizer, [NotNullWhen(true)] out Expression? expression)
    {
        var success = tokenizer.TryReadToken(out var first);
        if (!success)
        {
            expression = default;
            return false;
        }
        if (first.SequenceEqual(")"))
        {
            expression = default;
            return false;
        }
        expression = ReadExpression(first, ref tokenizer);
        return true;
    }

    private static Expression ReadExpression(ref Tokenizer tokenizer)
    {
        var first = tokenizer.ReadToken();
        return ReadExpression(first, ref tokenizer);
    }

    private static Expression ReadExpression(ReadOnlySpan<char> firstToken, ref Tokenizer tokenizer)
    {
        Expression firstExpression = ReadInitialExpression(firstToken, ref tokenizer);
        while (true)
        {
            if (!tokenizer.TryReadToken(out var op))
                return firstExpression;
            if (op.SequenceEqual(";"))
                return firstExpression;

            if (op.SequenceEqual(")") || op.SequenceEqual("}") || op.SequenceEqual("]") || op.SequenceEqual(":") || op.SequenceEqual(","))
            {
                tokenizer.GiveBack(op);
                return firstExpression;
            }

            if (op.SequenceEqual("=>"))
            {
                if (!ReadStatement(ref tokenizer, out var statement))
                    throw new Exception();

                return new FunctionExpression(new List<string> { ((VariableExpression)firstExpression).varName }, statement, isLambda: true);
            }

            if (op.SequenceEqual("?"))
            {
                var trueCaseExpression = ReadExpression(ref tokenizer);
                var next = tokenizer.ReadToken();
                if (!next.SequenceEqual(":"))
                    throw new Exception();
                var falseCaseExpression = ReadExpression(ref tokenizer);
                return new TernaryExpression(firstExpression, trueCaseExpression, falseCaseExpression);
            }

            if (TreeExpression.TryParseOperator(op, out var precedence, out var oper))
            {
                if (precedence < Precedence.FunctionCall)
                {
                    if (precedence == Precedence.PostfixIncrement)
                    {
                        if (firstExpression is TreeExpression tree)
                            tree.ReplaceLastPostFix(oper == Operator.MinusMinusPostfix);
                        else
                            firstExpression = new PostfixIncDecExpression(oper == Operator.MinusMinusPostfix, firstExpression);
                    }
                    else
                    {
                        var nextExpression = ReadInitialExpression(ref tokenizer);
                        if (firstExpression is TreeExpression tree)
                            firstExpression = tree.Combine(precedence, oper, nextExpression);
                        else
                            firstExpression = new TreeExpression(precedence, firstExpression, oper, nextExpression);
                    }
                }
                else if (oper == Operator.FunctionCall)
                {
                    var expressions = new List<Expression>();
                    var token = tokenizer.ReadToken();

                    if (!token.SequenceEqual(")"))
                    {
                        tokenizer.GiveBack(token);
                        while (true)
                        {
                            var expr = ReadExpression(ref tokenizer);
                            expressions.Add(expr);
                            token = tokenizer.ReadToken();
                            if (token.SequenceEqual(")"))
                                break;
                            if (token.SequenceEqual(","))
                                continue;
                            throw new Exception();
                        }
                    }

                    TreeExpression.CombineOp18(ref firstExpression, new FunctionCallChain(expressions));
                    continue;
                }
                else if (oper == Operator.ComputedMemberAccess)
                {
                    var expr = ReadExpression(ref tokenizer);
                    if (!tokenizer.ReadToken().SequenceEqual("]"))
                        throw new Exception();
                    TreeExpression.CombineOp18(ref firstExpression, new MemberAccessChain(expr));
                    continue;
                }
                else if (oper == Operator.DotMemberAccess)
                {
                    var token = tokenizer.ReadToken();
                    if (!IsVariableName(token))
                        throw new Exception();
                    TreeExpression.CombineOp18(ref firstExpression, new DotAccessChain(token.ToString()));
                    continue;
                }
                else
                    throw new Exception();
            }
        }

        throw new Exception();
    }

    static Expression ReadInitialExpression(ReadOnlySpan<char> firstToken, ref Tokenizer tokenizer)
    {
        switch (firstToken)
        {
            case "null": return nullExpression;
            case "true": return trueExpression;
            case "false": return falseExpression;
            case "function": return ReadFunction(ref tokenizer); // Read function expression
        }

        if (IsStaticString(firstToken))
        {
            var str = ParseStaticString(firstToken);
            var value = CustomValue.FromString(str);
            return new ValueExpression(value);
        }
        if (IsNumber(firstToken))
        {
            var d = double.Parse(firstToken);
            var value = CustomValue.FromNumber(d);
            return new ValueExpression(value);
        }
        if (IsVariableName(firstToken))
        {
            return new VariableExpression(firstToken);
        }
        bool isPlus = firstToken.SequenceEqual("+");
        bool isMinus = firstToken.SequenceEqual("-");
        if (isPlus || isMinus)
        {
            var expr = ReadInitialExpression(ref tokenizer);
            return new SinglePlusMinusExpression(isMinus, expr);
        }
        bool isPlusPlus = firstToken.SequenceEqual("++");
        bool isMinusMinus = firstToken.SequenceEqual("--");
        if (isPlusPlus || isMinusMinus)
        {
            var expr = ReadInitialExpression(ref tokenizer);
            return new PrefixIncDecExpression(isMinusMinus, expr);
        }
        if (firstToken.SequenceEqual("!"))
        {
            var expr = ReadInitialExpression(ref tokenizer);
            return new NotExpression(expr);
        }
        if (firstToken.SequenceEqual("("))
        {
            // Read parantheses expression or lambda function
            var expressions = new List<Expression>();
            while (true)
            {
                if (!TryReadExpression(ref tokenizer, out var expression))
                    break;
                expressions.Add(expression);

                var token = tokenizer.ReadToken();
                if (token.SequenceEqual(")"))
                    break;
                if (token.SequenceEqual(","))
                    continue;
                throw new Exception();
            }

            if (expressions.Count != 1)
            {
                // read for arrow for lambda expression
                var next = tokenizer.ReadToken();
                if (!next.SequenceEqual("=>"))
                    throw new Exception();

                if (!ReadStatement(ref tokenizer, out var statement))
                    throw new Exception();

                var parameters = expressions.Select(x => ((VariableExpression)x).varName).ToList();
                return new FunctionExpression(parameters, statement, isLambda: true);
            }

            if (!tokenizer.TryReadToken(out var nextToken))
                return new ParanthesesExpression(expressions[0]);
            if (nextToken.SequenceEqual("=>"))
            {
                // read for arrow for lambda expression
                throw new Exception();
            }
            else
            {
                tokenizer.GiveBack(nextToken);
            }
            return new ParanthesesExpression(expressions[0]);
        }
        if (firstToken.SequenceEqual("{"))
        {
            return ReadPlainObject(ref tokenizer);
        }
        throw new Exception();
    }

    private static Expression ReadInitialExpression(ref Tokenizer tokenizer)
    {
        var first = tokenizer.ReadToken();
        return ReadInitialExpression(first, ref tokenizer);
    }

    private static FunctionExpression ReadFunction(ref Tokenizer tokenizer)
    {
        // expects parantheses
        var token = tokenizer.ReadToken();
        if (!token.SequenceEqual("("))
            throw new Exception();
        var parameters = new List<string>();
        token = tokenizer.ReadToken();

        if (!token.SequenceEqual(")"))
        {
            tokenizer.GiveBack(token);
            while (true)
            {
                token = tokenizer.ReadToken();
                if (!IsVariableName(token))
                    throw new Exception();
                parameters.Add(token.ToString());
                token = tokenizer.ReadToken();
                if (token.SequenceEqual(")"))
                    break;
                if (token.SequenceEqual(","))
                    continue;
                throw new Exception();
            }
        }

        if (!ReadStatement(ref tokenizer, out var statement))
            throw new Exception();

        return new FunctionExpression(parameters, statement, isLambda: false);
    }

    private static PlainObjectExpression ReadPlainObject(ref Tokenizer tokenizer)
    {
        // beginning marker was already read
        var list = new List<(string, Expression)>();
        var token = tokenizer.ReadToken();

        if (!token.SequenceEqual("}"))
        {
            tokenizer.GiveBack(token);
            while (true)
            {
                string propName;
                token = tokenizer.ReadToken();
                if (IsStaticString(token))
                    propName = ParseStaticString(token);
                else if (IsVariableName(token))
                    propName = token.ToString();
                else
                    throw new Exception();

                if (!tokenizer.ReadToken().SequenceEqual(":")) throw new Exception();
                var expr = ReadExpression(ref tokenizer);
                list.Add((propName, expr));
                token = tokenizer.ReadToken();
                if (token.SequenceEqual("}"))
                    break;
                if (token.SequenceEqual(","))
                    continue;
                throw new Exception();
            }
        }

        return new PlainObjectExpression(list);
    }

    private static string ParseStaticString(ReadOnlySpan<char> s)
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

    private class Context
    {
        private readonly Dictionary<string, CustomValue> variables;
        private readonly Context? innerContext;

        public Context(Context? innerContext = null) : this(new Dictionary<string, CustomValue>(), innerContext)
        {

        }

        public Context(Dictionary<string, CustomValue> variables, Context? innerContext = null)
        {
            this.variables = variables;
            this.innerContext = innerContext;
        }

        public CustomValue GetVariable(string varName)
        {
            var context = this;
            while (true)
            {
                if (context.variables.TryGetValue(varName, out var value))
                {
                    return value;
                }
                if (context.innerContext == null)
                {
                    throw new Exception($"variable '{varName}' is not defined");
                }
                context = context.innerContext;
            }
        }

        public void SetExisting(string varName, CustomValue newValue)
        {
            var context = this;
            while (true)
            {
                if (context.variables.TryGetValue(varName, out var _))
                {
                    context.variables[varName] = newValue;
                    return;
                }
                if (context.innerContext == null)
                {
                    throw new Exception($"variable '{varName}' is not defined");
                }
                context = context.innerContext;
            }
        }

        public CustomValue Replace(string varName, Func<CustomValue, CustomValue> f, out CustomValue oldValue)
        {
            Context context = this;
            while (true)
            {
                if (context.variables.TryGetValue(varName, out oldValue))
                {
                    var newValue = f(oldValue);
                    context.variables[varName] = newValue;
                    return newValue;
                }
                if (context.innerContext == null)
                {
                    throw new Exception($"variable '{varName}' is not defined");
                }
                context = context.innerContext;
            }
        }

        public void SetOuterMost(string varName, CustomValue val)
        {
            this.variables[varName] = val;
        }
    }

    abstract class Expression : Statement
    {
        public abstract CustomValue Run(Context context);

        (CustomValue, ResultType) Statement.Run(Context context)
        {
            return (Run(context), ResultType.Normal);
        }
    }
    abstract class UnaryExpression : Expression
    {
        internal Expression expressionRest;

        protected UnaryExpression(Expression expressionRest)
        {
            this.expressionRest = expressionRest;
        }
    }
    interface ChainExpression
    {
        CustomValue Run(CustomValue lastValue, Context context);
    }
    class FunctionCallChain : ChainExpression
    {
        private readonly List<Expression> expressions;

        public FunctionCallChain(List<Expression> expressions)
        {
            this.expressions = expressions;
        }

        public CustomValue Run(CustomValue lastValue, Context context)
        {
            var f = (IFunction)lastValue.value;
            var args = expressions.Select(x => x.Run(context)).ToList();
            return f.Run(args);
        }
    }
    class MemberAccessChain : ChainExpression
    {
        internal readonly Expression expression;

        public MemberAccessChain(Expression expression)
        {
            this.expression = expression;
        }

        public CustomValue Run(CustomValue lastValue, Context context)
        {
            var val = expression.Run(context);
            if (val.value is string prop)
            {
                var obj = (IObject)lastValue.value;
                return obj.GetProp(prop);
            }

            throw new NotImplementedException();
        }
    }
    class DotAccessChain : ChainExpression
    {
        internal readonly string prop;

        public DotAccessChain(string prop)
        {
            this.prop = prop;
        }

        public CustomValue Run(CustomValue lastValue, Context context)
        {
            var obj = (IObject)lastValue.value;
            return obj.GetProp(prop);
        }
    }
    class Op18Expression : Expression
    {
        private readonly Expression baseExpression;
        internal readonly List<ChainExpression> links = new();

        private Op18Expression(Expression baseExpression)
        {
            this.baseExpression = baseExpression;
        }

        public static Op18Expression New(Expression baseExpression, ChainExpression expression)
        {
            var expr = new Op18Expression(baseExpression);
            expr.links.Add(expression);
            return expr;
        }

        public void AddChain(ChainExpression expression)
        {
            links.Add(expression);
        }

        public CustomValue RunAllButLast(Context context)
        {
            var baseValue = baseExpression.Run(context);
            foreach (var link in CollectionsMarshal.AsSpan(links)[..^1])
            {
                baseValue = link.Run(baseValue, context);
            }
            return baseValue;
        }

        public override CustomValue Run(Context context)
        {
            var baseValue = baseExpression.Run(context);
            foreach (var link in CollectionsMarshal.AsSpan(links))
            {
                baseValue = link.Run(baseValue, context);
            }
            return baseValue;
        }
    }
    interface Statement
    {
        (CustomValue value, ResultType resultType) Run(Context context);
    }

    #region Statements
    class BlockStatement : Statement
    {
        private readonly List<Statement> statements = new();

        public BlockStatement(ReadOnlySpan<char> firstToken, ref Tokenizer tokenizer)
        {
            var token = tokenizer.ReadToken();
            while (!token.SequenceEqual("}"))
            {
                if (!ReadStatement(token, ref tokenizer, out var statement))
                    throw new Exception();
                statements.Add(statement);
                token = tokenizer.ReadToken();
            }
        }

        public (CustomValue, ResultType) Run(Context context)
        {
            foreach (var statement in statements)
            {
                var res = statement.Run(context);
                if (res.resultType != ResultType.Normal)
                    return res;
            }
            return (CustomValue.Null, ResultType.Normal);
        }
    }
    class IfStatement : Statement
    {
        private readonly Expression ifConditionExpression;
        private readonly Statement ifBodyStatement;

        private readonly List<(Expression condition, Statement body)> elseIfStatements = new();
        private Statement? elseStatementBody = null;

        public IfStatement(Expression ifConditionExpression, Statement ifBodyStatement)
        {
            this.ifConditionExpression = ifConditionExpression;
            this.ifBodyStatement = ifBodyStatement;
        }

        public void AddElseIf((Expression condition, Statement body) tuple)
        {
            elseIfStatements.Add(tuple);
        }

        public void SetElse(Statement statement)
        {
            if (elseStatementBody != null) throw new Exception();
            elseStatementBody = statement;
        }

        public (CustomValue, ResultType) Run(Context context)
        {
            if (ifConditionExpression.Run(context).IsTruthy())
            {
                var res = ifBodyStatement.Run(context);
                if (res.resultType != ResultType.Normal)
                    return res;
                return (CustomValue.Null, ResultType.Normal);
            }
            foreach (var (cond, body) in elseIfStatements)
            {
                if (cond.Run(context).IsTruthy())
                {
                    var res = body.Run(context);
                    if (res.resultType != ResultType.Normal)
                        return res;
                    return (CustomValue.Null, ResultType.Normal);
                }
            }
            if (elseStatementBody != null)
            {
                var res = elseStatementBody.Run(context);
                if (res.resultType != ResultType.Normal)
                    return res;
            }
            return (CustomValue.Null, ResultType.Normal);
        }
    }
    class WhileStatement : Statement
    {
        private readonly Expression conditionExpression;
        private readonly Statement bodyStatement;

        public WhileStatement(Expression conditionExpression, Statement bodyStatement)
        {
            this.conditionExpression = conditionExpression;
            this.bodyStatement = bodyStatement;
        }

        public (CustomValue, ResultType) Run(Context context)
        {
            while (conditionExpression.Run(context).IsTruthy())
            {
                var res = bodyStatement.Run(context);
                if (res.resultType == ResultType.Break)
                    break;
                if (res.resultType == ResultType.Return)
                    return res;
            }
            return (CustomValue.Null, ResultType.Normal);
        }
    }
    class BreakStatement : Statement
    {
        public static BreakStatement instance = new();

        public (CustomValue, ResultType) Run(Context context)
        {
            return (CustomValue.Null, ResultType.Break);
        }
    }
    class ContinueStatement : Statement
    {
        public static ContinueStatement instance = new();

        public (CustomValue, ResultType) Run(Context context)
        {
            return (CustomValue.Null, ResultType.Continue);
        }
    }
    class ReturnStatementEmpty : Statement
    {
        public static ReturnStatementEmpty instance = new();

        public (CustomValue, ResultType) Run(Context context)
        {
            return (CustomValue.Null, ResultType.Return);
        }
    }
    class ReturnStatement : Statement
    {
        private readonly Expression expression;

        public ReturnStatement(Expression expression)
        {
            this.expression = expression;
        }

        public (CustomValue value, ResultType resultType) Run(Context context)
        {
            var val = expression.Run(context);
            return (val, ResultType.Return);
        }
    }
    class FunctionStatement : Statement
    {
        private readonly string functionName;
        private readonly FunctionExpression expression;

        public FunctionStatement(string functionName, FunctionExpression expression)
        {
            this.functionName = functionName;
            this.expression = expression;
        }

        public (CustomValue value, ResultType resultType) Run(Context context)
        {
            var f = expression.Run(context);
            context.SetOuterMost(functionName, f);
            return (CustomValue.Null, ResultType.Normal);
        }
    }
    #endregion

    #region Expressions
    class VarAssignmentExpression : Expression
    {
        private readonly string varName;
        private readonly Expression expression;

        private VarAssignmentExpression(string varName, Expression expression)
        {
            this.varName = varName;
            this.expression = expression;
        }

        public static VarAssignmentExpression New(string varName, Expression expression)
        {
            return new VarAssignmentExpression(varName, expression);
        }

        public static VarAssignmentExpression New(ref Tokenizer tokenizer)
        {
            var firstToken = tokenizer.ReadToken();

            var op = tokenizer.ReadToken();
            if (op.SequenceEqual(";"))
                return new VarAssignmentExpression(firstToken.ToString(), nullExpression);
            if (!op.SequenceEqual("="))
                throw new Exception();

            var expression = NewInterpreter.ReadExpression(ref tokenizer);
            return new VarAssignmentExpression(firstToken.ToString(), expression);
        }

        public override CustomValue Run(Context context)
        {
            context.SetOuterMost(varName, expression.Run(context));

            return CustomValue.Null;
        }
    }
    class ParanthesesExpression : Expression
    {
        internal readonly Expression insideExpression;

        public ParanthesesExpression(Expression insideExpression)
        {
            this.insideExpression = insideExpression;
        }

        public override CustomValue Run(Context context)
        {
            return insideExpression.Run(context);
        }
    }
    class ValueExpression : Expression
    {
        private readonly CustomValue value;

        public ValueExpression(CustomValue value)
        {
            this.value = value;
        }

        public override CustomValue Run(Context context)
        {
            return value;
        }
    }
    class VariableExpression : Expression
    {
        internal readonly string varName;

        public VariableExpression(ReadOnlySpan<char> varName)
        {
            this.varName = varName.ToString();
        }

        public override CustomValue Run(Context context)
        {
            return context.GetVariable(varName);
        }
    }
    class SinglePlusMinusExpression : UnaryExpression
    {
        private readonly bool isMinus;

        public SinglePlusMinusExpression(bool isMinus, Expression expressionRest) : base(expressionRest)
        {
            this.isMinus = isMinus;
        }

        public override CustomValue Run(Context context)
        {
            var rest = expressionRest.Run(context);
            if (rest.type != ValueType.Number)
                throw new Exception();

            if (isMinus)
                return CustomValue.FromNumber((double)rest.value * -1);
            else
                return CustomValue.FromNumber((double)rest.value);
        }
    }
    class PrefixIncDecExpression : UnaryExpression
    {
        private readonly bool isMinusMinus;

        public PrefixIncDecExpression(bool isMinusMinus, Expression expressionRest) : base(expressionRest)
        {
            this.isMinusMinus = isMinusMinus;
        }

        public override CustomValue Run(Context context)
        {
            return RunReplaceExpression(expressionRest, oldVal =>
            {
                return AddOrSubtract(oldVal, Operator.Plus, CustomValue.FromNumber(isMinusMinus ? -1 : 1));
            }, context, out var _);
        }
    }
    class PostfixIncDecExpression : UnaryExpression
    {
        private readonly bool isMinusMinus;

        public PostfixIncDecExpression(bool isMinusMinus, Expression expressionRest) : base(expressionRest)
        {
            this.isMinusMinus = isMinusMinus;
        }

        public override CustomValue Run(Context context)
        {
            RunReplaceExpression(expressionRest, oldVal =>
            {
                return AddOrSubtract(oldVal, Operator.Plus, CustomValue.FromNumber(isMinusMinus ? -1 : 1));
            }, context, out var oldValue);
            return oldValue;
        }
    }
    class NotExpression : UnaryExpression
    {
        public NotExpression(Expression expressionRest) : base(expressionRest)
        {
        }

        public override CustomValue Run(Context context)
        {
            var rest = expressionRest.Run(context);
            bool restValue = rest.IsTruthy();
            return restValue ? CustomValue.False : CustomValue.True;
        }
    }
    class TernaryExpression : Expression
    {
        private readonly Expression conditionExpression;
        private readonly Expression questionMarkExpression;
        private readonly Expression colonExpression;

        public TernaryExpression(Expression conditionExpression, Expression questionMarkExpression, Expression colonExpression)
        {
            this.conditionExpression = conditionExpression;
            this.questionMarkExpression = questionMarkExpression;
            this.colonExpression = colonExpression;
        }

        public override CustomValue Run(Context context)
        {
            var conditionValue = conditionExpression.Run(context);
            bool isTruthy = conditionValue.IsTruthy();

            if (isTruthy)
                return questionMarkExpression.Run(context);
            else
                return colonExpression.Run(context);
        }
    }
    class TreeExpression : Expression
    {
        internal readonly Precedence precedence;
        private readonly Expression firstExpression;
        internal readonly List<(Operator operatorToken, Expression expression)> nextValues;

        public TreeExpression(Precedence precedence, Expression firstExpression, Operator operatorToken, Expression expression)
        {
            this.precedence = precedence;
            this.firstExpression = firstExpression;
            this.nextValues = new List<(Operator operatorToken, Expression)>();
            nextValues.Add((operatorToken, expression));
        }

        public override CustomValue Run(Context context)
        {
            switch (precedence)
            {
                case Precedence.Assignment:
                    return HandleAssignment(context);
                case Precedence.EqualityCheck:
                    {
                        var value = firstExpression.Run(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = CheckEquality(value, op, expression, context);
                        }
                        return value;
                    }
                case Precedence.Comparison:
                    {
                        var value = firstExpression.Run(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = Compare(value, op, expression, context);
                        }
                        return value;
                    }
                case Precedence.AddSubtract:
                    {
                        var value = firstExpression.Run(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            value = AddOrSubtract(value, op, expression.Run(context));
                        }
                        return value;
                    }
                case Precedence.MultiplyDivide:
                    {
                        var value = (double)firstExpression.Run(context).value;
                        foreach (var (op, expression) in nextValues)
                        {
                            value = MultiplyOrDivide(value, op, expression.Run(context));
                        }
                        return CustomValue.FromNumber(value);
                    }
                case Precedence.AndAnd:
                    {
                        var value = firstExpression.Run(context);
                        if (!value.IsTruthy())
                            return CustomValue.False;
                        foreach (var (_, expression) in nextValues)
                        {
                            value = expression.Run(context);
                            if (!value.IsTruthy())
                                return CustomValue.False;
                        }
                        return CustomValue.True;
                    }
                case Precedence.OrOr:
                    {
                        var value = firstExpression.Run(context);
                        foreach (var (op, expression) in nextValues)
                        {
                            if (op == Operator.OrOr && value.IsTruthy())
                                return value;
                            if (op == Operator.DoubleQuestion && value.type != ValueType.Null)
                                return value;

                            value = expression.Run(context);
                        }
                        return value;
                    }
            }

            throw new NotImplementedException();
        }

        private CustomValue HandleAssignment(Context context)
        {
            var (oper, expr) = nextValues[0];
            var val = expr.Run(context);

            switch (oper)
            {
                case Operator.NormalAssign:
                    {
                        RunSetExpression(firstExpression, val, context);
                        return val;
                    }
                case Operator.PlusAssign:
                    {
                        return RunReplaceExpression(firstExpression, oldVal => AddOrSubtract(oldVal, Operator.Plus, val), context, out var _);
                    }
                case Operator.MinusAssign:
                    {
                        return RunReplaceExpression(firstExpression, oldVal => AddOrSubtract(oldVal, Operator.Minus, val), context, out var _);
                    }
                case Operator.MultiplyAssign:
                    {
                        return RunReplaceExpression(firstExpression, oldVal => MultiplyOrDivide(oldVal, Operator.Multiply, val), context, out var _);
                    }
                case Operator.DivideAssign:
                    {
                        return RunReplaceExpression(firstExpression, oldVal => MultiplyOrDivide(oldVal, Operator.Divide, val), context, out var _);
                    }
                case Operator.ModulusAssign:
                    {
                        return RunReplaceExpression(firstExpression, oldVal => MultiplyOrDivide(oldVal, Operator.Modulus, val), context, out var _);
                    }
                default:
                    throw new Exception();
            }
        }

        public static bool TryParseOperator(ReadOnlySpan<char> token, out Precedence precedence, out Operator op)
        {
            switch (token)
            {
                case "==":
                    {
                        precedence = Precedence.EqualityCheck;
                        op = Operator.CheckEquals;
                        return true;
                    }
                case "!=":
                    {
                        precedence = Precedence.EqualityCheck;
                        op = Operator.CheckNotEquals;
                        return true;
                    }
                case "<":
                    {
                        precedence = Precedence.Comparison;
                        op = Operator.LessThan;
                        return true;
                    }
                case "<=":
                    {
                        precedence = Precedence.Comparison;
                        op = Operator.LessThanOrEqual;
                        return true;
                    }
                case ">":
                    {
                        precedence = Precedence.Comparison;
                        op = Operator.GreaterThan;
                        return true;
                    }
                case ">=":
                    {
                        precedence = Precedence.Comparison;
                        op = Operator.GreaterThanOrEqual;
                        return true;
                    }
                case "+":
                    {
                        precedence = Precedence.AddSubtract;
                        op = Operator.Plus;
                        return true;
                    }
                case "-":
                    {
                        precedence = Precedence.AddSubtract;
                        op = Operator.Minus;
                        return true;
                    }
                case "*":
                    {
                        precedence = Precedence.MultiplyDivide;
                        op = Operator.Multiply;
                        return true;
                    }
                case "/":
                    {
                        precedence = Precedence.MultiplyDivide;
                        op = Operator.Divide;
                        return true;
                    }
                case "%":
                    {
                        precedence = Precedence.MultiplyDivide;
                        op = Operator.Modulus;
                        return true;
                    }
                case "=":
                    {
                        precedence = Precedence.Assignment;
                        op = Operator.NormalAssign;
                        return true;
                    }
                case "+=":
                    {
                        precedence = Precedence.Assignment;
                        op = Operator.PlusAssign;
                        return true;
                    }
                case "-=":
                    {
                        precedence = Precedence.Assignment;
                        op = Operator.MinusAssign;
                        return true;
                    }
                case "*=":
                    {
                        precedence = Precedence.Assignment;
                        op = Operator.MultiplyAssign;
                        return true;
                    }
                case "/=":
                    {
                        precedence = Precedence.Assignment;
                        op = Operator.DivideAssign;
                        return true;
                    }
                case "%=":
                    {
                        precedence = Precedence.Assignment;
                        op = Operator.ModulusAssign;
                        return true;
                    }
                case "(":
                    {
                        precedence = Precedence.FunctionCall;
                        op = Operator.FunctionCall;
                        return true;
                    }
                case "[":
                    {
                        precedence = Precedence.Indexing;
                        op = Operator.ComputedMemberAccess;
                        return true;
                    }
                case ".":
                    {
                        precedence = Precedence.DotAccess;
                        op = Operator.DotMemberAccess;
                        return true;
                    }
                case "&&":
                    {
                        precedence = Precedence.AndAnd;
                        op = Operator.AndAnd;
                        return true;
                    }
                case "||":
                    {
                        precedence = Precedence.OrOr;
                        op = Operator.OrOr;
                        return true;
                    }
                case "++":
                    {
                        precedence = Precedence.PostfixIncrement;
                        op = Operator.PlusPlusPostfix;
                        return true;
                    }
                case "--":
                    {
                        precedence = Precedence.PostfixIncrement;
                        op = Operator.MinusMinusPostfix;
                        return true;
                    }
                default:
                    throw new Exception();
            }
        }

        internal TreeExpression Combine(Precedence precedence, Operator oper, Expression nextExpression)
        {
            if (precedence == this.precedence)
            {
                this.nextValues.Add((oper, nextExpression));
                return this;
            }

            if (precedence < this.precedence)
            {
                return new TreeExpression(precedence, this, oper, nextExpression);
            }

            var nextValues = this.nextValues;
            while (true)
            {
                var nexts = CollectionsMarshal.AsSpan(nextValues);
                ref var lastNext = ref nexts[^1];
                if (lastNext.expression is not TreeExpression tree)
                {
                    lastNext.expression = new TreeExpression(precedence, lastNext.expression, oper, nextExpression);
                    return this;
                }
                else if (precedence < tree.precedence)
                {
                    lastNext.expression = new TreeExpression(precedence, lastNext.expression, oper, nextExpression);
                    return this;
                }
                else if (precedence == tree.precedence)
                {
                    tree.nextValues.Add((oper, nextExpression));
                    return this;
                }
                else
                    nextValues = tree.nextValues;
            }
        }

        internal void ReplaceLastPostFix(bool isMinusMinus)
        {
            var nextValues = this.nextValues;
            while (true)
            {
                var nexts = CollectionsMarshal.AsSpan(nextValues);
                ref var lastNext = ref nexts[^1];
                if (lastNext.expression is not TreeExpression tree)
                {
                    lastNext.expression = new PostfixIncDecExpression(isMinusMinus, lastNext.expression);
                    return;
                }
                else
                    nextValues = tree.nextValues;
            }
        }

        public static void CombineOp18(ref Expression expression, ChainExpression chain)
        {
            if (expression is VariableExpression variableExpression)
            {
                expression = Op18Expression.New(variableExpression, chain);
                return;
            }
            if (expression is ParanthesesExpression paranthesesExpression)
            {
                expression = Op18Expression.New(paranthesesExpression.insideExpression, chain);
                return;
            }
            if (expression is TreeExpression treeExpression)
            {
                var nextValues = CollectionsMarshal.AsSpan(treeExpression.nextValues);
                ref var last = ref nextValues[^1];
                CombineOp18(ref last.expression, chain);
                return;
            }
            if (expression is Op18Expression op18)
            {
                op18.AddChain(chain);
                return;
            }
            if (expression is UnaryExpression unary)
            {
                CombineOp18(ref unary.expressionRest, chain);
                return;
            }
            throw new NotImplementedException();
        }
    }
    class FunctionExpression : Expression
    {
        private readonly List<string> parameters;
        private readonly Statement body;
        private readonly bool isLambda;

        public FunctionExpression(List<string> parameters, Statement body, bool isLambda)
        {
            this.parameters = parameters;
            this.body = body;
            this.isLambda = isLambda;
        }

        public override CustomValue Run(Context context)
        {
            var f = new Function(parameters, body, context);
            return CustomValue.FromFunction(f);
        }
    }
    class PlainObjectExpression : Expression
    {
        private readonly List<(string, Expression)> list;

        public PlainObjectExpression(List<(string, Expression)> list)
        {
            this.list = list;
        }

        public override CustomValue Run(Context context)
        {
            var properties = new Dictionary<string, CustomValue>();
            foreach (var (prop, expr) in list)
            {
                properties[prop] = expr.Run(context);
            }

            return CustomValue.FromPlainObject(new PlainObject(properties));
        }
    }
    #endregion

    #region Eval Related Functions
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
                return CustomValue.FromString(totalString);
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
    private static CustomValue MultiplyOrDivide(CustomValue total, Operator operatorType, CustomValue value)
    {
        return CustomValue.FromNumber(MultiplyOrDivide((double)total.value, operatorType, value));
    }
    private static double MultiplyOrDivide(double total, Operator operatorType, CustomValue value)
    {
        if (operatorType == Operator.Multiply)
            total *= (double)value.value;
        else if (operatorType == Operator.Divide)
            total /= (double)value.value;
        else if (operatorType == Operator.Modulus)
            total %= (int)(double)value.value;
        else
            throw new Exception();
        return total;
    }
    private static CustomValue CheckEquality(CustomValue firstValue, Operator operatorType, Expression tree, Context context)
    {
        var value = tree.Run(context);

        return operatorType switch
        {
            Operator.CheckEquals => object.Equals(firstValue.value, value.value) ? CustomValue.True : CustomValue.False,
            Operator.CheckNotEquals => !object.Equals(firstValue.value, value.value) ? CustomValue.True : CustomValue.False,
            _ => throw new Exception(),
        };
    }
    private static CustomValue Compare(CustomValue firstValue, Operator operatorType, Expression tree, Context context)
    {
        var value = tree.Run(context);

        return operatorType switch
        {
            Operator.LessThan or
            Operator.GreaterThan or
            Operator.LessThanOrEqual or
            Operator.GreaterThanOrEqual => Compare(firstValue, operatorType, value) ? CustomValue.True : CustomValue.False,
            _ => throw new Exception(),
        };
    }
    private static bool Compare(CustomValue first, Operator operatorType, CustomValue second)
    {
        if (first.value is double d1 && second.value is double d2)
        {
            return operatorType switch
            {
                Operator.LessThan => d1 < d2,
                Operator.LessThanOrEqual => d1 <= d2,
                Operator.GreaterThan => d1 > d2,
                Operator.GreaterThanOrEqual => d1 >= d2,
                _ => throw new Exception(),
            };
        }

        if (first.value is string s1 && second.value is string s2)
        {
            return operatorType switch
            {
                Operator.LessThan => s1.CompareTo(s2) < 0,
                Operator.LessThanOrEqual => s1.CompareTo(s2) <= 0,
                Operator.GreaterThan => s1.CompareTo(s2) > 0,
                Operator.GreaterThanOrEqual => s1.CompareTo(s2) >= 0,
                _ => throw new Exception(),
            };
        }

        throw new Exception();
    }
    private static void RunSetExpression(Expression expr, CustomValue value, Context context)
    {
        if (expr is VariableExpression varex)
        {
            var varName = varex.varName;
            context.SetExisting(varName, value);
        }
        else
        {
            var (baseObj, prop) = GetBaseObjAndProp((Op18Expression)expr, context);
            baseObj.SetProp(prop, value);
        }
    }
    private static CustomValue RunReplaceExpression(Expression expr, Func<CustomValue, CustomValue> converter, Context context, out CustomValue oldValue)
    {
        if (expr is VariableExpression varex)
        {
            var varName = varex.varName;
            return context.Replace(varName, converter, out oldValue);
        }
        else
        {
            var (baseObj, prop) = GetBaseObjAndProp((Op18Expression)expr, context);
            oldValue = baseObj.GetProp(prop);
            var newValue = converter(oldValue);
            baseObj.SetProp(prop, newValue);
            return newValue;
        }
    }
    private static (IObject, string) GetBaseObjAndProp(Op18Expression op18, Context context)
    {
        if (op18.links[^1] is FunctionCallChain)
            throw new Exception();
        var baseObj = (IObject)op18.RunAllButLast(context).value;
        var lastExpression = op18.links[^1];
        string prop;
        if (lastExpression is MemberAccessChain memAcc)
            prop = (string)memAcc.expression.Run(context).value;
        else if (lastExpression is DotAccessChain dotAcc)
            prop = dotAcc.prop;
        else
            throw new Exception();

        return (baseObj, prop);
    }
    #endregion

    readonly struct CustomValue
    {
        public readonly object value;
        public readonly ValueType type;

        public static readonly CustomValue Null = new CustomValue(null!, ValueType.Null);
        public static readonly CustomValue True = new CustomValue(true, ValueType.Bool);
        public static readonly CustomValue False = new CustomValue(false, ValueType.Bool);

        public CustomValue(object value, ValueType type)
        {
            this.value = value;
            this.type = type;
        }

        public static CustomValue FromNumber(double d)
        {
            return new CustomValue(d, ValueType.Number);
        }

        public static CustomValue FromString(string str)
        {
            return new CustomValue(str, ValueType.String);
        }

        public static CustomValue FromFunction(IFunction function)
        {
            return new CustomValue(function, ValueType.Function);
        }

        public static CustomValue FromPlainObject(PlainObject plainObject)
        {
            return new CustomValue(plainObject, ValueType.PlainObject);
        }

        internal bool IsTruthy()
        {
            switch (type)
            {
                case ValueType.Null:
                    return false;
                case ValueType.Number:
                    double d = (double)value;
                    return d != 0 && !double.IsNaN(d);
                case ValueType.String:
                    return !string.IsNullOrEmpty((string)value);
                case ValueType.Bool:
                    return (bool)value;
                default:
                    return true;
            }
        }

        public override string ToString()
        {
            if (value is null)
                return "null";
            if (value is bool b)
                return b ? "true" : "false";
            return value.ToString()!;
        }
    }
    enum ValueType : int
    {
        Null,
        Number,
        String,
        Bool,
        Function,
        Class,
        PlainObject,
        Array,
        Promise,
        Generator,
        AsyncGenerator,
    }
    interface IFunction
    {
        CustomValue Run(List<CustomValue> arguments);
    }
    class Function : IFunction
    {
        private readonly List<string> parameters;
        private readonly Statement body;
        private readonly Context context;

        public Function(List<string> parameters, Statement body, Context context)
        {
            this.parameters = parameters;
            this.body = body;
            this.context = context;
        }

        public CustomValue Run(List<CustomValue> arguments)
        {
            var context = new Context(this.context);
            var paramsSpan = CollectionsMarshal.AsSpan(parameters);
            for (int i = 0; i < paramsSpan.Length; i++)
            {
                var param = paramsSpan[i];
                var val = i < arguments.Count ? arguments[i] : CustomValue.Null;
                context.SetOuterMost(param, val);
            }

            var res = body.Run(context);
            if (res.resultType == ResultType.Return)
                return res.value;
            if (res.resultType == ResultType.Normal)
                return res.value;
            throw new Exception();
        }
    }
    interface IObject
    {
        CustomValue GetProp(string prop);
        void SetProp(string prop, CustomValue value);
    }
    class PlainObject : IObject
    {
        private readonly Dictionary<string, CustomValue> properties = new();

        public PlainObject(Dictionary<string, CustomValue> properties)
        {
            this.properties = properties;
        }

        public CustomValue GetProp(string prop)
        {
            if (properties.TryGetValue(prop, out var value))
                return value;
            return CustomValue.Null;
        }

        public void SetProp(string prop, CustomValue value)
        {
            properties[prop] = value;
        }
    }
    enum Precedence : int
    {
        Assignment = 3,
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
    enum Operator : int
    {
        None,
        NormalAssign,
        Plus,
        PlusAssign,
        Minus,
        MinusAssign,
        Multiply,
        MultiplyAssign,
        Divide,
        DivideAssign,
        Modulus,
        ModulusAssign,
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
        PlusPlusPostfix,
        MinusMinusPostfix,
        DotMemberAccess,
        ComputedMemberAccess,
        FunctionCall,
        ConditionalDotMemberAccess,
        ConditionalComputedMemberAccess,
        ConditionalFunctionCall,
        New,
    }
    enum ResultType : int
    {
        Normal,
        Break,
        Continue,
        Return,
    }

    #region Tokenizing Related
    ref struct Tokenizer
    {
        readonly ReadOnlySpan<char> code;
        int i = 0;

        ReadOnlySpan<char> givenBack;
        bool hasGivenBack = false;

        public Tokenizer(ReadOnlySpan<char> code)
        {
            this.code = code;
        }

        public ReadOnlySpan<char> ReadToken()
        {
            if (!TryReadToken(out var token))
                throw new Exception();
            return token;
        }

        public bool TryReadToken(out ReadOnlySpan<char> token)
        {
            if (hasGivenBack)
            {
                hasGivenBack = false;
                token = givenBack;
                return true;
            }

        begin:
            while (i < code.Length && IsWhiteSpace(code[i]))
                i++;
            if (i < code.Length - 1)
            {
                if (code[i] == '/')
                {
                    if (code[i + 1] == '/')
                    {
                        i += 2;
                        while (i < code.Length && code[i] != '\n')
                            i++;
                        i++;
                        goto begin;
                    }
                    else if (code[i + 1] == '*')
                    {
                        i += 2;
                        while (true)
                        {
                            if (code[i] == '*' && code[i + 1] == '/')
                                break;
                            i++;
                        }
                        i += 2;
                        goto begin;
                    }
                }
            }

            if (i == code.Length)
            {
                token = "";
                return false;
            }

            char c = code[i];
            if (IsCharOrUnderscore(c))
            {
                int start = i++;
                while (i < code.Length && IsCharOrDigitOrUnderscore(code[i]))
                    i++;

                token = code[start..i];
                return true;
            }
            if (IsDigit(c))
            {
                int start = i++;
                while (i < code.Length && IsDigitOrDot(code[i]))
                    i++;

                token = code[start..i];
                return true;
            }
            if (c == '\'' || c == '"')
            {
                int start = i;
                i++;
                while (true)
                {
                    if (code[i] == '\\')
                        i += 2;
                    else if (code[i] == c)
                    {
                        i++;
                        break;
                    }
                    else
                        i++;
                }
                token = code[start..i];
                return true;
            }
            if (NewInterpreter.operatorsCompiled.TryGetValue(c, out var level1Map))
            {
                int start = i;
                i++;
                if (i < code.Length && level1Map.TryGetValue(code[i], out var level2Set))
                {
                    i++;
                    if (i < code.Length && level2Set.Contains(code[i]))
                    {
                        i++;
                    }
                }
                token = code[start..i];
                return true;
            }
            if (NewInterpreter.onlyChars.TryGetValue(c, out var tokenStr))
            {
                i++;
                token = tokenStr;
                return true;
            }
            throw new Exception();
        }

        public void GiveBack(ReadOnlySpan<char> token)
        {
            if (hasGivenBack) throw new Exception();
            hasGivenBack = true;
            givenBack = token;
        }
    }

    static readonly SearchValues<char> chars = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_");
    static bool IsCharOrUnderscore(char c)
    {
        return chars.Contains(c);
    }

    static readonly SearchValues<char> charsDigit = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_");
    static bool IsCharOrDigitOrUnderscore(char c)
    {
        return charsDigit.Contains(c);
    }

    static readonly SearchValues<char> digit = SearchValues.Create("0123456789");
    static bool IsDigit(char c)
    {
        return digit.Contains(c);
    }

    static readonly SearchValues<char> digitOrDot = SearchValues.Create("0123456789.");
    static bool IsDigitOrDot(char c)
    {
        return digitOrDot.Contains(c);
    }

    static readonly SearchValues<char> ws = SearchValues.Create(" \t\r\n");
    static bool IsWhiteSpace(char c)
    {
        return ws.Contains(c);
    }

    private static bool IsNumber(ReadOnlySpan<char> token)
    {
        return IsDigit(token[0]);
    }
    private static bool IsVariableName(ReadOnlySpan<char> token)
    {
        return IsCharOrUnderscore(token[0]);
    }
    private static bool IsStaticString(ReadOnlySpan<char> token)
    {
        return token[0] == '\'' || token[0] == '"';
    }

    #endregion
}
