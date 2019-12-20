﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Locations;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V4.Content;
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        static ActivityLifecycleContextListener lifecycleListener;

        public static Context AppContext => Application.Context;

        public static Activity CurrentActivity => lifecycleListener?.Activity;

        public static event EventHandler<ActivityStateChangedEventArgs> ActivityStateChanged;

        internal static void OnActivityStateChanged(Activity activity, ActivityState ev)
            => ActivityStateChanged?.Invoke(null, new ActivityStateChangedEventArgs(activity, ev));

        public static async Task<Activity> WaitForActivityAsync(CancellationToken cancelToken = default)
        {
            if (CurrentActivity != null)
                return CurrentActivity;

            var tcs = new TaskCompletionSource<Activity>();
            var handler = new EventHandler<ActivityStateChangedEventArgs>((sender, args) =>
            {
                if (args.State == ActivityState.Created || args.State == ActivityState.Resumed)
                    tcs.TrySetResult(args.Activity);
            });

            try
            {
                using (cancelToken.Register(() => tcs.TrySetCanceled()))
                {
                    ActivityStateChanged += handler;
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                ActivityStateChanged -= handler;
            }
        }

        internal static Activity GetCurrentActivity(bool throwOnNull)
        {
            var activity = lifecycleListener?.Activity;
            if (throwOnNull && activity == null)
                throw new NullReferenceException("The current Activity can not be detected. Ensure that you have called Init in your Activity or Application class.");

            return activity;
        }

        public static void Init(Application application)
        {
            lifecycleListener = new ActivityLifecycleContextListener();
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
                if (feature?.Name?.Equals(systemFeature, StringComparison.OrdinalIgnoreCase) ?? false)
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

        internal static AndroidUri GetShareableFileUri(string filename)
        {
            Java.IO.File sharedFile;
            if (FileProvider.IsFileInPublicLocation(filename))
            {
                // we are sharing a file in a "shared/public" location
                sharedFile = new Java.IO.File(filename);
            }
            else
            {
                var rootDir = FileProvider.GetTemporaryDirectory();

                // create a unique directory just in case there are multiple file with the same name
                var tmpDir = new Java.IO.File(rootDir, Guid.NewGuid().ToString("N"));
                tmpDir.Mkdirs();
                tmpDir.DeleteOnExit();

                // create the new temprary file
                var tmpFile = new Java.IO.File(tmpDir, System.IO.Path.GetFileName(filename));
                System.IO.File.Copy(filename, tmpFile.CanonicalPath);
                tmpFile.DeleteOnExit();

                sharedFile = tmpFile;
            }

            // create the uri, if N use file provider
            if (HasApiLevelN)
            {
                var providerAuthority = AppContext.PackageName + ".fileProvider";
                return FileProvider.GetUriForFile(
                    AppContext.ApplicationContext,
                    providerAuthority,
                    sharedFile);
            }

            // use the shared file path created
            return AndroidUri.FromFile(sharedFile);
        }

        internal static bool HasApiLevelN =>
#if __ANDROID_24__
            HasApiLevel(BuildVersionCodes.N);
#else
            false;
#endif

        internal static bool HasApiLevelNMr1 =>
#if __ANDROID_25__
        HasApiLevel(BuildVersionCodes.NMr1);
#else
        false;
#endif

        internal static bool HasApiLevelO =>
#if __ANDROID_26__
            HasApiLevel(BuildVersionCodes.O);
#else
            false;
#endif

        internal static bool HasApiLevelOMr1 =>
#if __ANDROID_27__
            HasApiLevel(BuildVersionCodes.OMr1);
#else
            false;
#endif

        internal static bool HasApiLevelP =>
#if __ANDROID_28__
            HasApiLevel(BuildVersionCodes.P);
#else
            false;
#endif

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

        internal static Java.Util.Locale GetLocale()
        {
            var resources = AppContext.Resources;
            var config = resources.Configuration;
#if __ANDROID_24__
            if (HasApiLevelN)
                return config.Locales.Get(0);
#endif

            return config.Locale;
        }

        internal static void SetLocale(Java.Util.Locale locale)
        {
            Java.Util.Locale.Default = locale;
            var resources = AppContext.Resources;
            var config = resources.Configuration;

            if (HasApiLevelN)
                config.SetLocale(locale);
            else
                config.Locale = locale;

#pragma warning disable CS0618 // Type or member is obsolete
            resources.UpdateConfiguration(config, resources.DisplayMetrics);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    public enum ActivityState
    {
        Created,
        Resumed,
        Paused,
        Destroyed,
        SaveInstanceState,
        Started,
        Stopped
    }

    public class ActivityStateChangedEventArgs : EventArgs
    {
        internal ActivityStateChangedEventArgs(Activity activity, ActivityState ev)
        {
            State = ev;
            Activity = activity;
        }

        public ActivityState State { get; }

        public Activity Activity { get; }
    }

    class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        WeakReference<Activity> currentActivity = new WeakReference<Activity>(null);

        internal Context Context =>
            Activity ?? Application.Context;

        internal Activity Activity
        {
            get => currentActivity.TryGetTarget(out var a) ? a : null;
            set => currentActivity.SetTarget(value);
        }

        void Application.IActivityLifecycleCallbacks.OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            Activity = activity;
            Platform.OnActivityStateChanged(activity, ActivityState.Created);
        }

        void Application.IActivityLifecycleCallbacks.OnActivityDestroyed(Activity activity) =>
            Platform.OnActivityStateChanged(activity, ActivityState.Destroyed);

        void Application.IActivityLifecycleCallbacks.OnActivityPaused(Activity activity)
        {
            Activity = activity;
            Platform.OnActivityStateChanged(activity, ActivityState.Paused);
        }

        void Application.IActivityLifecycleCallbacks.OnActivityResumed(Activity activity)
        {
            Activity = activity;
            Platform.OnActivityStateChanged(activity, ActivityState.Resumed);
        }

        void Application.IActivityLifecycleCallbacks.OnActivitySaveInstanceState(Activity activity, Bundle outState) =>
            Platform.OnActivityStateChanged(activity, ActivityState.SaveInstanceState);

        void Application.IActivityLifecycleCallbacks.OnActivityStarted(Activity activity) =>
            Platform.OnActivityStateChanged(activity, ActivityState.Started);

        void Application.IActivityLifecycleCallbacks.OnActivityStopped(Activity activity) =>
            Platform.OnActivityStateChanged(activity, ActivityState.Stopped);
    }
}
