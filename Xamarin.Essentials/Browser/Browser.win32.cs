using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            var p = Process.Start(uri.ToString());
            return Task.FromResult(p != null);
        }
    }
}
