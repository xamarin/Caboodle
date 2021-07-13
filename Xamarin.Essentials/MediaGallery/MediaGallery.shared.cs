using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public enum MediaFileType
    {
        Image,
        Video
    }

    public static partial class MediaGallery
    {
        public static Task SaveAsync(MediaFileType type, Stream fileStream, string fileName)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
            CheckParameters(fileName);

            return PlatformSaveAsync(type, fileStream, fileName);
        }

        public static Task SaveAsync(MediaFileType type, byte[] data, string fileName)
        {
            if (data == null || !(data.Length > 0))
                throw new ArgumentNullException(nameof(data));
            CheckParameters(fileName);

            return PlatformSaveAsync(type, data, fileName);
        }

        public static Task SaveAsync(MediaFileType type, string filePath)
        {
#if !NETSTANDARD1_0
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new ArgumentException(nameof(filePath));
#endif

            return PlatformSaveAsync(type, filePath);
        }

        static void CheckParameters(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException(nameof(fileName));
        }
    }
}
