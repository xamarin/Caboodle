using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Locations;
using Android.OS;
using Android.Runtime;

using AndroidLocation = Android.Locations.Location;
using LocationPower = Android.Locations.Power;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        const long twoMinutes = 120000;
        static readonly string[] ignoredProviders = new string[] { LocationManager.PassiveProvider, "local_database" };

        static ContinuousLocationListener continuousListener;
        static List<string> listeningProviders;

        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            var lm = Platform.LocationManager;
            AndroidLocation bestLocation = null;

            foreach (var provider in lm.GetProviders(true))
            {
                var location = lm.GetLastKnownLocation(provider);

                if (location != null && IsBetterLocation(location, bestLocation))
                    bestLocation = location;
            }

            return bestLocation?.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            var locationManager = Platform.LocationManager;

            var enabledProviders = locationManager.GetProviders(true);
            var hasProviders = enabledProviders.Any(p => !ignoredProviders.Contains(p));

            if (!hasProviders)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            // get the best possible provider for the requested accuracy
            var providerInfo = GetBestProvider(locationManager, request.DesiredAccuracy);

            // if no providers exist, we can't get a location
            // let's punt and try to get the last known location
            if (string.IsNullOrEmpty(providerInfo.Provider))
                return await GetLastKnownLocationAsync();

            var tcs = new TaskCompletionSource<AndroidLocation>();

            var allProviders = locationManager.GetProviders(false);

            var providers = new List<string>();
            if (allProviders.Contains(LocationManager.GpsProvider))
                providers.Add(LocationManager.GpsProvider);
            if (allProviders.Contains(LocationManager.NetworkProvider))
                providers.Add(LocationManager.NetworkProvider);

            if (providers.Count == 0)
                providers.Add(providerInfo.Provider);

            var listener = new SingleLocationListener(locationManager, providerInfo.Accuracy, providers);
            listener.LocationHandler = HandleLocation;

            cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
            cancellationToken.Register(Cancel);

            // start getting location updates
            // make sure to use a thread with a looper
            var looper = Looper.MyLooper() ?? Looper.MainLooper;

            foreach (var provider in providers)
                locationManager.RequestLocationUpdates(provider, 0, 0, listener, looper);

            var androidLocation = await tcs.Task;

            if (androidLocation == null)
                return null;

            return androidLocation.ToLocation();

            void HandleLocation(AndroidLocation location)
            {
                RemoveUpdates();
                tcs.TrySetResult(location);
            }

            void Cancel()
            {
                RemoveUpdates();
                tcs.TrySetResult(listener.BestLocation);
            }

            void RemoveUpdates()
            {
                for (var i = 0; i < providers.Count; i++)
                    locationManager.RemoveUpdates(listener);
            }
        }

        static (string Provider, float Accuracy) GetBestProvider(LocationManager locationManager, GeolocationAccuracy accuracy)
        {
            // Criteria: https://developer.android.com/reference/android/location/Criteria

            var criteria = new Criteria
            {
                BearingRequired = false,
                AltitudeRequired = false,
                SpeedRequired = false
            };

            var accuracyDistance = 100;

            switch (accuracy)
            {
                case GeolocationAccuracy.Lowest:
                    criteria.Accuracy = Accuracy.NoRequirement;
                    criteria.HorizontalAccuracy = Accuracy.NoRequirement;
                    criteria.PowerRequirement = LocationPower.NoRequirement;
                    accuracyDistance = 500;
                    break;
                case GeolocationAccuracy.Low:
                    criteria.Accuracy = Accuracy.Coarse;
                    criteria.HorizontalAccuracy = Accuracy.Low;
                    criteria.PowerRequirement = LocationPower.Low;
                    accuracyDistance = 500;
                    break;
                case GeolocationAccuracy.Default:
                case GeolocationAccuracy.Medium:
                    criteria.Accuracy = Accuracy.Coarse;
                    criteria.HorizontalAccuracy = Accuracy.Medium;
                    criteria.PowerRequirement = LocationPower.Medium;
                    accuracyDistance = 250;
                    break;
                case GeolocationAccuracy.High:
                    criteria.Accuracy = Accuracy.Fine;
                    criteria.HorizontalAccuracy = Accuracy.High;
                    criteria.PowerRequirement = LocationPower.High;
                    accuracyDistance = 100;
                    break;
                case GeolocationAccuracy.Best:
                    criteria.Accuracy = Accuracy.Fine;
                    criteria.HorizontalAccuracy = Accuracy.High;
                    criteria.PowerRequirement = LocationPower.High;
                    accuracyDistance = 50;
                    break;
            }

            var provider = locationManager.GetBestProvider(criteria, true) ?? locationManager.GetProviders(true).FirstOrDefault();

            return (provider, accuracyDistance);
        }

        internal static bool IsBetterLocation(AndroidLocation location, AndroidLocation bestLocation)
        {
            if (bestLocation == null)
                return true;

            var timeDelta = location.Time - bestLocation.Time;

            var isSignificantlyNewer = timeDelta > twoMinutes;
            var isSignificantlyOlder = timeDelta < -twoMinutes;
            var isNewer = timeDelta > 0;

            if (isSignificantlyNewer)
                return true;

            if (isSignificantlyOlder)
                return false;

            var accuracyDelta = (int)(location.Accuracy - bestLocation.Accuracy);
            var isLessAccurate = accuracyDelta > 0;
            var isMoreAccurate = accuracyDelta < 0;
            var isSignificantlyLessAccurage = accuracyDelta > 200;

            var isFromSameProvider = location?.Provider?.Equals(bestLocation?.Provider, StringComparison.OrdinalIgnoreCase) ?? false;

            if (isMoreAccurate)
                return true;

            if (isNewer && !isLessAccurate)
                return true;

            if (isNewer && !isSignificantlyLessAccurage && isFromSameProvider)
                return true;

            return false;
        }

        static bool PlatformIsListening() => continuousListener != null;

        static async Task<bool> PlatformStartListeningForegroundAsync(ListeningRequest request)
        {
            if (IsListening)
                throw new InvalidOperationException("This Geolocation is already listening");

            await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

            var locationManager = Platform.LocationManager;

            var enabledProviders = locationManager.GetProviders(true);
            var hasProviders = enabledProviders.Any(p => !ignoredProviders.Contains(p));

            if (!hasProviders)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            // get the best possible provider for the requested accuracy
            var providerInfo = GetBestProvider(locationManager, request.DesiredAccuracy);

            // if no providers exist, we can't listen for locations
            if (string.IsNullOrEmpty(providerInfo.Provider))
                return false;

            var allProviders = locationManager.GetProviders(false);

            listeningProviders = new List<string>();
            if (allProviders.Contains(LocationManager.GpsProvider))
                listeningProviders.Add(LocationManager.GpsProvider);
            if (allProviders.Contains(LocationManager.NetworkProvider))
                listeningProviders.Add(LocationManager.NetworkProvider);

            if (listeningProviders.Count == 0)
                listeningProviders.Add(providerInfo.Provider);

            continuousListener = new ContinuousLocationListener(locationManager, request.MinimumTime, listeningProviders);
            continuousListener.LocationHandler = HandleLocation;

            // start getting location updates
            // make sure to use a thread with a looper
            var looper = Looper.MyLooper() ?? Looper.MainLooper;

            var minTimeMilliseconds = (long)request.MinimumTime.TotalMilliseconds;

            foreach (var provider in listeningProviders)
                locationManager.RequestLocationUpdates(provider, minTimeMilliseconds, providerInfo.Accuracy, continuousListener, looper);

            return true;

            void HandleLocation(AndroidLocation androidLocation)
            {
                OnLocationChanged(androidLocation.ToLocation());
            }
        }

        static Task<bool> PlatformStopListeningForegroundAsync()
        {
            if (continuousListener == null)
                return Task.FromResult(true);

            if (listeningProviders == null)
                return Task.FromResult(true);

            var providers = listeningProviders;
            continuousListener.LocationHandler = null;

            var locationManager = Platform.LocationManager;

            for (var i = 0; i < providers.Count; i++)
            {
                try
                {
                    locationManager.RemoveUpdates(continuousListener);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to remove updates: " + ex);
                }
            }

            continuousListener = null;

            return Task.FromResult(true);
        }
    }

    class SingleLocationListener : Java.Lang.Object, ILocationListener
    {
        readonly object locationSync = new object();

        float desiredAccuracy;

        internal AndroidLocation BestLocation { get; set; }

        HashSet<string> activeProviders = new HashSet<string>();

        bool wasRaised = false;

        internal Action<AndroidLocation> LocationHandler { get; set; }

        internal SingleLocationListener(LocationManager manager, float desiredAccuracy, IEnumerable<string> activeProviders)
        {
            this.desiredAccuracy = desiredAccuracy;

            this.activeProviders = new HashSet<string>(activeProviders);

            foreach (var provider in activeProviders)
            {
                var location = manager.GetLastKnownLocation(provider);
                if (location != null && Geolocation.IsBetterLocation(location, BestLocation))
                    BestLocation = location;
            }
        }

        void ILocationListener.OnLocationChanged(AndroidLocation location)
        {
            if (location.Accuracy <= desiredAccuracy)
            {
                if (wasRaised)
                    return;

                wasRaised = true;

                LocationHandler?.Invoke(location);
                return;
            }

            lock (locationSync)
            {
                if (Geolocation.IsBetterLocation(location, BestLocation))
                    BestLocation = location;
            }
        }

        void ILocationListener.OnProviderDisabled(string provider)
        {
            lock (activeProviders)
                activeProviders.Remove(provider);
        }

        void ILocationListener.OnProviderEnabled(string provider)
        {
            lock (activeProviders)
                activeProviders.Add(provider);
        }

        void ILocationListener.OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            switch (status)
            {
                case Availability.Available:
                    ((ILocationListener)this).OnProviderEnabled(provider);
                    break;

                case Availability.OutOfService:
                    ((ILocationListener)this).OnProviderDisabled(provider);
                    break;
            }
        }
    }

    class ContinuousLocationListener : Java.Lang.Object, ILocationListener
    {
        readonly HashSet<string> activeProviders = new HashSet<string>();
        readonly LocationManager manager;
        IList<string> providers;

        string activeProvider;
        AndroidLocation lastLocation;
        TimeSpan timePeriod;

        public ContinuousLocationListener(LocationManager manager, TimeSpan timePeriod, IList<string> providers)
        {
            this.manager = manager;
            this.timePeriod = timePeriod;
            this.providers = providers;

            foreach (var p in providers)
            {
                if (manager.IsProviderEnabled(p))
                    activeProviders.Add(p);
            }
        }

        internal Action<AndroidLocation> LocationHandler { get; set; }

        void ILocationListener.OnLocationChanged(AndroidLocation location)
        {
            if (location.Provider != activeProvider)
            {
                if (activeProvider != null && manager.IsProviderEnabled(activeProvider))
                {
                    var pr = manager.GetProvider(location.Provider);
                    var lapsed = GetTimeSpan(location.Time) - GetTimeSpan(lastLocation.Time);

                    if (pr.Accuracy > manager.GetProvider(activeProvider).Accuracy
                      && lapsed < timePeriod.Add(timePeriod))
                    {
                        location.Dispose();
                        return;
                    }
                }

                activeProvider = location.Provider;
            }

            var previous = Interlocked.Exchange(ref lastLocation, location);
            if (previous != null)
                previous.Dispose();

            LocationHandler?.Invoke(location);
        }

        public void OnProviderDisabled(string provider)
        {
            lock (activeProviders)
                activeProviders.Remove(provider);
        }

        public void OnProviderEnabled(string provider)
        {
            if (provider == LocationManager.PassiveProvider)
                return;

            lock (activeProviders)
                activeProviders.Add(provider);
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            switch (status)
            {
                case Availability.Available:
                    OnProviderEnabled(provider);
                    break;

                case Availability.OutOfService:
                    OnProviderDisabled(provider);
                    break;
            }
        }

        TimeSpan GetTimeSpan(long time)
        {
            return new TimeSpan(TimeSpan.TicksPerMillisecond * time);
        }
    }
}
