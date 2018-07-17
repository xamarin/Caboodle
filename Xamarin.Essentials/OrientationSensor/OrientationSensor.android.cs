﻿using Android.Hardware;
using Android.Runtime;

namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        internal static bool IsSupported =>
            Platform.SensorManager?.GetDefaultSensor(SensorType.RotationVector) != null;

        static OrientationSensorListener listener;
        static Sensor orientationSensor;

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
                case SensorSpeed.UI:
                    delay = SensorDelay.Ui;
                    break;
            }

            listener = new OrientationSensorListener();
            orientationSensor = Platform.SensorManager.GetDefaultSensor(SensorType.RotationVector);
            Platform.SensorManager.RegisterListener(listener, orientationSensor, delay);
        }

        internal static void PlatformStop()
        {
            if (listener == null || orientationSensor == null)
                return;

            Platform.SensorManager.UnregisterListener(listener, orientationSensor);
            listener.Dispose();
            listener = null;
        }
    }

    class OrientationSensorListener : Java.Lang.Object, ISensorEventListener
    {
        internal OrientationSensorListener()
        {
        }

        void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            var data = new OrientationSensorData(e.Values[0], e.Values[1], e.Values[2], e.Values[3]);
            OrientationSensor.OnChanged(data);
        }
    }
}
