namespace Xamarin.Essentials
{
    public enum SensorSpeed
    {
        Default = 0,
        UI = 1,
        Game = 2,
        Fastest = 3,
    }

    internal static partial class SensorSpeedExtensions
    {
        // Timing intervals to match Android sensor speeds in milliseconds
        // https://developer.android.com/guide/topics/sensors/sensors_overview
        internal const uint sensorIntervalDefault = 200;
        internal const uint sensorIntervalUI = 60;
        internal const uint sensorIntervalGame = 20;
        internal const uint sensorIntervalFastest = 5;
    }
}
