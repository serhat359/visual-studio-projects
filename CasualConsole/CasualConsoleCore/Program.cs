using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using CasualConsoleCore.Xml;

namespace CasualConsoleCore;

public class Program
{
    private static readonly Lazy<HttpClient> client = new(() =>
    {
        var client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
        });
        client.DefaultRequestHeaders.Add("User-Agent", "Other");
        return client;
    });

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        //XmlParserTest.Test();

        //GeneratePinyinSrt();

        //FixMovieSubs();

        //await CutVideo();

        //Interpreter.Interpreter.Test();
        //Interpreter.NewInterpreterTest.Test();

        //Interpreter.Interpreter.Benchmark();

        //ZeldaTwilightPrincessPuzzle.Solve();

        //CategorizeUbuntuWallpapers();
    }

    private static void CategorizeUbuntuWallpapers()
    {
        var basePath = @"C:\Users\Xhertas\Downloads\ubuntu-wallpapers-25.04.2";
        var xmlFiles = Directory.GetFiles(basePath).Where(x => x.EndsWith(".xml.in")).ToList();
        foreach (var xmlFile in xmlFiles)
        {
            var newFolderPath = Path.Combine(basePath, GetFileName(xmlFile).Replace(".xml.in", ""));
            Directory.CreateDirectory(newFolderPath);

            var parsed = XmlParser.Parse(File.ReadAllText(xmlFile)
                .Replace("""<?xml version="1.0"?>""", "")
                .Replace("""<!DOCTYPE wallpapers SYSTEM "gnome-wp-list.dtd">""", ""));

            var wallpaperNames = parsed.ChildNodes[0].ChildNodes
                .Select(x => GetFileName(x.ChildNodes.First(x => x.TagName == "filename").InnerText))
                .ToList();
            foreach (var wallpaperName in wallpaperNames)
            {
                var existingFilePath = Path.Combine(basePath, wallpaperName);
                if (File.Exists(existingFilePath))
                {
                    File.Move(sourceFileName: existingFilePath, destFileName: Path.Combine(newFolderPath, wallpaperName));
                }
            }
        }
    }

    private static readonly char[] chars = { '/', '\\' };
    private static string GetFileName(string path)
    {
        int index = path.LastIndexOfAny(chars);
        if (index < 0)
            return path;
        return path[(index + 1)..];
    }

    private static async Task CutVideo()
    {
        var timeStart = "00:00:00";
        var timeEnd = "00:00:00";
        var originalFileName = ".mp4";
        var outputFileName = ".mp4";
        var videoFolder = @"C:\Users\Xhertas\Downloads";
        var ffmpegLocation = @"C:\Users\Xhertas\Downloads\ffmpeg.exe";

        static TimeSpan parseTime(string s)
        {
            var p = s.Split(":").Select(int.Parse).ToArray();
            if (p.Length != 3)
                throw new Exception();
            return new TimeSpan(hours: p[0], minutes: p[1], seconds: p[2]);
        }
        static string timespanToString(TimeSpan t)
        {
            return $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
        }
        TimeSpan diff = parseTime(timeEnd) - parseTime(timeStart);

        var commandArgs = $"-ss {timeStart}.0 -i {originalFileName} -c copy -t {timespanToString(diff)}.0 {outputFileName}";
        var proc = Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = videoFolder,
            Arguments = commandArgs,
            FileName = ffmpegLocation,
        });
        await proc!.WaitForExitAsync();
    }

    private static void ImportNotes()
    {
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        var basePath = @"C:\Users\Xhertas\Desktop\notes transfer page";
        var notes = JsonSerializer.Deserialize<string[]>(File.ReadAllText(basePath + "\\notes generated.json"))
            ?? throw new Exception("notes was null");

        var originalData = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(basePath + "\\manual_backup_orig.txt"))
            ?? throw new Exception("originalData was null");
        var firstNoteText = ((JsonElement)originalData["notes"])[0].ToString();

        var baseDate = DateTime.Now.AddDays(-1);
        static string DateToTextForNotes(DateTime dateTime) => dateTime.ToString("yyyy MM dd  HH:mm:ss");
        int id = 2;
        var newNotes = new List<Dictionary<string, object>>();
        foreach (var note in notes)
        {
            var newDate = baseDate.AddMinutes(id);
            var parsedNote = JsonSerializer.Deserialize<Dictionary<string, object>>(firstNoteText)
                ?? throw new Exception("parsedNote was null");
            parsedNote["uuid"] = Guid.NewGuid().ToString();
            parsedNote["id"] = id;
            parsedNote["content"] = note;
            parsedNote["createdDate"] = DateToTextForNotes(newDate);
            parsedNote["lastModifiedDate"] = DateToTextForNotes(newDate);
            parsedNote["color"] = -769226;
            var newText = JsonSerializer.Serialize(parsedNote, jsonOptions);
            id++;

            newNotes.Add(parsedNote);
        }
        originalData["notes"] = newNotes;
        File.WriteAllText(basePath + "\\manual_backup_modified.txt", contents: JsonSerializer.Serialize(originalData, jsonOptions));
    }

    private static async Task ImportFreecellGame(int gameNo)
    {
        var text = await (await client.Value.GetAsync($"https://freecellgamesolutions.com/fcs/?game={gameNo}")).Content.ReadAsStringAsync();
        var i1 = text.IndexOf("<table id");
        var end = "</table>";
        var i2 = text.IndexOf(end, i1);
        var between = text[i1..(i2 + end.Length)];

        var parts = XmlParser.GetParts(between).ToList();

        parts = parts.Where(x => !x.token.StartsWith('<')).ToList();
        if (parts.Count != 108)
            throw new Exception();
        var cardList = new List<string>(52);
        for (int i = 0; i < 52 * 2; i += 2)
        {
            var group = XmlParser.NormalizeXml(parts[i].token + parts[i + 1].token);
            cardList.Add(group);
        }
        var path = $@"C:\Users\Xhertas\Documents\Visual Studio 2017\Projects\CasualConsole\CasualConsoleCore\FreeCellGames\freecellGame{gameNo}.json";
        var jsonOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        File.WriteAllText(path: path, JsonSerializer.Serialize(new { cards = cardList }, jsonOptions));
    }

    private static void WriteSrtLinesToFile(string filePath, List<SrtParsedLine> lineList)
    {
        using var openWrite = File.OpenWrite(filePath);
        using var streamWriter = new StreamWriter(openWrite, Encoding.UTF8);
        for (int i = 0; i < lineList.Count; i++)
        {
            int lineNumber = i + 1;
            var item = lineList[i];

            streamWriter.WriteLine(lineNumber.ToString());
            streamWriter.WriteLine(item.TimeBegin + " --> " + item.TimeEnd);
            foreach (var xline in item.Lines)
                streamWriter.WriteLine(xline);
            streamWriter.WriteLine("");
        }
    }

    private static void FixMovieSubs()
    {
        var data = File.ReadAllText(@"D:\Downloads\decrypted_files\movie-English.srt");
        var lines = ParseSrtSubtitles(data.Replace("\r", ""));

        static string AddSeconds(string s)
        {
            var beginParsed = SrtTimeParse(s);
            beginParsed += TimeSpan.FromSeconds(31);
            return StringifySrtTimeSpan(beginParsed);
        }

        foreach (var item in lines)
        {
            item.TimeBegin = AddSeconds(item.TimeBegin);
            item.TimeEnd = AddSeconds(item.TimeEnd);
        }

        WriteSrtLinesToFile(@"D:\Downloads\decrypted_files\movie-English2.srt", lines);
    }

    record class SrtParsedLine
    {
        public required string TimeBegin { get; set; }
        public required string TimeEnd { get; set; }
        public required List<string> Lines { get; set; }
    }
    private static List<SrtParsedLine> ParseSrtSubtitles(string s)
    {
        var parsedLines = new List<SrtParsedLine>();

        var lines = s.Split("\n");
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!line.Contains("-->"))
                continue;

            var split = line.Split(" --> ");
            var timeBegin = split[0];
            var timeEnd = split[1];
            var dialogLines = new List<string>();
            i++;
            while (lines[i] != "")
            {
                dialogLines.Add(lines[i]);
                i++;
            }
            parsedLines.Add(new SrtParsedLine
            {
                TimeBegin = timeBegin,
                TimeEnd = timeEnd,
                Lines = dialogLines,
            });
        }
        return parsedLines;
    }

    private static void GeneratePinyinSrt()
    {
        var oldFilePath = @"D:\Downloads\decrypted_files\chinesemovie\chinesemovie-Chinese.srt";
        var newFilePath = @"D:\Downloads\decrypted_files\chinesemovie\chinesemovie-Chinese_2.srt";

        using var openWrite = File.Open(newFilePath, FileMode.Create);
        using var writer = new StreamWriter(openWrite);
        foreach (var line in File.ReadAllLines(oldFilePath))
        {
            var charLength = line.Length;
            var bytes = Encoding.UTF8.GetBytes(line);
            var byteLength = bytes.Length;

            if (charLength != byteLength)
            {
                var converted = PinyinConverter.Convert(line);
                writer.Write(converted);
                writer.Write("\n");
            }
            writer.Write(line);
            writer.Write("\n");
        }
    }

    private static TimeSpan SrtTimeParse(string timeStr)
    {
        var p1 = timeStr.Split(",");
        var millis = int.Parse(p1[1]);
        var p2 = p1[0].Split(":");
        var h = int.Parse(p2[0]);
        var m = int.Parse(p2[1]);
        var s = int.Parse(p2[2]);
        return new TimeSpan(0, h, m, s, millis);
    }

    private static string StringifySrtTimeSpan(TimeSpan time)
    {
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
    }
}

