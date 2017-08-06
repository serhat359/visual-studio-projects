using System;
using System.Collections.Generic;
using System.Threading;

namespace CasualConsole
{
    public static class MyThread
    {
        public static MyThread<T> DoInThread<T>(bool isBackground, Func<T> action)
        {
            return new MyThread<T>(isBackground, action);
        }

        public static MyThread<T> DoInThread<T>(Func<T> action)
        {
            return DoInThread(true, action);
        }

        private static void ParallelForeach<T>(IEnumerable<T> elems, Action<T> action)
        {
            List<MyThread<int>> threadList = new List<MyThread<int>>();

            foreach (T elem in elems)
            {
                Func<int> actionToFunc = () =>
                {
                    action(elem);

                    return 0;
                };

                var thread = MyThread.DoInThread(true, actionToFunc);

                threadList.Add(thread);
            }

            foreach (var thread in threadList)
            {
                thread.Await();
            }

            return;
        }
    }

    public class MyThread<T>
    {
        private T result;
        private bool isRunning = true;

        public bool IsRunning { get { return isRunning; } }

        public MyThread(bool isBackground, Func<T> action)
        {
            ThreadStart threadAction = () =>
            {
                result = action();
                isRunning = false;
            };

            Thread thread = new Thread(threadAction);
            thread.IsBackground = isBackground; // If this is true, the thread will terminate halfway when the main thread terminates.
            thread.Start();
        }

        public T Await()
        {
            while (isRunning)
            {
                Thread.Sleep(100);
            }

            return result;
        }
    }
}
