using System;
using System.Threading.Tasks;

namespace Manandre.Threading
{
    internal static class AsyncExtensions
    {
        public static IAsyncResult AsApm<T>(this Task<T> task,
                                            AsyncCallback callback,
                                            object state)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            var tcs = new TaskCompletionSource<T>(state);
            task.ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                    tcs.TrySetException(t.Exception.InnerExceptions);
                                else if (t.IsCanceled)
                                    tcs.TrySetCanceled();
                                else
                                    tcs.TrySetResult(t.Result);

                                if (callback != null)
                                    callback(tcs.Task);
                            }, TaskScheduler.Default);
            return tcs.Task;
        }

        public static IAsyncResult AsApm(this Task task,
                                            AsyncCallback callback,
                                            object state)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            var tcs = new TaskCompletionSource<object>(state);
            task.ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                    tcs.TrySetException(t.Exception.InnerExceptions);
                                else if (t.IsCanceled)
                                    tcs.TrySetCanceled();
                                else
                                    tcs.TrySetResult(null);

                                if (callback != null)
                                    callback(tcs.Task);
                            }, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}