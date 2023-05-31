using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    public class Program
    {
        const int count = 1000;

        public static void Main(string[] args)
        {
            //XorSolution();

            //Console.WriteLine(solveXorNew(ints, inte));
            //Console.WriteLine(solveXor(ints, inte));

            InterviewQuestion.SolveInterviewQuestion();
            InterviewQuestion2.SolveInterviewQuestion();

            var sw = new Stopwatch();

            while (true)
            {
                sw.Restart();
                for (int i = 0; i < count; i++)
                {
                    InterviewQuestion.SolveInterviewQuestion();
                }
                sw.Stop();
                Console.WriteLine("Algo 1: " + sw.ElapsedMilliseconds);

                sw.Restart();
                for (int i = 0; i < count; i++)
                {
                    InterviewQuestion2.SolveInterviewQuestion();
                }
                sw.Stop();
                Console.WriteLine("Algo 2: " + sw.ElapsedMilliseconds);
            }

            return;

            //TestGetXResult();

            var path = @"C:\Users\Xhertas\Documents\Visual Studio 2017\Projects\CasualConsole\ConsoleApp1\input.txt";

            var strList = File.ReadAllLines(path).Select(x => int.Parse(x)).OrderBy(c => c).ToList();

            //var result = 1;
            //for (int i = 0; i < strList.Count; i++)
            //{
            //    var elem = strList[i] - i;
            //    result *= elem;
            //}

            //long result = 0;
            //for (int k = 0; k < strList.Count - 1; k++)
            //{
            //    var first = strList[k];
            //    for (int i = 1; i <= first; i++)
            //    {
            //        for (int y = 1; y < strList.Count - k; y++)
            //        {
            //            var thatElem = strList[y];
            //            if (thatElem >= i)
            //                result += thatElem - 1;
            //            else
            //                result += thatElem;
            //        }

            //        result %= 1000000007;
            //    }
            //}

            //for (int k = 0; k < strList.Count - 1; k++)
            //{
            //    var first = strList[k];

            //    for (int y = 1; y < strList.Count - k; y++)
            //    {
            //        var thatElem = strList[y + k];
            //        result += (first - 1) * thatElem;
            //        result %= 1000000007;
            //    }
            //}

            // Sort it first before the next step
            //var boxes = strList.OrderBy(c => c).ToArray();

            //long result2 = 1;
            //for (int i = 0; i < boxes.Length; i++)
            //{
            //    var marblesInBox = boxes[i];
            //    var usedMarbles = i;
            //    result2 = (result2 * (marblesInBox - usedMarbles)) % 1000000007;
            //}

            long result = 1;
            for (int i = 0; i < strList.Count; i++)
            {
                var elem = strList[i] - i;
                result = (result * elem) % 1000000007;
            }

            System.Console.WriteLine(result);
            //Console.WriteLine(possibility);
            Console.WriteLine(result % 1000000007);

            Console.ReadKey();
        }

        private static void TestGetXResult()
        {
            List<KeyValuePair<int[], int[]>> cases = new List<KeyValuePair<int[], int[]>>();
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 1, 0, 0, 1, 1 }, new[] { 1, 0, 1 }));
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 1, 0, 0, 1, 1, 1 }, new[] { 1, 0, 1, 0, 1, 1 }));
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 0, 1, 1, 1, 1 }, new[] { 1, 0, 1 }));
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 1, 1, 0, 1, 1 }, new[] { 0, 0, 1 }));
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 1, 0, 0, 1 }, new[] { 1, 0, 1, 1 }));
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 0, 1, 1, 1 }, new[] { 1, 0, 1, 1 }));
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 1, 1, 1, 1 }, new[] { 1, 0, 1, 1 }));
            cases.Add(new KeyValuePair<int[], int[]>(new[] { 1, 0, 1, 0, 1, 1 }, new[] { 1, 1, 1, 1 }));

            foreach (var A in cases)
            {
                var result = GetXResult(A.Key);
                var value = A.Value;

                if (result.Length != value.Length)
                {
                    throw new Exception("Sizes don't match!");
                }

                for (int i = 0; i < result.Length; i++)
                {
                    if (value[i] != result[i])
                    {
                        throw new Exception("Values don't match!");
                    }
                }
            }
        }

        private static int[] GetXResult(int[] A)
        {
            int[] result = Enumerable.Repeat(0, A.Length).ToArray();

            int i = 0;
            HandleStart(A, result, ref i);

            while (i < A.Length)
            {
                if (A[i] == 0)
                {
                    i++;
                    continue;
                }

                if (IsOneOne(A, i))
                {
                    var oldResult = result[i - 1];
                    if (oldResult == 0)
                    {
                        result[i - 1] = 1;
                        i += 2;
                        continue;
                    }

                    throw new Exception();
                }

                if (IsOnlyOne(A, i) || IsOneZero(A, i))
                {
                    if (result[i] == 1 || result[i - 1] == 1)
                    {
                        throw new Exception();
                    }

                    result[i] = 1;
                    result[i - 1] = 1;

                    i++;
                    continue;
                }

                if (i == A.Length)
                {
                    break;
                }

                throw new Exception();
            }

            int lastIndex = -1;
            for (int newI = result.Length - 1; newI > 0; newI--)
            {
                if (result[newI] == 1)
                {
                    lastIndex = newI;
                    break;
                }
            }

            if (lastIndex == result.Length - 1)
            {
                return result;
            }
            else
            {
                return result.Take(lastIndex + 1).ToArray();
            }
        }

        private static void HandleStart(int[] A, int[] result, ref int i)
        {
            if (IsZero(A, i))
            {
                i++;
                return;
            }
            else if (IsOneZero(A, i))
            {
                result[i] = 1;
                i += 2;
                return;
            }
            else if (IsOneOneZero(A, i))
            {
                i += 3;
                return;
            }
            else if (IsOneOneOne(A, i))
            {
                result[i] = 1;
                i += 3;
                return;
            }

            throw new Exception();
        }

        private static bool IsOneOne(int[] arr, int index)
        {
            if (index + 1 >= arr.Length)
                return false;

            if (arr[index] == 1 && arr[index + 1] == 1)
                return true;
            else
                return false;
        }

        private static bool IsOneZero(int[] arr, int index)
        {
            if (index + 1 >= arr.Length)
                return false;

            if (arr[index] == 1 && arr[index + 1] == 0)
                return true;
            else
                return false;
        }

        private static bool IsOnlyOne(int[] arr, int index)
        {
            return arr[index] == 1 && arr.Length - 1 == index;
        }

        private static bool IsZero(int[] arr, int index)
        {
            return arr[index] == 0;
        }

        private static bool IsOneOneZero(int[] arr, int index)
        {
            if (index + 2 >= arr.Length)
                return false;

            if (arr[index] == 1 && arr[index + 1] == 1 && arr[index + 2] == 0)
                return true;
            else
                return false;
        }

        private static bool IsOneOneOne(int[] arr, int index)
        {
            if (index + 2 >= arr.Length)
                return false;

            if (arr[index] == 1 && arr[index + 1] == 1 && arr[index + 2] == 1)
                return true;
            else
                return false;
        }

        private static void XorSolution()
        {
            var y = 262343;
            var u = 1249266262;

            Console.WriteLine(solveXorNew(y, u));
            Console.WriteLine(solveXorBrute(y, u));
            Console.WriteLine(solveXor(y, u));
        }

        public static int solveXorBrute(int start, int end)
        {
            var result = start;
            for (int i = start + 1; i <= end; i++)
            {
                result ^= i;
            }
            return result;
        }

        public static int solveXorNew(int start, int end)
        {
            int count = end - start + 1;
            int result = 0;
            for (int bit = 32; bit > 1; bit--)
            {
                int subResult;
                int bitMinusOne = bit - 1;
                uint startRegion = ((uint)start) >> (bitMinusOne);
                uint endRegion = ((uint)end) >> (bitMinusOne);

                if (startRegion == endRegion)
                {
                    var bitThere = startRegion & 1;
                    subResult = ((int)bitThere * count) % 2;
                }
                else
                {
                    int startRegionMax = (int)((startRegion + 1) << (bitMinusOne));
                    int endRegionMin = (int)((endRegion) << (bitMinusOne));

                    int startBitThere = (int)(startRegion & 1);
                    int startRegionBit = ((startRegionMax - start) * startBitThere) % 2;

                    int endBitThere = (int)(endRegion & 1);
                    int endRegionBit = ((end - endRegionMin + 1) * endBitThere) % 2;

                    subResult = startRegionBit ^ endRegionBit;
                }

                result |= subResult << (bitMinusOne);
            }

            // lastbit
            {
                int rem = count % 2;
                int div = count / 2;

                int bitFromDiv = div % 2;
                int bitFromRem = rem == 0 ? 0 : (start % 2);

                result |= ((bitFromDiv + bitFromRem) % 2);
            }

            return result;
        }

        public static int solveXor(int start, int end)
        {
            int count = end - start + 1;

            int result = 0;
            for (int bit = 1; bit < 32; bit++)
            {
                var subres = CalcForBitNo(start, end, bit, count);
                result |= (subres << (bit - 1));
            }

            return result;
        }

        public static int CalcForBitNo(int start, int end, int bitNo, int count)
        {
            int wholeNo = 1 << bitNo;

            int rem = count % wholeNo;

            int bitFromDiv;
            if (bitNo == 1)
            {
                int div = count / wholeNo;
                bitFromDiv = (div * (bitNo % 2)) % 2;
            }
            else
                bitFromDiv = 0;

            for (int i = 0; i < rem; i++)
            {
                var s = ((end - i) >> (bitNo - 1)) & 1;
                bitFromDiv ^= s;
            }

            return bitFromDiv % 2;
        }

        public static int solution(int[] A)
        {
            var hashSet = new HashSet<int>(A);

            for (int i = 1; i < int.MaxValue; i++)
            {
                if (!hashSet.Contains(i))
                {
                    return i;
                }
            }

            throw new Exception();
        }

    }
}
