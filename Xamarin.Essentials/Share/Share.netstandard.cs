﻿using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        static Task PlatformRequestAsync(ShareTextRequest request) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformRequestAsync(ShareFileRequest request) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
