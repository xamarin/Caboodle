using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            if (!CLLocationManager.LocationServicesEnabled)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            var manager = new CLLocationManager();
            var location = manager.Location;

            return location?.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            if (!CLLocationManager.LocationServicesEnabled)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            // the location manager requires an active run loop
            // so just use the main loop
            var manager = MainThread.InvokeOnMainThread(() => new CLLocationManager());

            var tcs = new TaskCompletionSource<CLLocation>(manager);

            var listener = new SingleLocationListener();
            listener.LocationHandler += HandleLocation;

            cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
            cancellationToken.Register(Cancel);

            manager.DesiredAccuracy = request.PlatformDesiredAccuracy;
            manager.Delegate = listener;

#if __IOS__
            // we're only listening for a single update
            manager.PausesLocationUpdatesAutomatically = false;
#endif

            manager.StartUpdatingLocation();

            var clLocation = await tcs.Task;

            return clLocation?.ToLocation();

            void HandleLocation(CLLocation location)
            {
                manager.StopUpdatingLocation();
                tcs.TrySetResult(location);
            }

            void Cancel()
            {
                manager.StopUpdatingLocation();
                tcs.TrySetResult(null);
            }
        }

        static CLLocationManager listeningManager;

        static bool PlatformIsListening() => listeningManager != null;

        static async Task<bool> PlatformStartListeningForegroundAsync(ListeningRequest request)
        {
            if (PlatformIsListening())
                throw new InvalidOperationException("already listening to location changes");

            if (!CLLocationManager.LocationServicesEnabled)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            // the location manager requires an active run loop
            // so just use the main loop
            listeningManager = MainThread.InvokeOnMainThread(() => new CLLocationManager());

            var listener = new ContinuousLocationListener();
            listener.LocationHandler += HandleLocation;

            listeningManager.DesiredAccuracy = request.PlatformDesiredAccuracy;
            listeningManager.Delegate = listener;

#if __IOS__
            // allow pausing updates
            listeningManager.PausesLocationUpdatesAutomatically = true;
#endif

            listeningManager.StartUpdatingLocation();

            return true;

            static void HandleLocation(CLLocation location)
            {
                OnLocationChanged(location.ToLocation());
            }
        }

        static Task<bool> PlatformStopListeningForegroundAsync()
        {
            if (!PlatformIsListening())
                return Task.FromResult(true);

            listeningManager.StopUpdatingLocation();
            listeningManager.Delegate = null;

            listeningManager = null;

            return Task.FromResult<bool>(true);
        }
    }

    class SingleLocationListener : CLLocationManagerDelegate
    {
        bool wasRaised = false;

        internal Action<CLLocation> LocationHandler { get; set; }

        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
            if (wasRaised)
                return;

            wasRaised = true;

            var location = locations?.LastOrDefault();

            if (location == null)
                return;

            LocationHandler?.Invoke(location);
        }

        public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager) => false;
    }

    class ContinuousLocationListener : CLLocationManagerDelegate
    {
        internal Action<CLLocation> LocationHandler { get; set; }

        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
            var location = locations?.LastOrDefault();

            if (location == null)
                return;

            LocationHandler?.Invoke(location);
        }

        public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager) => false;
    }
}
