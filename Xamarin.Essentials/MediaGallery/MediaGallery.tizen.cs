using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaGallery
    {
        static Task PlatformSaveAsync(MediaFileType type, byte[] data, string fileName)
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task PlatformSaveAsync(MediaFileType type, string filePath)
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task PlatformSaveAsync(MediaFileType type, Stream fileStream, string fileName)
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
