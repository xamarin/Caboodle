﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class TextToSpeech
    {
        internal const float PitchMax = 2.0f;
        internal const float PitchDefault = 1.0f;
        internal const float PitchMin = 0.0f;

        internal const float VolumeMax = 1.0f;
        internal const float VolumeDefault = 0.5f;
        internal const float VolumeMin = 0.0f;

        static SemaphoreSlim semaphore;

        public static Task<IEnumerable<Locale>> GetLocalesAsync() =>
            PlatformGetLocalesAsync();

        public static Task SpeakAsync(string text, CancellationToken cancelToken = default) =>
            SpeakAsync(text, default, cancelToken);

        public static async Task SpeakAsync(string text, SpeakSettings settings, CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text), "Text cannot be null or empty string");

            if (settings?.Volume.HasValue ?? false)
            {
                if (settings.Volume.Value < VolumeMin || settings.Volume.Value > VolumeMax)
                    throw new ArgumentOutOfRangeException($"Volume must be >= {VolumeMin} and <= {VolumeMax}");
            }

            if (settings?.Pitch.HasValue ?? false)
            {
                if (settings.Pitch.Value < PitchMin || settings.Pitch.Value > PitchMax)
                    throw new ArgumentOutOfRangeException($"Pitch must be >= {PitchMin} and <= {PitchMin}");
            }

            if (semaphore == null)
                semaphore = new SemaphoreSlim(1, 1);

            try
            {
                await semaphore.WaitAsync(cancelToken);
                await PlatformSpeakAsync(text, settings, cancelToken);
            }
            finally
            {
                if (semaphore.CurrentCount == 0)
                    semaphore.Release();
            }
        }

        internal static float PlatformNormalize(float min, float max, float percent)
        {
            var range = max - min;
            var add = range * percent;
            return min + add;
        }
    }

    public partial struct Locale
    {
        public string Language { get; }

        public string Country { get; }

        public string Name { get; }

        public string Id { get; }

        internal Locale(string language, string country, string name, string id)
        {
            Language = language;
            Country = country;
            Name = name;
            Id = id;
        }
    }

    public class SpeakSettings
    {
        public Locale Locale { get; set; }

        public float? Pitch { get; set; }

        public float? Volume { get; set; }
    }
}
