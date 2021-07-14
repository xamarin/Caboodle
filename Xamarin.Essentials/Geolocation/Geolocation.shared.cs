using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        public static Task<Location> GetLastKnownLocationAsync() =>
            PlatformLastKnownLocationAsync();

        public static Task<Location> GetLocationAsync() =>
            PlatformLocationAsync(new GeolocationRequest(), default);

        public static Task<Location> GetLocationAsync(GeolocationRequest request) =>
            PlatformLocationAsync(request ?? new GeolocationRequest(), default);

        public static Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken) =>
            PlatformLocationAsync(request ?? new GeolocationRequest(), cancelToken);

        public static bool IsListening => PlatformIsListening();

        public static Task<bool> StartListeningForegroundAsync(ListeningRequest request) =>
            PlatformStartListeningForegroundAsync(request);

        public static Task<bool> StopListeningForegroundAsync() =>
            PlatformStopListeningForegroundAsync();

        public static event EventHandler<LocationEventArgs> LocationChanged;

        internal static void OnLocationChanged(Location location) =>
            OnLocationChanged(new LocationEventArgs(location));

        internal static void OnLocationChanged(LocationEventArgs e) =>
            LocationChanged?.Invoke(null, e);
    }

    public class LocationEventArgs : EventArgs
    {
        public Location Location { get; }

        public LocationEventArgs(Location location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            Location = location;
        }
    }
}
