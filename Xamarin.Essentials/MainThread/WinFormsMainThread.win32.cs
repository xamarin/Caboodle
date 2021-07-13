using System;
using System.Threading;
using System.Windows.Threading;

namespace Xamarin.Essentials
{
    sealed class WinFormsMainThread : IMainThread
    {
        readonly System.Windows.Forms.Control c = new System.Windows.Forms.Control();

        public bool IsMainThread => System.Windows.Forms.Application.MessageLoop;

        public void BeginInvokeOnMainThread(Action action) => c.BeginInvoke(action);
    }
}
