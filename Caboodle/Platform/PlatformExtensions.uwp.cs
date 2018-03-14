﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Microsoft.Caboodle
{
    internal static partial class PlatformExtensions
    {
        internal static void WatchForError(this IAsyncAction self) =>
            self.AsTask().WatchForError();

        internal static void WatchForError<T>(this IAsyncOperation<T> self) =>
            self.AsTask().WatchForError();

        internal static void WatchForError(this Task self)
        {
            var context = SynchronizationContext.Current;
            if (context == null)
                return;

            self.ContinueWith(
                t =>
            {
                var exception = t.Exception.InnerExceptions.Count > 1 ? t.Exception : t.Exception.InnerException;

                context.Post(e => { throw (Exception)e; }, exception);
            }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }
    }
}
