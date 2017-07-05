using System;
using System.Threading;

namespace CasualConsole
{
    public static class MyThread
    {
        public static MyThread<T> DoInThread<T>(Func<T> action, bool isBackground)
        {
            return new MyThread<T>(action, isBackground);
        }
    }

    public class MyThread<T>
    {
        private T result;
        private bool isRunning = true;

        public bool IsRunning { get { return isRunning; } }

        public MyThread(Func<T> action, bool isBackground)
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
