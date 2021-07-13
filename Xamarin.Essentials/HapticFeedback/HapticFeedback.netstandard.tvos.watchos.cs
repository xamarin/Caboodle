using System;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        static HapticFeedbackGenerator PlatformPrepareGenerator(HapticFeedbackType type)
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }

    public partial class HapticFeedbackGenerator
    {
        void PlatformPerform()
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        void PlatformDispose()
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
