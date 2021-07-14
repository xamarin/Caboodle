﻿using CoreLocation;

namespace Xamarin.Essentials
{
    public partial class ListeningRequest
    {
        internal double PlatformDesiredAccuracy
        {
            get
            {
                switch (DesiredAccuracy)
                {
                    case GeolocationAccuracy.Lowest:
                        return CLLocation.AccuracyThreeKilometers;
                    case GeolocationAccuracy.Low:
                        return CLLocation.AccuracyKilometer;
                    case GeolocationAccuracy.Default:
                    case GeolocationAccuracy.Medium:
                        return CLLocation.AccuracyHundredMeters;
                    case GeolocationAccuracy.High:
                        return CLLocation.AccuracyNearestTenMeters;
                    case GeolocationAccuracy.Best:
                        return CLLocation.AccurracyBestForNavigation;
                    default:
                        return CLLocation.AccuracyHundredMeters;
                }
            }
        }
    }
}
