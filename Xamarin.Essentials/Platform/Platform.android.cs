﻿using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Locations;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        static ActivityLifecycleContextListener lifecycleListener;
        static AppStateLifecycleListener componentCallbackListener;

        internal static Context AppContext =>
            Application.Context;

        internal static Activity GetCurrentActivity(bool throwOnNull)
        {
            var activity = lifecycleListener?.Activity;
            if (throwOnNull && activity == null)
                throw new NullReferenceException("The current Activity can not be detected. Ensure that you have called Init in your Activity or Application class.");

            return activity;
        }

        public static void Init(Application application)
        {
            componentCallbackListener = new AppStateLifecycleListener();
            lifecycleListener = new ActivityLifecycleContextListener();
            application.RegisterComponentCallbacks(componentCallbackListener);
            application.RegisterActivityLifecycleCallbacks(lifecycleListener);
        }

        public static void Init(Activity activity, Bundle bundle)
        {
            Init(activity.Application);
            lifecycleListener.Activity = activity;
        }

        public static void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) =>
            Permissions.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        internal static bool HasSystemFeature(string systemFeature)
        {
            var packageManager = AppContext.PackageManager;
            foreach (var feature in packageManager.GetSystemAvailableFeatures())
            {
                if (feature.Name.Equals(systemFeature, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        internal static bool IsIntentSupported(Intent intent)
        {
            var manager = AppContext.PackageManager;
            var activities = manager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return activities.Any();
        }

        internal static bool HasApiLevel(BuildVersionCodes versionCode) =>
            (int)Build.VERSION.SdkInt >= (int)versionCode;

        internal static CameraManager CameraManager =>
            AppContext.GetSystemService(Context.CameraService) as CameraManager;

        internal static ConnectivityManager ConnectivityManager =>
            AppContext.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

        internal static Vibrator Vibrator =>
            AppContext.GetSystemService(Context.VibratorService) as Vibrator;

        internal static WifiManager WifiManager =>
            AppContext.GetSystemService(Context.WifiService) as WifiManager;

        internal static SensorManager SensorManager =>
            AppContext.GetSystemService(Context.SensorService) as SensorManager;

        internal static ClipboardManager ClipboardManager =>
            AppContext.GetSystemService(Context.ClipboardService) as ClipboardManager;

        internal static LocationManager LocationManager =>
            AppContext.GetSystemService(Context.LocationService) as LocationManager;

        internal static PowerManager PowerManager =>
            AppContext.GetSystemService(Context.PowerService) as PowerManager;
    }

    sealed class AppStateLifecycleListener : Java.Lang.Object, IComponentCallbacks2
    {
        public void OnConfigurationChanged(Configuration newConfig)
        {
        }

        public void OnLowMemory()
        {
        }

        public void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            if (level == TrimMemory.UiHidden)
                AppInfo.UpdateState(AppState.Background);
        }
    }

    sealed class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        WeakReference<Activity> currentActivity = new WeakReference<Activity>(null);

        internal Context Context =>
            Activity ?? Application.Context;

        internal Activity Activity
        {
            get => currentActivity.TryGetTarget(out var a) ? a : null;
            set => currentActivity.SetTarget(value);
        }

        void Application.IActivityLifecycleCallbacks.OnActivityCreated(Activity activity, Bundle savedInstanceState) =>
            Activity = activity;

        void Application.IActivityLifecycleCallbacks.OnActivityDestroyed(Activity activity)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityPaused(Activity activity) =>
            Activity = activity;

        void Application.IActivityLifecycleCallbacks.OnActivityResumed(Activity activity)
        {
            Activity = activity;
            AppInfo.UpdateState(AppState.Foreground);
        }

        void Application.IActivityLifecycleCallbacks.OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityStarted(Activity activity)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityStopped(Activity activity)
        {
        }
    }
}
