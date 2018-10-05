﻿using System;
using System.Numerics;

namespace Xamarin.Essentials
{
    public static partial class Accelerometer
    {
        static bool useSyncContext;

        public static event EventHandler<AccelerometerChangedEventArgs> ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (IsMonitoring)
                return;

            IsMonitoring = true;
            useSyncContext = sensorSpeed == SensorSpeed.Normal || sensorSpeed == SensorSpeed.UI;

            try
            {
                PlatformStart(sensorSpeed);
            }
            catch
            {
                IsMonitoring = false;
                throw;
            }
        }

        public static void Stop()
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (!IsMonitoring)
                return;

            IsMonitoring = false;

            try
            {
                PlatformStop();
            }
            catch
            {
                IsMonitoring = true;
                throw;
            }
        }

        internal static void OnChanged(AccelerometerData reading) =>
            OnChanged(new AccelerometerChangedEventArgs(reading));

        internal static void OnChanged(AccelerometerChangedEventArgs e)
        {
            if (useSyncContext)
                MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
            else
                ReadingChanged?.Invoke(null, e);
        }
    }

    public class AccelerometerChangedEventArgs : EventArgs
    {
        internal AccelerometerChangedEventArgs(AccelerometerData reading) => Reading = reading;

        public AccelerometerData Reading { get; }
    }

    public readonly struct AccelerometerData : IEquatable<AccelerometerData>
    {
        internal AccelerometerData(double x, double y, double z)
            : this((float)x, (float)y, (float)z)
        {
        }

        internal AccelerometerData(float x, float y, float z) =>
            Acceleration = new Vector3(x, y, z);

        public Vector3 Acceleration { get; }

        public override bool Equals(object obj) =>
            (obj is AccelerometerData data) && Equals(data);

        public bool Equals(AccelerometerData other) =>
            Acceleration.Equals(other.Acceleration);

        public static bool operator ==(AccelerometerData left, AccelerometerData right) =>
            Equals(left, right);

        public static bool operator !=(AccelerometerData left, AccelerometerData right) =>
           !Equals(left, right);

        public override int GetHashCode() =>
            Acceleration.GetHashCode();

        public override string ToString() =>
            $"{nameof(Acceleration.X)}: {Acceleration.X}, " +
            $"{nameof(Acceleration.Y)}: {Acceleration.Y}, " +
            $"{nameof(Acceleration.Z)}: {Acceleration.Z}";
    }
}
