using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    public class InterviewQuestion
    {
        static int size = 1000000;
        static Dictionary<int, double> dic = new Dictionary<int, double>
        {
            { 15440, 0.0250 },
            { 29240, 0.0532 },
            { 40572, 0.0686 },
            { 57247, 0.0887 },
            { 63718, 0.1147 },
            { 70820, 0.1409 },
            { 77275, 0.1522 },
            { 98732, 0.1856 },
            { 134928, 0.2307 },
            { 139603, 0.2541 },
            { 143807, 0.2660 },
            { 190457, 0.2933 },
        };
        static int[] dicKeys = dic.Select(c => c.Key).ToArray();

        public static void SolveInterviewQuestion()
        {
            var dicList = dic.OrderBy(c => c.Key).ToList();

            var incMax = GetMax(new int[] { 0 }, new int[] { });
            var nonIncMax = GetMax(new int[] { }, new int[] { 0 });

            var result = Math.Max(incMax ?? 0, nonIncMax ?? 0) + 12.5;
            Console.WriteLine($"The result is: {result}");
            Console.ReadKey();
        }

        public static double? GetMax(int[] included, int[] excluded)
        {
            if (included.Select(c => dicKeys[c]).Sum() > size)
            {
                return null;
            }

            int nextIndex = included.Concat(excluded).Max() + 1;
            if (nextIndex >= dicKeys.Length)
            {
                return included.Select(c => dic[dicKeys[c]]).Sum();
            }

            var incMax = GetMax(included.Concat(new int[] { nextIndex }).ToArray(), excluded);
            var nonIncMax = GetMax(included, excluded.Concat(new int[] { nextIndex }).ToArray());

            return Math.Max(incMax ?? 0, nonIncMax ?? 0);
        }

    }
}
