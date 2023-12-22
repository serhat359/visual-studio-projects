using System;
using System.Collections.Generic;
using System.Text;

namespace CasualConsoleCore.Templater;

public class Templater
{
    public static Func<object, Dictionary<string, Func<object, object>>, string> CompileTemplate(string template)
    {
        var handlers = new List<object>();
        var (handler, end) = GetHandler(template, 0);
        handlers.Add(handler);

        while (end < template.Length)
        {
            (handler, end) = GetHandler(template, end);
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
                (var handlers, end) = GetBodyHandlers(template, end);

                while (char.IsWhiteSpace(template[end]))
                    end++;
                if (template[end] == '{' && template[end + 1] == '{')
                {
                    var tempEnd = end + 2;
                    while (template[tempEnd] == ' ')
                        tempEnd++;
                    int tempStart = tempEnd++;
                    while (template[tempEnd] != '.' && template[tempEnd] != '}' && template[tempEnd] != ' ')
                        tempEnd++;
                    var word = template[tempStart..tempEnd];
                    if (word == "else")
                    {
                        var elseIfHandlers = new List<(Func<Context, object>, List<object>)>();
                        List<object>? elseHandlers = null;

                        var elseTokensEnd = tempStart;
                        while (true)
                        {
                            (var elseTokens, elseTokensEnd) = GetTokens(template, elseTokensEnd);
                            if (elseTokens.Count == 1)
                            {
                                (var elseInnerHandlers, elseTokensEnd) = GetBodyHandlers(template, elseTokensEnd);
                                elseHandlers = elseInnerHandlers;
                                break;
                            }
                            else
                            {
                                // Has else if
                                if (elseTokens[1] != "if")
                                    throw new Exception();
                                var elseIfExpr = GetExpression(elseTokens, 2);

                                (var elseIfInnerHandlers, elseTokensEnd) = GetBodyHandlers(template, elseTokensEnd);
                                elseIfHandlers.Add((elseIfExpr, elseIfInnerHandlers));
                            }
                        }
                        Action<Action<string>, Context> handler = (writer, context) =>
                        {
                            if (Truthy(ifExpr(context)))
                            {
                                HandleMulti(writer, context, handlers);
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
                        return (handler, elseTokensEnd);
                    }
                }

                Action<Action<string>, Context> simpleHandler = (writer, context) =>
                {
                    var val = ifExpr(context);
                    if (Truthy(val))
                    {
                        HandleMulti(writer, context, handlers);
                    }
                };
                return (simpleHandler, end);
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
            else if (first == "end")
            {
                return (null, end);
            }
            else
            {
                var expr = GetExpression(tokens, 0);
                Action<Action<string>, Context> handler = (writer, context) =>
                {
                    var value = expr(context);
                    writer(value?.ToString());
                };
                return (handler, end);
            }
        }
        else if (i < 0)
        {
            return (template[start..], template.Length);
        }
        else
        {
            return (template[start..i], i);
        }
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
            while (template[i] != '.' && template[i] != '}' && template[i] != ' ')
                i++;
            var token = template[start..i];
            tokens.Add(token);
        }
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

        if (tokens.Count - start == 1)
        {
            // has one token
            var token = tokens[start];
            return context => Call(context.Get(token));
        }

        // has two or more token
        if (tokens[start + 1] == ".")
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
        else
        {
            // function call
            var f = tokens[start];
            var expr = GetExpression(tokens, start+1);
            return context =>
            {
                var func = (Func<object, object>)context.Get(f);
                var val = expr(context);
                return func(val);
            };
        }
    }
    private static bool Truthy(object o)
    {
        if (o is null) return false;
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
        Dictionary<string, Func<object, object>> helpers;
        object global;

        public RealContext(Dictionary<string, Func<object, object>> helpers, object global)
        {
            this.helpers = helpers;
            this.global = global;
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
