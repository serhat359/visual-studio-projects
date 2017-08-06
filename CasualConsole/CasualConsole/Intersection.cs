using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualConsole
{
    class Intersection
    {
        public static IntersectionResult<T, E, V> Intersect<T, E, V>(IEnumerable<T> source1, Func<T, V> source1Selector, IEnumerable<E> source2, Func<E, V> source2Selector)
        {
            Dictionary<V, T> source1Dic = source1.ToDictionary(source1Selector);

            List<Result<T, E, V>> matches = new List<Result<T, E, V>>();
            List<E> rightSide = new List<E>();

            foreach (E source2Item in source2)
            {
                V source2Value = source2Selector(source2Item);

                T source1Match;
                if (source1Dic.TryGetValue(source2Value, out source1Match))
                {
                    Result<T, E, V> match = new Result<T, E, V>()
                    {
                        LeftValue = source1Match,
                        MatchedValue = source2Value,
                        RightValue = source2Item,
                    };

                    matches.Add(match);

                    source1Dic.Remove(source2Value);
                }
                else
                {
                    rightSide.Add(source2Item);
                }
            }

            IntersectionResult<T, E, V> result = new IntersectionResult<T, E, V>
            {
                LeftSide = source1Dic.Values,
                Intersection = matches,
                RightSide = rightSide,
            };

            return result;
        }

        public class IntersectionResult<T, E, V>
        {
            public ICollection<T> LeftSide { get; set; }
            public ICollection<Result<T, E, V>> Intersection { get; set; }
            public ICollection<E> RightSide { get; set; }
        }

        public class Result<T, E, V>
        {
            public T LeftValue { get; set; }
            public E RightValue { get; set; }
            public V MatchedValue { get; set; }
        }

        public static void TestIntersect()
        {
            int[] firstList = { 2, 5, 3, 7, 4, 8 };

            Debt[] secondList = {
                                    new Debt{ From= "asd", To="x", HowMuch = 2, When = 3},
                                    new Debt{ From= "kjs", To="rrtu", HowMuch = 6, When = 2},
                                    new Debt{ From= "ret", To="cc", HowMuch = 9, When = 5}
                                };

            var intersected = Intersect(firstList, x => x, secondList, x => x.HowMuch);

            Console.WriteLine(string.Join(",", intersected.LeftSide));
            Console.WriteLine(string.Join(",", intersected.Intersection.Select(x => x.MatchedValue)));
            Console.WriteLine(string.Join(",", intersected.RightSide.Select(x => x.ToString())));
        }

    }
}
