﻿using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class ScreenLockViewModel : BaseViewModel
    {
        public ScreenLockViewModel()
        {
            RequestActiveCommand = new Command(OnRequestActive);
            RequestReleaseCommand = new Command(OnRequestRelease);
        }

        public bool IsActive => ScreenLock.IsActive;

        public ICommand RequestActiveCommand { get; }

        public ICommand RequestReleaseCommand { get; }

        private void OnRequestActive()
        {
            ScreenLock.RequestActive();

            OnPropertyChanged(nameof(IsActive));
        }

        private void OnRequestRelease()
        {
            ScreenLock.RequestRelease();

            OnPropertyChanged(nameof(IsActive));
        }
    }
}
