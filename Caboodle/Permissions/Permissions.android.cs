﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;

namespace Microsoft.Caboodle
{
    internal static partial class Permissions
    {
        static readonly object locker = new object();

        static Dictionary<PermissionType, (int requestCode, TaskCompletionSource<PermissionStatus> tcs)> requests =
            new Dictionary<PermissionType, (int, TaskCompletionSource<PermissionStatus>)>();

        static void PlatformEnsureDeclared(PermissionType permission)
        {
            var androidPermissions = permission.ToAndroidPermissions();

            // No actual android permissions required here, just return
            if (androidPermissions == null || !androidPermissions.Any())
                return;

            var context = Platform.CurrentContext;

            foreach (var ap in androidPermissions)
            {
                var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.Permissions);
                var requestedPermissions = packageInfo?.RequestedPermissions;

                // If the manifest is missing any of the permissions we need, throw
                if (!requestedPermissions?.Any(r => r.Equals(ap, StringComparison.OrdinalIgnoreCase)) ?? false)
                    throw new PermissionException($"You need to declare the permission: `{ap}` in your AndroidManifest.xml");
            }
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            PlatformEnsureDeclared(permission);

            // If there are no android permissions for the given permission type
            // just return granted since we have none to ask for
            var androidPermissions = permission.ToAndroidPermissions();
            if (androidPermissions == null || !androidPermissions.Any())
                return Task.FromResult(PermissionStatus.Granted);

            var context = Platform.CurrentContext;
            var targetsMOrHigher = context.ApplicationInfo.TargetSdkVersion >= Android.OS.BuildVersionCodes.M;

            foreach (var ap in androidPermissions)
            {
                if (targetsMOrHigher)
                {
                    if (context.CheckSelfPermission(ap) != Permission.Granted)
                        return Task.FromResult(PermissionStatus.Denied);
                }
                else
                {
                    if (PermissionChecker.CheckSelfPermission(context, ap) != PermissionChecker.PermissionGranted)
                        return Task.FromResult(PermissionStatus.Denied);
                }
            }

            return Task.FromResult(PermissionStatus.Granted);
        }

        static async Task<PermissionStatus> PlatformRequestAsync(PermissionType permission)
        {
            // Check status before requesting first
            if (await PlatformCheckStatusAsync(permission) == PermissionStatus.Granted)
                return PermissionStatus.Granted;

            TaskCompletionSource<PermissionStatus> tcs;
            var doRequest = true;
            var requestCode = 0;

            lock (locker)
            {
                if (requests.ContainsKey(permission))
                {
                    tcs = requests[permission].tcs;
                    doRequest = false;
                }
                else
                {
                    tcs = new TaskCompletionSource<PermissionStatus>();

                    // Get new request code and wrap it around for next use if it's going to reach max
                    if (++requestCode >= int.MaxValue)
                        requestCode = 1;

                    requests.Add(permission, (requestCode, tcs));
                }
            }

            if (!doRequest)
                return await tcs.Task;

            var androidPermissions = permission.ToAndroidPermissions().ToArray();

            ActivityCompat.RequestPermissions(Platform.CurrentActivity, androidPermissions, requestCode);

            return await tcs.Task;
        }

        internal static void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            lock (locker)
            {
                // Check our pending requests for one with a matching request code
                foreach (var kvp in requests)
                {
                    if (kvp.Value.requestCode == requestCode)
                    {
                        var tcs = kvp.Value.tcs;

                        // Look for any denied requests, and deny the whole request if so
                        // Remember, each PermissionType is tied to 1 or more android permissions
                        // so if any android permissions denied the whole PermissionType is considered denied
                        if (grantResults.Any(g => g == Permission.Denied))
                            tcs.TrySetResult(PermissionStatus.Denied);
                        else
                            tcs.TrySetResult(PermissionStatus.Granted);
                        break;
                    }
                }
            }
        }
    }

    internal static class PermissionTypeExtensions
    {
        internal static string[] ToAndroidPermissions(this PermissionType permissionType)
        {
            switch (permissionType)
            {
                case PermissionType.Battery:
                    return new[] { Manifest.Permission.BatteryStats };
                case PermissionType.LocationWhenInUse:
                    return new[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation };
                case PermissionType.NetworkState:
                    return new[] { Manifest.Permission.AccessNetworkState };
            }

            return null;
        }
    }
}
