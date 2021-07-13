using System;
using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        static HapticFeedbackGenerator PlatformPrepareGenerator(HapticFeedbackType type)
        {
            Permissions.EnsureDeclared<Permissions.Vibrate>();

            return new HapticFeedbackGenerator(
                type,
                () => Platform.CurrentActivity?.Window?.DecorView?.PerformHapticFeedback(ConvertType(type)));
        }

        static FeedbackConstants ConvertType(HapticFeedbackType type) =>
            type switch
            {
                HapticFeedbackType.LongPress => FeedbackConstants.LongPress,
                _ => FeedbackConstants.ContextClick
            };
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
