using System;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
        static IMainThread implementation;

        static MainThread()
        {
            foreach (var assemblyName in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            {
                if (assemblyName.Name == "PresentationFramework")
                {
                    if (System.Windows.Application.Current != null)
                    {
                        implementation = new WpfMainThread();
                        break;
                    }
                }
                else if (assemblyName.Name == "System.Windows.Forms")
                {
                    implementation = new WinFormsMainThread();
                }
            }

            if (implementation == null)
            {
                // need an alternative fallback as at this point we will be running on neither WPF or WinForms
                implementation = new WinFormsMainThread();
            }
        }

        static void PlatformBeginInvokeOnMainThread(Action action) => implementation.BeginInvokeOnMainThread(action);

        static bool PlatformIsMainThread => implementation.IsMainThread;
    }
}
