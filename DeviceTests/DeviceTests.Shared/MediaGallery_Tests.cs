using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests.Shared
{
    public class MediaGallery_Tests
    {
        const string jpgName = "lomonosov.jpg";

        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public async Task SaveAsync()
        {
            var platform = DeviceInfo.Platform;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (platform == DevicePlatform.iOS || platform == DevicePlatform.macOS)
                    await Permissions.RequestAsync<Permissions.Photos>();
                else if (platform == DevicePlatform.Android)
                    await Permissions.RequestAsync<Permissions.StorageWrite>();
            });

            var assembly = typeof(MediaGallery_Tests).GetTypeInfo().Assembly;
            var resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(jpgName));

            using var fileStream = assembly.GetManifestResourceStream(resourceName);

            await MediaGallery.SaveAsync(MediaFileType.Image, fileStream, jpgName);
        }
    }
}
