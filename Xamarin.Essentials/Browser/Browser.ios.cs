﻿using System;
using System.Threading.Tasks;
using Foundation;
using SafariServices;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task PlatformOpenAsync(Uri uri, BrowserLaunchMode launchType)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var nativeUrl = new NSUrl(uri.AbsoluteUri);

            switch (launchType)
            {
                case BrowserLaunchMode.SystemPreferred:
                    var sfViewController = new SFSafariViewController(nativeUrl, false);
                    var vc = Platform.GetCurrentViewController();

                    if (sfViewController.PopoverPresentationController != null)
                    {
                        sfViewController.PopoverPresentationController.SourceView = vc.View;
                    }
                    vc.PresentViewController(sfViewController, true, null);
                    break;
                case BrowserLaunchMode.External:
                    UIKit.UIApplication.SharedApplication.OpenUrl(nativeUrl);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
