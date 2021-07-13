using System;
using System.Threading;
using System.Windows.Threading;

namespace Xamarin.Essentials
{
    internal interface IMainThread
    {
        void BeginInvokeOnMainThread(Action action);

        bool IsMainThread { get; }
    }
}
