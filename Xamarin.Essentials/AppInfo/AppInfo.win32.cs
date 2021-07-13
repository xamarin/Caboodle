using System;
using System.Diagnostics;
using System.Reflection;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static readonly Assembly launchingAssembly = Assembly.GetEntryAssembly();

        static string PlatformGetPackageName()
        {
            var attr = launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (attr != null)
            {
                return attr.Title;
            }

            return string.Empty;
        }

        static string PlatformGetName()
        {
            var attr = launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (attr != null)
            {
                return attr.Title;
            }

            return string.Empty;
        }

        static string PlatformGetVersionString() =>
            launchingAssembly.GetName().Version.ToString();

        static string PlatformGetBuild()
        {
            return launchingAssembly.GetName().Version.Build.ToString();
        }

        static void PlatformShowSettingsUI()
        {
            Process.Start(new ProcessStartInfo { FileName = "ms-settings:appsfeatures-app", UseShellExecute = true });
        }

        static AppTheme PlatformRequestedTheme()
        {
            var themeVal = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize").GetValue("AppsUseLightTheme");

            if (themeVal != null)
            {
                var val = (int)themeVal;
                switch (val)
                {
                    case 1:
                        return AppTheme.Light;

                    case 0:
                        return AppTheme.Dark;
                }
            }

            return AppTheme.Unspecified;
        }
    }
}
