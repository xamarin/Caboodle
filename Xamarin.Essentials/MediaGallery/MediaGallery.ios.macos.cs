using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using Photos;

namespace Xamarin.Essentials
{
    public static partial class MediaGallery
    {
        static readonly string cacheDir = "EssentialsPhotosCacheDir";

        static async Task PlatformSaveAsync(MediaFileType type, byte[] data, string fileName)
        {
            string filePath = null;

            try
            {
                filePath = GetFilePath(fileName);
                await File.WriteAllBytesAsync(filePath, data);

                await PlatformSaveAsync(type, filePath);
            }
            finally
            {
                DeleteFile(filePath);
            }
        }

        static async Task PlatformSaveAsync(MediaFileType type, Stream fileStream, string fileName)
        {
            string filePath = null;

            try
            {
                filePath = GetFilePath(fileName);
                using var stream = File.Create(filePath);
                await fileStream.CopyToAsync(stream);
                stream.Close();

                await PlatformSaveAsync(type, filePath);
            }
            finally
            {
                DeleteFile(filePath);
            }
        }

        static async Task PlatformSaveAsync(MediaFileType type, string filePath)
        {
            using var fileUri = new NSUrl(filePath);

            await PhotoLibraryPerformChanges(() =>
            {
                using var request = type == MediaFileType.Video
                ? PHAssetChangeRequest.FromVideo(fileUri)
                : PHAssetChangeRequest.FromImage(fileUri);
            });
        }

        static async Task PhotoLibraryPerformChanges(Action action)
        {
            var tcs = new TaskCompletionSource<Exception>();

            PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(
                () =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetResult(ex);
                    }
                },
                (success, error) =>
                    tcs.TrySetResult(error != null ? new NSErrorException(error) : null));

            var exception = await tcs.Task;
            if (exception != null)
                throw exception;
        }

        static void DeleteFile(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                File.Delete(filePath);
        }

        static string GetFilePath(string fileName)
        {
            fileName = fileName.Trim();
            var dirPath = Path.Combine(FileSystem.CacheDirectory, cacheDir);
            var filePath = Path.Combine(dirPath, fileName);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            return filePath;
        }
    }
}
