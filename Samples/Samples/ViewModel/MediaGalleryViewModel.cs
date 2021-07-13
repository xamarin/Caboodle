using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Samples.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class MediaGalleryViewModel : BaseViewModel
    {
        public MediaGalleryViewModel()
        {
            SavevPngCommand = new Command(() => Save(MediaFileType.Image, EmbeddedMedia.baboonPng));
            SaveJpgCommand = new Command(() => Save(MediaFileType.Image, EmbeddedMedia.lomonosovJpg));
            SaveGifCommand = new Command(() => Save(MediaFileType.Image, EmbeddedMedia.newtonsCradleGif));
            SaveVideoCommand = new Command(() => Save(MediaFileType.Video, EmbeddedMedia.earthMp4));

            PngSource = EmbeddedResourceProvider.GetImageSource(EmbeddedMedia.baboonPng);
            JpgSource = EmbeddedResourceProvider.GetImageSource(EmbeddedMedia.lomonosovJpg);
            GifSource = EmbeddedResourceProvider.GetImageSource(EmbeddedMedia.newtonsCradleGif);
        }

        public bool FromStream { get; set; } = true;

        public bool FromByteArray { get; set; }

        public bool FromCacheDirectory { get; set; }

        public ImageSource PngSource { get; }

        public ImageSource JpgSource { get; }

        public ImageSource GifSource { get; }

        public ICommand SavevPngCommand { get; }

        public ICommand SaveJpgCommand { get; }

        public ICommand SaveGifCommand { get; }

        public ICommand SaveVideoCommand { get; }

        async void Save(MediaFileType type, string name)
        {
            try
            {
                using var stream = EmbeddedResourceProvider.Load(name);

                if (FromStream)
                {
                    await MediaGallery.SaveAsync(type, stream, name);
                }
                else if (FromByteArray)
                {
                    using var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);

                    await MediaGallery.SaveAsync(type, memoryStream.ToArray(), name);
                }
                else if (FromCacheDirectory)
                {
                    var filePath = SaveFileToCache(stream, name);

                    await MediaGallery.SaveAsync(type, filePath);
                }

                await DisplayAlertAsync("Save Completed Successfully");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(ex.Message);
            }
        }

        string SaveFileToCache(Stream data, string fileName)
        {
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            var stream = File.Create(filePath);
            data.CopyTo(stream);
            stream.Close();

            return filePath;
        }
    }
}
