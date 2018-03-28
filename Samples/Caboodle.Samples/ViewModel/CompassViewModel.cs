﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    class CompassViewModel : BaseViewModel
    {
        bool compass1IsActive;
        bool compass2IsActive;
        double compass1;
        double compass2;
        int speed1 = 2;
        int speed2 = 2;

        public CompassViewModel()
        {
            StartCompass1Command = new Command(OnStartCompass1);
            StopCompass1Command = new Command(OnStopCompass1);
            StartCompass2Command = new Command(OnStartCompass2);
            StopCompass2Command = new Command(OnStopCompass2);
        }

        public ICommand StartCompass1Command { get; }

        public ICommand StopCompass1Command { get; }

        public ICommand StartCompass2Command { get; }

        public ICommand StopCompass2Command { get; }

        public bool Compass1IsActive
        {
            get => compass1IsActive;
            set => SetProperty(ref compass1IsActive, value);
        }

        public bool Compass2IsActive
        {
            get => compass2IsActive;
            set => SetProperty(ref compass2IsActive, value);
        }

        public double Compass1
        {
            get => compass1;
            set => SetProperty(ref compass1, value);
        }

        public double Compass2
        {
            get => compass2;
            set => SetProperty(ref compass2, value);
        }

        public int Speed1
        {
            get => speed1;
            set => SetProperty(ref speed1, value);
        }

        public int Speed2
        {
            get => speed2;
            set => SetProperty(ref speed2, value);
        }

        public List<string> CompassSpeeds { get; } =
            new List<string>
            {
                "Fastest",
                "Game",
                "Normal",
                "User Interface"
            };

        async void OnStartCompass1()
        {
            try
            {
                if (Compass.IsMonitoring)
                    OnStopCompass2();

                Compass.Start((SensorSpeed)Speed1, (data) =>
                {
                    switch ((SensorSpeed)Speed1)
                    {
                        case SensorSpeed.Fastest:
                        case SensorSpeed.Game:
                            Platform.BeginInvokeOnMainThread(() => { Compass1 = data.HeadingMagneticNorth; });
                            break;
                        default:
                            Compass1 = data.HeadingMagneticNorth;
                            break;
                    }
                });
                Compass1IsActive = true;
            }
            catch (Exception)
            {
                await DisplayAlert("Compass not supported");
            }
        }

        void OnStopCompass1()
        {
            Compass1IsActive = false;
            Compass.Stop();
        }

        async void OnStartCompass2()
        {
            try
            {
                if (Compass.IsMonitoring)
                    OnStopCompass1();

                Compass.Start((SensorSpeed)Speed2, (data) =>
                {
                    switch ((SensorSpeed)Speed2)
                    {
                        case SensorSpeed.Fastest:
                        case SensorSpeed.Game:
                            Platform.BeginInvokeOnMainThread(() => { Compass2 = data.HeadingMagneticNorth; });
                            break;
                        default:
                            Compass2 = data.HeadingMagneticNorth;
                            break;
                    }
                });
                Compass2IsActive = true;
            }
            catch (Exception)
            {
                await DisplayAlert("Compass not supported");
            }
        }

        void OnStopCompass2()
        {
            Compass2IsActive = false;
            Compass.Stop();
        }

        public override void OnDisappearing()
        {
            OnStopCompass1();
            OnStopCompass2();

            base.OnDisappearing();
        }
    }
}
