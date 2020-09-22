using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Support.V4.App;

namespace Xamarin.Essentials
{
    public static partial class LocalNotifications
    {
        const string extraMetadataPrefix = "xemetadata_";

        static Task PlatformCancelAll()
        {
            var mgr = NotificationManager.FromContext(Platform.AppContext);
            mgr.CancelAll();

            return Task.CompletedTask;
        }

        static Task PlatformCancel(int id)
        {
            var mgr = NotificationManager.FromContext(Platform.AppContext);

            mgr.Cancel(id);

            return Task.CompletedTask;
        }

        static Task PlatformSend(LocalNotification notification)
        {
            var context = Platform.AppContext;

            var mgr = NotificationManager.FromContext(context);

            var metadataBundle = new global::Android.OS.Bundle();

            // Prefix extras with a known value since they can contain more than our bundle
            foreach (var kvp in notification.Metadata)
                metadataBundle.PutString($"{extraMetadataPrefix}{kvp.Key}", kvp.Value);

            NotificationCompat.Builder builder = default;

#if __ANDROID_26__
            // We should be good to use this even if _running_ on < 25
            // since this is a support lib api and it will gracefull degrade
            builder = new NotificationCompat.Builder(context, "Default");
#else
            builder = new NotificationCompat.Builder(context);
#endif
            var n = builder.SetContentTitle(notification.Title)
                .SetContentText(notification.Message)
                .SetExtras(metadataBundle)
                .Build();

            mgr.Notify(notification.Id, n);

            return Task.CompletedTask;
        }

        static Task<IEnumerable<LocalNotification>> PlatformGetNotifications()
        {
            var mgr = NotificationManager.FromContext(Platform.AppContext);

            var r = new List<LocalNotification>();

            var activeNotifications = mgr.GetActiveNotifications();

            foreach (var activeNotification in activeNotifications)
            {
                // Find any extras that begin with our known key prefix
                var metadata = new Dictionary<string, string>();
                foreach (var key in activeNotification.Notification.Extras.KeySet())
                {
                    if (key.StartsWith(extraMetadataPrefix, StringComparison.Ordinal))
                        metadata[key.Substring(extraMetadataPrefix.Length)] = activeNotification.Notification.Extras.GetString(key, null);
                }

                r.Add(new LocalNotification
                {
                    Id = activeNotification.Id,
                    Title = activeNotification.Notification.Extras.GetString(Notification.ExtraTitle, null),
                    Message = activeNotification.Notification.Extras.GetString(Notification.ExtraText, null),
                    Metadata = metadata
                });
            }

            return Task.FromResult<IEnumerable<LocalNotification>>(r);
        }
    }
}
