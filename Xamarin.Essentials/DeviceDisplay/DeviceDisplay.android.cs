﻿using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static OrientationEventListener orientationListener;

        static ScreenMetrics GetScreenMetrics()
        {
            var displayMetrics = Platform.AppContext.Resources?.DisplayMetrics;

            return new ScreenMetrics(
                width: displayMetrics?.WidthPixels ?? 0,
                height: displayMetrics?.HeightPixels ?? 0,
                density: displayMetrics?.Density ?? 0,
                orientation: CalculateOrientation(),
                rotation: CalculateRotation());
        }

        static void StartScreenMetricsListeners()
        {
            orientationListener = new Listener(Platform.AppContext, OnScreenMetricsChanged);
            orientationListener.Enable();
        }

        static void StopScreenMetricsListeners()
        {
            orientationListener?.Disable();
            orientationListener?.Dispose();
            orientationListener = null;
        }

        static void OnScreenMetricsChanged()
        {
            var metrics = GetScreenMetrics();
            OnScreenMetricsChanged(metrics);
        }

        static ScreenRotation CalculateRotation()
        {
            var service = Platform.AppContext.GetSystemService(Context.WindowService);
            var display = service?.JavaCast<IWindowManager>()?.DefaultDisplay;

            if (display != null)
            {
                switch (display.Rotation)
                {
                    case SurfaceOrientation.Rotation270:
                        return ScreenRotation.Rotation270;
                    case SurfaceOrientation.Rotation180:
                        return ScreenRotation.Rotation180;
                    case SurfaceOrientation.Rotation90:
                        return ScreenRotation.Rotation90;
                    case SurfaceOrientation.Rotation0:
                        return ScreenRotation.Rotation0;
                }
            }

            return ScreenRotation.Rotation0;
        }

        static ScreenOrientation CalculateOrientation()
        {
            var config = Platform.AppContext.Resources?.Configuration;

            if (config != null)
            {
                switch (config.Orientation)
                {
                    case Orientation.Landscape:
                        return ScreenOrientation.Landscape;
                    case Orientation.Portrait:
                    case Orientation.Square:
                        return ScreenOrientation.Portrait;
                }
            }

            return ScreenOrientation.Unknown;
        }

        static string GetSystemSetting(string name)
           => Settings.System.GetString(Platform.AppContext.ContentResolver, name);
    }

    class Listener : OrientationEventListener
    {
        Action onChanged;

        internal Listener(Context context, Action handler)
            : base(context)
        {
            onChanged = handler;
        }

        public override void OnOrientationChanged(int orientation) => onChanged();
    }
}
