using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Samples.Helpers
{
    public static class EmbeddedMedia
    {
        public const string baboonPng = "baboon.png";
        public const string earthMp4 = "earth.mp4";
        public const string lomonosovJpg = "lomonosov.jpg";
        public const string newtonsCradleGif = "newtons_cradle.gif";
    }

    public static class EmbeddedResourceProvider
    {
        static readonly Assembly assembly = typeof(EmbeddedResourceProvider).GetTypeInfo().Assembly;
        static readonly string[] resources = assembly.GetManifestResourceNames();

        public static Stream Load(string name)
        {
            name = GetFullName(name);

            if (string.IsNullOrWhiteSpace(name))
                return null;

            return assembly.GetManifestResourceStream(name);
        }

        public static ImageSource GetImageSource(string name)
        {
            name = GetFullName(name);

            if (string.IsNullOrWhiteSpace(name))
                return null;

            return ImageSource.FromResource(name, assembly);
        }

        static string GetFullName(string name)
            => resources.FirstOrDefault(n => n.EndsWith($".Media.{name}"));
    }
}
