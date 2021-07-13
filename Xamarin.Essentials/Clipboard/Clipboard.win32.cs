using System.Threading.Tasks;
using WindowsClipboard = System.Windows.Clipboard;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static Task PlatformSetTextAsync(string text)
        {
            WindowsClipboard.SetText(text);
            return Task.CompletedTask;
        }

        static bool PlatformHasText
            => WindowsClipboard.ContainsText();

        static Task<string> PlatformGetTextAsync() => Task.FromResult(WindowsClipboard.GetText());

        static void StartClipboardListeners()
        {
        }

        static void StopClipboardListeners()
        {
        }
    }
}
