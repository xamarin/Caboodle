﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class BrowserViewModel : BaseViewModel
    {
        private string browserStatus;
        private string uri = "http://xamarin.com";
        private int browserType = (int)BrowserLaunchType.SystemPreferred;

        public BrowserViewModel()
        {
            OpenUriCommand = new Command(OpenUri);
        }

        public ICommand OpenUriCommand { get; }

        public string BrowserStatus
        {
            get => browserStatus;
            set => SetProperty(ref browserStatus, value);
        }

        public string Uri
        {
            get => uri;
            set => SetProperty(ref uri, value);
        }

        public List<string> BrowserLaunchTypes { get; } =
            new List<string>
            {
                $"Use Default Browser App",
                $"Use System-Preferred Browser",
            };

        public int BrowserType
        {
            get => browserType;
            set => SetProperty(ref browserType, value);
        }

        private async void OpenUri()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                await Browser.OpenAsync(uri, (BrowserLaunchType)BrowserType);
            }
            catch (Exception e)
            {
                BrowserStatus = $"Unable to open Uri {e.Message}";
                Debug.WriteLine(browserStatus);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
