using System;
using System.Globalization;
using Tizen.Applications;
using Tizen.System;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string PlatformGetPackageName()
            => Application.Current.ApplicationInfo.PackageId;

        static string PlatformGetName()
            => Application.Current.ApplicationInfo.Label;

        static string PlatformGetVersionString()
            => Platform.CurrentPackage.Version;

        static string PlatformGetBuild()
            => Version.Build.ToString(CultureInfo.InvariantCulture);

        static void PlatformShowSettingsUI()
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();
            AppControl.SendLaunchRequest(new AppControl() { Operation = AppControlOperations.Setting });
        }

        static BrightnessOverride PlatformSetBrightness(Brightness brightness)
        {
            var display = Display.Displays[0];
            var oldValue = NormalizeBrightness(display);
            display.Brightness = Math.Min((int)brightness.Value * display.MaxBrightness, display.MaxBrightness);
            return new BrightnessOverride(new Brightness(oldValue), brightness);
        }

        static double NormalizeBrightness(Display display)
        {
            return (1d / display.MaxBrightness) * display.Brightness;
        }

        static bool PlatformIsBrightnessSupported() => true;

        static Brightness PlatformGetBrightness() => new Brightness(Tizen.System.Display.Displays[0].Brightness);

        static bool PlatformIsBrightnessOverrideActive() => false;

        static AppTheme PlatformRequestedTheme()
        {
            return AppTheme.Unspecified;
        }
    }
}