public static class JsonExtensions
{
    public static List<JsonElement> ToList(this JsonElement element)
    {
        var list = new List<JsonElement>(element.GetArrayLength());
        foreach (var x in element.EnumerateArray())
        {
            list.Add(x);
        }
        return list;
    }

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        if (source == null)
            return Enumerable.Empty<T>();
        return source;
    }

    public static T[] EmptyIfNull<T>(this T[]? source)
    {
        if (source == null)
            return Array.Empty<T>();
        return source;
    }

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                value = item;
                return true;
            }
        }
        value = default;
        return false;
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

    /// <summary>
    /// Runs the tasks in parallel with a limit to the thread count. Returns the elements in an unordered way.
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="elements"></param>
    /// <param name="converter"></param>
    /// <param name="threadCount"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<T> ParallelSelectUnorderedAsync<E, T>(this IReadOnlyCollection<E> elements, Func<E, Task<T>> converter, int threadCount)
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
                await channel.Writer.WriteAsync(await currentTask);
            }
        }).ToList();

        for (int i = 0; i < elements.Count; i++)
            yield return await channel.Reader.ReadAsync();
    }

    public static async IAsyncEnumerable<T> ParallelSelectOrderedAsync<E, T>(this IReadOnlyCollection<E> elements, Func<E, Task<T>> converter, int threadCount)
    {
        var channel = Channel.CreateBounded<T>(elements.Count);
        var enumerator = elements.Select(converter).GetEnumerator();

        var innerTasks = new Task?[threadCount]; // Initial values should be null
        int nonNullCount = 0;

        // Initial loop
        for (int i = 0; i < threadCount; i++)
        {
            if (!enumerator.MoveNext())
                break;

            var currentTask = enumerator.Current;
            innerTasks[i] = Task.Run(async () =>
            {
                await channel.Writer.WriteAsync(await currentTask);
            });
            nonNullCount++;
        }

        while (nonNullCount > 0)
        {
            for (int i = 0; i < threadCount; i++)
            {
                var t = innerTasks[i];
                if (t != null)
                    await t;

                if (!enumerator.MoveNext())
                {
                    innerTasks[i] = null;
                    nonNullCount--;
                }
                else
                {
                    var currentTask = enumerator.Current;
                    innerTasks[i] = Task.Run(async () =>
                    {
                        await channel.Writer.WriteAsync(await currentTask);
                    });
                }
            }
        }

        for (int i = 0; i < elements.Count; i++)
            yield return await channel.Reader.ReadAsync();
    }
}

public static class Expressions
{
    public static Expression<Func<T, bool>> Or<T>(IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        var enumerator = expressions.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new Exception("Expression list cannot be empty");
        }

        var expr = enumerator.Current.Body;
        while (enumerator.MoveNext())
            expr = Expression.OrElse(expr, enumerator.Current.Body);

        var parameter = Expression.Parameter(typeof(T), "p");
        var combined = new ParameterReplacer(parameter).Visit(expr);
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    public static Expression<Func<T, bool>> And<T>(IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        var enumerator = expressions.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new Exception("Expression list cannot be empty");
        }

        var expr = enumerator.Current.Body;
        while (enumerator.MoveNext())
            expr = Expression.AndAlso(expr, enumerator.Current.Body);

        var parameter = Expression.Parameter(typeof(T), "p");
        var combined = new ParameterReplacer(parameter).Visit(expr);
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    class ParameterReplacer(ParameterExpression parameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return parameter;
        }
    }
}