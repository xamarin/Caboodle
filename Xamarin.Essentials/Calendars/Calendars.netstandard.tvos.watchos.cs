using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        static Task<IEnumerable<Calendar>> PlatformGetCalendarsAsync() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<Calendar> PlatformGetCalendarAsync(string calendarId) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<IEnumerable<CalendarEvent>> PlatformGetEventsAsync(string calendarId, DateTimeOffset? startDate, DateTimeOffset? endDate) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<CalendarEvent> PlatformGetEventAsync(string eventId) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
