using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using CoreImage;
using Foundation;
using Photos;
using UIKit;

namespace Xamarin.Essentials
{
    public partial class FileSystem
    {
        internal static async Task<FileResult[]> EnsurePhysicalFileResultsAsync(params NSUrl[] urls)
        {
            if (urls == null || urls.Length == 0)
                return Array.Empty<FileResult>();

            var opts = NSFileCoordinatorReadingOptions.WithoutChanges;
            var intents = urls.Select(x => NSFileAccessIntent.CreateReadingIntent(x, opts)).ToArray();

            using var coordinator = new NSFileCoordinator();

            var tcs = new TaskCompletionSource<FileResult[]>();

            coordinator.CoordinateAccess(intents, new NSOperationQueue(), error =>
            {
                if (error != null)
                {
                    tcs.TrySetException(new NSErrorException(error));
                    return;
                }

                var bookmarks = new List<FileResult>();

                foreach (var intent in intents)
                {
                    var url = intent.Url;
                    var result = new BookmarkDataFileResult(url);
                    bookmarks.Add(result);
                }

                tcs.TrySetResult(bookmarks.ToArray());
            });

            return await tcs.Task;
        }
    }

    class BookmarkDataFileResult : FileResult
    {
        NSData bookmark;

        internal BookmarkDataFileResult(NSUrl url)
            : base()
        {
            try
            {
                url.StartAccessingSecurityScopedResource();

                var newBookmark = url.CreateBookmarkData(0, Array.Empty<string>(), null, out var bookmarkError);
                if (bookmarkError != null)
                    throw new NSErrorException(bookmarkError);

                UpdateBookmark(url, newBookmark);
            }
            finally
            {
                url.StopAccessingSecurityScopedResource();
            }
        }

        void UpdateBookmark(NSUrl url, NSData newBookmark)
        {
            bookmark = newBookmark;

            var doc = new UIDocument(url);
            FullPath = doc.FileUrl?.Path;
            FileName = doc.LocalizedName ?? Path.GetFileName(FullPath);
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            var url = NSUrl.FromBookmarkData(bookmark, 0, null, out var isStale, out var error);

            if (error != null)
                throw new NSErrorException(error);

            url.StartAccessingSecurityScopedResource();

            if (isStale)
            {
                var newBookmark = url.CreateBookmarkData(NSUrlBookmarkCreationOptions.SuitableForBookmarkFile, Array.Empty<string>(), null, out error);
                if (error != null)
                    throw new NSErrorException(error);

                UpdateBookmark(url, newBookmark);
            }

            var fileStream = File.OpenRead(FullPath);
            Stream stream = new SecurityScopedStream(fileStream, url);
            return Task.FromResult(stream);
        }

        class SecurityScopedStream : Stream
        {
            FileStream stream;
            NSUrl url;

            internal SecurityScopedStream(FileStream stream, NSUrl url)
            {
                this.stream = stream;
                this.url = url;
            }

            public override bool CanRead => stream.CanRead;

            public override bool CanSeek => stream.CanSeek;

            public override bool CanWrite => stream.CanWrite;

            public override long Length => stream.Length;

            public override long Position
            {
                get => stream.Position;
                set => stream.Position = value;
            }

            public override void Flush() =>
                stream.Flush();

            public override int Read(byte[] buffer, int offset, int count) =>
                stream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) =>
                stream.Seek(offset, origin);

            public override void SetLength(long value) =>
                stream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) =>
                stream.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    stream?.Dispose();
                    stream = null;

                    url?.StopAccessingSecurityScopedResource();
                    url = null;
                }
            }
        }
    }

    class UIDocumentFileResult : FileResult
    {
        internal UIDocumentFileResult(NSUrl url)
            : base()
        {
            var doc = new UIDocument(url);
            FullPath = doc.FileUrl?.Path;
            FileName = doc.LocalizedName ?? Path.GetFileName(FullPath);
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            Stream fileStream = File.OpenRead(FullPath);

            return Task.FromResult(fileStream);
        }
    }

    class UIImageFileResult : FileResult
    {
        internal UIImageFileResult(UIImage image, NSDictionary metadata)
            : base()
        {
            SaveImageWithMetadata(image, metadata);
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            Stream fileStream = File.OpenRead(FullPath);

            return Task.FromResult(fileStream);
        }

        void SaveImageWithMetadata(UIImage image, NSDictionary metadata)
        {
            if (image == null)
            {
                throw new FileResultCreationException($"{nameof(image)} cannot be null", new ArgumentNullException(nameof(image)));
            }

            if (metadata == null)
            {
                throw new FileResultCreationException($"{nameof(metadata)} cannot be null", new ArgumentNullException(nameof(metadata)));
            }

            var fn = Guid.NewGuid() + FileSystem.Extensions.Jpg;
            var tempDir = NSFileManager.DefaultManager.GetTemporaryDirectory().Path;
            var fp = Path.Combine(tempDir, fn);

            try
            {
                var imageData = image.AsJPEG();

                if (imageData == null)
                {
                    throw new FileResultCreationException("Could not encode UIImage into NSData.");
                }

                // Copy over meta data
                using var ciImage = CIImage.FromData(imageData);
                var originalColorSpace = ciImage.ColorSpace;
                using var newImageSource = ciImage.CreateBySettingProperties(metadata);
                using var ciContext = new CIContext();
                var result = ciContext.WriteJpegRepresentation(newImageSource, NSUrl.FromFilename(fp), originalColorSpace ?? CGColorSpace.CreateSrgb(), new NSDictionary(), out var error);
                if (result)
                {
                    FileName = fn;
                    FullPath = fp;
                }
                else
                {
                    var msg = "Could not create final image representation.";
                    if (error != null)
                    {
                        msg += $" Reason: {error.Description}";
                    }
                    throw new FileResultCreationException(msg);
                }
            }
            catch (FileResultCreationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"<||> {ex}");

                throw new FileResultCreationException(ex.Message, ex);
            }
            finally
            {
                image.Dispose();
            }
        }
    }

    class PHAssetFileResult : FileResult
    {
        readonly PHAsset phAsset;

        internal PHAssetFileResult(NSUrl url, PHAsset asset, string originalFilename)
            : base()
        {
            phAsset = asset;

            FullPath = url?.AbsoluteString;
            FileName = originalFilename;
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            var tcsStream = new TaskCompletionSource<Stream>();

            PHImageManager.DefaultManager.RequestImageData(phAsset, null, new PHImageDataHandler((data, str, orientation, dict) =>
                tcsStream.TrySetResult(data.AsStream())));

            return tcsStream.Task;
        }
    }
}
