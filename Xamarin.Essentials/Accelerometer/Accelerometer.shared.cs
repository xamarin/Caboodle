﻿using System;
using System.Numerics;

namespace Xamarin.Essentials
{
    public static partial class Accelerometer
    {
        const double accelerationThreshold = 2;

        const int shakenInterval = 500;

        static DateTime shakenTimeSpan = DateTime.Now;

        static bool useSyncContext;

        public static event EventHandler<AccelerometerChangedEventArgs> ReadingChanged;

        public static event EventHandler OnShaked;

        public static bool IsMonitoring { get; private set; }

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (IsMonitoring)
                throw new InvalidOperationException("Accelerometer has already been started.");

            IsMonitoring = true;
            useSyncContext = sensorSpeed == SensorSpeed.Default || sensorSpeed == SensorSpeed.UI;

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

            if (OnShaked != null)
                ProcessShakenEvents(e);
        }

        static void ProcessShakenEvents(AccelerometerChangedEventArgs e)
        {
            var g = Math.Round(e.Reading.Acceleration.X.Square() + e.Reading.Acceleration.Y.Square() + e.Reading.Acceleration.Z.Square());
            if (g > accelerationThreshold && DateTime.Now.Subtract(shakenTimeSpan).Milliseconds > shakenInterval)
            {
                shakenTimeSpan = DateTime.Now;
                OnShaked?.Invoke(null, EventArgs.Empty);
            }
        }

        static double Square(this float q) => q * q;
    }

    public class AccelerometerChangedEventArgs : EventArgs
    {
        public AccelerometerChangedEventArgs(AccelerometerData reading) => Reading = reading;

        public AccelerometerData Reading { get; }
    }

    public readonly struct AccelerometerData : IEquatable<AccelerometerData>
    {
        public AccelerometerData(double x, double y, double z)
            : this((float)x, (float)y, (float)z)
        {
        }

        public AccelerometerData(float x, float y, float z) =>
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
