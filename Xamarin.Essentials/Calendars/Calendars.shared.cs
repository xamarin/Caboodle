using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        static readonly TimeSpan defaultStartTimeFromNow = TimeSpan.Zero;
        static readonly TimeSpan defaultEndTimeFromStartTime = TimeSpan.FromDays(14);

        public static async Task<IEnumerable<Calendar>> GetCalendarsAsync()
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            return await PlatformGetCalendarsAsync();
        }

        public static async Task<Calendar> GetCalendarAsync(string calendarId)
        {
            if (calendarId == null)
                throw new ArgumentNullException(nameof(calendarId));
            if (string.IsNullOrWhiteSpace(calendarId))
                throw InvalidCalendar(calendarId);

            await Permissions.RequestAsync<Permissions.CalendarRead>();

            return await PlatformGetCalendarAsync(calendarId);
        }

        public static async Task<IEnumerable<CalendarEvent>> GetEventsAsync(string calendarId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            return await PlatformGetEventsAsync(calendarId, startDate, endDate);
        }

        public static async Task<CalendarEvent> GetEventAsync(string eventId)
        {
            if (eventId == null)
                throw new ArgumentNullException(nameof(eventId));
            if (string.IsNullOrWhiteSpace(eventId))
                throw InvalidEvent(eventId);

            await Permissions.RequestAsync<Permissions.CalendarRead>();

            return await PlatformGetEventAsync(eventId);
        }

        public static Task<CalendarEvent> GetEventInstanceAsync(string eventId, DateTimeOffset instanceDate) => PlatformGetEventInstanceAsync(eventId, instanceDate);

        public static Task<string> CreateCalendarEvent(CalendarEvent newEvent) => PlatformCreateCalendarEvent(newEvent);

        public static Task<bool> UpdateCalendarEvent(CalendarEvent eventToUpdate) => PlatformUpdateCalendarEvent(eventToUpdate);

        public static Task<bool> SetEventRecurrenceEndDate(string eventId, DateTimeOffset recurrenceEndDate) => PlatformSetEventRecurrenceEndDate(eventId, recurrenceEndDate);

        public static Task<bool> DeleteCalendarEventInstanceByDate(string eventId, string calendarId, DateTimeOffset dateOfInstanceUtc) => PlatformDeleteCalendarEventInstanceByDate(eventId, calendarId, dateOfInstanceUtc);

        public static Task<bool> DeleteCalendarEvent(string eventId, string calendarId) => PlatformDeleteCalendarEvent(eventId, calendarId);

        public static Task<string> CreateCalendar(Calendar newCalendar) => PlatformCreateCalendar(newCalendar);

        public static Task<bool> AddAttendeeToEvent(CalendarEventAttendee newAttendee, string eventId) => PlatformAddAttendeeToEvent(newAttendee, eventId);

        public static Task<bool> RemoveAttendeeFromEvent(CalendarEventAttendee newAttendee, string eventId) => PlatformRemoveAttendeeFromEvent(newAttendee, eventId);

        static ArgumentException InvalidCalendar(string calendarId) =>
            new ArgumentOutOfRangeException($"No calendar exists with the ID '{calendarId}'.", nameof(calendarId));

        static ArgumentException InvalidEvent(string eventId) =>
            new ArgumentOutOfRangeException($"No event exists with the ID '{eventId}'.", nameof(eventId));
    }
}
