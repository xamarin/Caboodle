using System;
using System.Threading.Tasks;
using IO = System.IO;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        static string PlatformCacheDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        static string PlatformAppDataDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        static Task<IO.Stream> PlatformOpenAppPackageFileAsync(string filename)
             => Task.FromResult<IO.Stream>(IO.File.OpenRead(IO.Path.Combine(Environment.CurrentDirectory, filename)));
    }

    public partial class FileBase
    {
        internal void PlatformInit(FileBase file)
        {
            ContentType = PlatformGetContentType(file.FullPath);
        }

        static string PlatformGetContentType(string extension)
        {
            return System.Web.MimeMapping.GetMimeMapping(extension);
        }

        internal virtual Task<IO.Stream> PlatformOpenReadAsync()
        {
            var stream = IO.File.OpenRead(FullPath);
            return Task.FromResult<IO.Stream>(stream);
        }
    }
}
