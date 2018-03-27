﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

#if __ANDROID__
[assembly: Android.App.UsesPermission(Android.Manifest.Permission.BatteryStats)]
#endif

namespace Caboodle.DeviceTests
{
    public class Permissions_Tests
    {
        [Theory]
        [InlineData(PermissionType.Battery)]
        [InlineData(PermissionType.NetworkState)]
        internal void Ensure_Declared(PermissionType permission)
        {
            Permissions.EnsureDeclared(permission);
        }

        [Theory]
        [InlineData(PermissionType.LocationWhenInUse)]
        internal void Ensure_Declared_Throws(PermissionType permission)
        {
            Assert.Throws<PermissionException>(() => Permissions.EnsureDeclared(permission));
        }

        [Theory]
        [InlineData(PermissionType.Battery, PermissionStatus.Granted)]
        [InlineData(PermissionType.NetworkState, PermissionStatus.Granted)]
        internal async Task Check_Status(PermissionType permission, PermissionStatus expectedStatus)
        {
            var status = await Permissions.CheckStatusAsync(permission);

            Assert.Equal(expectedStatus, status);
        }

        [Theory]
        [InlineData(PermissionType.LocationWhenInUse)]
        internal Task Check_Status_Throws(PermissionType permission)
        {
            return Assert.ThrowsAsync<PermissionException>(async () => await Permissions.CheckStatusAsync(permission));
        }

        [Theory]
        [InlineData(PermissionType.Battery, PermissionStatus.Granted)]
        [InlineData(PermissionType.NetworkState, PermissionStatus.Granted)]
        internal async Task Request(PermissionType permission, PermissionStatus expectedStatus)
        {
            var status = await Permissions.CheckStatusAsync(permission);

            Assert.Equal(expectedStatus, status);
        }
    }
}
