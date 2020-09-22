using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class LocalNotifications
    {
        static Task PlatformCancelAll()
            => throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformCancel(int id)
            => throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformSend(LocalNotification notification)
            => throw new NotImplementedInReferenceAssemblyException();

        static Task<IEnumerable<LocalNotification>> PlatformGetNotifications()
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
