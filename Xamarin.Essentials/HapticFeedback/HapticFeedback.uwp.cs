using System;
using System.Linq;
using Windows.Devices.Haptics;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        static HapticFeedbackGenerator PlatformPrepareGenerator(HapticFeedbackType type)
        {
            var generator = new HapticFeedbackGenerator(type);
            generator.Init();
            return generator;
        }
    }

    public partial class HapticFeedbackGenerator
    {
        const string vibrationDeviceApiType = "Windows.Devices.Haptics.VibrationDevice";
        SimpleHapticsControllerFeedback feedback;
        SimpleHapticsController controller;

        internal async void Init()
        {
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(vibrationDeviceApiType)
                && await VibrationDevice.RequestAccessAsync() == VibrationAccessStatus.Allowed))
                throw new FeatureNotSupportedException(HapticFeedback.notSupportedMessage);

            controller = (await VibrationDevice.GetDefaultAsync())?.SimpleHapticsController;
            if (controller != null)
                feedback = FindFeedback(controller, ConvertType(Type));
        }

        void PlatformPerform()
            => controller?.SendHapticFeedback(feedback);

        void PlatformDispose()
        {
            controller = null;
            feedback = null;
        }

        SimpleHapticsControllerFeedback FindFeedback(SimpleHapticsController controller, ushort type)
            => controller?.SupportedFeedback?.FirstOrDefault(a => a.Waveform == type);

        ushort ConvertType(HapticFeedbackType type) =>
            type switch
            {
                HapticFeedbackType.LongPress => KnownSimpleHapticsControllerWaveforms.Press,
                _ => KnownSimpleHapticsControllerWaveforms.Click
            };
    }
}
