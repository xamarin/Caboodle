using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Xamarin.Essentials
{
    public static partial class LocalNotifications
    {
        static Task PlatformCancelAll()
        {
            ToastNotificationManager.GetDefault().History.Clear();

            return Task.CompletedTask;
        }

        static Task PlatformCancel(int id)
        {
            ToastNotificationManager.GetDefault().History.Remove(id.ToString());

            return Task.CompletedTask;
        }

        static Task PlatformSend(LocalNotification notification)
        {
            var argsLaunch = $"action=xamarinEssentialsNotificationCallback={notification.Id}";

            var toastXmlString =
$@"<toast launch='{argsLaunch}'>
  <visual>
    <binding template='ToastGeneric'>
      <text>{notification.Title}</text>
      <text>{notification.Message}</text>
    </binding>
  </visual>
</toast>";

            var toastXml = new XmlDocument();
            toastXml.LoadXml(toastXmlString);

            var toast = new ToastNotification(toastXml)
            {
                Tag = notification.Id.ToString()
            };

            foreach (var kvp in notification.Metadata)
                toast.Data.Values[kvp.Key] = kvp.Value;

            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(toast);

            return Task.CompletedTask;
        }

        static Task<IEnumerable<LocalNotification>> PlatformGetNotifications()
        {
            var history = ToastNotificationManager.GetDefault().History.GetHistory();

            var r = new List<LocalNotification>();

            foreach (var n in history)
            {
                // Tag isn't an int, so we didn't do the notification
                if (!int.TryParse(n.Tag, out var idInt))
                    continue;

                var title = n.Content.SelectSingleNode("/toast/visual/binding[@template='ToastGeneric']/text[1]")?.NodeValue?.ToString();
                var message = n.Content.SelectSingleNode("/toast/visual/binding[@template='ToastGeneric']/text[2]")?.NodeValue?.ToString();

                if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message))
                    continue;

                r.Add(new LocalNotification
                {
                    Id = idInt,
                    Title = title,
                    Message = message,
                    Metadata = n.Data?.Values ?? new Dictionary<string, string>()
                });
            }

            return Task.FromResult<IEnumerable<LocalNotification>>(r);
        }
    }
}
