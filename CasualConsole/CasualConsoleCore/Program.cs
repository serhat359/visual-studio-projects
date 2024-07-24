using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CasualConsoleCore;

public class Program
{
    private static readonly HttpClient client = new(new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.All,
        ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
    });

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        client.DefaultRequestHeaders.Add("User-Agent", "Other");

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

        //XmlParserTest.Test();

        //FixSubtitleTimings();

        //GeneratePinyinSrt();

        //FixMovieSubs();

        //Interpreter.Interpreter.Test();
        //Interpreter.NewInterpreterTest.Test();

        return;
        var newFilePath = @"D:\Downloads\decrypted_files\chinesemovie\chinesemovie-Chinese_2.srt";
        var englishFilePath = @"D:\Downloads\decrypted_files\chinesemovie\chinesemovie_English.srt";
        var mergedFilePath = @"D:\Downloads\decrypted_files\chinesemovie\chinesemovie_merged.srt";

        var chineseSrt = File.ReadAllText(newFilePath).Replace("\r", "");
        var englishSrt = File.ReadAllText(englishFilePath).Replace("\r", "");

        var chineseParsed = ParseSrtSubtitles(chineseSrt);
        var englishParsed = ParseSrtSubtitles(englishSrt);
        foreach (var englishParsedItem in englishParsed)
            if (englishParsedItem.Lines.Count > 1)
                englishParsedItem.Lines = new List<string> { string.Join(" ", englishParsedItem.Lines) };

        var mergedList = chineseParsed.Concat(englishParsed).OrderBy(x => x.TimeBegin).ToList();

        WriteSrtLinesToFile(mergedFilePath, mergedList);
    }

    private static void ImportNotes()
    {
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        var basePath = @"C:\Users\Xhertas\Desktop\notes transfer page";
        var notes = JsonSerializer.Deserialize<string[]>(File.ReadAllText(basePath + "\\notes generated.json"));

        var originalData = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(basePath + "\\manual_backup_orig.txt"));
        var firstNoteText = ((JsonElement)originalData["notes"])[0].ToString();

        var baseDate = DateTime.Now.AddDays(-1);
        static string DateToTextForNotes(DateTime dateTime) => dateTime.ToString("yyyy MM dd  HH:mm:ss");
        int id = 2;
        var newNotes = new List<Dictionary<string, object>>();
        foreach (var note in notes)
        {
            var newDate = baseDate.AddMinutes(id);
            var parsedNote = JsonSerializer.Deserialize<Dictionary<string, object>>(firstNoteText);
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
        var text = await (await client.GetAsync($"https://freecellgamesolutions.com/fcs/?game={gameNo}")).Content.ReadAsStringAsync();
        var i1 = text.IndexOf("<table id");
        var end = "</table>";
        var i2 = text.IndexOf(end, i1);
        var between = text[i1..(i2 + end.Length)];

        var parts = XmlParser.XmlParser.GetParts(between).ToList();

        parts = parts.Where(x => !x.token.StartsWith('<')).ToList();
        if (parts.Count != 108)
            throw new Exception();
        var cardList = new List<string>(52);
        for (int i = 0; i < 52 * 2; i += 2)
        {
            var group = XmlParser.XmlParser.NormalizeXml(parts[i].token + parts[i + 1].token);
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

        using (var openWrite = File.OpenWrite(newFilePath))
        {
            void WriteToFile(string s) => openWrite.Write(Encoding.UTF8.GetBytes(s));

            foreach (var line in File.ReadAllLines(oldFilePath))
            {
                var charLength = line.Length;
                var bytes = Encoding.UTF8.GetBytes(line);
                var byteLength = bytes.Length;

                if (charLength != byteLength)
                {
                    var converted = PinyinConverter.Convert(line);
                    WriteToFile(converted);
                    WriteToFile("\n");
                }
                WriteToFile(line);
                WriteToFile("\n");
            }
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