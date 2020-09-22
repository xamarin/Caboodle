using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class LocalNotifications
    {
        /// <summary>
        /// Get scheduled notifications
        /// </summary>
        /// <returns></returns>
        public static Task<IEnumerable<LocalNotification>> GetNotifications()
            => PlatformGetNotifications();

        /// <summary>
        /// Cancel all scheduled notifications
        /// </summary>
        public static Task CancelAll()
            => PlatformCancelAll();

        /// <summary>
        /// Cancel a specific notification
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns>Returns true if message found and cancelled successfully</returns>
        public static Task Cancel(int notificationId)
            => PlatformCancel(notificationId);

        /// <summary>
        /// Send a notification
        /// </summary>
        /// <param name="notification"></param>
        /// <returns>The messageID that you can use to cancel with</returns>
        public static Task Send(LocalNotification notification)
            => PlatformSend(notification);
    }

    public class LocalNotification
    {
        const int firstId = 1001;

        static int idOn = firstId;

        static int NextId()
        {
            idOn++;
            if (idOn >= int.MaxValue)
                idOn = firstId;
            return idOn;
        }

        public int Id { get; set; } = NextId();

        public string Title { get; set; }

        public string Message { get; set; }

        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
