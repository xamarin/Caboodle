﻿using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string PlatformGetPackageName() => GetBundleValue("CFBundleIdentifier");

        static string PlatformGetName() => GetBundleValue("CFBundleDisplayName") ?? GetBundleValue("CFBundleName");

        static string PlatformGetVersionString() => GetBundleValue("CFBundleShortVersionString");

        static string PlatformGetBuild() => GetBundleValue("CFBundleVersion");

        static string GetBundleValue(string key)
           => NSBundle.MainBundle.ObjectForInfoDictionary(key)?.ToString();

#if __IOS__ || __TVOS__
        static void PlatformShowSettingsUI() =>
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
#else
        static void PlatformShowSettingsUI() =>
            throw new FeatureNotSupportedException();
#endif

#if __IOS__ || __TVOS__
        static WindowSize PlatformWindowAppSize()
        {
            var currentView = Platform.GetCurrentViewController().View.Window?.Frame;

            if (currentView == null)
                return default;

            return new WindowSize(currentView.Value.Width, currentView.Value.Height);
        }
#else
        static WindowSize PlatformWindowAppSize() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
#endif
    }
}
