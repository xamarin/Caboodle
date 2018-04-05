﻿using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Caboodle
{
    public static partial class Gyroscope
    {
        internal static bool IsSupported =>
               Platform.SensorManager?.GetDefaultSensor(SensorType.Gyroscope) != null;

        static GyroscopeListener listener;
        static Sensor gyroscope;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var delay = SensorDelay.Normal;
            switch (sensorSpeed)
            {
                case SensorSpeed.Normal:
                    delay = SensorDelay.Normal;
                    break;
                case SensorSpeed.Fastest:
                    delay = SensorDelay.Fastest;
                    break;
                case SensorSpeed.Game:
                    delay = SensorDelay.Game;
                    break;
                case SensorSpeed.Ui:
                    delay = SensorDelay.Ui;
                    break;
            }

            listener = new GyroscopeListener();
            gyroscope = Platform.SensorManager.GetDefaultSensor(SensorType.Gyroscope);
            Platform.SensorManager.RegisterListener(listener, gyroscope, delay);
        }

        internal static void PlatformStop()
        {
            if (listener == null || gyroscope == null)
            {
                return;
            }

            Platform.SensorManager.UnregisterListener(listener, gyroscope);
            listener.Dispose();
            listener = null;
        }
    }

    internal class GyroscopeListener : Java.Lang.Object, ISensorEventListener
    {
        public GyroscopeListener()
        {
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            var data = new GyroscopeData(e.Values[0], e.Values[1], e.Values[2]);
            Gyroscope.OnChanged(data);
        }
    }
}
