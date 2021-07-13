using System;
using Tizen.System;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        static HapticFeedbackGenerator PlatformPrepareGenerator(HapticFeedbackType type)
        {
            Permissions.EnsureDeclared<Permissions.Vibrate>();
            return new HapticFeedbackGenerator(type, ConvertType(type));
        }

        static string ConvertType(HapticFeedbackType type)
            => type switch
            {
                HapticFeedbackType.LongPress => "Hold",
                _ => "Tap"
            };
    }

    public partial class HapticFeedbackGenerator
    {
        string pattern;
        Feedback feedback;

        internal HapticFeedbackGenerator(HapticFeedbackType type, string pattern)
            : this(type)
        {
            this.pattern = pattern;
            feedback = new Feedback();

            if (!feedback.IsSupportedPattern(FeedbackType.Vibration, pattern))
               throw new FeatureNotSupportedException(HapticFeedback.notSupportedMessage);
        }

        void PlatformPerform()
            => feedback.Play(FeedbackType.Vibration, pattern);

        void PlatformDispose()
        {
            pattern = null;
            feedback = null;
        }
    }
}
