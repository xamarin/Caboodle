﻿using System;
using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Magnetometer_Tests
    {
        bool TestSupported =>
            DeviceInfo.Platform == DeviceInfo.Platforms.Android ||
            (DeviceInfo.DeviceType == DeviceType.Physical && DeviceInfo.Platform == DeviceInfo.Platforms.iOS);

        public Magnetometer_Tests()
        {
            Magnetometer.Stop();
        }

        [Fact]
        public void IsSupported()
        {
            if (!TestSupported)
            {
                Assert.False(Magnetometer.IsSupported);
                return;
            }

            Assert.True(Magnetometer.IsSupported);
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Monitor(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<MagnetometerData>();
            Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
            Magnetometer.Start(sensorSpeed);

            void Magnetometer_ReadingChanged(MagnetometerChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;

            Magnetometer.Stop();
            Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task IsMonitoring(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<MagnetometerData>();
            Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
            Magnetometer.Start(sensorSpeed);

            void Magnetometer_ReadingChanged(MagnetometerChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;
            Assert.True(Magnetometer.IsMonitoring);
            Magnetometer.Stop();
            Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Stop_Monitor(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<MagnetometerData>();

            Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
            Magnetometer.Start(sensorSpeed);

            void Magnetometer_ReadingChanged(MagnetometerChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;

            Magnetometer.Stop();
            Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;

            Assert.False(Magnetometer.IsMonitoring);
        }
    }
}
