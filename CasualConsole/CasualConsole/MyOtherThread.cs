using System;
using System.Threading;

namespace CasualConsole
{
    public static class MyOtherThread
    {
        public static MyOtherThread<T, A> DoInThread<T, A>(bool isBackground, Func<A, T> action, A val)
        {
            return new MyOtherThread<T, A>(isBackground, action, val);
        }
    }

    public class MyOtherThread<T, A>
    {
        private T result;
        private bool isRunning = true;

        public bool IsRunning { get { return isRunning; } }

        public MyOtherThread(bool isBackground, Func<A, T> action, A val)
        {
            ThreadStart threadAction = () =>
            {
                result = action(val);
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
