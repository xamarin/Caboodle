using System.Globalization;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using UwpBrightness = Windows.Graphics.Display.BrightnessOverride;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string PlatformGetPackageName() => Package.Current.Id.Name;

        static string PlatformGetName() => Package.Current.DisplayName;

        static string PlatformGetVersionString()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        static BrightnessOverride PlatformSetBrightness(Brightness brightness)
        {
            var @override = UwpBrightness.GetForCurrentView();
            var oldLevel = @override.BrightnessLevel;
            @override.SetBrightnessLevel(brightness.Value, DisplayBrightnessOverrideOptions.None);
            @override.StartOverride();
            return new BrightnessOverride(new Brightness(oldLevel), brightness, @override);
        }

        static bool PlatformIsBrightnessSupported() => UwpBrightness.GetForCurrentView().IsSupported;

        static Brightness PlatformGetBrightness() => new Brightness(UwpBrightness.GetForCurrentView().BrightnessLevel);

        static bool PlatformIsBrightnessOverrideActive() => UwpBrightness.GetForCurrentView().IsOverrideActive;

        static string PlatformGetBuild() =>
            Package.Current.Id.Version.Build.ToString(CultureInfo.InvariantCulture);

        static void PlatformShowSettingsUI() =>
            Windows.System.Launcher.LaunchUriAsync(new System.Uri("ms-settings:appsfeatures-app")).WatchForError();

        static AppTheme PlatformRequestedTheme() =>
            Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
    }
}
