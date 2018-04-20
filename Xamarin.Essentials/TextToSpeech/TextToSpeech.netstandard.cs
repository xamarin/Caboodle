﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Xamarin.Essentials
{
    public static partial class TextToSpeech
    {
        internal static Task PlatformSpeakAsync(string text, SpeakSettings settings, CancellationToken cancelToken = default) =>
            throw new NotImplementedInReferenceAssemblyException();

        internal static Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
            throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class SpeakSettings
    {
        internal SpeakSettings PlatformSetSpeakRate(SpeakRate speakRate) =>
            throw new NotImplementedInReferenceAssemblyException();

        internal SpeakSettings PlatformSetPitch(Pitch pitch) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
