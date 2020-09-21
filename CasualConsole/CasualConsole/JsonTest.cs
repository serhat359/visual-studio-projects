using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CasualConsole
{
    public class JsonTest
    {

        private static void BenchmarkJsonParsers()
        {
            var dictionary = new List<PairAndValueComparer<string, object>>();
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

            var jsonDic = @"[
            {
              ""Key"": 3,
              ""Value"": ""uc""
            },
            {
              ""Key"": 4,
              ""Value"": ""alti""
            }
            ]";
            Dictionary<int, string> objDic = new Dictionary<int, string>
            {
                { 3, "uc" },
                { 4, "alti" }
            };
            EquatableEnumerableAdd(dictionary, jsonDic, objDic, new KeyValuePairEquator<int, string>());

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

            var tests = new JsonTestCase[]{
                new JsonTestCase
                {
                    Description = "string with Tiny, object with Tiny",
                    Serializer = JSONParserTiny.ToJson,
                    Deserializer = JSONParserTiny.FromJson
                },
                new JsonTestCase
                {
                    Description = "string with Tiny, object with Newtonsoft",
                    Serializer = JSONParserTiny.ToJson,
                    Deserializer = JsonConvert.DeserializeObject
                },
                new JsonTestCase
                {
                    Description = "string with Newtonsoft, object with Tiny",
                    Serializer = JsonConvert.SerializeObject,
                    Deserializer = JSONParserTiny.FromJson
                }
            };

            foreach (var test in tests)
            {
                foreach (var pair in dictionary)
                {
                    var originalValue = pair.Value;

                    var json = test.Serializer(originalValue);
                    var result = test.Deserializer(json, originalValue.GetType());

                    Type type = result.GetType();

                    if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                    {
                        var methodInfo = pair.Comparer;

                        bool compareResult = methodInfo(originalValue, result);

                        if (!compareResult)
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
            }
            Console.WriteLine("All json tests are successful!");

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

        private static void EquatableEnumerableAdd<T>(List<PairAndValueComparer<string, object>> dictionary, string jsonText, IEnumerable<T> actualObject, IEqualityComparer<T> baseComparer)
        {
            object value = actualObject;
            Func<IEnumerable<T>, IEnumerable<T>, bool> comparer = (x, y) => x.SafeEquals(y, (a, b) => baseComparer.Equals(a, b));
            Func<object, object, bool> castedComparer = (x, y) => comparer((IEnumerable<T>)x, (IEnumerable<T>)y);

            var obj = new PairAndValueComparer<string, object>
            {
                Key = jsonText,
                Value = actualObject,
                Comparer = castedComparer
            };

            dictionary.Add(obj);
        }

        private static void EquatableEnumerableAdd<T>(List<PairAndValueComparer<string, object>> dictionary, string jsonText, IEnumerable<T> actualObject) where T : IEquatable<T>
        {
            object value = actualObject;
            Func<IEnumerable<T>, IEnumerable<T>, bool> comparer = (x, y) => x.SafeEquals(y, (a, b) => a.Equals(b));
            Func<object, object, bool> castedComparer = (x, y) => comparer((x as IEnumerable).Cast<T>(), (y as IEnumerable).Cast<T>());

            var obj = new PairAndValueComparer<string, object>
            {
                Key = jsonText,
                Value = actualObject,
                Comparer = castedComparer
            };

            dictionary.Add(obj);
        }

        private static void EquatableAdd<T>(List<PairAndValueComparer<string, object>> dictionary, string jsonText, T actualObject) where T : IEquatable<T>
        {
            dictionary.Add(new PairAndValueComparer<string, object>
            {
                Key = jsonText,
                Value = actualObject,
                Comparer = (x, y) => x.Equals(y)
            });
        }

        private static void TestJsonParser(List<PairAndValueComparer<string, object>> dictionary, Func<string, object, object> converter)
        {
            foreach (var pair in dictionary)
            {
                var result = converter(pair.Key, pair.Value);
            }
        }

        private static MethodInfo GetMethodInfo(Delegate d)
        {
            return d.Method;
        }

        private static void TestJsonParserWithTest(List<PairAndValueComparer<string, object>> dictionary, Func<string, object, object> converter)
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

    }

    class IntClass : IEquatable<IntClass>
    {
        public int Number { get; set; }

        public bool Equals(IntClass other)
        {
            return this.Number == other.Number;
        }
    }

    class PairAndValueComparer<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }
        public Func<V, V, bool> Comparer { get; set; }
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

    public class KeyValuePairEquator<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
        where TKey : IEquatable<TKey>
        where TValue : IEquatable<TValue>
    {
        public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            return x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
        }

        public int GetHashCode(KeyValuePair<TKey, TValue> obj)
        {
            throw new NotImplementedException();
        }
    }

    public class JsonTestCase
    {
        public string Description { get; set; }
        public Func<object, string> Serializer { get; set; }
        public Func<string, Type, object> Deserializer { get; set; }
    }

}
