using System.Device.Location;

namespace Xamarin.Essentials
{
    public partial class GeolocationRequest
    {
        internal GeoPositionAccuracy PlatformDesiredAccuracy
        {
            get
            {
                switch (DesiredAccuracy)
                {
                    case GeolocationAccuracy.High:
                    case GeolocationAccuracy.Best:
                        return GeoPositionAccuracy.High;

                    default:
                        return GeoPositionAccuracy.Default;
                }
            }
        }
    }
}
