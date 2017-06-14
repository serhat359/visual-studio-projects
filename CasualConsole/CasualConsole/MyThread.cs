using System;
using System.Threading;

namespace CasualConsole
{
    public class MyThread
    {
        public static MyThread<T> DoInThread<T>(Func<T> action)
        {
            return new MyThread<T>(action);
        }
    }

    public class MyThread<T>
    {
        private ThreadStart threadAction;
        private T result;
        private bool isRunning = true;

        public MyThread(Func<T> action)
        {
            threadAction = () =>
            {
                result = action();
                isRunning = false;
            };

            Thread thread = new Thread(threadAction);
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
