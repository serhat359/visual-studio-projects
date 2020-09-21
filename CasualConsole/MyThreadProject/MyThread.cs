using System;
using System.Collections.Generic;
using System.Threading;

namespace MyThreadProject
{
    public static class MyThread
    {
        public static MyThread<T> DoInThread<T>(bool isBackground, Func<T> action)
        {
            return MyThread<T>.New(isBackground, action);
        }

        public static MyThread<T> DoInThread<E, T>(bool isBackground, Func<E, T> action, E val)
        {
            return MyThread<T>.New(isBackground, action, val);
        }

        public static MyThread<T> DoInThread<T>(Func<T> action)
        {
            return DoInThread(true, action);
        }

        public static MyThread<T> DoInThread<E, T>(Func<E, T> action, E val)
        {
            return DoInThread(true, action, val);
        }

        public static void ParallelForeach<T>(IEnumerable<T> elems, Action<T> action)
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

        // Private Constructor
        private MyThread() { }

        public static MyThread<T> New(bool isBackground, Func<T> action)
        {
            return New(isBackground, x => action(), 0);
        }

        public static MyThread<T> New<E>(bool isBackground, Func<E, T> action, E val)
        {
            var o = new MyThread<T>();

            ThreadStart threadAction = () =>
            {
                o.result = action(val);
                o.isRunning = false;
            };

            Thread thread = new Thread(threadAction);
            thread.IsBackground = isBackground; // If this is true, the thread will terminate halfway when the main thread terminates.
            thread.Start();

            return o;
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
