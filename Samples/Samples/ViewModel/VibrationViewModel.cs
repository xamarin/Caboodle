﻿using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class VibrationViewModel : BaseViewModel
    {
        private int duration = 500;
        private bool isSupported = true;

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

        public override void OnDisappearing()
        {
            OnCancel();

            base.OnDisappearing();
        }

        private void OnVibrate()
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

        private void OnCancel()
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
