using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials.Background
{
    public static partial class Background
    {
        internal static void PlatformStart()
        {
            BackgroundService.Start();
        }
    }
}
