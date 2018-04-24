﻿using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static ScreenMetrics GetScreenMetrics(DisplayInformation di = null)
        {
            di = di ?? DisplayInformation.GetForCurrentView();

            var rotation = CalculateRotation(di);
            var perpendicular =
                rotation == ScreenRotation.Rotation90 ||
                rotation == ScreenRotation.Rotation270;

            var w = di.ScreenWidthInRawPixels;
            var h = di.ScreenHeightInRawPixels;

            return new ScreenMetrics
            {
                Width = perpendicular ? h : w,
                Height = perpendicular ? w : h,
                Density = di.LogicalDpi / 96.0,
                Orientation = CalculateOrientation(di),
                Rotation = rotation
            };
        }

        static void StartScreenMetricsListeners()
        {
            Xamarin.Essentials.Platform.BeginInvokeOnMainThread(() =>
            {
                var di = DisplayInformation.GetForCurrentView();

                di.DpiChanged += OnDisplayInformationChanged;
                di.OrientationChanged += OnDisplayInformationChanged;
            });
        }

        static void StopScreenMetricsListeners()
        {
            Xamarin.Essentials.Platform.BeginInvokeOnMainThread(() =>
            {
                var di = DisplayInformation.GetForCurrentView();

                di.DpiChanged -= OnDisplayInformationChanged;
                di.OrientationChanged -= OnDisplayInformationChanged;
            });
        }

        static void OnDisplayInformationChanged(DisplayInformation di, object args)
        {
            var metrics = GetScreenMetrics(di);
            OnScreenMetricsChanaged(metrics);
        }

        static ScreenOrientation CalculateOrientation(DisplayInformation di)
        {
            switch (di.CurrentOrientation)
            {
                case DisplayOrientations.Landscape:
                case DisplayOrientations.LandscapeFlipped:
                    return ScreenOrientation.Landscape;
                case DisplayOrientations.Portrait:
                case DisplayOrientations.PortraitFlipped:
                    return ScreenOrientation.Portrait;
            }

            return ScreenOrientation.Unknown;
        }

        static ScreenRotation CalculateRotation(DisplayInformation di)
        {
            var native = di.NativeOrientation;
            var current = di.CurrentOrientation;

            if (native == DisplayOrientations.Portrait)
            {
                switch (current)
                {
                    case DisplayOrientations.Landscape: return ScreenRotation.Rotation90;
                    case DisplayOrientations.Portrait: return ScreenRotation.Rotation0;
                    case DisplayOrientations.LandscapeFlipped: return ScreenRotation.Rotation270;
                    case DisplayOrientations.PortraitFlipped: return ScreenRotation.Rotation180;
                }
            }
            else if (native == DisplayOrientations.Landscape)
            {
                switch (current)
                {
                    case DisplayOrientations.Landscape: return ScreenRotation.Rotation0;
                    case DisplayOrientations.Portrait: return ScreenRotation.Rotation270;
                    case DisplayOrientations.LandscapeFlipped: return ScreenRotation.Rotation180;
                    case DisplayOrientations.PortraitFlipped: return ScreenRotation.Rotation90;
                }
            }

            return ScreenRotation.Rotation0;
        }
    }
}
