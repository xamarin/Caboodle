﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class AccelerometerViewModel : BaseViewModel
    {
        private double x;
        private double y;
        private double z;
        private bool isActive;
        private int speed = 2;

        public AccelerometerViewModel()
        {
            StartCommand = new Command(OnStart);
            StopCommand = new Command(OnStop);

            Accelerometer.ReadingChanged += OnReadingChanged;
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

        public bool IsActive
        {
            get => isActive;
            set => SetProperty(ref isActive, value);
        }

        public List<string> Speeds { get; } =
           new List<string>
           {
                "Fastest",
                "Game",
                "Normal",
                "User Interface"
           };

        public int Speed
        {
            get => speed;
            set => SetProperty(ref speed, value);
        }

        public override void OnDisappearing()
        {
            OnStop();
            Accelerometer.ReadingChanged -= OnReadingChanged;

            base.OnDisappearing();
        }

        private async void OnStart()
        {
            try
            {
                Accelerometer.Start((SensorSpeed)Speed);
                IsActive = true;
            }
            catch (Exception)
            {
                await DisplayAlert("Accelerometer not supported");
            }
        }

        private void OnStop()
        {
            IsActive = false;
            Accelerometer.Stop();
        }

        private void OnReadingChanged(AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            switch ((SensorSpeed)Speed)
            {
                case SensorSpeed.Fastest:
                case SensorSpeed.Game:
                    Platform.BeginInvokeOnMainThread(() =>
                    {
                        X = data.AccelerometerX;
                        Y = data.AccelerometerY;
                        Z = data.AccelerometerZ;
                    });
                    break;
                default:
                    X = data.AccelerometerX;
                    Y = data.AccelerometerY;
                    Z = data.AccelerometerZ;
                    break;
            }
        }
    }
}
