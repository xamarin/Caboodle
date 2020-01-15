﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
using UIKit;

#if __IOS__
using CoreMotion;
#elif __WATCHOS__
using CoreMotion;
using UIDevice = WatchKit.WKInterfaceDevice;
#endif

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
#if __IOS__
        [DllImport(Constants.SystemLibrary, EntryPoint = "sysctlbyname")]
#else
        [DllImport(Constants.libSystemLibrary, EntryPoint = "sysctlbyname")]
#endif
        internal static extern int SysctlByName([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        internal static string GetSystemLibraryProperty(string property)
        {
            var lengthPtr = Marshal.AllocHGlobal(sizeof(int));
            SysctlByName(property, IntPtr.Zero, lengthPtr, IntPtr.Zero, 0);

            var propertyLength = Marshal.ReadInt32(lengthPtr);

            if (propertyLength == 0)
            {
                Marshal.FreeHGlobal(lengthPtr);
                throw new InvalidOperationException("Unable to read length of property.");
            }

            var valuePtr = Marshal.AllocHGlobal(propertyLength);
            SysctlByName(property, valuePtr, lengthPtr, IntPtr.Zero, 0);

            var returnValue = Marshal.PtrToStringAnsi(valuePtr);

            Marshal.FreeHGlobal(lengthPtr);
            Marshal.FreeHGlobal(valuePtr);

            return returnValue;
        }

        internal static bool HasOSVersion(int major, int minor) =>
            UIDevice.CurrentDevice.CheckSystemVersion(major, minor);

#if __IOS__ || __TVOS__

        public static UIViewController GetCurrentUIViewController() =>
            GetCurrentViewController(false);

        internal static UIViewController GetCurrentViewController(bool throwIfNull = true)
        {
            UIViewController viewController = null;

            var window = UIApplication.SharedApplication.KeyWindow;

            if (window.WindowLevel == UIWindowLevel.Normal)
                viewController = window.RootViewController;

            if (viewController == null)
            {
                window = UIApplication.SharedApplication
                    .Windows
                    .OrderByDescending(w => w.WindowLevel)
                    .FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);

                if (window == null)
                    throw new InvalidOperationException("Could not find current view controller.");
                else
                    viewController = window.RootViewController;
            }

            while (viewController.PresentedViewController != null)
                viewController = viewController.PresentedViewController;

            if (throwIfNull && viewController == null)
                throw new InvalidOperationException("Could not find current view controller.");

            return viewController;
        }
#endif

#if __IOS__ || __WATCHOS__
        static CMMotionManager motionManager;

        internal static CMMotionManager MotionManager =>
            motionManager ?? (motionManager = new CMMotionManager());
#endif

        internal static NSOperationQueue GetCurrentQueue() =>
            NSOperationQueue.CurrentQueue ?? new NSOperationQueue();
    }
}
