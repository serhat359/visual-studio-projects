using CasualConsoleCore.XmlParser;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace CasualConsoleCore
{
    public class Program
    {
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
}