using System;
using System.Threading;
using System.Threading.Tasks;

namespace CasualConsole
{
    public static class TaskExt
    {
        public static Task Run<T>(Action<T> action, T state)
        {
            Action task = () => action(state);
            return Task.Run(task);
        }

        public static Task<E> Run<T, E>(Func<T, E> action, T state)
        {
            Func<E> task = () => action(state);
            return Task.Run(task);
        }

        public static Task Run<T>(Func<T, Task> action, T state)
        {
            Func<Task> task = () => action(state);
            return Task.Run(task);
        }

        public static Task<E> Run<T, E>(Func<T, Task<E>> action, T state)
        {
            Func<Task<E>> task = () => action(state);
            return Task.Run(task);
        }

        public static Task Run<T>(Action<T> action, T state, CancellationToken token)
        {
            Action task = () => action(state);
            return Task.Run(task, token);
        }

        public static Task<E> Run<T, E>(Func<T, E> action, T state, CancellationToken token)
        {
            Func<E> task = () => action(state);
            return Task.Run(task, token);
        }

        public static Task Run<T>(Func<T, Task> action, T state, CancellationToken token)
        {
            Func<Task> task = () => action(state);
            return Task.Run(task, token);
        }

        public static Task<E> Run<T, E>(Func<T, Task<E>> action, T state, CancellationToken token)
        {
            Func<Task<E>> task = () => action(state);
            return Task.Run(task, token);
        }
    }

    public static class WhenDoneExtensions
    {
        #region Task Extensions
        public static Task WhenDone(this Task task, Action otherTask)
        {
            return Task.Run(async () => { await task; otherTask(); });
        }

        public static Task<E> WhenDone<E>(this Task task, Func<E> otherTask)
        {
            return Task.Run(async () => { await task; return otherTask(); });
        }

        public static Task WhenDone(this Task task, Func<Task> otherTask)
        {
            return Task.Run(async () => { await task; await otherTask(); });
        }

        public static Task<E> WhenDone<E>(this Task task, Func<Task<E>> otherTask)
        {
            return Task.Run(async () => { await task; return await otherTask(); });
        }
        #endregion

        #region Task<T> Extensions
        public static Task WhenDone<T>(this Task<T> task, Action<T> otherTask)
        {
            return Task.Run(async () => otherTask(await task));
        }

        public static Task<E> WhenDone<T, E>(this Task<T> task, Func<T, E> otherTask)
        {
            return Task.Run(async () => otherTask(await task));
        }

        public static Task WhenDone<T>(this Task<T> task, Func<T, Task> otherTask)
        {
            return Task.Run(async () => await otherTask(await task));
        }

        public static Task<E> WhenDone<T, E>(this Task<T> task, Func<T, Task<E>> otherTask)
        {
            return Task.Run(async () => await otherTask(await task));
        }
        #endregion
    }
}
