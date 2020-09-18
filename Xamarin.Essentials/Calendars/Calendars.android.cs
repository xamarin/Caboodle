using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Database;
using Android.Provider;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        static Task<IEnumerable<Calendar>> PlatformGetCalendarsAsync()
        {
            var calendarsUri = CalendarContract.Calendars.ContentUri;
            var calendarsProjection = new List<string>
            {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName
            };
            var queryConditions =
                $"{CalendarContract.Calendars.InterfaceConsts.Deleted} != 1";

            using var cur = Platform.AppContext.ApplicationContext.ContentResolver.Query(calendarsUri, calendarsProjection.ToArray(), queryConditions, null, null);

            return Task.FromResult<IEnumerable<Calendar>>(ToCalendars(cur, calendarsProjection).ToList());
        }

        static Task<Calendar> PlatformGetCalendarAsync(string calendarId)
        {
            // Android ids are always integers
            if (!int.TryParse(calendarId, out _))
                throw InvalidCalendar(calendarId);

            var calendarsUri = CalendarContract.Calendars.ContentUri;
            var calendarsProjection = new List<string>
            {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName
            };
            var queryConditions =
                $"{CalendarContract.Calendars.InterfaceConsts.Deleted} != 1 AND " +
                $"{CalendarContract.Calendars.InterfaceConsts.Id} = {calendarId}";

            using var cur = Platform.AppContext.ApplicationContext.ContentResolver.Query(calendarsUri, calendarsProjection.ToArray(), queryConditions, null, null);

            if (cur.Count <= 0)
                throw InvalidCalendar(calendarId);

            cur.MoveToNext();

            return Task.FromResult(ToCalendar(cur, calendarsProjection));
        }

        static async Task<IEnumerable<CalendarEvent>> PlatformGetEventsAsync(string calendarId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            // Android ids are always integers
            if (!string.IsNullOrEmpty(calendarId) && !int.TryParse(calendarId, out _))
                throw InvalidCalendar(calendarId);

            var sDate = startDate ?? DateTimeOffset.Now.Add(defaultStartTimeFromNow);
            var eDate = endDate ?? sDate.Add(defaultEndTimeFromStartTime);

            var eventsUri = CalendarContract.Events.ContentUri;
            var eventsProjection = new List<string>
            {
                CalendarContract.Events.InterfaceConsts.Id,
                CalendarContract.Events.InterfaceConsts.CalendarId,
                CalendarContract.Events.InterfaceConsts.Title,
                CalendarContract.Events.InterfaceConsts.Description,
                CalendarContract.Events.InterfaceConsts.EventLocation,
                CalendarContract.Events.InterfaceConsts.AllDay,
                CalendarContract.Events.InterfaceConsts.Dtstart,
                CalendarContract.Events.InterfaceConsts.Dtend,
                CalendarContract.Events.InterfaceConsts.Deleted
            };
            var calendarSpecificEvent =
                $"{CalendarContract.Events.InterfaceConsts.Dtend} >= {sDate.AddMilliseconds(sDate.Offset.TotalMilliseconds).ToUnixTimeMilliseconds()} AND " +
                $"{CalendarContract.Events.InterfaceConsts.Dtstart} <= {eDate.AddMilliseconds(sDate.Offset.TotalMilliseconds).ToUnixTimeMilliseconds()} AND " +
                $"{CalendarContract.Events.InterfaceConsts.Deleted} != 1 ";
            if (!string.IsNullOrEmpty(calendarId))
                calendarSpecificEvent += $" AND {CalendarContract.Events.InterfaceConsts.CalendarId} = {calendarId}";
            var sortOrder = $"{CalendarContract.Events.InterfaceConsts.Dtstart} ASC";

            using var cur = Platform.AppContext.ApplicationContext.ContentResolver.Query(eventsUri, eventsProjection.ToArray(), calendarSpecificEvent, null, sortOrder);

            // confirm the calendar exists if no events were found
            // the PlatformGetCalendarAsync wll throw if not
            if (cur.Count == 0 && !string.IsNullOrEmpty(calendarId))
                await PlatformGetCalendarAsync(calendarId).ConfigureAwait(false);

            return ToEvents(cur, eventsProjection).ToList();
        }

        static Task<CalendarEvent> PlatformGetEventAsync(string eventId)
        {
            // Android ids are always integers
            if (!string.IsNullOrEmpty(eventId) && !int.TryParse(eventId, out _))
                throw InvalidCalendar(eventId);

            var eventsUri = CalendarContract.Events.ContentUri;
            var eventsProjection = new List<string>
            {
                CalendarContract.Events.InterfaceConsts.Id,
                CalendarContract.Events.InterfaceConsts.CalendarId,
                CalendarContract.Events.InterfaceConsts.Title,
                CalendarContract.Events.InterfaceConsts.Description,
                CalendarContract.Events.InterfaceConsts.EventLocation,
                CalendarContract.Events.InterfaceConsts.AllDay,
                CalendarContract.Events.InterfaceConsts.Dtstart,
                CalendarContract.Events.InterfaceConsts.Dtend
            };
            var calendarSpecificEvent = $"{CalendarContract.Events.InterfaceConsts.Id} = {eventId}";

            using var cur = Platform.AppContext.ApplicationContext.ContentResolver.Query(eventsUri, eventsProjection.ToArray(), calendarSpecificEvent, null, null);

            if (cur.Count <= 0)
                throw InvalidEvent(eventId);

            cur.MoveToNext();

            return Task.FromResult(ToEvent(cur, eventsProjection));
        }

        static IEnumerable<CalendarEventAttendee> PlatformGetAttendees(string eventId)
        {
            // Android ids are always integers
            if (!string.IsNullOrEmpty(eventId) && !int.TryParse(eventId, out _))
                throw InvalidCalendar(eventId);

            var attendeesUri = CalendarContract.Attendees.ContentUri;
            var attendeesProjection = new List<string>
            {
                CalendarContract.Attendees.InterfaceConsts.EventId,
                CalendarContract.Attendees.InterfaceConsts.AttendeeEmail,
                CalendarContract.Attendees.InterfaceConsts.AttendeeName
            };
            var attendeeSpecificAttendees =
                $"{CalendarContract.Attendees.InterfaceConsts.EventId}={eventId}";

            using var cur = Platform.AppContext.ApplicationContext.ContentResolver.Query(attendeesUri, attendeesProjection.ToArray(), attendeeSpecificAttendees, null, null);

            return ToAttendees(cur, attendeesProjection).ToList();
        }

        static IEnumerable<Calendar> ToCalendars(ICursor cur, List<string> projection)
        {
            while (cur.MoveToNext())
            {
                yield return ToCalendar(cur, projection);
            }
        }

        static Calendar ToCalendar(ICursor cur, List<string> projection) =>
            new Calendar
            {
                Id = cur.GetString(projection.IndexOf(CalendarContract.Calendars.InterfaceConsts.Id)),
                Name = cur.GetString(projection.IndexOf(CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName)),
            };

        static IEnumerable<CalendarEvent> ToEvents(ICursor cur, List<string> projection)
        {
            while (cur.MoveToNext())
            {
                yield return ToEvent(cur, projection);
            }
        }

        static CalendarEvent ToEvent(ICursor cur, List<string> projection)
        {
            var allDay = cur.GetInt(projection.IndexOf(CalendarContract.Events.InterfaceConsts.AllDay)) != 0;
            var start = DateTimeOffset.FromUnixTimeMilliseconds(cur.GetLong(projection.IndexOf(CalendarContract.Events.InterfaceConsts.Dtstart)));
            var end = DateTimeOffset.FromUnixTimeMilliseconds(cur.GetLong(projection.IndexOf(CalendarContract.Events.InterfaceConsts.Dtend)));

            return new CalendarEvent
            {
                Id = cur.GetString(projection.IndexOf(CalendarContract.Events.InterfaceConsts.Id)),
                CalendarId = cur.GetString(projection.IndexOf(CalendarContract.Events.InterfaceConsts.CalendarId)),
                Title = cur.GetString(projection.IndexOf(CalendarContract.Events.InterfaceConsts.Title)),
                Description = cur.GetString(projection.IndexOf(CalendarContract.Events.InterfaceConsts.Description)),
                Location = cur.GetString(projection.IndexOf(CalendarContract.Events.InterfaceConsts.EventLocation)),
                AllDay = allDay,
                StartDate = start,
                EndDate = end,
                Attendees = PlatformGetAttendees(cur.GetString(projection.IndexOf(CalendarContract.Events.InterfaceConsts.Id))).ToList()
            };
        }

        static IEnumerable<CalendarEventAttendee> ToAttendees(ICursor cur, List<string> projection)
        {
            while (cur.MoveToNext())
            {
                yield return ToAttendee(cur, projection);
            }
        }

        static CalendarEventAttendee ToAttendee(ICursor cur, List<string> attendeesProjection) =>
           new CalendarEventAttendee
           {
               Name = cur.GetString(attendeesProjection.IndexOf(CalendarContract.Attendees.InterfaceConsts.AttendeeName)),
               Email = cur.GetString(attendeesProjection.IndexOf(CalendarContract.Attendees.InterfaceConsts.AttendeeEmail)),
           };
    }
}
