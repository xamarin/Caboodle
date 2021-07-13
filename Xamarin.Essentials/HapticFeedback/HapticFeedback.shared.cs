using System;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal const string notSupportedMessage =
            "Haptic Feedback is not supported on the current device";

        public static void Perform(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            using var generator = PrepareGenerator(type);
            generator.Perform();
        }

        public static HapticFeedbackGenerator PrepareGenerator(HapticFeedbackType type = HapticFeedbackType.Click)
            => PlatformPrepareGenerator(type);
    }

    public partial class HapticFeedbackGenerator : IDisposable
    {
        internal HapticFeedbackGenerator(HapticFeedbackType type)
            => Type = type;

        public HapticFeedbackType Type { get; }

        public void Perform()
            => PlatformPerform();

        public void Dispose()
        {
            PlatformDispose();
            GC.SuppressFinalize(this);
        }
    }
}
