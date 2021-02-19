namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string PlatformGetPackageName() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static string PlatformGetName() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static string PlatformGetVersionString() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static string PlatformGetBuild() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformShowSettingsUI() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static AppTheme PlatformRequestedTheme() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static BrightnessOverride PlatformSetBrightness(Brightness brightness) => throw ExceptionUtils.NotSupportedOrImplementedException;

        static Brightness PlatformGetBrightness() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static bool PlatformIsBrightnessOverrideActive() => throw ExceptionUtils.NotSupportedOrImplementedException;

        static bool PlatformIsBrightnessSupported() => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
