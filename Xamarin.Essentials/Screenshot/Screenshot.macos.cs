using System;
using System.IO;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using ImageIO;
using MobileCoreServices;

namespace Xamarin.Essentials
{
    public static partial class Screenshot
    {
        internal static bool PlatformIsCaptureSupported => true;

        static Task<ScreenshotResult> PlatformCaptureAsync()
        {
            var window = Platform.GetCurrentWindow(false);
            if (window == null)
            {
                throw new InvalidOperationException("No window found");
            }

            var rep = window.ContentView.BitmapImageRepForCachingDisplayInRect(window.ContentView.Bounds);
            window.ContentView.CacheDisplay(window.ContentView.Bounds, rep);
            var nsImage = new NSImage();
            nsImage.AddRepresentation(rep);
            var result = new ScreenshotResult(nsImage);
            return Task.FromResult(result);
        }
    }

    public partial class ScreenshotResult
    {
        readonly NSImage uiImage;

        internal ScreenshotResult(NSImage image)
        {
            uiImage = image;

            Width = (int)image.Size.Width;
            Height = (int)image.Size.Height;
        }

        internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format)
        {
            return Task.FromResult<Stream>(new ImageTypeStream(uiImage.CGImage, format));
        }

        // This way we do not leak memory and can read the stream infinity times. We also do not require a finalizer this way
        internal class ImageTypeStream : Stream
        {
            readonly Stream decoratedStream;
            readonly CGImage nativeImage;

            internal ImageTypeStream(CGImage image, ScreenshotFormat format)
            {
                nativeImage = image;

                var utType = format switch
                {
                    ScreenshotFormat.Png => UTType.PNG,
                    ScreenshotFormat.Jpeg => UTType.JPEG,
                    _ => throw new NotImplementedException("The ScreenshotFormat is not supported"),
                };

                var data = new NSMutableData();
                var dest = CGImageDestination.Create(data, utType, imageCount: 1);
                dest.AddImage(nativeImage);
                dest.Close();

                decoratedStream = data.AsStream();
            }

            public override bool CanRead => decoratedStream.CanRead;

            public override bool CanSeek => decoratedStream.CanSeek;

            public override bool CanWrite => decoratedStream.CanWrite;

            public override long Length => decoratedStream.Length;

            public override long Position { get => decoratedStream.Position; set => decoratedStream.Position = value; }

            public override void Flush() => decoratedStream.Flush();

            public override int Read(byte[] buffer, int offset, int count) => decoratedStream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => decoratedStream.Seek(offset, origin);

            public override void SetLength(long value) => decoratedStream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) => decoratedStream.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                nativeImage.Dispose();
                decoratedStream.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
