﻿using Foundation;
using ObjCRuntime;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static NSObject observer;

        static ScreenMetrics GetScreenMetrics()
        {
            var bounds = UIScreen.MainScreen.Bounds;
            var scale = UIScreen.MainScreen.Scale;

            return new ScreenMetrics
            {
                Width = bounds.Width * scale,
                Height = bounds.Height * scale,
                Density = scale,
                Orientation = CalculateOrientation(),
                Rotation = CalculateRotation()
            };
        }

        static void StartScreenMetricsListeners()
        {
            var notificationCenter = NSNotificationCenter.DefaultCenter;
            var notification = UIApplication.DidChangeStatusBarOrientationNotification;
            observer = notificationCenter.AddObserver(notification, OnScreenMetricsChanaged);
        }

        static void StopScreenMetricsListeners()
        {
            observer?.Dispose();
            observer = null;
        }

        static void OnScreenMetricsChanaged(NSNotification obj)
        {
            var metrics = GetScreenMetrics();
            OnScreenMetricsChanaged(metrics);
        }

        static ScreenOrientation CalculateOrientation()
        {
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;

            if (orientation.IsLandscape())
                return ScreenOrientation.Landscape;

            return ScreenOrientation.Portrait;
        }

        static ScreenRotation CalculateRotation()
        {
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;

            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                    return ScreenRotation.Rotation0;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return ScreenRotation.Rotation180;
                case UIInterfaceOrientation.LandscapeLeft:
                    return ScreenRotation.Rotation270;
                case UIInterfaceOrientation.LandscapeRight:
                    return ScreenRotation.Rotation90;
            }

            return ScreenRotation.Rotation0;
        }
    }
}
