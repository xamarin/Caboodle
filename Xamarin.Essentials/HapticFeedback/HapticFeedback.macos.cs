using System;
using AppKit;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        static HapticFeedbackGenerator PlatformPrepareGenerator(HapticFeedbackType type)
            => new HapticFeedbackGenerator(
                type,
                () =>
                {
                    if (type == HapticFeedbackType.LongPress)
                    {
                        Performer.PerformFeedback(
                            NSHapticFeedbackPattern.Generic,
                            NSHapticFeedbackPerformanceTime.Default);
                    }
                });

        static INSHapticFeedbackPerformer Performer
            => NSHapticFeedbackManager.DefaultPerformer;
    }

    public partial class HapticFeedbackGenerator
    {
        Action perform;

        internal HapticFeedbackGenerator(HapticFeedbackType type, Action perform)
            : this(type)
            => this.perform = perform;

        void PlatformPerform()
            => perform.Invoke();

        void PlatformDispose()
            => perform = null;
    }
}
