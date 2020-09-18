using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        static Task<AppointmentStore> uwpAppointmentStore;

        static Task<AppointmentStore> GetInstanceAsync() =>
            uwpAppointmentStore ??= AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly).AsTask();

        static async Task<IEnumerable<Calendar>> PlatformGetCalendarsAsync()
        {
            var instance = await GetInstanceAsync().ConfigureAwait(false);

            var calendars = await instance.FindAppointmentCalendarsAsync(FindAppointmentCalendarsOptions.IncludeHidden).AsTask().ConfigureAwait(false);

            return ToCalendars(calendars).ToList();
        }

        static async Task<IEnumerable<CalendarEvent>> PlatformGetEventsAsync(string calendarId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            var options = new FindAppointmentsOptions();

            // properties
            options.FetchProperties.Add(AppointmentProperties.Subject);
            options.FetchProperties.Add(AppointmentProperties.Details);
            options.FetchProperties.Add(AppointmentProperties.Location);
            options.FetchProperties.Add(AppointmentProperties.StartTime);
            options.FetchProperties.Add(AppointmentProperties.Duration);
            options.FetchProperties.Add(AppointmentProperties.AllDay);
            options.FetchProperties.Add(AppointmentProperties.Invitees);

            // calendar
            if (!string.IsNullOrEmpty(calendarId))
                options.CalendarIds.Add(calendarId);

            // dates
            var sDate = startDate ?? DateTimeOffset.Now.Add(defaultStartTimeFromNow);
            var eDate = endDate ?? sDate.Add(defaultEndTimeFromStartTime);
            if (eDate < sDate)
                eDate = sDate;

            var instance = await GetInstanceAsync().ConfigureAwait(false);

            var events = await instance.FindAppointmentsAsync(sDate, eDate.Subtract(sDate), options).AsTask().ConfigureAwait(false);

            // confirm the calendar exists if no events were found
            // the PlatformGetCalendarAsync wll throw if not
            if ((events == null || events.Count == 0) && !string.IsNullOrEmpty(calendarId))
                await PlatformGetCalendarAsync(calendarId).ConfigureAwait(false);

            return ToEvents(events.OrderBy(e => e.StartTime)).ToList();
        }

        static async Task<Calendar> PlatformGetCalendarAsync(string calendarId)
        {
            var instance = await GetInstanceAsync().ConfigureAwait(false);

            var calendar = await instance.GetAppointmentCalendarAsync(calendarId).AsTask().ConfigureAwait(false);
            if (calendar == null)
                throw InvalidCalendar(calendarId);

            return ToCalendar(calendar);
        }

        static async Task<CalendarEvent> PlatformGetEventAsync(string eventId)
        {
            var instance = await GetInstanceAsync().ConfigureAwait(false);

            var e = await instance.GetAppointmentAsync(eventId).AsTask().ConfigureAwait(false);
            if (e == null)
                throw InvalidEvent(eventId);

            return ToEvent(e);
        }

        static IEnumerable<Calendar> ToCalendars(IEnumerable<AppointmentCalendar> native)
        {
            foreach (var calendar in native)
            {
                yield return ToCalendar(calendar);
            }
        }

        static Calendar ToCalendar(AppointmentCalendar calendar) =>
            new Calendar
            {
                Id = calendar.LocalId,
                Name = calendar.DisplayName
            };

        static IEnumerable<CalendarEvent> ToEvents(IEnumerable<Appointment> native)
        {
            foreach (var e in native)
            {
                yield return ToEvent(e);
            }
        }

        static CalendarEvent ToEvent(Appointment e) =>
            new CalendarEvent
            {
                Id = e.LocalId,
                CalendarId = e.CalendarId,
                Title = e.Subject,
                Description = e.Details,
                Location = e.Location,
                StartDate = e.StartTime,
                AllDay = e.AllDay,
                EndDate = e.StartTime.Add(e.Duration),
                Attendees = e.Invitees != null
                    ? ToAttendees(e.Invitees).ToList()
                    : new List<CalendarEventAttendee>()
            };

        static IEnumerable<CalendarEventAttendee> ToAttendees(IEnumerable<AppointmentInvitee> native)
        {
            foreach (var attendee in native)
            {
                yield return new CalendarEventAttendee
                {
                    Name = attendee.DisplayName,
                    Email = attendee.Address
                };
            }
        }
    }
}
