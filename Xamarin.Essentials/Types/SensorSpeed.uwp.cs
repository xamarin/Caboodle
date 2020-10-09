namespace Xamarin.Essentials
{
    static class SensorSpeedExtensions
    {
        internal static uint ToPlatform(this SensorSpeed sensorSpeed)
        {
            // Timing intervals to match Android sensor speeds in milliseconds
            // https://developer.android.com/guide/topics/sensors/sensors_overview
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    return 5;
                case SensorSpeed.Game:
                    return 20;
                case SensorSpeed.UI:
                    return 60;
            }

            return 200;
        }
    }
}
