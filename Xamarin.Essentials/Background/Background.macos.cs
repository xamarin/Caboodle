using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Essentials.Background
{
    public partial class Background
    {
        internal static void PlatformStart()
        {
            // TODO?: https://developer.apple.com/documentation/backgroundtasks
            StartJobs();
        }
    }
}
