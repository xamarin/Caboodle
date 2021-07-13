using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;

namespace Xamarin.Essentials
{
    public static partial class MediaGallery
    {
        static async Task PlatformSaveAsync(MediaFileType type, byte[] data, string fileName)
        {
            var file = await GetStorageFile(type, fileName);
            var buffer = WindowsRuntimeBuffer.Create(data, 0, data.Length, data.Length);
            await FileIO.WriteBufferAsync(file, buffer);
        }

        static async Task PlatformSaveAsync(MediaFileType type, string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            await PlatformSaveAsync(type, fileStream, Path.GetFileName(filePath));
        }

        static async Task PlatformSaveAsync(MediaFileType type, Stream fileStream, string fileName)
        {
            var file = await GetStorageFile(type, fileName);
            using var stream = await file.OpenStreamForWriteAsync();
            await fileStream.CopyToAsync(stream);
            stream.Close();
        }

        static async Task<StorageFile> GetStorageFile(MediaFileType type, string fileName)
        {
            var albumFolder = await GetAlbumFolder(type, AppInfo.Name);
            return await albumFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
        }

        static async Task<StorageFolder> GetAlbumFolder(MediaFileType type, string albumName)
        {
            var mediaFolder = type == MediaFileType.Image
                    ? KnownFolders.PicturesLibrary
                    : KnownFolders.VideosLibrary;

            var folder = (await mediaFolder.GetFoldersAsync())?.FirstOrDefault(a => a.Name == albumName);
            return folder ?? await mediaFolder.CreateFolderAsync(albumName);
        }
    }
}
