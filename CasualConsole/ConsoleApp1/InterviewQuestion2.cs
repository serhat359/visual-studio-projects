using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    public class InterviewQuestion2
    {
        static int totalSize = 1000000;
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
            var keys = map.Keys.OrderByDescending(e => e).ToArray();
            var keysTotalSize = keys.Sum();

            var includedList = new SummedCollection();
            var excludedList = new SummedCollection();

            var result = GetMax(includedList, excludedList, 0, keys, keysTotalSize) + 12.5;

            //Console.WriteLine($"The result is: {result}");
            //Console.ReadKey();
        }

        public static double? GetMax(SummedCollection includedList, SummedCollection excludedList, int nextKeyIndex, int[] keys, int keysTotalSize)
        {
            int remainingSize = totalSize - includedList.Sum;
            int remainingTotalKeySize = keysTotalSize - includedList.Sum - excludedList.Sum;

            if (remainingTotalKeySize <= remainingSize)
            {
                return keys.Where(e => !excludedList.Contains(e)).Select(e => map[e]).Sum();
            }

            var nextKey = keys[nextKeyIndex];

            excludedList.Add(nextKey);
            var excludedMax = GetMax(includedList, excludedList, nextKeyIndex + 1, keys, keysTotalSize);
            excludedList.Remove(nextKey);

            double? includedMax = null;
            if (nextKey <= remainingSize)
            {
                includedList.Add(nextKey);
                includedMax = GetMax(includedList, excludedList, nextKeyIndex + 1, keys, keysTotalSize);
                includedList.Remove(nextKey);
            }

            return Math.Max(includedMax ?? 0, excludedMax ?? 0);
        }
    }

    public class SummedCollection
    {
        private HashSet<int> elements = new HashSet<int>();
        private int sum = 0;

        public int Sum => sum;

        public bool Contains(int elem)
        {
            return elements.Contains(elem);
        }

        public void Add(int elem)
        {
            if (elements.Add(elem))
                sum += elem;
        }

        public void Remove(int elem)
        {
            if (elements.Remove(elem))
                sum -= elem;
        }
    }
}
