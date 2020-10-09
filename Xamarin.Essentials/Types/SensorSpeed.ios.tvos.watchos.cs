namespace Xamarin.Essentials
{
    static class SensorSpeedExtensions
    {
        internal static double ToPlatform(this SensorSpeed sensorSpeed)
        {
            // Timing intervals to match Android sensor speeds in seconds
            // https://developer.android.com/guide/topics/sensors/sensors_overview
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    return .005;
                case SensorSpeed.Game:
                    return .020;
                case SensorSpeed.UI:
                    return .060;
            }

            return .200;
        }
    }
}
