﻿using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static Task PlatformSetTextAsync(string text)
            => throw new NotImplementedInReferenceAssemblyException();

        static bool PlatformHasText
            => throw new NotImplementedInReferenceAssemblyException();

        static Task<string> PlatformGetTextAsync()
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
