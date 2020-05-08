using System;
using System.Threading;

namespace Business
{
    public static class MyThread
    {
        public static MyThread<T> DoInThread<T>(bool isBackground, Func<T> action)
        {
            return new MyThread<T>(isBackground, action);
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
