﻿using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;

using Camera = Android.Hardware.Camera;

namespace Microsoft.Caboodle
{
    public static partial class Flashlight
    {
        static readonly object locker = new object();

#pragma warning disable CS0618
        static Camera camera;
#pragma warning restore CS0618
        static SurfaceTexture surface;

        internal static bool IsSupported
            => Platform.HasSystemFeature(PackageManager.FeatureCameraFlash);

        internal static bool AlwaysUseCameraApi { get; set; } = false;

        static async Task PlatformTurnOnAsync()
        {
            await Permissions.RequireAsync(PermissionType.Flashlight);

            if (!IsSupported)
                throw new FeatureNotSupportedException();

            await ToggleTorchAsync(true);
        }

        static async Task PlatformTurnOffAsync()
        {
            await Permissions.RequireAsync(PermissionType.Flashlight);

            if (!IsSupported)
                throw new FeatureNotSupportedException();

            await ToggleTorchAsync(false);
        }

        static Task ToggleTorchAsync(bool switchOn)
        {
            return Task.Run(() =>
            {
                lock (locker)
                {
                    if (Platform.HasApiLevel(BuildVersionCodes.M) && !AlwaysUseCameraApi)
                    {
                        var cameraManager = Platform.CameraManager;
                        foreach (var id in cameraManager.GetCameraIdList())
                        {
                            var hasFlash = cameraManager.GetCameraCharacteristics(id).Get(CameraCharacteristics.FlashInfoAvailable);
                            if (Java.Lang.Boolean.True.Equals(hasFlash))
                            {
                                cameraManager.SetTorchMode(id, switchOn);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (camera == null)
                        {
                            if (surface == null)
                                surface = new SurfaceTexture(0);

                            camera = Camera.Open();

                            // Nexus 5 and some devices require a preview texture
                            camera.SetPreviewTexture(surface);
                        }

                        var param = camera.GetParameters();
#pragma warning disable CS0618
                        param.FlashMode = switchOn ? Camera.Parameters.FlashModeTorch : Camera.Parameters.FlashModeOff;
#pragma warning restore CS0618
                        camera.SetParameters(param);

                        if (switchOn)
                        {
                            camera.StartPreview();
                        }
                        else
                        {
                            camera.StopPreview();
                            camera.Release();
                            camera.Dispose();
                            camera = null;
                            surface.Dispose();
                            surface = null;
                        }
                    }
                }
            });
        }
    }
}
