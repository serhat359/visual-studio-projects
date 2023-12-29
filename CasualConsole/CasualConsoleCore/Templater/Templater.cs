using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CasualConsoleCore.Templater;

public class Templater
{
    public static Func<object, Dictionary<string, object>, string> CompileTemplate(string template)
    {
        var handlers = new List<object>();
        var end = 0;
        while (end < template.Length)
        {
            (var handler, end) = GetHandler(template, end);
            if (handler == null)
                throw new Exception();
            handlers.Add(handler);
        }
        return (data, helpers) =>
        {
            var stringBuilder = new StringBuilder();
            Action<string> writer = x => stringBuilder.Append(x);
            HandleMulti(writer, new RealContext(helpers, data), handlers);
            return stringBuilder.ToString();
        };
    }

    private static (object, int) GetHandler(string template, int start)
    {
        var i = template.IndexOf("{{", start);
        if (i == start)
        {
            var (tokens, end) = GetTokens(template, i + 2);
            var first = tokens[0];
            if (first == "if")
            {
                var ifExpr = GetExpression(tokens, 1);
                (var ifHandlers, end) = GetBodyHandlers(template, end);
                var nextHandlerType = GetHandlerType(template, end);
                if (nextHandlerType != "else")
                {
                    Action<Action<string>, Context> simpleHandler = (writer, context) =>
                    {
                        if (Truthy(ifExpr(context)))
                        {
                            HandleMulti(writer, context, ifHandlers);
                        }
                    };
                    return (simpleHandler, end);
                }

                var elseIfHandlers = new List<(Func<Context, object>, List<object>)>();
                List<object>? elseHandlers = null;
                while (true)
                {
                    (var elseTokens, end) = GetTokens(template, end + 2);
                    if (elseTokens.Count == 1)
                    {
                        (elseHandlers, end) = GetBodyHandlers(template, end);
                        break;
                    }
                    else
                    {
                        // Has else if
                        if (elseTokens[1] != "if")
                            throw new Exception();
                        var elseIfExpr = GetExpression(elseTokens, 2);

                        (var elseIfInnerHandlers, end) = GetBodyHandlers(template, end);
                        elseIfHandlers.Add((elseIfExpr, elseIfInnerHandlers));
                    }
                    nextHandlerType = GetHandlerType(template, end);
                    if (nextHandlerType != "else")
                        break;
                }
                Action<Action<string>, Context> handler = (writer, context) =>
                {
                    if (Truthy(ifExpr(context)))
                    {
                        HandleMulti(writer, context, ifHandlers);
                        return;
                    }
                    foreach (var (elseIfExpr, elseIfHandlers) in elseIfHandlers)
                    {
                        if (Truthy(elseIfExpr(context)))
                        {
                            HandleMulti(writer, context, elseIfHandlers);
                            return;
                        }
                    }
                    if (elseHandlers != null)
                    {
                        HandleMulti(writer, context, elseHandlers);
                        return;
                    }
                };
                return (handler, end);
            }
            else if (first == "for")
            {
                var loopVarName = tokens[1];
                if (tokens[2] != "in")
                    throw new Exception();
                var loopValuesExpr = GetExpression(tokens, 3);
                (var handlers, end) = GetBodyHandlers(template, end);
                Action<Action<string>, Context> handler = (writer, context) =>
                {
                    var loopValues = loopValuesExpr(context);
                    foreach (var val in (object[])loopValues)
                    {
                        context.Set(loopVarName, val);
                        HandleMulti(writer, context, handlers);
                    }
                };
                return (handler, end);
            }
            else if (first == "end" || first == "else")
            {
                return (null, end);
            }
            else
            {
                var expr = GetExpression(tokens, 0);
                Action<Action<string>, Context> handler = (writer, context) =>
                {
                    var value = expr(context);
                    writer(HttpUtility.HtmlEncode(value?.ToString()));
                };
                return (handler, end);
            }
        }
        else if (i < 0)
        {
            return (CheckString(template[start..]), template.Length);
        }
        else
        {
            return (CheckString(template[start..i]), i);
        }
    }
    private static string CheckString(string s)
    {
        if (s.Length == 0)
            throw new Exception();
        return s;
    }
    private static void HandleMulti(Action<string> writer, Context context, List<object> handlers)
    {
        foreach (var handler in handlers)
        {
            if (handler is string s)
            {
                writer(s);
            }
            else
            {
                var h = (Action<Action<string>, Context>)handler;
                h(writer, context);
            }
        }
    }
    private static (List<string>, int) GetTokens(string template, int i)
    {
        var tokens = new List<string>();
        while (true)
        {
            while (template[i] == ' ')
                i++;

            if (template[i] == '}' && template[i + 1] == '}')
                return (tokens, i + 2);
            if (template[i] == '.')
            {
                tokens.Add(".");
                i++;
                continue;
            }

            int start = i++;
            while (i < template.Length && template[i] != '.' && template[i] != '}' && template[i] != ' ')
                i++;
            var token = template[start..i];
            if (token.Length == 0)
                throw new Exception();
            tokens.Add(token);
        }
    }
    private static string? GetHandlerType(string template, int start)
    {
        while (start < template.Length && char.IsWhiteSpace(template[start]))
            start++;

        if (start + 1 < template.Length && template[start] == '{' && template[start + 1] == '{')
        {
            start += 2;
            while (template[start] == ' ')
                start++;
            int tempStart = start++;
            while (template[start] != '.' && template[start] != '}' && template[start] != ' ')
                start++;
            return template[tempStart..start];
        }
        return null;
    }
    private static (List<object>, int) GetBodyHandlers(string template, int end)
    {
        var handlers = new List<object>();
        while (true)
        {
            (var handler, end) = GetHandler(template, end);
            if (handler == null)
                break;
            handlers.Add(handler);
        }
        return (handlers, end);
    }
    private static Func<Context, object> GetExpression(List<string> tokens, int start)
    {
        if (tokens.Count - start == 0)
            throw new Exception();
        if (tokens.Count - start == 1)
        {
            // has one token
            var token = tokens[start];
            return GetTokenAsExpression(token);
        }

        // has two or more tokens
        if (tokens[start + 1] == ".")
        {
            var argGroups = GetArgGroups(tokens, start);
            if (argGroups.Count == 1)
                return argGroups[0];

            throw new Exception();
        }
        else
        {
            // function call
            var f = tokens[start];
            var argGroups = GetArgGroups(tokens, start + 1);
            if (argGroups.Count == 1)
            {
                var expr = argGroups[0];
                return context =>
                {
                    var funcVal = context.Get(f);
                    if (funcVal is Func<object, object> func1)
                    {
                        var val = expr(context);
                        return func1(val);
                    }
                    throw new Exception();
                };
            }
            else
            {
                return context =>
                {
                    var funcVal = context.Get(f);
                    if (funcVal is Func<object[], object> func1)
                    {
                        var args = argGroups.Select(expr => expr(context)).ToArray();
                        return func1(args);
                    }
                    throw new Exception();
                };
            }
        }
    }
    private static Func<Context, object> GetTokenAsExpression(string token)
    {
        return context => Call(context.Get(token));
    }
    private static Func<Context, object> GetMemberAccessExpression(List<string> tokens, int start, int end)
    {
        if (end - start == 1)
        {
            return GetTokenAsExpression(tokens[start]);
        }
        else if (end - start == 3)
        {
            var objName = tokens[start];
            var key = tokens[start + 2];
            return context =>
            {
                var obj = context.Get(objName);
                if (key == "length")
                {
                    if (obj is object[] arr)
                        return arr.Length;
                    else if (obj is string s)
                        return s.Length;
                }
                return Call(obj.GetType().GetProperty(key).GetValue(obj));
            };
        }
        throw new Exception();
    }
    static object Call(object value)
    {
        if (value == null)
            return null;
        var valueType = value.GetType();
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Func<>))
        {
            if (value is Func<object> f)
                return f();
            else
                throw new Exception();
        }
        return value;
    }
    private static List<Func<Context, object>> GetArgGroups(List<string> tokens, int start)
    {
        var args = new List<Func<Context, object>>();

        while (true)
        {
            int end = start;
            if (tokens[end] == ".") throw new Exception();
            end++;
            while (end < tokens.Count && tokens[end] == ".")
            {
                end++;
                if (end < tokens.Count && tokens[end] == ".") throw new Exception();
                end++;
            }

            args.Add(GetMemberAccessExpression(tokens, start, end));
            if (end == tokens.Count)
            {
                return args;
            }
            else
            {
                if (start == end) throw new Exception();
                start = end;
            }
        }
    }
    private static bool Truthy(object o)
    {
        if (o is null) return false;
        if (o is false) return false;
        if (o is string s) return s != "";
        if (o is int i) return i != 0;
        if (o is double d) return d != 0;
        return true;
    }

    interface Context
    {
        object Get(string key);
        void Set(string key, object value);
    }
    class RealContext : Context
    {
        Dictionary<string, object> values = new();
        Dictionary<string, object> helpers;
        object global;

        public RealContext(Dictionary<string, object> helpers, object global)
        {
            this.helpers = helpers;
            this.global = global;

            values["$"] = global;
        }

        public object? Get(string key)
        {
            return values.GetValueOrDefault(key)
                ?? helpers.GetValueOrDefault(key)
                ?? global.GetType().GetProperty(key)?.GetValue(global);
        }
        public void Set(string key, object value)
        {
            values[key] = value;
        }
    }
}
