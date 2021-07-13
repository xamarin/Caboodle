using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class HapticFeedback_Tests
    {
        [Fact]
        public void Click() => HapticFeedback.Perform(HapticFeedbackType.Click);

        [Fact]
        public void LongPress() => HapticFeedback.Perform(HapticFeedbackType.LongPress);

        [Fact]
        public void ClickWithGenerator()
        {
            using var generator = HapticFeedback.PrepareGenerator(HapticFeedbackType.Click);
            generator.Perform();
        }

        [Fact]
        public void LongPressWithGenerator()
        {
            using var generator = HapticFeedback.PrepareGenerator(HapticFeedbackType.LongPress);
            generator.Perform();
        }
    }
}
