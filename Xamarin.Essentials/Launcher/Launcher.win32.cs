using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task PlatformOpenAsync(Uri uri)
        {
            Process.Start(uri.ToString());
            return Task.CompletedTask;
        }

        static Task PlatformOpenAsync(OpenFileRequest request)
        {
            if (File.Exists(request.File.FullPath))
            {
                Process.Start(request.File.FullPath);
            }

            return Task.CompletedTask;
        }

        static Task<bool> PlatformTryOpenAsync(Uri uri)
        {
            var success = Process.Start(uri.ToString()) is object;
            return Task.FromResult(success);
        }
    }
}
