﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
#if __IOS__
using SafariServices;
#endif
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class WebAuthenticator
    {
        static TaskCompletionSource<WebAuthenticatorResult> tcsResponse;
        static UIViewController currentViewController;
        static Uri redirectUri;

        internal static async Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
        {
            // Cancel any previous task that's still pending
            if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
                tcsResponse.TrySetCanceled();

            tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
            redirectUri = callbackUrl;

            try
            {
                var scheme = redirectUri.Scheme;

                if (!VerifyHasUrlSchemeOrDoesntRequire(scheme))
                {
                    tcsResponse.TrySetException(new InvalidOperationException("You must register your URL Scheme handler in your app's Info.plist!"));
                    return await tcsResponse.Task;
                }

#if __IOS__
                void AuthSessionCallback(NSUrl cbUrl, NSError error)
                {
                    if (error == null)
                        OpenUrl(cbUrl);
                    else
                        tcsResponse.TrySetException(new NSErrorException(error));
                }

                if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
                {
                    var was = new ASWebAuthenticationSession(new NSUrl(url.OriginalString), scheme, AuthSessionCallback);
                    was.PresentationContextProvider = new ContextProvider(Platform.GetCurrentWindow());
                    was.Start();
                    return await tcsResponse.Task;
                }

                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    var sf = new SFAuthenticationSession(new NSUrl(url.OriginalString), scheme, AuthSessionCallback);
                    sf.Start();
                    return await tcsResponse.Task;
                }

                // THis is only on iOS9+ but we only support 10+ in Essentials anyway
                var controller = new SFSafariViewController(new NSUrl(url.OriginalString), false)
                {
                    Delegate = new NativeSFSafariViewControllerDelegate
                    {
                        DidFinishHandler = (svc) =>
                        {
                            // Cancel our task if it wasn't already marked as completed
                            if (!(tcsResponse?.Task?.IsCompleted ?? true))
                                tcsResponse.TrySetException(new OperationCanceledException());
                        }
                    },
                };

                currentViewController = controller;
                await Platform.GetCurrentUIViewController().PresentViewControllerAsync(controller, true);
                return await tcsResponse.Task;
#else
                var opened = UIApplication.SharedApplication.OpenUrl(url);
                if (!opened)
                    tcsResponse.TrySetException(new Exception("Error opening Safari"));
#endif
            }
            catch (Exception ex)
            {
                tcsResponse.TrySetException(ex);
            }

            return await tcsResponse.Task;
        }

        internal static bool OpenUrl(Uri uri)
        {
            // If we aren't waiting on a task, don't handle the url
            if (tcsResponse?.Task?.IsCompleted ?? true)
                return false;

            try
            {
                // If we can't handle the url, don't
                if (!WebUtils.CanHandleCallback(redirectUri, uri))
                    return false;

                currentViewController?.DismissViewControllerAsync(true);
                currentViewController = null;

                tcsResponse.TrySetResult(new WebAuthenticatorResult(uri));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        static bool VerifyHasUrlSchemeOrDoesntRequire(string scheme)
        {
            // iOS11+ uses sfAuthenticationSession which handles its own url routing
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                return true;

            var cleansed = scheme.Replace("://", string.Empty);
            var schemes = GetCFBundleURLSchemes().ToList();
            return schemes.Any(x => x != null && x.Equals(cleansed, StringComparison.InvariantCultureIgnoreCase));
        }

        static IEnumerable<string> GetCFBundleURLSchemes()
        {
            var schemes = new List<string>();

            NSObject nsobj = null;
            if (!NSBundle.MainBundle.InfoDictionary.TryGetValue((NSString)"CFBundleURLTypes", out nsobj))
                return schemes;

            var array = nsobj as NSArray;

            if (array == null)
                return schemes;

            for (nuint i = 0; i < array.Count; i++)
            {
                var d = array.GetItem<NSDictionary>(i);
                if (d == null || !d.Any())
                    continue;

                if (!d.TryGetValue((NSString)"CFBundleURLSchemes", out nsobj))
                    continue;

                var a = nsobj as NSArray;
                var urls = ConvertToIEnumerable<NSString>(a).Select(x => x.ToString()).ToArray();
                foreach (var url in urls)
                    schemes.Add(url);
            }

            return schemes;
        }

        static IEnumerable<T> ConvertToIEnumerable<T>(NSArray array)
            where T : class, ObjCRuntime.INativeObject
        {
            for (nuint i = 0; i < array.Count; i++)
                yield return array.GetItem<T>(i);
        }

#if __IOS__
        class NativeSFSafariViewControllerDelegate : SFSafariViewControllerDelegate
        {
            public Action<SFSafariViewController> DidFinishHandler { get; set; }

            public override void DidFinish(SFSafariViewController controller) =>
                DidFinishHandler?.Invoke(controller);
        }

        class ContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
        {
            public ContextProvider(UIWindow window) =>
                Window = window;

            public UIWindow Window { get; private set; }

            public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
                => Window;
        }
#endif
    }
}
