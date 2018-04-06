﻿using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using RuntimeDeviceType = Xamarin.Essentials.DeviceType;
using XUnitFilter = UnitTests.HeadlessRunner.Xunit.XUnitFilter;

namespace DeviceTests
{
    internal static class Traits
    {
        public const string DeviceType = "DeviceType";
        public const string InteractionType = "InteractionType";

        internal static class DeviceTypes
        {
            public const string Physical = "Physical";
            public const string Virtual = "Virtual";

            internal static string ToExclude =>
                DeviceInfo.DeviceType == RuntimeDeviceType.Physical ? Virtual : Physical;
        }

        internal static class InteractionTypes
        {
            public const string Human = "Human";
            public const string Machine = "Machine";

            internal static string ToExclude => Human;
        }

        internal static List<XUnitFilter> GetCommonTraits(params XUnitFilter[] additionalFilters)
        {
            var filters = new List<XUnitFilter>
            {
                new XUnitFilter(DeviceType, DeviceTypes.ToExclude, true),
                new XUnitFilter(InteractionType, InteractionTypes.ToExclude, true),
            };

            if (additionalFilters != null && additionalFilters.Any())
                filters.AddRange(additionalFilters);

            return filters;
        }
    }
}
