using System;
using System.Threading;

namespace BackupHomeFolder
{
    public class MyThread
    {
        public static MyTask<T> DoInThread<T>(Func<T> action)
        {
            return new MyTask<T>(action);
        }
    }

    public class MyTask<T>
    {
        ThreadStart threadAction;
        T result;
        bool isRunning = true;

        public MyTask(Func<T> action)
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
