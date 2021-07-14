using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            // no need for permissions as AllowFallbackToConsentlessPositions
            // will allow the device to return a location regardless

            var geolocator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.Default,
            };
            geolocator.AllowFallbackToConsentlessPositions();

            var location = await geolocator.GetGeopositionAsync().AsTask();

            return location?.Coordinate?.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            var geolocator = new Geolocator
            {
                DesiredAccuracyInMeters = request.PlatformDesiredAccuracy
            };

            CheckStatus(geolocator.LocationStatus);

            cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);

            var location = await geolocator.GetGeopositionAsync().AsTask(cancellationToken);

            return location?.Coordinate?.ToLocation();
        }

        static void CheckStatus(PositionStatus status)
        {
            switch (status)
            {
                case PositionStatus.Disabled:
                case PositionStatus.NotAvailable:
                    throw new FeatureNotEnabledException("Location services are not enabled on device.");
            }
        }

        static Geolocator listeningGeolocator;

        static bool PlatformIsListening() => listeningGeolocator != null;

        static async Task<bool> PlatformStartListeningForegroundAsync(ListeningRequest request)
        {
            if (request.MinimumTime.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(request.MinimumTime));

            if (PlatformIsListening())
                throw new InvalidOperationException();

            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            listeningGeolocator = new Geolocator
            {
                DesiredAccuracyInMeters = request.PlatformDesiredAccuracy,
                ReportInterval = (uint)request.MinimumTime.TotalMilliseconds,
                MovementThreshold = request.PlatformDesiredAccuracy,
            };

            CheckStatus(listeningGeolocator.LocationStatus);

            listeningGeolocator.PositionChanged += OnLocatorPositionChanged;
            listeningGeolocator.StatusChanged += OnLocatorStatusChanged;

            return true;
        }

        static Task<bool> PlatformStopListeningForegroundAsync()
        {
            if (!PlatformIsListening())
                return Task.FromResult(true);

            listeningGeolocator.PositionChanged -= OnLocatorPositionChanged;
            listeningGeolocator.StatusChanged -= OnLocatorStatusChanged;

            listeningGeolocator = null;

            return Task.FromResult(true);
        }

        static void OnLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs e) =>
            OnLocationChanged(e.Position.ToLocation());

        static async void OnLocatorStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            if (PlatformIsListening())
            {
                await PlatformStopListeningForegroundAsync();
            }
        }
    }
}
