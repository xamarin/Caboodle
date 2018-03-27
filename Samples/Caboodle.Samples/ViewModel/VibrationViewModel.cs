﻿using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    public class VibrationViewModel : BaseViewModel
    {
        int duration = 500;
        bool isSupported = true;

        public VibrationViewModel()
        {
            VibrateCommand = new Command(OnVibrate);
            CancelCommand = new Command(OnCancel);
        }

        public ICommand VibrateCommand { get; }

        public ICommand CancelCommand { get; }

        public int Duration
        {
            get => duration;
            set => SetProperty(ref duration, value);
        }

        public bool IsSupported
        {
            get => isSupported;
            set => SetProperty(ref isSupported, value);
        }

        void OnVibrate()
        {
            try
            {
                Vibration.Vibrate(duration);
            }
            catch (FeatureNotSupportedException)
            {
                IsSupported = false;
            }
        }

        void OnCancel()
        {
            try
            {
                Vibration.Cancel();
            }
            catch (FeatureNotSupportedException)
            {
                IsSupported = false;
            }
        }
    }
}
