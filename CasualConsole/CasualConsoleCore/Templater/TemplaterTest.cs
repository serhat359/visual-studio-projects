using NPOI.HSSF.Record;
using System;
using System.Collections.Generic;

namespace CasualConsoleCore.Templater;

public class TemplaterTest
{
    public static void Test()
    {
        Func<object> AsFunc(Func<object> f) => f;

        static object toFixed(object? o)
        {
            if (o is double d)
                return d.ToString("N2");
            throw new Exception();
        }
        static object isPos(object? o)
        {
            if (o is double d)
                return d > 0;
            throw new Exception();
        }
        static object upper(object? o)
        {
            if (o is string s)
                return s.ToUpperInvariant();
            throw new Exception();
        }
        var helpers = new Dictionary<string, Func<object, object>> {
            { "fixed", toFixed },
            { "isPos", isPos },
            { "upper", upper },
        };

        var tests = new (string, object, string)[] {
            ("", new { }, ""),
            ("hello", new { }, "hello"),
            ("<script>", new { }, "<script>"),
            ("{{t}}", new { t="<script>" }, "&lt;script&gt;"),
            ("hello {{w}}", new { w="world" }, "hello world"),
            ("hello {{upper w}}", new { w="world" }, "hello WORLD"),
            ("{{h}} {{w}}", new { h="hello", w="world" }, "hello world"),
            ("the number is {{if b}}there{{end}}{{else}}NOT there{{end}}", new { b = 2 }, "the number is there"),
            ("the number is {{if b}}there{{end}}{{else}}NOT there{{end}}", new { b = 0 }, "the number is NOT there"),
            ("numbers: {{for x in num.inner}}{{x}},{{end}}", new { num=new { inner=new object[]{ 1,2,3 } } }, "numbers: 1,2,3,"),
            ("numbers count: {{num.length}}", new { num=new object[]{ 1,2,3 } }, "numbers count: 3"),
            ("numbers count: {{text.length}}", new { text="hello world" }, "numbers count: 11"),
            ("num: {{numF}}", new { numF = AsFunc(() => 3) }, "num: 3"),
            ("{{a.b}}", new { a = new{ b = "text" } }, "text"),
            ("{{a.b}}", new { a = new{ b = AsFunc(() => "text") } }, "text"),
            ("{{for e in num}}{{e.x}}{{end}}", new { num = new object[]{ new { x=2 }, new { x = 5 }, new { x = 8 }, } }, "258"),
            ("{{if arr.length}}{{for a in arr}}{{end}}{{end}}{{else}}No record found{{end}}", new { arr=new object[]{  } }, "No record found"),
            ("{{fixed number}}", new { number=2.762736723 }, "2.76"),
            ("{{fixed number}}", new { number=AsFunc(()=>2.762736723) }, "2.76"),
            ("{{fixed a.b}}", new {a = new { b = 2.762736723 } }, "2.76"),
            ("{{if isPos a.b}}YES{{end}}{{else}}NO{{end}}", new {a = new { b = 2.762736723 } }, "YES"),
            ("{{if x}}{{end}}{{x}}", new { x=2 }, "2"),
            ("{{if x}}{{end}}    {{else if x}}   {{end}}    {{x}}", new { x=2 }, "    2"),
        };

        foreach (var (template, data, expected) in tests)
        {
            var compiled = Templater.CompileTemplate(template);
            var rendered = compiled(data, helpers);
            if (rendered != expected)
            {
                throw new Exception();
            }
        }

        Console.WriteLine("Passed all Templater tests!!");
    }
}
