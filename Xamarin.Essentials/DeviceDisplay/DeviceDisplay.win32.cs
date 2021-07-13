using System;
using System.Runtime.InteropServices;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        const float mmToInch = 0.03937008f;

        static bool keepingScreenOn = false;

        static bool PlatformKeepScreenOn
        {
            get => keepingScreenOn;
            set
            {
                keepingScreenOn = value;
                NativeMethods.SetThreadExecutionState((value ? NativeMethods.EXECUTION_STATE.DISPLAY_REQUIRED : 0) | NativeMethods.EXECUTION_STATE.CONTINUOUS);
            }
        }

        static DisplayInfo GetMainDisplayInfo()
        {
            var hMonitor = NativeMethods.MonitorFromWindow(NativeMethods.GetDesktopWindow(), NativeMethods.MONITOR_DEFAULTTO.PRIMARY);
            if (hMonitor != IntPtr.Zero)
            {
                var mi = default(NativeMethods.MONITORINFOEX);
                mi.cbSize = Marshal.SizeOf(mi);

                if (NativeMethods.GetMonitorInfoW(hMonitor, ref mi))
                {
                    var dc = NativeMethods.CreateDCW(mi.szDevice, null, null, IntPtr.Zero);
                    if (dc != IntPtr.Zero)
                    {
                        var xpixels = NativeMethods.GetDeviceCaps(dc, NativeMethods.DeviceCap.HORZRES);
                        var xsize = NativeMethods.GetDeviceCaps(dc, NativeMethods.DeviceCap.HORZSIZE);
                        var rawDpiX = xpixels / (xsize * mmToInch);

                        var ypixels = NativeMethods.GetDeviceCaps(dc, NativeMethods.DeviceCap.VERTRES);

                        var orientation = xpixels > ypixels ? DisplayOrientation.Landscape : DisplayOrientation.Portrait;
                        NativeMethods.DeleteDC(dc);

                        return new DisplayInfo(xpixels, ypixels, rawDpiX, orientation, DisplayRotation.Unknown);
                    }
                }
            }

            return default;
        }

        static void StartScreenMetricsListeners()
        {
        }

        static void StopScreenMetricsListeners()
        {
        }

        static class NativeMethods
        {
            [DllImport("Kernel32", SetLastError = true, ExactSpelling = true)]
            internal static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

            internal enum EXECUTION_STATE : uint
            {
                CONTINUOUS = 0x80000000,
                DISPLAY_REQUIRED = 0x00000002,
            }

            [DllImport("user32", ExactSpelling = true)]
            internal static extern IntPtr GetDesktopWindow();

            [DllImport("user32", ExactSpelling = true)]
            internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, MONITOR_DEFAULTTO dwFlags);

            internal enum MONITOR_DEFAULTTO : int
            {
                NULL = 0,
                PRIMARY = 1,
                NEAREST = 2,
            }

            [DllImport("user32", ExactSpelling = true)]
            internal static extern bool GetMonitorInfoW(IntPtr hMonitor, ref MONITORINFOEX lpmi);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct MONITORINFOEX
            {
                internal int cbSize;
                internal RECT rcMonitor;
                internal RECT rcWork;
                internal uint dwFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                internal string szDevice;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct RECT
            {
                internal int left;
                internal int top;
                internal int right;
                internal int bottom;
            }

            [DllImport("gdi32", ExactSpelling = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateDCW(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

            [DllImport("gdi32", ExactSpelling = true)]
            internal static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32", ExactSpelling = true)]
            internal static extern int GetDeviceCaps(IntPtr hdc, DeviceCap nIndex);

            internal enum DeviceCap : int
            {
                HORZSIZE = 4,
                VERTSIZE = 6,
                HORZRES = 8,
                VERTRES = 10,
            }
        }
    }
}
