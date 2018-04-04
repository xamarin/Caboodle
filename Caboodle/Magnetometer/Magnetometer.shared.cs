﻿using System;

namespace Microsoft.Caboodle
{
    public static partial class Magnetometer
    {
        public static event MagnetometerChangedEventHandler ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
            {
                throw new FeatureNotSupportedException();
            }

            if (IsMonitoring)
            {
                return;
            }

            UseSyncContext = sensorSpeed == SensorSpeed.Normal || sensorSpeed == SensorSpeed.Ui;
            PlatformStart(sensorSpeed);
            IsMonitoring = true;
        }

        public static void Stop()
        {
            if (!IsMonitoring)
            {
                return;
            }

            PlatformStop();
            IsMonitoring = false;
        }

        internal static bool UseSyncContext { get; set; }

        internal static void OnChanged(MagnetometerData reading)
            => OnChanged(new MagnetometerChangedEventArgs(reading));

        internal static void OnChanged(MagnetometerChangedEventArgs e)
        {
            if (ReadingChanged == null)
                return;

            if (UseSyncContext)
            {
                Platform.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(e));
            }
            else
            {
                ReadingChanged?.Invoke(e);
            }
        }
    }

    public delegate void MagnetometerChangedEventHandler(MagnetometerChangedEventArgs e);

    public class MagnetometerChangedEventArgs : EventArgs
    {
        internal MagnetometerChangedEventArgs(MagnetometerData reading) => Reading = reading;

        public MagnetometerData Reading { get; }
    }

    public struct MagnetometerData
    {
        internal MagnetometerData(double x, double y, double z)
        {
            MagneticFieldX = x;
            MagneticFieldY = y;
            MagneticFieldZ = z;
        }

        public double MagneticFieldX { get; }

        public double MagneticFieldY { get; }

        public double MagneticFieldZ { get; }
    }
}
