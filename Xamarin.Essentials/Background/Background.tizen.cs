using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Essentials.Background
{
    public partial class Background
    {
        internal static void PlatformStart()
        {
            // TODO: https://docs.tizen.org/application/native/guides/applications/service-app/
            throw ExceptionUtils.NotSupportedOrImplementedException;
        }
    }
}
