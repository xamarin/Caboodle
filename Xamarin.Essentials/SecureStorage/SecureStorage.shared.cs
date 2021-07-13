using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class SecureStorage
    {
        // Special Alias that is only used for Secure Storage. All others should use: Preferences.GetPrivatePreferencesSharedName
        internal static readonly string Alias = $"{AppInfo.PackageName}.xamarinessentials";

        // Special alias needed to used grouping
        internal static string GetAlias(string accessGroup)
           => string.IsNullOrWhiteSpace(accessGroup) ? Alias : accessGroup;

        public static Task<string> GetAsync(string key, string accessGroup)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return PlatformGetAsync(key, accessGroup);
        }

        public static Task<string> GetAsync(string key)
            => GetAsync(key, null);

        public static Task SetAsync(string key, string value, string accessGroup)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return PlatformSetAsync(key, value, accessGroup);
        }

        public static Task SetAsync(string key, string value)
            => SetAsync(key, value, null);

        public static bool Remove(string key, string accessGroup)
            => PlatformRemove(key, accessGroup);

        public static bool Remove(string key)
            => Remove(key, null);

        public static void RemoveAll(string accessGroup)
            => PlatformRemoveAll(accessGroup);

        public static void RemoveAll()
            => RemoveAll(null);
    }
}
