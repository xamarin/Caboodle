using System.Device.Location;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            var geolocator = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            geolocator.Start();

            while (geolocator.Status != GeoPositionStatus.Ready)
            {
                await Task.Delay(100);
            }

            var position = geolocator.Position;
            geolocator.Stop();

            return position?.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            var geolocator = new GeoCoordinateWatcher(request.PlatformDesiredAccuracy);
            geolocator.Start();

            CheckStatus(geolocator.Status);

            while (geolocator.Status != GeoPositionStatus.Ready)
            {
                await Task.Delay(100);
            }

            var position = geolocator.Position;
            geolocator.Stop();

            return position?.ToLocation();

            void CheckStatus(GeoPositionStatus status)
            {
                switch (status)
                {
                    case GeoPositionStatus.Disabled:
                        throw new FeatureNotEnabledException("Location services are not enabled on device.");
                }
            }
        }
    }
}
