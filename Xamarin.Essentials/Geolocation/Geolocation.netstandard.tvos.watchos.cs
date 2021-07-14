using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        static Task<Location> PlatformLastKnownLocationAsync() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static bool PlatformIsListening() => false;

        static Task<bool> PlatformStartListeningForegroundAsync(ListeningRequest request) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<bool> PlatformStopListeningForegroundAsync() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
