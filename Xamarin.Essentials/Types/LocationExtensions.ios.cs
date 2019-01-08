﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class LocationExtensions
    {
        internal static Location ToLocation(this CLPlacemark placemark) =>
            new Location
            {
                Latitude = placemark.Location.Coordinate.Latitude,
                Longitude = placemark.Location.Coordinate.Longitude,
                Altitude = placemark.Location.Altitude,
                Timestamp = DateTimeOffset.UtcNow
            };

        internal static IEnumerable<Location> ToLocations(this IEnumerable<CLPlacemark> placemarks) =>
            placemarks?.Select(a => a.ToLocation());

        internal static Location ToLocation(this CLLocation location) =>
            new Location
            {
                Latitude = location.Coordinate.Latitude,
                Longitude = location.Coordinate.Longitude,
                Altitude = location.VerticalAccuracy < 0 ? default(double?) : location.Altitude,
                Accuracy = location.HorizontalAccuracy,
                Timestamp = location.Timestamp.ToDateTime(),
                Course = location.Course < 0 ? default(double?) : location.Course,
                Speed = location.Speed < 0 ? default(double?) : location.Speed,
                IsFromMockProvider = DeviceInfo.DeviceType == DeviceType.Virtual
            };

        internal static DateTimeOffset ToDateTime(this NSDate timestamp)
        {
            try
            {
                return new DateTimeOffset((DateTime)timestamp);
            }
            catch
            {
                return DateTimeOffset.UtcNow;
            }
        }
    }
}
