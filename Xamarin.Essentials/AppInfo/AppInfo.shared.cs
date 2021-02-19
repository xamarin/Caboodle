using System;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        public static string PackageName => PlatformGetPackageName();

        public static string Name => PlatformGetName();

        public static string VersionString => PlatformGetVersionString();

        public static Version Version => Utils.ParseVersion(VersionString);

        public static string BuildString => PlatformGetBuild();

        public static void ShowSettingsUI() => PlatformShowSettingsUI();

        public static AppTheme RequestedTheme => PlatformRequestedTheme();

        public static Brightness CurrentBrightness => PlatformGetBrightness();

        public static bool IsBrightnessOverrideActive => PlatformIsBrightnessOverrideActive();

        public static BrightnessOverride SetBrightness(Brightness brightness) => PlatformSetBrightness(brightness);

        public static bool IsBrightnessSupported => PlatformIsBrightnessSupported();
    }
}
