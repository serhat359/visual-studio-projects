using Bencode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace CasualConsole
{
    class IntClass : IEquatable<IntClass>
    {
        public int Number { get; set; }

        public bool Equals(IntClass other)
        {
            return this.Number == other.Number;
        }
    }

    class RadicalEntry : IEquatable<RadicalEntry>
    {
        public string Kanji { get; set; }
        public int Stroke { get; set; }
        public int[] Radicals { get; set; }

        public bool Equals(RadicalEntry other)
        {
            return this.Kanji == other.Kanji &&
                this.Stroke == other.Stroke &&
                this.Radicals.SafeEquals(other.Radicals);
        }
    }

    class BigJsonClass : IEquatable<BigJsonClass>
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public int[] IntArray { get; set; }
        public C Object { get; set; }
        public List<D> ObjArray { get; set; }
        public List<E> AnotherObjArray { get; set; }

        public bool Equals(BigJsonClass other)
        {
            return this.Number.Equals(other.Number) &&
                this.Text.Equals(other.Text) &&
                this.IntArray.SafeEquals(other.IntArray) &&
                this.Object.Equals(other.Object) &&
                this.ObjArray.SafeEquals(other.ObjArray) &&
                this.AnotherObjArray.SafeEquals(other.AnotherObjArray);
        }
    }

    class E : IEquatable<E>
    {
        public int? Num { get; set; }

        public bool Equals(E other)
        {
            return this.Num == other.Num;
        }
    }

    class D : IEquatable<D>
    {
        public object Field { get; set; }
        public DateTime Date { get; set; }

        public bool Equals(D other)
        {
            return (this.Field == null) == (other.Field == null) &&
                this.Date == other.Date;
        }
    }

    class C : IEquatable<C>
    {
        public bool SomeBool { get; set; }

        public bool Equals(C other)
        {
            return this.SomeBool == other.SomeBool;
        }
    }

    class PhoneMaker
    {
        public int Maker { get; set; }
        public string[] PhoneLinks { get; set; }
    }

    class Link
    {
        public string Url { get; set; }
        public bool IsChecked { get; set; }
        public bool IsSuccess { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsNotAndroid { get; set; }
        public bool IsNoResolution { get; set; }
        public double? ScreenSize { get; set; }
        public string OSInfo { get; set; }
    }

    public class Option
    {
        public int[] Changes { get; set; }
    }

    public class Result
    {
        public List<int> FinalCopy { get; set; }
        public List<int> Comb { get; set; }
    }

    public class EqComparer : IEqualityComparer<List<int>>
    {
        public bool Equals(List<int> x, List<int> y)
        {
            if (x.Count != y.Count)
                return false;

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<int> obj)
        {
            return obj[0] + 10 * obj[1] + 100 * obj[2] + 1000 * obj[3];
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            client.Encoding = Encoding.UTF8;

            //TestPivot();

            //TestRegex();

            //TestSplitWithCondition();

            //Intersection.TestIntersect();

            //GetPokemonWeaknesses();

            //TestMyRegexReplace();

            //TestRegexReplaceAllFiles();

            //TestAddingBlur();

            //MultiThreadJobQueueTest();

            //TestColorBlending();

            //TestCsvParser();

            //TestTorrentDuplicateFinder();

            //UnityIncreaseMapHeightAll();

            //DumpActiveProcessAndServiceList();

            //var threads = UseAllCPUResources();

            //TestStackPool();

            //TestInputParser();

            //TestDNSPings();

            //BenchmarkJsonParsers();

            //SomeMethodWithAttributeParameter(null);

            //LaytonClockPuzzle.TestLaytonClockPuzzle();

            //MvcBinder.Test();

            //CreateFileLinks();

            //ConvertLinks();

            //const string path = @"C:\Users\Xhertas\Desktop\allLinks.txt";
            //var allLinks = JsonConvert.DeserializeObject<List<Link>>(File.ReadAllText(path));

            //SetResolutions(path, allLinks);

            //SetScreenSize(path, allLinks);

            //FilterFurther(path, allLinks);

            //FilterMoreAndMore();

            //OtherLaytonPuzzle();

            // Closing, Do Not Delete!
            Console.WriteLine();
            Console.WriteLine("Program has terminated, press a key to exit");
            Console.ReadKey();
        }

        private static void OtherLaytonPuzzle()
        {
            var start = new List<int> { 1, 2, 3, 4 };

            var ops = new List<Option> {
                new Option{ Changes = new int[]{  0, 1, 1, 0 } },
                new Option{ Changes = new int[]{  1,-1, 1,-1 } },
                new Option{ Changes = new int[]{ -2, 0, 0, 2 } },
                new Option{ Changes = new int[]{  0, 0, 1, 4 } },
                new Option{ Changes = new int[]{ -4,-3,-2,-1 } },
            };

            var combinations = new List<List<int>>();

            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    for (int k = 0; k < 5; k++)
                        for (int l = 0; l < 5; l++)
                            combinations.Add(new List<int> { i, j, k, l });

            foreach (var comb in combinations)
            {
                comb.Sort();
            }

            combinations = combinations.Distinct(new EqComparer()).ToList();

            Action<Option, List<int>> apply = (op, ar) =>
            {
                for (int i = 0; i < 4; i++)
                {
                    ar[i] += op.Changes[i];
                    ar[i] = ((ar[i] % 6) + 6) % 6;
                    if (ar[i] == 0) ar[i] = 6;
                }
            };

            List<Result> results = combinations.Select(comb =>
            {
                var copy = start.ToList();
                var selectedOptions = comb.Select(x => ops[x]);
                foreach (var op in selectedOptions)
                {
                    apply(op, copy);
                }
                return new Result { FinalCopy = copy, Comb = comb };
            }).ToList();

            var correctResult = results.Where(x => x.FinalCopy.All(y => y == 3)).Select(x => x.Comb).ToList();
        }

        private static void FilterMoreAndMore()
        {
            const string filteredPath = @"C:\Users\Xhertas\Desktop\filteredLinks.txt";
            var filteredLinks = JsonConvert.DeserializeObject<List<Link>>(File.ReadAllText(filteredPath));
            int notCheckCount = filteredLinks.Where(x => x.OSInfo == null).Count();

            int i = 0;
            foreach (var link in filteredLinks.Where(x => x.OSInfo == null))
            {
                i++;

                Action write = () =>
                {
                    File.WriteAllText(filteredPath, JsonConvert.SerializeObject(filteredLinks));
                    Console.WriteLine($"Writing {i} out of {notCheckCount}...");
                };

                if (i % 10 == 0) write();

                string content = client.DownloadString(link.Url);

                var part = GetPart(content, 0, "td class=\"nfo\" data-spec=\"os\"", " </td>");
                var index = part.IndexOf('>');
                var lastIndex = part.IndexOf('<', index);

                var realPart = part.Substring(index + 1, lastIndex - index - 1);

                link.OSInfo = realPart;
            }

            filteredLinks = filteredLinks.OrderBy(x => x.Height / (double)x.Width).ToList();

            filteredLinks.RemoveAll(x => x.ScreenSize > 4.5);
            filteredLinks.RemoveAll(x => x.OSInfo.Contains("Android 2.3"));
            filteredLinks.RemoveAll(x => x.OSInfo.Contains("Android 2.1"));
            filteredLinks.RemoveAll(x => x.OSInfo.Contains("Android 2.2"));
            filteredLinks.RemoveAll(x => x.OSInfo.Contains("planned upgrade to 4.4.2"));

            File.WriteAllText(filteredPath, JsonConvert.SerializeObject(filteredLinks));
        }

        private static void FilterFurther(string path, List<Link> allLinks)
        {
            foreach (var item in allLinks.Where(x => x.IsSuccess && x.ScreenSize.HasValue))
            {
                if (item.Width == 480 && item.Height == 854) item.IsSuccess = false;
                if (item.Width == 240) item.IsSuccess = false;
                if (item.Height == 240) item.IsSuccess = false;
                if (item.Width == 320) item.IsSuccess = false;
                if (item.Height == 320) item.IsSuccess = false;
                if (item.Width == 400) item.IsSuccess = false;
                if (item.Width == 360) item.IsSuccess = false;

                if (item.ScreenSize.Value > 5.0) item.IsSuccess = false;
                if (item.ScreenSize.Value < 3.6) item.IsSuccess = false;

                //Console.WriteLine($"width: {item.Width}, height: {item.Height}, ratio: {(item.Height / (double)item.Width).ToString("N2")}, size: {item.ScreenSize.Value}");
                //Console.WriteLine(item.Url);
            }

            int allcount = allLinks.Count;
            int IsNotAndroid = allLinks.Count(x => x.IsNotAndroid);
            int IsNoResolution = allLinks.Count(x => x.IsNoResolution);
            int IsSuccess = allLinks.Count(x => x.IsSuccess);

            var successOnes = allLinks.Where(x => x.IsSuccess).Select(x => x.Url).ToList();

            var ss = successOnes
                .GroupBy(x => x.Replace("https://www.gsmarena.com/", "").Split('_')[0])
                .Where(x => x.Key != "yezz"
                         && x.Key != "orange"
                         && x.Key != "oppo"
                         && x.Key != "zte"
                         && x.Key != "xolo"
                         && x.Key != "meizu"
                         && x.Key != "maxwest"
                         && x.Key != "wiko"
                         && x.Key != "vivo"
                         && x.Key != "verykool"
                         && x.Key != "niu"
                         && x.Key != "unnecto"
                         && x.Key != "panasonic"
                         && x.Key != "pantech"
                         && x.Key != "plum"
                         && x.Key != "posh"
                         && x.Key != "spice"
                         && x.Key != "lava"
                         && x.Key != "allview"
                         && x.Key != "dell"
                         && x.Key != "cat"
                         && x.Key != "casio"
                         && x.Key != "qmobile"
                         && x.Key != "gionee"
                         && x.Key != "archos"
                         && x.Key != "energizer"
                         && x.Key != "icemobile"
                         && x.Key != "kyocera"
                         && x.Key != "prestigio"
                         && x.Key != "gigabyte"
                         && x.Key != "vertu"
                         && x.Key != "sonim"
                         && x.Key != "sharp"
                         && x.Key != "sagem");

            var otherLinks = new List<Link>();
            foreach (var item in ss.OrderByDescending(x => x.Count()))
            {
                Console.WriteLine(item.Key);
                foreach (var link in item.AsEnumerable())
                {
                    Console.WriteLine("    " + link);

                    otherLinks.Add(allLinks.First(x => x.Url == link));
                }
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(allLinks));
            File.WriteAllText(@"C:\Users\Xhertas\Desktop\filteredLinks.txt", JsonConvert.SerializeObject(otherLinks));
        }

        private static void SetScreenSize(string path, List<Link> allLinks)
        {
            IEnumerable<Link> willBeCheckedLinks = allLinks.Where(x => x.IsSuccess && x.ScreenSize == null);
            int notCheckCount = willBeCheckedLinks.Count();

            int i = 0;
            foreach (var link in willBeCheckedLinks)
            {
                i++;

                Action write = () =>
                {
                    File.WriteAllText(path, JsonConvert.SerializeObject(allLinks));
                    Console.WriteLine($"Writing {i} out of {notCheckCount}...");
                };

                if (i % 10 == 0) write();

                string content = client.DownloadString(link.Url);

                var part = GetPart(content, 0, "<span data-spec=\"displaysize", "</span>");
                var index = part.IndexOf('>');
                var lastIndex = part.IndexOf('<', index);

                var realPart = part.Substring(index + 1, lastIndex - index - 1);

                realPart = realPart.Replace("\"", "");

                if (realPart == "&nbsp;") continue;

                link.ScreenSize = double.Parse(realPart, CultureInfo.InvariantCulture);
            }
        }

        private static void SetResolutions(string path, List<Link> allLinks)
        {
            int notCheckCount = allLinks.Where(x => !x.IsChecked).Count();

            int i = 0;
            foreach (var link in allLinks.Where(x => !x.IsChecked))
            {
                i++;

                Action write = () =>
                {
                    File.WriteAllText(path, JsonConvert.SerializeObject(allLinks));
                    Console.WriteLine($"Writing {i} out of {notCheckCount}...");
                };

                if (i % 10 == 0) write();

                string content = client.DownloadString(link.Url);

                if (!content.Contains("Android"))
                {
                    link.IsChecked = true;
                    link.IsNotAndroid = true;
                    continue;
                }

                var part = GetPart(content, 0, "<td class=\"nfo\" data-spec=\"displayre", "</td>");

                var index = part.IndexOf('>');
                var lastIndex = part.IndexOf('<', index);

                var realPart = part.Substring(index + 1, lastIndex - index - 1);
                var split = realPart.Split(' ');

                if (string.IsNullOrEmpty(split[0]))
                {
                    link.IsNoResolution = true;
                    link.IsChecked = true;
                    continue;
                }

                var first = int.Parse(split[0]);
                var second = int.Parse(split[2].Replace(",", ""));

                var gcd = GCD(first, second);

                var firstSimplified = first / gcd;
                var secondSimplified = second / gcd;

                link.IsChecked = true;

                if (firstSimplified == 9 && secondSimplified == 16)
                    continue;

                link.IsSuccess = true;
                link.Width = first;
                link.Height = second;

                write();
            }
        }

        private static void ConvertLinks()
        {
            var makers = JsonConvert.DeserializeObject<List<PhoneMaker>>(File.ReadAllText(@"C:\Users\Xhertas\Desktop\gsmArenaLinks.txt"));

            var allLinks = makers.SelectMany(x => x.PhoneLinks).Select(x => "https://www.gsmarena.com/" + x);

            var links = allLinks.Select(x => new Link { Url = x }).ToList();

            File.WriteAllText(@"C:\Users\Xhertas\Desktop\allLinks.txt", JsonConvert.SerializeObject(links));
        }

        private static int GCD(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }

        private static void CreateFileLinks()
        {
            var baseLinks = Resource.GsmArenaLinks.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<PhoneMaker> makers = new List<PhoneMaker>();

            int i = 0;
            foreach (var maker in baseLinks)
            {
                var allLinks = GetAllLinksFromBase(maker);
                makers.Add(new PhoneMaker
                {
                    Maker = ++i,
                    PhoneLinks = allLinks.ToArray()
                });

                File.WriteAllText(@"C:\Users\Xhertas\Desktop\gsmArenaLinks.txt", JsonConvert.SerializeObject(makers));
            }
        }

        static MyWebClient client = new MyWebClient();

        public static List<string> GetAllLinksFromBase(string baseLink)
        {
            client.Encoding = Encoding.UTF8;
            string s = client.DownloadString(baseLink);

            var part = GetPart(s, 0, "<div class=\"nav-pages\"", "</div>");
            int pageCount;

            if (part == null)
            {
                pageCount = 1;
            }
            else
            {
                var lastA = part.Substring(part.LastIndexOf("<a"));
                var lastAEnd = lastA.Substring(lastA.IndexOf(">") + 1);
                var number = lastAEnd.Substring(0, lastAEnd.IndexOf("<"));

                pageCount = int.Parse(number);
            }

            var lastDashIndex = baseLink.LastIndexOf('-');
            var leftOfDash = baseLink.Substring(0, lastDashIndex);
            var rightOfDash = baseLink.Substring(lastDashIndex);
            var rightSplit = rightOfDash.Split('.');

            var allPageLinks = new string[] { baseLink }.Concat(Enumerable.Range(2, pageCount - 1).Select(i => leftOfDash + "-f" + rightSplit[0] + "-0-p" + i + ".php")).ToArray();

            var phoneLinks = GetPhoneLinksFromContent(s);
            foreach (var link in allPageLinks.Skip(1))
            {
                client.Encoding = Encoding.UTF8;
                string otherPageContent = client.DownloadString(link);
                var otherPhoneLinks = GetPhoneLinksFromContent(otherPageContent);
                foreach (var phoneLink in otherPhoneLinks)
                {
                    phoneLinks.Add(phoneLink);
                }
            }

            return phoneLinks;
        }

        private static List<string> GetPhoneLinksFromContent(string s)
        {
            var makers = GetPart(s, 0, "<div class=\"makers\">", "</div>");

            var allHrefs = GetAllParts(makers, "<a", "/a>");

            var allLinks = allHrefs.Select(x =>
            {
                var linkPart = GetPart(x, 0, "\"", "\"");
                return linkPart.Substring(1, linkPart.Length - 2);
            }).ToList();

            return allLinks;
        }

        public static List<string> GetAllParts(string wholeString, string start, string end)
        {
            List<string> foundOnes = new List<string>();

            int index = 0;
            while (true)
            {
                var foundIndex = wholeString.IndexOf(start, index);
                if (foundIndex < 0) break;

                var lastIndex = wholeString.IndexOf(end, foundIndex);
                foundOnes.Add(wholeString.Substring(foundIndex, lastIndex - foundIndex + end.Length));

                index = lastIndex;
            }

            return foundOnes;
        }

        public static string GetPart(string wholeString, int startIndex, string start, string end)
        {
            var foundIndex = wholeString.IndexOf(start, startIndex);
            if (foundIndex < 0)
            {
                return null;
            }
            var lastIndex = wholeString.IndexOf(end, foundIndex + 1);

            return wholeString.Substring(foundIndex, lastIndex - foundIndex + end.Length);
        }

        public static void SomeMethodWithAttributeParameter([XmlArray]string parameter)
        {
            var props = typeof(Program).GetMethod(nameof(SomeMethodWithAttributeParameter));

            var attribute = props.GetParameters()[0].GetCustomAttribute<XmlArrayAttribute>();

            var methodName = MethodBase.GetCurrentMethod().Name;

            Console.WriteLine(methodName);
        }

        private static void BenchmarkJsonParsers()
        {
            var dictionary = new List<KeyValuePair<string, object>>();
            EquatableAdd(dictionary, "true", true);
            EquatableAdd(dictionary, "false", false);
            EquatableAdd(dictionary, "1", 1);
            EquatableAdd(dictionary, "-8", -8);
            EquatableAdd(dictionary, "\"serhat\"", "serhat");
            EquatableAdd(dictionary, "\"I said \\\"GO\\\" get it?\"", "I said \"GO\" get it?");
            EquatableAdd(dictionary, "\"2018-03-08\"", new DateTime(2018, 3, 8));
            EquatableAdd(dictionary, "{ \"Number\" : 4 }", new IntClass { Number = 4 });
            EquatableEnumerableAdd(dictionary, "[-3,8]", new int[] { -3, 8 });
            EquatableEnumerableAdd(dictionary, "[-3,8]", new List<int> { -3, 8 });
            EquatableAdd(dictionary, "{ \"Kanji\" : \"上\" }", new RadicalEntry { Kanji = "上" });

            string bigClassJson = @"
            {
	            ""Number"": -2,
	            ""Text"": ""Serhat"",
	            ""IntArray"": [2,3],
	            ""Object"": {
		            ""SomeBool"": true
	            },
	            ""ObjArray"": [
		            { ""Field"": null, ""Date"": ""2017-02-03"" },
		            { ""Field"": {}, ""Date"": ""2018-05-09"" }
	            ],
                ""AnotherObjArray"" : [
                    { ""Num"" : null },
                    { ""Num"" : 4 }
                ]
            }
            ";
            BigJsonClass bigJsonObj = new BigJsonClass
            {
                Number = -2,
                Text = "Serhat",
                IntArray = new int[] { 2, 3 },
                Object = new C { SomeBool = true },
                ObjArray = new List<D> {
                    new D { Field = null, Date = new DateTime(2017, 2, 3) },
                    new D { Field = new object(), Date = new DateTime(2018, 5, 9) }
                },
                AnotherObjArray = new List<E> {
                    new E { Num = null },
                    new E { Num = 4 }
                }
            };
            EquatableAdd(dictionary, bigClassJson, bigJsonObj);

            // Parser test 1
            foreach (var originalValue in dictionary.Select(x => x.Value))
            {
                var json = JSONParserTiny.ToJson(originalValue);
                var result = JSONParserTiny.FromJson(json, originalValue.GetType());

                Type type = result.GetType();

                if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                {
                    Type underlyingType = type.GetGenericArguments().FirstOrDefault() ?? type.GetElementType();

                    var methodInfo = GetMethodInfo((Func<int[], int[], bool>)Extensions.SafeEquals);

                    object compareResult = methodInfo.Invoke(result, new object[] { originalValue, result });

                    if (!(bool)compareResult)
                    {
                        throw new Exception("These are not equals");
                    }
                }
                else
                {
                    MethodInfo methodInfo = type.GetMethod("Equals", new Type[] { type });

                    if (!(bool)methodInfo.Invoke(result, new object[] { originalValue }))
                    {
                        throw new Exception("These are not equals");
                    }
                }
            }

            // Parser test 2
            foreach (var originalValue in dictionary.Select(x => x.Value))
            {
                var json = JSONParserTiny.ToJson(originalValue);
                var result = JsonConvert.DeserializeObject(json, originalValue.GetType());

                Type type = result.GetType();

                if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                {
                    Type underlyingType = type.GetGenericArguments().FirstOrDefault() ?? type.GetElementType();

                    var methodInfo = GetMethodInfo((Func<int[], int[], bool>)Extensions.SafeEquals);

                    object compareResult = methodInfo.Invoke(result, new object[] { originalValue, result });

                    if (!(bool)compareResult)
                    {
                        throw new Exception("These are not equals");
                    }
                }
                else
                {
                    MethodInfo methodInfo = type.GetMethod("Equals", new Type[] { type });

                    if (!(bool)methodInfo.Invoke(result, new object[] { originalValue }))
                    {
                        throw new Exception("These are not equals");
                    }
                }
            }

            // Parser test 3
            foreach (var originalValue in dictionary.Select(x => x.Value))
            {
                var json = JsonConvert.SerializeObject(originalValue);
                var result = JSONParserTiny.FromJson(json, originalValue.GetType());

                Type type = result.GetType();

                if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                {
                    Type underlyingType = type.GetGenericArguments().FirstOrDefault() ?? type.GetElementType();

                    var methodInfo = GetMethodInfo((Func<int[], int[], bool>)Extensions.SafeEquals);

                    object compareResult = methodInfo.Invoke(result, new object[] { originalValue, result });

                    if (!(bool)compareResult)
                    {
                        throw new Exception("These are not equals");
                    }
                }
                else
                {
                    MethodInfo methodInfo = type.GetMethod("Equals", new Type[] { type });

                    if (!(bool)methodInfo.Invoke(result, new object[] { originalValue }))
                    {
                        throw new Exception("These are not equals");
                    }
                }
            }

            Console.WriteLine("Starting benchmark");
            for (int y = 0; y < 10; y++)
            {
                {
                    Func<string, object, object> customParser = (s, o) => MyJsonConvert.CustomJsonParse(s, o.GetType());
                    TestJsonParserWithTest(dictionary, customParser);
                    DateTime now = DateTime.Now;
                    for (int i = 0; i < 10000; i++)
                    {
                        TestJsonParser(dictionary, customParser);
                    }
                    var diff = DateTime.Now - now;
                    Console.WriteLine(diff);
                }

                {
                    Func<string, object, object> originalParser = (s, o) => JsonConvert.DeserializeObject(s, o.GetType());
                    TestJsonParserWithTest(dictionary, originalParser);
                    DateTime now = DateTime.Now;
                    for (int i = 0; i < 10000; i++)
                    {
                        TestJsonParser(dictionary, originalParser);
                    }
                    var diff = DateTime.Now - now;
                    Console.WriteLine(diff);
                }

                {
                    Func<string, object, object> tinyParser = (s, o) => JSONParserTiny.FromJson(s, o.GetType());
                    TestJsonParserWithTest(dictionary, tinyParser);
                    DateTime now = DateTime.Now;
                    for (int i = 0; i < 10000; i++)
                    {
                        TestJsonParser(dictionary, tinyParser);
                    }
                    var diff = DateTime.Now - now;
                    Console.WriteLine(diff);
                }

                Console.WriteLine();
            }
        }

        private static void EquatableEnumerableAdd<T>(List<KeyValuePair<string, object>> dictionary, string jsonText, IEnumerable<T> actualObject) where T : IEquatable<T>
        {
            dictionary.Add(new KeyValuePair<string, object>(jsonText, actualObject));
        }

        private static void EquatableAdd<T>(List<KeyValuePair<string, object>> dictionary, string jsonText, T actualObject) where T : IEquatable<T>
        {
            dictionary.Add(new KeyValuePair<string, object>(jsonText, actualObject));
        }

        private static string ListToJson(System.Collections.IEnumerable list)
        {
            List<string> serializedOnes = new List<string>();

            foreach (var item in list)
            {
                if (item is System.Collections.IEnumerable itemList)
                {
                    serializedOnes.Add(ListToJson(itemList));
                }
                else
                {
                    serializedOnes.Add(item.ToString());
                }
            }

            return "[" + string.Join(",", serializedOnes.ToArray()) + "]";
        }

        private static void TestJsonParser(List<KeyValuePair<string, object>> dictionary, Func<string, object, object> converter)
        {
            foreach (var pair in dictionary)
            {
                var result = converter(pair.Key, pair.Value);
            }
        }

        private static void TestJsonParserWithTest(List<KeyValuePair<string, object>> dictionary, Func<string, object, object> converter)
        {
            foreach (var pair in dictionary)
            {
                var result = converter(pair.Key, pair.Value);

                Type type = result.GetType();

                if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                {
                    Type underlyingType = type.GetGenericArguments().FirstOrDefault() ?? type.GetElementType();

                    var methodInfo = GetMethodInfo((Func<int[], int[], bool>)Extensions.SafeEquals);

                    object compareResult = methodInfo.Invoke(result, new object[] { pair.Value, result });

                    if (!(bool)compareResult)
                    {
                        throw new Exception("These are not equals");
                    }
                }
                else
                {
                    MethodInfo methodInfo = type.GetMethod("Equals", new Type[] { type });

                    object compareResult = methodInfo.Invoke(result, new object[] { pair.Value });

                    if (!(bool)compareResult)
                    {
                        throw new Exception("These are not equals");
                    }
                }
            }
        }

        private static MethodInfo GetMethodInfo(Delegate d)
        {
            return d.Method;
        }

        public static IEnumerable<int> GetHanoiNumbers()
        {
            List<int> sentNumbers = new List<int>();
            int lastNumber = 0;

            while (true)
            {
                int count = sentNumbers.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    yield return sentNumbers[i];
                    sentNumbers.Add(sentNumbers[i]);
                }

                lastNumber++;
                yield return lastNumber;
                sentNumbers.Add(lastNumber);
            }
        }

        private static void SafeSetTrue(bool[][] expandedArray, int x, int y)
        {
            if (x >= 0 && x < expandedArray.Length)
                if (y >= 0 && y < expandedArray[0].Length)
                    expandedArray[x][y] = true;
        }

        private static void TestDNSPings()
        {
            string[] dnsAddresses = @"
209.244.0.3
64.6.64.6
8.8.8.8
9.9.9.9
84.200.69.80
8.26.56.26
208.67.222.222
199.85.126.10
81.218.119.11
195.46.39.39
69.195.152.204
208.76.50.50
216.146.35.35
37.235.1.174
198.101.242.72
77.88.8.8
91.239.100.100
74.82.42.42
109.69.8.51
".Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, int> results = dnsAddresses.ToDictionary(x => x, x => GetPingDNSResult(x));

            string serialized = string.Join("\n", results.Select(x => x.Key + "\t" + x.Value));
        }

        public static int GetPingDNSResult(string dnsIP)
        {
            try
            {
                string output = TestExecuteCommand("ping " + dnsIP);

                string averageString = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[8].Split(',')[2].Split('=')[1];

                int result = int.Parse(Regex.Replace(averageString, "[^0-9]", ""));

                return result;
            }
            catch (Exception)
            {
                return int.MaxValue;
            }
        }

        public static IEnumerable<Point> GetSpiralPoints()
        {
            int level = 1;

            while (true)
            {
                for (int x = -level, y = -level; x < level; x++)
                    yield return new Point(x, y);
                for (int x = level, y = -level; y < level; y++)
                    yield return new Point(x, y);
                for (int x = level, y = level; x > -level; x--)
                    yield return new Point(x, y);
                for (int x = -level, y = level; y > -level; y--)
                    yield return new Point(x, y);

                level++;
            }
        }

        private static void TestInputParser()
        {
            FilterInputParsed result = ParseFilterInput("hello and me");

            result = ParseFilterInput("hello and \"me\"");

            result = ParseFilterInput("hello -and some");

            result = ParseFilterInput("hello -\"and\" -some or this");

            result = ParseFilterInput("hello -\"and\" -some or this \"-thisHasAMinusInItAndShouldBeIncluded\"");
        }

        private static FilterInputParsed ParseFilterInput(string filterInput)
        {
            int valueStartIndex = 0;

            List<string> included = new List<string>();
            List<string> excluded = new List<string>();

            bool hasMinus = false;
            while (valueStartIndex <= filterInput.Length)
            {
                if (GetChar(filterInput, valueStartIndex) == '-')
                {
                    hasMinus = true;
                    valueStartIndex++;
                    continue;
                }
                else if (GetChar(filterInput, valueStartIndex) == '"')
                {
                    valueStartIndex++;
                    int valueEndIndex = valueStartIndex;

                    while (true)
                    {
                        char? c = GetChar(filterInput, valueEndIndex);

                        if (c == null)
                            throw new Exception("Incorrect csv format");
                        else if (c == '"')
                        {
                            char? followingChar = GetChar(filterInput, valueEndIndex + 1);

                            if (followingChar == '"')
                            {
                                valueEndIndex += 2;
                                continue;
                            }
                            else
                                break;
                        }
                        else
                            valueEndIndex++;
                    }

                    // valueEndIndex points to the second quote character
                    string substr = filterInput.Substring(valueStartIndex, valueEndIndex - valueStartIndex).Replace("\"\"", "\"");

                    if (hasMinus)
                        excluded.Add(substr);
                    else
                        included.Add(substr);

                    valueStartIndex = valueEndIndex + 2;
                }
                else
                {
                    int valueEndIndex = valueStartIndex;

                    while (true)
                    {
                        char? c = GetChar(filterInput, valueEndIndex);

                        if (c == null)
                        {
                            break;
                        }

                        if (c == ' ')
                        {
                            break;
                        }

                        valueEndIndex++;
                    }

                    string substr = filterInput.Substring(valueStartIndex, valueEndIndex - valueStartIndex);

                    if (hasMinus)
                        excluded.Add(substr);
                    else
                        included.Add(substr);

                    valueStartIndex = valueEndIndex + 1;
                }

                hasMinus = false;
            }

            return new FilterInputParsed { Excluded = excluded.ToArray(), Included = included.ToArray() };
        }

        private static void TestStackPool()
        {
            StackPool<int> y = new StackPool<int>(5);

            y.Push(1);
            y.Push(3);
            y.Push(2);
            y.Push(-10);
            y.Push(9);
            y.Push(0);
            y.Push(58);
            y.Push(1);
            y.Push(3);
            y.Push(2);
            y.Push(-10);
            y.Push(9);
            y.Push(0);
            y.Push(58);

            if (!Enumerable.SequenceEqual(y.LastToFirst(), new int[] { 58, 0, 9, -10, 2 }))
            {
                throw new Exception();
            }

            if (y.Count != 5)
            {
                throw new Exception();
            }

            if (y.Peek() != 58)
            {
                throw new Exception();
            }

            if (y.Pop() != 58)
            {
                throw new Exception();
            }

            if (y.Peek() != 0)
            {
                throw new Exception();
            }

            if (y.Count != 4)
            {
                throw new Exception();
            }

            if (!Enumerable.SequenceEqual(y.FirstToLast(), new int[] { 2, -10, 9, 0 }))
            {
                throw new Exception();
            }

            if (!Enumerable.SequenceEqual(y.LastToFirst(), new int[] { 0, 9, -10, 2 }))
            {
                throw new Exception();
            }

            if (y.Pop() != 0)
            {
                throw new Exception();
            }

            if (y.Count != 3)
            {
                throw new Exception();
            }

            y.Push(3);

            if (y.Count != 4)
            {
                throw new Exception();
            }

            if (!Enumerable.SequenceEqual(y.LastToFirst(), new int[] { 3, 9, -10, 2 }))
            {
                throw new Exception();
            }

            y.Pop();
            y.Pop();

            if (!Enumerable.SequenceEqual(y.FirstToLast(), new int[] { 2, -10 }))
            {
                throw new Exception();
            }
        }

        private static List<MyThread<int>> UseAllCPUResources()
        {
            List<MyThread<int>> threadList = new List<MyThread<int>>();

            for (int i = 0; i < 4; i++)
            {
                MyThread<int> x = new MyThread<int>(true, () =>
                {
                    while (true)
                    {

                    }
                    return 0;
                });

                threadList.Add(x);
            }

            return threadList;
        }

        private static void DumpActiveProcessAndServiceList()
        {
            var services = ServiceController.GetServices();
            var processes = Process.GetProcesses();

            var allServices = services.Select(service => string.Format("{0} : {1}",
                    service.DisplayName,
                    service.Status));

            var allProcesses = processes.Select(x => new { ProcessName = x.ProcessName, Id = x.Id }).OrderBy(x => x.ProcessName).Select(process => string.Format("{0}",
                 process.ProcessName,
                 process.Id));

            var allLines = ConcatAll(Enumerable.Repeat("Processes:", 1), allProcesses, Enumerable.Repeat("\nServices:", 1), allServices);

            File.WriteAllLines(@"C:\Users\Xhertas\Desktop\serviceStates.txt", allLines);
        }

        public static IEnumerable<T> ConcatAll<T>(params IEnumerable<T>[] elemsArray)
        {
            foreach (var elems in elemsArray)
            {
                foreach (var item in elems)
                {
                    yield return item;
                }
            }
        }

        static string TestExecuteCommand(string command)
        {
            Console.WriteLine("Executing command: {0}", command);

            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine(output);
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();

            return output;
        }

        private static void UnityIncreaseMapHeightAll()
        {
            var originalFile = File.Open(@"C:\Users\Xhertas\Documents\Unity\Ders 1\terrain.raw", FileMode.Open);
            var modifiedFile = File.Open(@"C:\Users\Xhertas\Documents\Unity\Ders 1\terrain_edited.raw", FileMode.Create);

            int addAmount = 20;

            int b;

            for (b = originalFile.ReadByte(); b >= 0; b = originalFile.ReadByte())
            {
                modifiedFile.WriteByte((byte)(Math.Min(255, b + addAmount)));
            }

            originalFile.Close();
            modifiedFile.Close();
        }

        private static void TestTorrentDuplicateFinder()
        {
            string path = Directory.GetCurrentDirectory() + "\\";

            string[] filePaths = Directory.GetFiles(path, "*.torrent", SearchOption.AllDirectories);

            Dictionary<string, string> hashes = new Dictionary<string, string>();

            foreach (var torrentfilePath in filePaths)
            {
                string hash = GetHashStringFromTorrent(torrentfilePath);

                if (hashes.ContainsKey(hash))
                {
                    Console.WriteLine("Hash: {0}, filename: {1}", hash, hashes[hash]);
                    Console.WriteLine("Hash: {0}, filename: {1}", hash, Path.GetFileName(torrentfilePath));
                }
                else
                {
                    hashes.Add(hash, Path.GetFileName(torrentfilePath));
                }
            }
        }

        private static string GetHashStringFromTorrent(string torrentfile)
        {
            var bencode = BencodeUtility.DecodeDictionary(File.ReadAllBytes(torrentfile));

            SHA1Managed sha1 = new SHA1Managed();

            byte[] hash = sha1.ComputeHash(BencodeUtility.Encode(bencode["info"]).ToArray());

            return ConvertByteHashToString(hash);
        }

        private static void TestCsvParser()
        {
            string csvText = Resource.CsvText;

            string[] csvSplittedValues = CsvSplit(csvText);

            csvSplittedValues = CsvSplit("");

            csvSplittedValues = CsvSplit(",");

            csvSplittedValues = CsvSplit(",,");

            csvSplittedValues = CsvSplit("ad,");

            csvSplittedValues = CsvSplit(",adasd");

            csvSplittedValues = CsvSplit(",\"ad,asd\"");

            csvSplittedValues = CsvSplit("\"hello,my name is \"\"serhat\"\"!\",2.4");
        }

        private static string[] CsvSplit(string csvText)
        {
            int valueStartIndex = 0;

            List<string> splittedValues = new List<string>();

            while (valueStartIndex <= csvText.Length)
            {
                if (GetChar(csvText, valueStartIndex) == '"')
                {
                    valueStartIndex++;
                    int valueEndIndex = valueStartIndex;

                    while (true)
                    {
                        char? c = GetChar(csvText, valueEndIndex);

                        if (c == null)
                            throw new Exception("Incorrect csv format");
                        else if (c == '"')
                        {
                            char? followingChar = GetChar(csvText, valueEndIndex + 1);

                            if (followingChar == '"')
                            {
                                valueEndIndex += 2;
                                continue;
                            }
                            else
                                break;
                        }
                        else
                            valueEndIndex++;
                    }

                    // valueEndIndex points to the second quote character
                    string substr = csvText.Substring(valueStartIndex, valueEndIndex - valueStartIndex).Replace("\"\"", "\"");

                    splittedValues.Add(substr);

                    valueStartIndex = valueEndIndex + 2;
                }
                else
                {
                    int valueEndIndex = valueStartIndex;

                    while (true)
                    {
                        char? c = GetChar(csvText, valueEndIndex);

                        if (c == null)
                        {
                            break;
                        }

                        if (c == ',')
                        {
                            break;
                        }

                        valueEndIndex++;
                    }

                    string substr = csvText.Substring(valueStartIndex, valueEndIndex - valueStartIndex);

                    splittedValues.Add(substr);

                    valueStartIndex = valueEndIndex + 1;
                }
            }

            return splittedValues.ToArray();
        }

        private static char? GetChar(string text, int index)
        {
            if (index < text.Length)
                return text[index];
            else
                return null;
        }

        private static void TestColorBlending()
        {
            Bitmap otherPicture = new Bitmap(400, 200);

            for (int x = 0; x < otherPicture.Width; x++)
            {
                int blue = 255 * x / otherPicture.Width;
                int red = blue;
                int green = 255 - blue;

                Color newColor = Color.FromArgb(red, green, blue);

                for (int y = 0; y < otherPicture.Height; y++)
                {
                    otherPicture.SetPixel(x, y, newColor);
                }
            }

            var otherwindow = new MyWindow(otherPicture, "Lazy Method");
            otherwindow.Show();
            otherwindow.Invalidate();

            //Application.Run(otherwindow);

            /////////////////////////////////////
            Bitmap picture = new Bitmap(400, 200);

            for (int x = 0; x < picture.Width; x++)
            {
                int red = 255 * 255 * (x * 100 / picture.Width) / 100;
                int blue = red;
                int green = 255 * 255 * (100 - (x * 100 / picture.Width)) / 100;

                Color newColor = Color.FromArgb((int)Math.Sqrt(red), (int)Math.Sqrt(green), (int)Math.Sqrt(blue));

                for (int y = 0; y < picture.Height; y++)
                {
                    picture.SetPixel(x, y, newColor);
                }
            }

            var newwindow = new MyWindow(picture, "Sqrt Method");
            newwindow.Show();
            newwindow.Invalidate();

            Application.Run(newwindow);
        }

        private static void Benchmark(string operationName, Action action, int executionCount)
        {
            long startTicks = DateTime.Now.Ticks;

            for (int i = 0; i < executionCount; i++)
                action();

            long endTicks = DateTime.Now.Ticks;

            Console.WriteLine("The operation {1} took {0:n} ticks", (endTicks - startTicks), operationName);
        }

        private static void MultiThreadJobQueueTest()
        {
            bool willEnqueueJob = true;

            Queue<ConvertJob> convertQueue = new Queue<ConvertJob>();

            convertQueue.Enqueue(new ConvertJob { FileName = "some name" });
            convertQueue.Enqueue(new ConvertJob { FileName = "some other name" });
            convertQueue.Enqueue(new ConvertJob { FileName = "another name" });
            convertQueue.Enqueue(new ConvertJob { FileName = "yet another name" });

            Action<int, ConsoleColor> convertThreadAction = (threadIndex, color) =>
            {
                while (true)
                {
                    bool hadJob = convertQueue.SafeQueueDoJob(job =>
                    {
                        Console.ForegroundColor = color;
                        Console.WriteLine("I'm thread {0} and I'm starting converting \"{1}\"...", threadIndex, job.FileName);

                        int randomMillis = DateTime.Now.Millisecond % 1000;

                        int jobDuration = randomMillis * 9 + 2000;

                        Thread.Sleep(jobDuration);

                        Console.ForegroundColor = color;
                        Console.WriteLine("I'm thread {0} and I just finished converting \"{1}\" and it took {2} milliseconds", threadIndex, job.FileName, jobDuration);
                    });

                    if (!hadJob)
                    {
                        //Console.WriteLine("Thread {0} reporting: Had no job available, so I'm waiting...", threadIndex);

                        if (willEnqueueJob)
                            Thread.Sleep(500);
                        else
                            break;
                    }
                }
            };

            ConsoleColor[] colors = new ConsoleColor[] { ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue };

            List<MyThread<int>> converterThreadList = Enumerable.Range(0, 4).Select(threadIndex => MyThread.DoInThread(false, () =>
            {
                convertThreadAction(threadIndex, colors[threadIndex]);
                return 0;
            })).ToList();

            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(1500);
                lock (convertQueue)
                {
                    convertQueue.Enqueue(new ConvertJob { FileName = "new file " + i });
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Just enqueued job {0}", i);
            }

            willEnqueueJob = false;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("No more jobs to enqueue, waiting for the thread to finish");

            foreach (var thread in converterThreadList)
            {
                thread.Await();
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("All threads have finished working");
        }

        private static void TestAddingBlur()
        {
            Bitmap picture = new Bitmap(@"C:\Users\Xhertas\Pictures\harfler.png");

            Bitmap newBitmap = new Bitmap(picture);

            for (int x = 1; x < picture.Width - 1; x++)
            {
                for (int y = 1; y < picture.Height - 1; y++)
                {
                    var pixel = picture.GetPixel(x, y);

                    Color[] aroundPixels = {
                        picture.GetPixel(x - 1, y),
                        picture.GetPixel(x + 1, y),
                        picture.GetPixel(x, y - 1),
                        picture.GetPixel(x, y + 1)
                    };

                    int red = aroundPixels.Select(c => (int)c.R).Sum() / aroundPixels.Length;
                    int green = aroundPixels.Select(c => (int)c.G).Sum() / aroundPixels.Length;
                    int blue = aroundPixels.Select(c => (int)c.B).Sum() / aroundPixels.Length;

                    Color mixedPixel = Color.FromArgb(red, green, blue);

                    //newBitmap.SetPixel(x, y, MixPixels(pixel, mixedPixel, 0.5));

                    newBitmap.SetPixel(x, y, mixedPixel);
                }
            }

            var newwindow = new MyWindow(newBitmap, "Image");
            newwindow.Show();
            newwindow.Invalidate();

            Application.Run(newwindow);
        }

        private static Color MixPixels(Color first, Color second, double firstPercentage)
        {
            double secondPercentage = 1 - firstPercentage;
            return Color.FromArgb(
                (int)(first.R * firstPercentage + second.R * secondPercentage),
                (int)(first.G * firstPercentage + second.G * secondPercentage),
                (int)(first.B * firstPercentage + second.B * secondPercentage)
            );
        }

        private static IEnumerable<List<int>> GetBatchValues(IEnumerable<int> source)
        {
            var enumerator = source.GetEnumerator();

            bool hasNext = enumerator.MoveNext();

            do
            {
                List<int> list = new List<int>();

                for (int i = 0; i < 5; i++)
                {
                    if (hasNext)
                        list.Add(enumerator.Current);
                    else
                        break;

                    hasNext = enumerator.MoveNext();
                }

                yield return list;
            } while (hasNext);
        }

        private static string ConvertByteHashToString(byte[] torrentHash)
        {
            string otherHash = string.Concat(torrentHash.Select(x => x.ToString("X2")));

            return otherHash;
        }

        private static void TestRegexReplaceAllFiles()
        {
            string folder = @"C:\Users\Xhertas\Documents\Reflector\Disassembler\DS-Scene Rom Tool";

            string[] allCsFiles = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);

            foreach (string csFile in allCsFiles)
            {
                string oldPattern = @"get => 
                {};";

                string newPattern = "get { return {}; }";

                RegexReplaceFile(csFile, oldPattern, newPattern);
            }
        }

        private static void RegexReplaceFile(string fileLocation, string oldPattern, string newPattern)
        {
            string fileContent = File.ReadAllText(fileLocation);

            fileContent = MyRegexReplace(fileContent, oldPattern, newPattern);

            File.WriteAllText(fileLocation, fileContent);
        }

        private static void TestMyRegexReplace()
        {
            string faultyString = "I've got 10 values";

            string regexFrom = "I've got {} values";
            string regexTo = "The number of values I've got: {}";

            string supposedResult = "The number of values I've got: 10";

            string regexReplaceResult = MyRegexReplace(faultyString, regexFrom, regexTo);

            if (supposedResult != regexReplaceResult)
                throw new Exception("It's not working!");
            else
                Console.WriteLine("The program is working successfully!!!\n\n");
        }

        private static void GetPokemonWeaknesses()
        {
            string[] types = { "normal", "fight", "flying", "poison",
                "ground", "rock", "bug", "ghost",
                "steel", "fire", "water", "grass",
                "electr", "psychic", "ice", "dragon", "dark" };

            int[,] values = {
                { 10,10,10,10,10, 5,10, 0, 5,10,10,10,10,10,10,10,10 },
                { 20,10, 5, 5,10,20, 5, 0,20,10,10,10,10, 5,20,10,20 },
                { 10,20,10,10,10, 5,20,10, 5,10,10,20, 5,10,10,10,10 },
                { 10,10,10, 5, 5, 5,10, 5, 0,10,10,20,10,10,10,10,10 },
                { 10,10, 0,20,10,20, 5,10,20,20,10, 5,20,10,10,10,10 },
                { 10, 5,20,10, 5,10,20,10, 5,20,10,10,10,10,20,10,10 },
                { 10, 5, 5, 5,10,10,10, 5, 5, 5,10,20,10,20,10,10,20 },
                {  0,10,10,10,10,10,10,20, 5,10,10,10,10,20,10,10, 5 },
                { 10,10,10,10,10,20,10,10, 5, 5, 5,10, 5,10,20,10,10 },
                { 10,10,10,10,10, 5,20,10,20, 5, 5,20,10,10,20, 5,10 },
                { 10,10,10,10,20,20,10,10,10,20, 5, 5,10,10,10, 5,10 },
                { 10,10, 5, 5,20,20, 5,10, 5, 5,20, 5,10,10,10, 5,10 },
                { 10,10,20,10, 0,10,10,10,10,10,20, 5, 5,10,10, 5,10 },
                { 10,20,10,20,10,10,10,10, 5,10,10,10,10, 5,10,10, 0 },
                { 10,10,20,10,20,10,10,10, 5, 5, 5,20,10,10, 5,20,10 },
                { 10,10,10,10,10,10,10,10, 5,10,10,10,10,10,10,20,10 },
                { 10, 5,10,10,10,10,10,20, 5,10,10,10,10,20,10,10, 5 },
            };

            int typeCount = types.Length;

            Func<int, IEnumerable<int>> getTakenDamages = col => Enumerable.Range(0, typeCount).Select(row => values[row, col]);

            var allValues = Enumerable.Range(0, typeCount)
                .Select(i => new
                {
                    name = types[i],
                    total = getTakenDamages(i).Sum() * 10,
                    weakness = getTakenDamages(i).Select((val, index) => new { name = types[index], value = val * 10 }).Where(x => x.value > 100).ToList()
                })
                .ToList();

            for (int i = 0; i < typeCount; i++)
            {
                for (int j = i + 1; j < typeCount; j++)
                {
                    List<Pair<int, int>> weakIndices = new List<Pair<int, int>>();
                    int total = 0;
                    foreach (var row in Enumerable.Range(0, typeCount))
                    {
                        var value = values[row, i] * values[row, j];
                        total += value;
                        if (value > 100)
                            weakIndices.Add(new Pair<int, int>(row, value));
                    }

                    allValues.Add(new
                    {
                        name = types[i] + "-" + types[j],
                        total = total,
                        weakness = weakIndices.Select(x => new { name = types[x.value1], value = x.value2 }).ToList()
                    });
                }
            }

            List<string> results = new List<string>();

            double least = allValues.Select(x => x.total).Min();
            foreach (var item in allValues.OrderBy(x => x.total))
            {
                results.Add(
                    string.Format("Name: {0}\tTotalValue: {1}\tWeakness: {2}\tWeakness Count: {3}",
                        item.name,
                        item.total * 100 / least,
                        string.Join(", ", item.weakness.Select(y => y.name + "-" + y.value).Distinct()),
                        item.weakness.Count
                    )
                );
            }

            string allResults = string.Join("\n", results);
        }

        private static int Fib(int x)
        {
            switch (x)
            {
                case 0:
                case 1:
                    return x;
                default:
                    return Fib(x - 1) + Fib(x - 2);
            }

        }

        private static void PrintArray<T>(IEnumerable<T> arr)
        {
            foreach (var item in arr)
            {
                Console.Write(item + "/");
            }
        }

        private static void Dump<T>(T obj)
        {
            DumpProperties(obj);
            Console.WriteLine();

            DumpFields(obj);
            Console.WriteLine();

            DumpMethods(obj);
            Console.WriteLine();
        }

        private static void DumpProperties<T>(T obj)
        {
            var properties = typeof(T).GetProperties();

            Console.WriteLine("Properties: ");
            foreach (var prop in properties)
            {
                Console.WriteLine(prop.Name + ": " + prop.PropertyType.Name + " " + prop.GetValue(obj, null));
            }
        }

        private static void DumpFields<T>(T obj)
        {
            var fields = typeof(T).GetFields();

            Console.WriteLine("Fields: ");
            foreach (var field in fields)
            {
                Console.WriteLine(field.Name + ": " + field.FieldType.Name + " " + field.GetValue(obj));
            }
        }

        private static void DumpMethods<T>(T obj)
        {
            var methods = typeof(T).GetMethods();

            Console.WriteLine("Methods: ");
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0)
                {
                    try
                    {
                        Console.WriteLine(method.Name + ": " + method.Invoke(obj, null));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private static void TestSplitWithCondition()
        {
            string text = @"a,[a,c,b],c,d,[e,x]";

            Func<string, int, bool> splitCond = (e, i) =>
            {
                string leftPart = e.Substring(0, i);

                int bracketIndex = leftPart.LastIndexOfAny(new char[] { '[', ']' });

                if (bracketIndex < 0)
                    return true;
                else if (e[bracketIndex] == ']')
                    return true;
                else if (e[bracketIndex] == '[')
                    return false;
                else
                    throw new Exception();
            };

            string[] splitted = SplitWithCondition(text, ',', splitCond);
        }

        private static string[] SplitWithCondition(string text, char splitChar, Func<string, int, bool> condition)
        {
            List<int> matchIndexes = new List<int>();

            for (int lastFound = 0, index = text.IndexOf(splitChar, lastFound); index >= 0; index = text.IndexOf(splitChar, lastFound))
            {
                bool isValid = condition(text, index);

                if (isValid)
                {
                    matchIndexes.Add(index);
                }

                lastFound = index + 1;
            }

            string[] result = new string[matchIndexes.Count + 1];
            int matchBefore = 0;
            for (int i = 0; i < matchIndexes.Count; i++)
            {
                int currentMatch = matchIndexes[i];
                result[i] = text.Substring(matchBefore, currentMatch - matchBefore);
                matchBefore = currentMatch + 1; // length of split character
            }
            result[matchIndexes.Count] = text.Substring(matchBefore);

            return result;
        }

        private static string MyRegexReplace(string baseString, string regexFrom, string regexTo)
        {
            string pattern = regexFrom
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                //.Replace("{}", "[a-z,A-Z,0-9]+");
                .Replace("{}", ".+");

            string[] patternBrokenDown = Regex.Split(regexFrom, "{}");
            string patternLeft = patternBrokenDown[0];
            string patternRight = patternBrokenDown[1];

            Regex regex = new Regex(pattern);

            var matchCollection = Regex.Matches(baseString, pattern);

            int matchCount = matchCollection.Count;

            Func<Match, string> evaluator = match =>
            {
                string matchWholeValue = match.Value;

                string extractedValue = matchWholeValue.ReplaceOnce(patternLeft, "").ReplaceOnce(patternRight, "");

                string result = regexTo.Replace("{}", extractedValue);

                return result;
            };

            string replaced = regex.Replace(baseString, new MatchEvaluator(evaluator));

            return replaced;
        }

        private static void TestRegex()
        {
            string pattern = "asd<[a-z]+><[0-8]>";
            string text = "asd<hey><4>asd<youthere><2>";

            Regex regex = new Regex(pattern);

            string replaced = regex.Replace(text, new MatchEvaluator(a => new string('*', a.Length)));

            var matchCollection = Regex.Matches(text, pattern);

            Match x = matchCollection[0];
            Match x2 = matchCollection[1];

            int matchCount = matchCollection.Count;

            Group y = x.Groups[0];
            Group y2 = x2.Groups[0];

            int groupCount = x.Groups.Count;

            Capture z = y.Captures[0];
            Capture z2 = y2.Captures[0];

            int captureCount = y.Captures.Count;

            foreach (var match in matchCollection)
            {
                Group group = match as Group;

                string patterMatchingValue = group.Captures[0].Value;

                //foreach (var capture in captures)
                //{
                //    string resultString = capture.ToString();
                //}

                //foreach (var group in match.Groups)
                //{
                //    string grouoString = group.ToString();
                //}

                //string nextvalue = match.NextMatch().Value;
            }
        }

        private static void TestPivot()
        {
            DataTable table = GetTestDebtPivotTable();

            List<FooBar> foobars = new List<FooBar>()
            {
                new FooBar{ Text = "a", Date = new DateTime(2015,12,10), Amount = 3},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,10), Amount = 1},
                new FooBar{ Text = "a", Date = new DateTime(2015,12,10), Amount = 3},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,10), Amount = 4},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,12), Amount = 5},
                new FooBar{ Text = "a", Date = new DateTime(2015,12,12), Amount = 4},
                new FooBar{ Text = "b", Date = new DateTime(2015,12,12), Amount = 2},
            };

            table = DataUtil.PivotAll(foobars, x => x.Date, x => x.Amount, x => x.Average().ToString(), "0", x => x.Month + "-" + x.Day);

            PrintDataTable(table);

            table = DataUtil.Pivot(foobars, x => x.Text, x => x.Date, x => x.Amount, x => x.Sum(), 0, x => x.ToString("MM-dd"));

            PrintDataTable(table);
        }

        private static DataTable GetTestDebtPivotTable()
        {
            List<Debt> debtList = new List<Debt>() {
                new Debt { From = "a", To = "b", When = 2, HowMuch = 4 },
                new Debt { From = "a", To = "c", When = 3, HowMuch = 2 },
                new Debt { From = "b", To = "a", When = 4, HowMuch = 1 },
                new Debt { From = "a", To = "b", When = 4, HowMuch = 3 },
                new Debt { From = "b", To = "a", When = 3, HowMuch = 1 },
                new Debt { From = "a", To = "b", When = 2, HowMuch = 1 },
                new Debt { From = "b", To = "a", When = 2, HowMuch = 2 },
            };

            Func<IEnumerable<int>, string> groupConcat = x => string.Join(",", x);

            DataTable table = DataUtil.PivotAll(debtList, x => x.When, x => x.HowMuch, groupConcat, "0");

            return table;
        }

        private static void PrintDataTable(DataTable table)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                Console.Write(column.ColumnName + "\t");
            }
            Console.WriteLine();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                foreach (object item in row.ItemArray)
                {
                    Console.Write(item + "\t");
                }
                Console.WriteLine();
            }
        }
    }

    public class Pair<T, E>
    {
        public T value1 { get; set; }
        public E value2 { get; set; }

        public Pair(T value1, E value2)
        {
            this.value1 = value1;
            this.value2 = value2;
        }

        public string ToolString()
        {
            return "{" + value1 + "," + value2 + "}";
        }
    }

    public class Debt
    {
        public string From { get; set; }

        public string To { get; set; }

        public int When { get; set; }

        public int HowMuch { get; set; }

        public override string ToString()
        {
            return string.Format("From: {0}, To: {1}, When: {2}, HowMuch: {3}", From, To, When, HowMuch);
        }
    }

    [XmlRoot("foobar")]
    public class FooBar
    {
        [XmlAttribute("text")]
        public string Text { get; set; }

        public DateTime Date { get; set; }

        [XmlElement("amount")]
        public int Amount { get; set; }

        [XmlElement("value")]
        public int[] MultiValue { get; set; }
    }

    public class Dummy : IEquatable<Dummy>
    {
        public int index;
        public string text;
        public string StringProperty { get; set; }

        public Dummy()
        {
        }

        public Dummy(int x, string text)
        {
            this.index = x;
            this.text = text;
        }

        public override string ToString()
        {
            return "Dummy " + index + " and " + text;
        }

        public bool Equals(Dummy dummyObj)
        {
            return dummyObj.text == this.text && dummyObj.index == this.index;
        }
    }

    public class MyWindow : Form
    {
        Bitmap picture;

        public MyWindow(Bitmap picture, string title)
        {
            this.picture = picture;
            this.Name = title;
            this.Text = title;

            this.Size = new Size(16 + picture.Width, 38 + picture.Height);
            this.SetDesktopLocation(500, 300);

            this.Paint += new PaintEventHandler(this.GameFrame_Paint);
        }

        private void GameFrame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g2d = e.Graphics;

            g2d.DrawImage(picture, new Point());
        }
    }

    public class FilterInputParsed
    {
        public string[] Included { get; set; }
        public string[] Excluded { get; set; }
    }
    
    public class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.GZip;
            return request;
        }
    }
}
