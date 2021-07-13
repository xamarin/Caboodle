using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = GetAppControlData(latitude, longitude);

            return Launch(appControl);
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = GetAppControlData(placemark);

            return Launch(appControl);
        }

        internal static Task<bool> PlatformTryOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = GetAppControlData(latitude, longitude);

            return TryLaunch(appControl);
        }

        internal static Task<bool> PlatformTryOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = GetAppControlData(placemark);

            return TryLaunch(appControl);
        }

        internal static AppControl GetAppControlData(double latitude, double longitude)
        {
            var appControl = new AppControl
            {
                Operation = AppControlOperations.View,
                Uri = "geo:",
            };

            appControl.Uri += $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";
            return appControl;
        }

        internal static AppControl GetAppControlData(Placemark placemark)
        {
            var appControl = new AppControl
            {
                Operation = AppControlOperations.Pick,
                Uri = "geo:",
            };

            appControl.Uri += $"0,0?q={placemark.GetEscapedAddress()}";
            return appControl;
        }

        internal static Task Launch(AppControl appControl)
        {
            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }

        internal static Task<bool> TryLaunch(AppControl appControl)
        {
            var canLaunch = AppControl.GetMatchedApplicationIds(appControl).Any();

            if (canLaunch)
            {
                Launch(appControl);
            }

            return Task.FromResult(canLaunch);
        }
    }
}
