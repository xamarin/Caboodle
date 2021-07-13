using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = GetMapsUri(latitude, longitude, options);

            return OpenUri(uri);
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            var uri = GetMapsUri(placemark, options);

            return OpenUri(uri);
        }

        internal static Task<bool> PlatformTryOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = GetMapsUri(latitude, longitude, options);

            return TryOpenUri(uri);
        }

        internal static Task<bool> PlatformTryOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            var uri = GetMapsUri(placemark, options);

            return TryOpenUri(uri);
        }

        internal static string GetMapsUri(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = string.Empty;
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lng = longitude.ToString(CultureInfo.InvariantCulture);

            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"geo:{lat},{lng}?q={lat},{lng}";

                if (!string.IsNullOrWhiteSpace(options.Name))
                    uri += $"({AndroidUri.Encode(options.Name)})";
            }
            else
            {
                uri = $"google.navigation:q={lat},{lng}{GetMode(options.NavigationMode)}";
            }

            return uri;
        }

        internal static string GetMapsUri(Placemark placemark, MapLaunchOptions options)
        {
            var uri = string.Empty;
            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"geo:0,0?q={placemark.GetEscapedAddress()}";
                if (!string.IsNullOrWhiteSpace(options.Name))
                    uri += $"({AndroidUri.Encode(options.Name)})";
            }
            else
            {
                uri = $"google.navigation:q={placemark.GetEscapedAddress()}{GetMode(options.NavigationMode)}";
            }

            return uri;
        }

        internal static string GetMode(NavigationMode mode)
        {
            switch (mode)
            {
                case NavigationMode.Bicycling: return "&mode=b";
                case NavigationMode.Driving: return "&mode=d";
                case NavigationMode.Walking: return "&mode=w";
            }
            return string.Empty;
        }

        internal static Task OpenUri(string uri)
        {
            var intent = ResolveMapIntent(uri);

            Platform.AppContext.StartActivity(intent);

            return Task.CompletedTask;
        }

        internal static Task<bool> TryOpenUri(string uri)
        {
            var intent = ResolveMapIntent(uri);

            var canStart = CanStartIntent(intent);

            if (canStart)
                Platform.AppContext.StartActivity(intent);

            return Task.FromResult(canStart);
        }

        internal static bool CanStartIntent(Intent intent)
        {
            if (Platform.AppContext == null)
                return false;

            var manager = Platform.AppContext.PackageManager;
            var supportedResolvedInfos = manager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);

            return supportedResolvedInfos.Any();
        }

        static Intent ResolveMapIntent(string uri)
        {
            var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri));
            var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
#if __ANDROID_24__
            if (Platform.HasApiLevelN)
                flags |= ActivityFlags.LaunchAdjacent;
#endif

            intent.SetFlags(flags);

            return intent;
        }
    }
}
