﻿using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        static void PlatformEnsureDeclared(PermissionType permission)
        {
            // Info.plist declarations were only required in >= iOS 8.0
            if (!Platform.HasOSVersion(8, 0))
                return;

            var info = NSBundle.MainBundle.InfoDictionary;

            if (permission == PermissionType.LocationWhenInUse)
            {
                if (!info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                    throw new PermissionException("On iOS 8.0 and higher you must set either `NSLocationWhenInUseUsageDescription` or `NSLocationAlwaysUsageDescription` in your Info.plist file to enable Authorization Requests for Location updates!");
            }
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            PlatformEnsureDeclared(permission);

            switch (permission)
            {
                case PermissionType.LocationWhenInUse:
                    return Task.FromResult(GetLocationStatus());
            }

            return Task.FromResult(PermissionStatus.Granted);
        }

        static async Task<PermissionStatus> PlatformRequestAsync(PermissionType permission)
        {
            // Check status before requesting first
            if (await PlatformCheckStatusAsync(permission) == PermissionStatus.Granted)
                return PermissionStatus.Granted;

            PlatformEnsureDeclared(permission);

            switch (permission)
            {
                case PermissionType.LocationWhenInUse:
                    return await RequestLocationAsync();
            }

            return PermissionStatus.Granted;
        }

        static PermissionStatus GetLocationStatus()
        {
            if (!CLLocationManager.LocationServicesEnabled)
                return PermissionStatus.Disabled;

            var status = CLLocationManager.Status;

            if (Platform.HasOSVersion(8, 0))
            {
                switch (status)
                {
                    case CLAuthorizationStatus.AuthorizedAlways:
                    case CLAuthorizationStatus.AuthorizedWhenInUse:
                        return PermissionStatus.Granted;
                    case CLAuthorizationStatus.Denied:
                        return PermissionStatus.Denied;
                    case CLAuthorizationStatus.Restricted:
                        return PermissionStatus.Restricted;
                    default:
                        return PermissionStatus.Unknown;
                }
            }

            switch (status)
            {
                case CLAuthorizationStatus.Authorized:
                    return PermissionStatus.Granted;
                case CLAuthorizationStatus.Denied:
                    return PermissionStatus.Denied;
                case CLAuthorizationStatus.Restricted:
                    return PermissionStatus.Restricted;
                default:
                    return PermissionStatus.Unknown;
            }
        }

        static Task<PermissionStatus> RequestLocationAsync()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                return Task.FromResult(PermissionStatus.Unknown);

            var manager = new CLLocationManager();

            var tcs = new TaskCompletionSource<PermissionStatus>(manager);

            manager.AuthorizationChanged += LocationAuthCallback;
            manager.RequestWhenInUseAuthorization();

            return tcs.Task;

            void LocationAuthCallback(object sender, CLAuthorizationChangedEventArgs e)
            {
                if (e.Status == CLAuthorizationStatus.NotDetermined)
                    return;

                manager.AuthorizationChanged -= LocationAuthCallback;
                tcs.TrySetResult(GetLocationStatus());
            }
        }
    }
}
