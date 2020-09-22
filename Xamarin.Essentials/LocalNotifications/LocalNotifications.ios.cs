using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UserNotifications;

namespace Xamarin.Essentials
{
    public static partial class LocalNotifications
    {
        static Task PlatformCancelAll()
        {
            UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
            UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();

            return Task.CompletedTask;
        }

        static Task PlatformCancel(int id)
        {
            var idStr = new[] { id.ToString() };

            UNUserNotificationCenter.Current.RemovePendingNotificationRequests(idStr);
            UNUserNotificationCenter.Current.RemoveDeliveredNotifications(idStr);

            return Task.CompletedTask;
        }

        static Task PlatformSend(LocalNotification notification)
        {
            var content = new UNMutableNotificationContent
            {
                Title = notification.Title,
                Body = notification.Message
            };

            foreach (var kvp in notification.Metadata)
                content.UserInfo.SetValueForKey(new NSString(kvp.Value), new NSString(kvp.Key));

            var request = UNNotificationRequest.FromIdentifier(
                notification.Id.ToString(),
                content,
                null);

            return UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
        }

        static Task<IEnumerable<LocalNotification>> PlatformGetNotifications()
        {
            var tcs = new TaskCompletionSource<IEnumerable<LocalNotification>>();

            // Need to get these on the UI thread
            Platform.GetCurrentQueue().BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var requests = await UNUserNotificationCenter
                        .Current
                        .GetPendingNotificationRequestsAsync();

                    var notifications = new List<LocalNotification>();

                    foreach (var r in requests)
                    {
                        // If not int, it's not one we setup, skip it
                        if (!int.TryParse(r.Identifier, out var intId))
                            continue;

                        notifications.Add(new LocalNotification
                        {
                            Id = intId,
                            Title = r.Content.Title,
                            Message = r.Content.Body
                        });
                    }

                    tcs.TrySetResult(notifications);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
