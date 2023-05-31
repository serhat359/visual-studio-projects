using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    public class InterviewQuestion
    {
        static int size = 1000000;
        static Dictionary<int, double> map = new Dictionary<int, double>
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

        public static void SolveInterviewQuestion()
        {
            var mapKeys = map.Select(c => c.Key).OrderByDescending(c => c).ToArray();

            var max = GetMax(new SummedList(), 0, mapKeys);

            var result = max + 12.5;
            //Console.WriteLine($"The result is: {result}");
            //Console.ReadKey();
        }

        public static double? GetMax(SummedList included, int nextIndex, int[] mapKeys)
        {
            if (included.Sum > size)
            {
                return null;
            }

            if (nextIndex >= mapKeys.Length)
            {
                return included.GetMapSum(mapKeys, map);
            }

            included.Add(nextIndex, mapKeys);
            var incMax = GetMax(included, nextIndex + 1, mapKeys);
            included.RemoveLast(mapKeys);

            var nonIncMax = GetMax(included, nextIndex + 1, mapKeys);

            return Math.Max(incMax ?? 0, nonIncMax ?? 0);
        }
    }
    
    public class SummedList
    {
        Stack<int> list = new Stack<int>();
        int sum = 0;

        public int Sum => sum;

        public SummedList()
        {
        }

        public void Add(int index, int[] mapKeys)
        {
            list.Push(index);
            sum += mapKeys[index];
        }

        public void RemoveLast(int[] mapKeys)
        {
            var index = list.Pop();
            sum -= mapKeys[index];
        }

        public double GetMapSum(int[] mapKeys, Dictionary<int, double> map)
        {
            double sum = 0;
            foreach (var index in list)
            {
                sum += map[mapKeys[index]];
            }
            return sum;
        }
    }
}
