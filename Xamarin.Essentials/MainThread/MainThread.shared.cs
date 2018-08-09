﻿using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
        public static bool IsMainThread =>
            PlatformIsMainThread;

        public static void BeginInvokeOnMainThread(Action action) =>
            PlatformBeginInvokeOnMainThread(action);

        internal static Task InvokeOnMainThread(Action action)
        {
            if (IsMainThread)
            {
                action();
                return Task.FromResult(true); // Can not use CompletedTask as .net Standard 1.0 does not support it
            }

            var tcs = new TaskCompletionSource<bool>();

            BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        internal static Task<T> InvokeOnMainThread<T>(Func<T> action)
        {
            if (IsMainThread)
            {
                return Task.FromResult(action());
            }

            var tcs = new TaskCompletionSource<T>();

            BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var result = action();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
