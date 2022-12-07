using CasualConsoleCore.XmlParser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CasualConsoleCore
{
    public class Program
    {
        private static HttpClient client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
        });

        static void Main(string[] args)
        {
            //Interpreter.Interpreter.Test();
            //Interpreter.Interpreter.Benchmark();

            //StartInterpreterConsole();

            //FixSubtitleTimings();

            XmlParserTest.Test();
        }

        private static void FixSubtitleTimings()
        {
            var path = @"D:\Downloads\decrypted_files\House.of.the.Dragon.S01E03.Second.of.His.Name.2160p.CRAV.WEB-DL.DDP5.1.HEVC-NTb - No HI.srt";
            var path2 = @"D:\Downloads\decrypted_files\House.of.the.Dragon.S01E03.Second.of.His.Name.2160p.CRAV.WEB-DL.DDP5.1.HEVC-NTb - No HI_2.srt";

            var content = System.IO.File.ReadAllText(path);

            var expectedTime1 = new TimeSpan(0, 0, 2, 21, 160);
            var inFile1 = new TimeSpan(0, 0, 2, 19, 662);
            var off1 = inFile1 - expectedTime1;

            var expectedTime2 = new TimeSpan(0, 0, 52, 31, 931);
            var inFile2 = new TimeSpan(0, 0, 52, 30, 501);
            var off2 = inFile2 - expectedTime2;

            var inFileDiff = inFile2 - inFile1;
            var offDiff = off2 - off1;
            var ratio = offDiff / inFileDiff;

            static TimeSpan parser(string timeStr)
            {
                var p1 = timeStr.Split(",");
                var millis = int.Parse(p1[1]);
                var p2 = p1[0].Split(":");
                var h = int.Parse(p2[0]);
                var m = int.Parse(p2[1]);
                var s = int.Parse(p2[2]);
                return new TimeSpan(0, h, m, s, millis);
            }

            static string stringifier(TimeSpan time)
            {
                return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
            }

            string fixer(string timeStr)
            {
                var timex = parser(timeStr);
                var offRealx = ratio * (timex - inFile1) + off1;

                return stringifier(timex - offRealx);
            }

            var newContent = string.Join("\n", content.Split("\n").Select(row =>
            {
                row = row.Replace("\r", "");
                if (!row.Contains("-->"))
                    return row;

                var splitter = " --> ";
                var parts = row.Split(splitter);

                return fixer(parts[0]) + splitter + fixer(parts[1]);
            }));

            System.IO.File.WriteAllText(path2, newContent);
        }

        private static void StartInterpreterConsole()
        {
            Console.WriteLine("Welcome to Serhat's JS Interpreter!");
            var consoleInterpreter = new Interpreter.Interpreter();
            while (true)
            {
                Console.Write("$: ");
                string line = Console.ReadLine()!;
                try
                {
                    var val = consoleInterpreter.InterpretCode(line);
                    if (val is null)
                    {
                        var oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Error.WriteLine("(null)");
                        Console.ForegroundColor = oldColor;
                    }
                    else if (val is bool valbool)
                        Console.WriteLine(valbool ? "true" : "false");
                    else
                        Console.WriteLine(val.ToString());
                }
                catch (Exception e)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.Message);
                    Console.ForegroundColor = oldColor;
                }
            }
        }
    }

    public static class AsyncExtensions
    {
        public static async IAsyncEnumerable<E> SelectAsync<T, E>(this IEnumerable<T> source, Func<T, Task<E>> converter)
        {
            foreach (var item in source)
                yield return await converter(item);
        }

        public static async IAsyncEnumerable<E> Select<T, E>(this IAsyncEnumerable<T> source, Func<T, E> converter)
        {
            await foreach (var item in source)
                yield return converter(item);
        }

        public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
        {
            await foreach (var item in source)
                if (predicate(item))
                    yield return item;
        }

        public static async IAsyncEnumerable<T> ParallelSelectAsync<E, T>(this IReadOnlyCollection<E> elements, Func<E, Task<T>> converter, int threadCount)
        {
            var channel = Channel.CreateBounded<T>(elements.Count);
            var enumerator = elements.Select(converter).GetEnumerator();

            var _innerTasks = Enumerable.Range(0, threadCount).Select(async x =>
            {
                while (true)
                {
                    Task<T> currentTask;
                    lock (enumerator)
                    {
                        if (!enumerator.MoveNext())
                            return;
                        currentTask = enumerator.Current;
                    }
                    var res = await currentTask;
                    await channel.Writer.WriteAsync(res);
                }
            }).ToList();

            for (int i = 0; i < elements.Count; i++)
                yield return await channel.Reader.ReadAsync();
        }
    }

    public static class XmlExtensions
    {
        public static IEnumerable<MyXmlNode> GetAllRecursive(this MyXmlNode node)
        {
            yield return node;
            foreach (var childNode in node.ChildNodes)
            {
                foreach (var childChildNode in GetAllRecursive(childNode))
                {
                    yield return childChildNode;
                }
            }
        }
    }
}