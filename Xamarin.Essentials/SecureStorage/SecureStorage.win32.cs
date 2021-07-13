using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public partial class SecureStorage
    {
        static async Task<string> PlatformGetAsync(string key) => await Task.Run(() =>
                                                                            {
                                                                                string defaultEncStr = null;
                                                                                var encData = Preferences.Get(Md5Hash(key), defaultEncStr, Alias);
                                                                                if (!string.IsNullOrEmpty(encData))
                                                                                {
                                                                                    var encryptedBytes = Convert.FromBase64String(encData);
                                                                                    var unencryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                                                                                    return Encoding.UTF8.GetString(unencryptedBytes);
                                                                                }

                                                                                return null;
                                                                            });

        static async Task PlatformSetAsync(string key, string data) => await Task.Run(() =>
                                                                     {
                                                                         var bytes = Encoding.UTF8.GetBytes(data);

                                                                         var buffer = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                                                                         Preferences.Set(Md5Hash(key), Convert.ToBase64String(buffer), Alias);
                                                                     });

        static bool PlatformRemove(string key)
        {
            var keyHash = Md5Hash(key);

            if (Preferences.ContainsKey(keyHash, Alias))
            {
                Preferences.Remove(keyHash, Alias);
                return true;
            }

            return false;
        }

        static void PlatformRemoveAll() => Preferences.Clear(Alias);

        internal static string Md5Hash(string input)
        {
            var hash = new StringBuilder();
            var md5provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var bytes = md5provider.ComputeHash(Encoding.UTF8.GetBytes(input));

            for (var i = 0; i < bytes.Length; i++)
                hash.Append(bytes[i].ToString("x2"));

            return hash.ToString();
        }
    }
}
