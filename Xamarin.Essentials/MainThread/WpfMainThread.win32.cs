using System;
using System.Threading;
using System.Windows.Threading;

namespace Xamarin.Essentials
{
    sealed class WpfMainThread : IMainThread
    {
        public bool IsMainThread => Dispatcher.FromThread(Thread.CurrentThread) != null;

        public void BeginInvokeOnMainThread(Action action) => Dispatcher.CurrentDispatcher.BeginInvoke(action);
    }
}
