using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;
using Android.Webkit;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using MediaColumns = Android.Provider.MediaStore.MediaColumns;
using Path = System.IO.Path;
using Stream = System.IO.Stream;
using Uri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class MediaGallery
    {
        static async Task PlatformSaveAsync(MediaFileType type, byte[] data, string fileName)
        {
            using var ms = new MemoryStream(data);
            await PlatformSaveAsync(type, ms, fileName);
        }

        static async Task PlatformSaveAsync(MediaFileType type, string filePath)
        {
            using var fileStream = System.IO.File.OpenRead(filePath);
            await PlatformSaveAsync(type, fileStream, Path.GetFileName(filePath));
        }

        static async Task PlatformSaveAsync(MediaFileType type, Stream fileStream, string fileName)
        {
            var albumName = AppInfo.Name;

            await Permissions.EnsureGrantedAsync<Permissions.StorageWrite>();

            var context = Platform.AppContext;
            var dateTimeNow = DateTime.Now;

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName).ToLower();
            var newFileName = $"{fileNameWithoutExtension}_{dateTimeNow:yyyyMMdd_HHmmss}{extension}";

            using var values = new ContentValues();

            values.Put(MediaColumns.DateAdded, TimeSeconds(dateTimeNow));
            values.Put(MediaColumns.Title, fileNameWithoutExtension);
            values.Put(MediaColumns.DisplayName, newFileName);

            var mimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension.Replace(".", string.Empty));
            if (!string.IsNullOrWhiteSpace(mimeType))
                values.Put(MediaColumns.MimeType, mimeType);

            using var externalContentUri = type == MediaFileType.Image
                ? MediaStore.Images.Media.ExternalContentUri
                : MediaStore.Video.Media.ExternalContentUri;

            var relativePath = type == MediaFileType.Image
                ? Environment.DirectoryPictures
                : Environment.DirectoryMovies;

            if (Platform.HasApiLevelQ)
            {
#if MONOANDROID10_0
                values.Put(MediaColumns.RelativePath, Path.Combine(relativePath, albumName));
                values.Put(MediaColumns.IsPending, true);

                using var uri = context.ContentResolver.Insert(externalContentUri, values);
                using var stream = context.ContentResolver.OpenOutputStream(uri);
                fileStream.CopyTo(stream);
                stream.Close();

                values.Put(MediaColumns.IsPending, false);
                context.ContentResolver.Update(uri, values, null, null);
#else
                throw new Exception("For using save to gallery on android 10 and newer, use the target TargetFrameworks MonoAndroid10.0 or MonoAndroid11.0");
#endif
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                using var directory = new File(Environment.GetExternalStoragePublicDirectory(relativePath), albumName);

                directory.Mkdirs();
                using var file = new File(directory, newFileName);

                using var fileOutputStream = System.IO.File.Create(file.AbsolutePath);
                fileStream.CopyTo(fileOutputStream);
                fileOutputStream.Close();

                values.Put(MediaColumns.Data, file.AbsolutePath);
                context.ContentResolver.Insert(externalContentUri, values);

                using var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(Uri.FromFile(file));
                context.SendBroadcast(mediaScanIntent);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        static readonly DateTime jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // JavaSystem.CurrentTimeMillis()

        static long TimeMillis(DateTime current)
            => (long)CalcTimeDifference(current).TotalMilliseconds;

        static long TimeSeconds(DateTime current)
            => (long)CalcTimeDifference(current).TotalSeconds;

        static TimeSpan CalcTimeDifference(DateTime current)
            => current.ToUniversalTime() - jan1st1970;
    }
}
