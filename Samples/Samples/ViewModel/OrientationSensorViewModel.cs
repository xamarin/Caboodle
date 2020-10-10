using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    class OrientationSensorViewModel : BaseViewModel
    {
        double x;
        double y;
        double z;
        double w;
        bool isActive;
        int speed = 0;

        double i;
        int cnt = 0;
        double sum = 0;
        Stopwatch sw = new Stopwatch();

        public OrientationSensorViewModel()
        {
            StartCommand = new Command(OnStart);
            StopCommand = new Command(OnStop);
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public double X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        public double Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        public double Z
        {
            get => z;
            set => SetProperty(ref z, value);
        }

        public double W
        {
            get => w;
            set => SetProperty(ref w, value);
        }

        public bool IsActive
        {
            get => isActive;
            set => SetProperty(ref isActive, value);
        }

        public string[] Speeds { get; } =
           Enum.GetNames(typeof(SensorSpeed));

        public int Speed
        {
            get => speed;
            set => SetProperty(ref speed, value);
        }

        public double Interval
        {
            get => i;
            set => SetProperty(ref i, value);
        }

        public override void OnAppearing()
        {
            OrientationSensor.ReadingChanged += OnReadingChanged;
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            OnStop();
            OrientationSensor.ReadingChanged -= OnReadingChanged;

            base.OnDisappearing();
        }

        async void OnStart()
        {
            cnt = 0;
            sum = 0;
            sw.Reset();
            try
            {
                OrientationSensor.Start((SensorSpeed)Speed);
                IsActive = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync($"Unable to start orientation sensor: {ex.Message}");
            }
        }

        void OnStop()
        {
            IsActive = false;
            OrientationSensor.Stop();
            sw.Stop();
        }

        void OnReadingChanged(object sender, OrientationSensorChangedEventArgs e)
        {
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            if (ms > 0)
            {
                sum += ms;
                cnt += 1;
                Interval = sum / cnt;
            }
            sw.Reset();
            sw.Start();

            var data = e.Reading;
            switch ((SensorSpeed)Speed)
            {
                case SensorSpeed.Fastest:
                case SensorSpeed.Game:
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        X = data.Orientation.X;
                        Y = data.Orientation.Y;
                        Z = data.Orientation.Z;
                        W = data.Orientation.W;
                    });
                    break;
                default:
                    X = data.Orientation.X;
                    Y = data.Orientation.Y;
                    Z = data.Orientation.Z;
                    W = data.Orientation.W;
                    break;
            }
        }
    }
}
