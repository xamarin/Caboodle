using System;
#if GTK
using System.Threading;
#endif

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
#if GTK
#pragma warning disable SA1130
        static void PlatformBeginInvokeOnMainThread(Action action) => Gtk.Application.Invoke(delegate { action(); });

        static int? uiThreadId = null;

        static bool PlatformIsMainThread
        {
            get
            {
                if (uiThreadId == null)
                {
                    Gtk.Application.Invoke(delegate { uiThreadId = Thread.CurrentThread.ManagedThreadId; });
                    return false;
                }
                else
                {
                    return System.Threading.Thread.CurrentThread.ManagedThreadId == uiThreadId;
                }
            }
        }
#pragma warning restore SA1130
#else
        static void PlatformBeginInvokeOnMainThread(Action action) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static bool PlatformIsMainThread =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
#endif
    }
}
