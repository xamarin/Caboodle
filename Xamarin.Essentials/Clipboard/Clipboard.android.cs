﻿using System.Threading.Tasks;
using Android.Content;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static Task PlatformSetTextAsync(string text)
        {
            Platform.ClipboardManager.PrimaryClip = ClipData.NewPlainText("Text", text);
            return Task.CompletedTask;
        }

        static bool PlatformHasText
            => Platform.ClipboardManager.HasPrimaryClip;

        static Task<string> PlatformGetTextAsync()
            => Task.FromResult(Platform.ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);
    }
}
