using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

namespace Xamarin.Essentials
{
    public static partial class LocationExtensions
    {
        internal static Location ToLocation(this GeoPosition<GeoCoordinate> position) =>
            new Location
            {
                Latitude = position.Location.Latitude,
                Longitude = position.Location.Longitude,
                Altitude = position.Location.Altitude,
                Accuracy = position.Location.HorizontalAccuracy,
                Speed = position.Location.Speed,
                Course = position.Location.Course,
                Timestamp = position.Timestamp
            };
    }
}
