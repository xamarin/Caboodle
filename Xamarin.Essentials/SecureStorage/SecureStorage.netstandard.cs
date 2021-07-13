using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public partial class SecureStorage
    {
        static Task<string> PlatformGetAsync(string key, string accessGroup) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task PlatformSetAsync(string key, string data, string accessGroup) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static bool PlatformRemove(string key, string accessGroup) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformRemoveAll(string accessGroup) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
