﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Samples.Helpers;
using Samples.View;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Device = Xamarin.Forms.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Samples
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Enable currently experimental features
            Device.SetFlags(new string[] { "MediaElement_Experimental" });

            VersionTracking.Track();

            MainPage = new NavigationPage(new HomePage());

            if (Device.RuntimePlatform != Device.macOS)
                AppActions.OnAppAction += AppActions_OnAppAction;
        }

        protected override async void OnStart()
        {
            if ((Device.RuntimePlatform == Device.Android && CommonConstants.AppCenterAndroid != "AC_ANDROID") ||
               (Device.RuntimePlatform == Device.iOS && CommonConstants.AppCenteriOS != "AC_IOS") ||
               (Device.RuntimePlatform == Device.UWP && CommonConstants.AppCenterUWP != "AC_UWP"))
            {
                AppCenter.Start(
                $"ios={CommonConstants.AppCenteriOS};" +
                $"android={CommonConstants.AppCenterAndroid};" +
                $"uwp={CommonConstants.AppCenterUWP}",
                typeof(Analytics),
                typeof(Crashes),
                typeof(Distribute));
            }

            if (Device.RuntimePlatform != Device.macOS)
            {
                await AppActions.SetAsync(
                    new AppAction("app_info", "App Info", icon: "app_info_action_icon"),
                    new AppAction("battery_info", "Battery Info"));
            }
        }

        void AppActions_OnAppAction(object sender, AppActionEventArgs e)
        {
            // Don't handle events fired for old application instances
            // and cleanup the old instance's event handler
            if (Application.Current != this && Application.Current is App app)
            {
                AppActions.OnAppAction -= app.AppActions_OnAppAction;
                return;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                var page = e.AppAction.Id switch
                {
                    "battery_info" => new BatteryPage(),
                    "app_info" => new AppInfoPage(),
                    _ => default(Page)
                };

                if (page != null)
                {
                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                    await Application.Current.MainPage.Navigation.PushAsync(page);
                }
            });
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
