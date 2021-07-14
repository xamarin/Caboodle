using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class WebAuthenticator
    {
        static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
