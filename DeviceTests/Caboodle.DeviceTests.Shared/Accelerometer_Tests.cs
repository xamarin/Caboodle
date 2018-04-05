﻿using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Accelerometer_Tests
    {
        bool TestSupported =>
            DeviceInfo.Platform == DeviceInfo.Platforms.Android ||
            (DeviceInfo.DeviceType == DeviceType.Physical && DeviceInfo.Platform == DeviceInfo.Platforms.iOS);

        public Accelerometer_Tests()
        {
            Accelerometer.Stop();
        }

        [Fact]
        public void IsSupported()
        {
            if (!TestSupported)
            {
                Assert.False(Accelerometer.IsSupported);
                return;
            }

            Assert.True(Accelerometer.IsSupported);
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Monitor(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<AccelerometerData>();
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(sensorSpeed);

            void Accelerometer_ReadingChanged(AccelerometerChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;

            Accelerometer.Stop();
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task IsMonitoring(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<AccelerometerData>();
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(sensorSpeed);

            void Accelerometer_ReadingChanged(AccelerometerChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;
            Assert.True(Accelerometer.IsMonitoring);
            Accelerometer.Stop();
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Stop_Monitor(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<AccelerometerData>();

            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(sensorSpeed);

            void Accelerometer_ReadingChanged(AccelerometerChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;

            Accelerometer.Stop();
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;

            Assert.False(Accelerometer.IsMonitoring);
        }
    }
}
