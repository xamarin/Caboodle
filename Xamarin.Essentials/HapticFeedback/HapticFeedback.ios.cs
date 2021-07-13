using UIKit;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        static HapticFeedbackGenerator PlatformPrepareGenerator(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            if (!Platform.HasOSVersion(10, 0))
                throw new FeatureNotSupportedException(notSupportedMessage);

            return new HapticFeedbackGenerator(type, ConvertType(type));
        }

        static UIImpactFeedbackStyle ConvertType(HapticFeedbackType type)
            => type switch
            {
                HapticFeedbackType.LongPress => UIImpactFeedbackStyle.Medium,
                _ => UIImpactFeedbackStyle.Light
            };
    }

    public partial class HapticFeedbackGenerator
    {
        UIImpactFeedbackGenerator impact;

        internal HapticFeedbackGenerator(HapticFeedbackType type, UIImpactFeedbackStyle nativeType)
            : this(type)
        {
            impact = new UIImpactFeedbackGenerator(nativeType);
            impact.Prepare();
        }

        protected internal virtual void PlatformPerform()
            => impact.ImpactOccurred();

        protected internal virtual void PlatformDispose()
        {
            impact?.Dispose();
            impact = null;
        }
    }
}
