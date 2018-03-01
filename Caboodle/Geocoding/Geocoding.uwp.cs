﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Microsoft.Caboodle
{
    public partial class Geocoding
    {
        public static async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
        {
            ValidateMapKey();

            var point = new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude });

            var queryResults = await MapLocationFinder.FindLocationsAtAsync(point).AsTask();

            return queryResults?.Locations?.ToPlacemarks();
        }

        public static async Task<IEnumerable<Location>> GetLocationsAsync(string address)
        {
            ValidateMapKey();

            var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);

            return queryResults?.Locations?.ToLocations();
        }

        internal static void ValidateMapKey()
        {
            if (string.IsNullOrWhiteSpace(MapKey) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
            {
                Console.WriteLine("Map API key is required on UWP to reverse geolocate.");
                throw new ArgumentNullException(nameof(MapKey));

            }

            if (!string.IsNullOrWhiteSpace(MapKey))
                MapService.ServiceToken = MapKey;
        }
    }
}
