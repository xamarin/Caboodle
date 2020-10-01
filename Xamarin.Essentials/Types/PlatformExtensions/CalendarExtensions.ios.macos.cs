using System;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class CalendarExtensions
    {
        // https://developer.apple.com/documentation/foundation/nsdate
        // NSDate minimum date is 2001/01/01
        static DateTime iosNSDateTimeSystemZeroPoint = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static NSDate ToNSDate(this DateTimeOffset date) => NSDate.FromTimeIntervalSinceReferenceDate((date.UtcDateTime - iosNSDateTimeSystemZeroPoint).TotalSeconds);
    }
}
