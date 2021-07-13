using Microsoft.Win32;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        internal static bool IsSupported
        {
            get
            {
                using var key = Registry.ClassesRoot.OpenSubKey("tel");
                return key.SubKeyCount > 0;
            }
        }

        static void PlatformOpen(string number)
        {
            ValidateOpen(number);

            Launcher.OpenAsync("tel:" + number);
        }
    }
}
