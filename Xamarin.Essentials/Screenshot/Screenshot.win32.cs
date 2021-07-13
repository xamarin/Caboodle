using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformIsCaptureSupported =>
            true;

        static Task<ScreenshotResult> PlatformCaptureAsync()
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;

            if (!NativeMethods.GetWindowRect(hwnd, out var rc))
            {
                throw new ExternalException("Unable to determine window rectangle", Marshal.GetLastWin32Error());
            }

            var bmp = new Bitmap(rc.Right - rc.Left, rc.Bottom - rc.Top, PixelFormat.Format32bppArgb);
            var rectangleRegion = IntPtr.Zero;
            try
            {
                using var gfxBmp = Graphics.FromImage(bmp);
                IntPtr hdcBitmap;
                hdcBitmap = gfxBmp.GetHdc();
                var succeeded = NativeMethods.PrintWindow(hwnd, hdcBitmap, 0);
                gfxBmp.ReleaseHdc(hdcBitmap);
                if (!succeeded)
                {
                    throw new ExternalException("Unable to print window", Marshal.GetLastWin32Error());
                }

                rectangleRegion = NativeMethods.CreateRectRgn(0, 0, 0, 0);
                if (NativeMethods.GetWindowRgn(hwnd, rectangleRegion) != NativeMethods.RegionFlags.SIMPLEREGION)
                {
                    throw new InvalidOperationException("Could not find single region for application");
                }

                var region = Region.FromHrgn(rectangleRegion);
                if (!region.IsEmpty(gfxBmp))
                {
                    gfxBmp.ExcludeClip(region);
                    gfxBmp.Clear(Color.Transparent);
                }
            }
            finally
            {
                if (rectangleRegion != IntPtr.Zero)
                    NativeMethods.DeleteObject(rectangleRegion);
            }
            return Task.FromResult(new ScreenshotResult(bmp));
        }
    }

    public partial class ScreenshotResult
    {
        readonly Bitmap bmp;

        internal ScreenshotResult(Bitmap bmp)
        {
            this.bmp = bmp;
        }

        internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format)
        {
            var content = format switch
            {
                ScreenshotFormat.Jpeg => ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatDescription == "JPEG") ?? throw new InvalidOperationException("No JPEG encoder found"),
                ScreenshotFormat.Png => ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatDescription == "PNG") ?? throw new InvalidOperationException("No PNG encoder found"),
                _ => throw new NotImplementedException()
            };
            var encoderParameters = new EncoderParameters();
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 50L);
            var stream = new MemoryStream();
            bmp.Save(stream, content, encoderParameters);
            return Task.FromResult<Stream>(stream);
        }
    }

    class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        internal enum RegionFlags
        {
            ERROR = 0,
            NULLREGION = 1,
            SIMPLEREGION = 2,
            COMPLEXREGION = 3,
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern RegionFlags GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr gdiObject);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    }
}
