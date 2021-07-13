using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.System;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = GetMapsUri(latitude, longitude, options);

            return LaunchUri(uri);
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            var uri = GetMapsUri(placemark, options);

            return LaunchUri(uri);
        }

        internal static async Task<bool> PlatformTryOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = GetMapsUri(latitude, longitude, options);

            return await TryLaunchUri(uri);
        }

        internal static async Task<bool> PlatformTryOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            var uri = GetMapsUri(placemark, options);

            return await TryLaunchUri(uri);
        }

        internal static Uri GetMapsUri(double latitude, double longitude, MapLaunchOptions options)
        {
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lng = longitude.ToString(CultureInfo.InvariantCulture);
            var name = options.Name ?? string.Empty;
            var uri = string.Empty;

            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"bingmaps:?collection=point.{lat}_{lng}_{name}";
            }
            else
            {
                uri = $"bingmaps:?rtp=~pos.{lat}_{lng}_{name}{GetMode(options.NavigationMode)}";
            }

            return new Uri(uri);
        }

        internal static Uri GetMapsUri(Placemark placemark, MapLaunchOptions options)
        {
            var uri = string.Empty;

            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"bingmaps:?where={placemark.GetEscapedAddress()}";
            }
            else
            {
                uri = $"bingmaps:?rtp=~adr.{placemark.GetEscapedAddress()}{GetMode(options.NavigationMode)}";
            }

            return new Uri(uri);
        }

        internal static string GetMode(NavigationMode mode)
        {
            switch (mode)
            {
                case NavigationMode.Driving: return "&mode=d";
                case NavigationMode.Transit: return "&mode=t";
                case NavigationMode.Walking: return "&mode=w";
            }
            return string.Empty;
        }

        internal static async Task<bool> TryLaunchUri(Uri uri)
        {
            var canLaunch = await CanLaunchUri(uri);

            if (canLaunch)
            {
                await LaunchUri(uri);
            }

            return canLaunch;
        }

        static Task LaunchUri(Uri mapsUri) =>
           Windows.System.Launcher.LaunchUriAsync(mapsUri).AsTask();

        static async Task<bool> CanLaunchUri(Uri uri)
        {
            var supported = await Windows.System.Launcher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri);
            return supported == LaunchQuerySupportStatus.Available;
        }
    }
}
