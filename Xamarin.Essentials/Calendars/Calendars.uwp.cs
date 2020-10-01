using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        static Task<AppointmentStore> uwpAppointmentStore;
        static AppointmentStoreAccessType lastRequestType;

        static Task<AppointmentStore> GetInstanceAsync(AppointmentStoreAccessType type = AppointmentStoreAccessType.AppCalendarsReadWrite)
        {
            if (uwpAppointmentStore == null || lastRequestType != type)
            {
                uwpAppointmentStore = AppointmentManager.RequestStoreAsync(type).AsTask();
                lastRequestType = type;
            }

            return uwpAppointmentStore;
        }

        static async Task<IEnumerable<Calendar>> PlatformGetCalendarsAsync()
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var instance = await GetInstanceAsync(AppointmentStoreAccessType.AllCalendarsReadWrite).ConfigureAwait(false);
            var calendars = await instance.FindAppointmentCalendarsAsync(FindAppointmentCalendarsOptions.IncludeHidden).AsTask().ConfigureAwait(false);

            return ToCalendars(calendars).ToList();
        }

        static async Task<IEnumerable<CalendarEvent>> PlatformGetEventsAsync(string calendarId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var options = new FindAppointmentsOptions();
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
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var instance = await GetInstanceAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            Appointment uwpAppointment;
            try
            {
                uwpAppointment = await instance.GetAppointmentAsync(eventId);
                uwpAppointment.DetailsKind = AppointmentDetailsKind.PlainText;
            }
            catch (ArgumentException)
            {
                if (string.IsNullOrWhiteSpace(eventId))
                {
                    throw new ArgumentException($"[UWP]: No Event found for event Id {eventId}");
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"[UWP]: No Event found for event Id {eventId}");
                }
            }
            RecurrenceRule rules = null;
            if (uwpAppointment.Recurrence != null)
            {
                rules = new RecurrenceRule();
                rules.Frequency = (RecurrenceFrequency)uwpAppointment.Recurrence.Unit;
                rules.Interval = uwpAppointment.Recurrence.Interval;
                rules.EndDate = uwpAppointment.Recurrence.Until;
                rules.TotalOccurrences = uwpAppointment.Recurrence.Occurrences;
                switch (rules.Frequency)
                {
                    case RecurrenceFrequency.Daily:
                    case RecurrenceFrequency.Weekly:
                        rules.DaysOfTheWeek = ConvertBitFlagToIntList((int)uwpAppointment.Recurrence.DaysOfWeek, (int)AppointmentDaysOfWeek.Saturday).ToList();
                        break;
                    case RecurrenceFrequency.MonthlyOnDay:
                        rules.WeekOfMonth = (IterationOffset)uwpAppointment.Recurrence.WeekOfMonth;
                        rules.DaysOfTheWeek = ConvertBitFlagToIntList((int)uwpAppointment.Recurrence.DaysOfWeek, (int)AppointmentDaysOfWeek.Saturday).ToList();
                        break;
                    case RecurrenceFrequency.Monthly:
                        rules.DayOfTheMonth = uwpAppointment.Recurrence.Day;
                        break;
                    case RecurrenceFrequency.YearlyOnDay:
                        rules.WeekOfMonth = (IterationOffset)uwpAppointment.Recurrence.WeekOfMonth;
                        rules.DaysOfTheWeek = ConvertBitFlagToIntList((int)uwpAppointment.Recurrence.DaysOfWeek, (int)AppointmentDaysOfWeek.Saturday).ToList();
                        break;
                    case RecurrenceFrequency.Yearly:
                        rules.DayOfTheMonth = uwpAppointment.Recurrence.Day;
                        rules.MonthOfTheYear = (MonthOfYear)uwpAppointment.Recurrence.Month;
                        break;
                }
            }

            return new CalendarEvent()
            {
                Id = uwpAppointment.LocalId,
                CalendarId = uwpAppointment.CalendarId,
                Title = uwpAppointment.Subject,
                Description = uwpAppointment.Details,
                Location = uwpAppointment.Location,
                Url = uwpAppointment.Uri != null ? uwpAppointment.Uri.ToString() : string.Empty,
                StartDate = uwpAppointment.StartTime,
                EndDate = !uwpAppointment.AllDay ? (DateTimeOffset?)uwpAppointment.StartTime.Add(uwpAppointment.Duration) : null,
                Attendees = GetAttendeesForEvent(uwpAppointment.Invitees, uwpAppointment.Organizer),
                RecurrancePattern = rules,
                Reminders = uwpAppointment.Reminder.HasValue ? new List<CalendarEventReminder>() { new CalendarEventReminder() { MinutesPriorToEventStart = uwpAppointment.Reminder.Value.Minutes } } : null
            };
        }

        static async Task<CalendarEvent> PlatformGetEventInstanceAsync(string eventId, DateTimeOffset instanceDate)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var instance = await GetInstanceAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            Appointment uwpAppointment;
            try
            {
                uwpAppointment = await instance.GetAppointmentInstanceAsync(eventId, instanceDate);
                uwpAppointment.DetailsKind = AppointmentDetailsKind.PlainText;
            }
            catch (ArgumentException)
            {
                if (string.IsNullOrWhiteSpace(eventId))
                {
                    throw new ArgumentException($"[UWP]: No Event found for event Id {eventId}");
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"[UWP]: No Event found for event Id {eventId}");
                }
            }
            RecurrenceRule rules = null;
            if (uwpAppointment.Recurrence != null)
            {
                rules = new RecurrenceRule();
                rules.Frequency = (RecurrenceFrequency)uwpAppointment.Recurrence.Unit;
                rules.Interval = uwpAppointment.Recurrence.Interval;
                rules.EndDate = uwpAppointment.Recurrence.Until;
                rules.TotalOccurrences = uwpAppointment.Recurrence.Occurrences;
                switch (rules.Frequency)
                {
                    case RecurrenceFrequency.Daily:
                    case RecurrenceFrequency.Weekly:
                        rules.DaysOfTheWeek = ConvertBitFlagToIntList((int)uwpAppointment.Recurrence.DaysOfWeek, (int)AppointmentDaysOfWeek.Saturday).ToList();
                        break;
                    case RecurrenceFrequency.MonthlyOnDay:
                        rules.WeekOfMonth = (IterationOffset)uwpAppointment.Recurrence.WeekOfMonth;
                        rules.DaysOfTheWeek = ConvertBitFlagToIntList((int)uwpAppointment.Recurrence.DaysOfWeek, (int)AppointmentDaysOfWeek.Saturday).ToList();
                        break;
                    case RecurrenceFrequency.Monthly:
                        rules.DayOfTheMonth = uwpAppointment.Recurrence.Day;
                        break;
                    case RecurrenceFrequency.YearlyOnDay:
                        rules.WeekOfMonth = (IterationOffset)uwpAppointment.Recurrence.WeekOfMonth;
                        rules.DaysOfTheWeek = ConvertBitFlagToIntList((int)uwpAppointment.Recurrence.DaysOfWeek, (int)AppointmentDaysOfWeek.Saturday).ToList();
                        rules.MonthOfTheYear = (MonthOfYear)uwpAppointment.Recurrence.Month;
                        break;
                    case RecurrenceFrequency.Yearly:
                        rules.DayOfTheMonth = uwpAppointment.Recurrence.Day;
                        rules.MonthOfTheYear = (MonthOfYear)uwpAppointment.Recurrence.Month;
                        break;
                }
            }

            return new CalendarEvent()
            {
                Id = uwpAppointment.LocalId,
                CalendarId = uwpAppointment.CalendarId,
                Title = uwpAppointment.Subject,
                Description = uwpAppointment.Details,
                Location = uwpAppointment.Location,
                Url = uwpAppointment.Uri != null ? uwpAppointment.Uri.ToString() : string.Empty,
                StartDate = uwpAppointment.StartTime,
                EndDate = !uwpAppointment.AllDay ? (DateTimeOffset?)uwpAppointment.StartTime.Add(uwpAppointment.Duration) : null,
                Attendees = GetAttendeesForEvent(uwpAppointment.Invitees, uwpAppointment.Organizer),
                RecurrancePattern = rules,
                Reminders = uwpAppointment.Reminder.HasValue ? new List<CalendarEventReminder>() { new CalendarEventReminder() { MinutesPriorToEventStart = uwpAppointment.Reminder.Value.Minutes } } : null
            };
        }

        static List<CalendarDayOfWeek> ConvertBitFlagToIntList(int wholeNumber, int maxValue)
        {
            var currentVal = wholeNumber;
            var toReturn = new List<CalendarDayOfWeek>();
            for (var i = maxValue; i > 0; i /= 2)
            {
                if (currentVal >= i)
                {
                    toReturn.Add((CalendarDayOfWeek)i);
                    currentVal -= i;
                }
            }
            return toReturn;
        }

        static int ConvertIntListToBitFlag(List<int> listOfNumbers)
        {
            var toReturn = 0;
            foreach (var i in listOfNumbers)
            {
                toReturn += i;
            }
            return toReturn;
        }

        public static T GetEnumByIndex<T>(int index)
        {
            var enumValues = Enum.GetValues(typeof(T));
            return (T)enumValues.GetValue(index);
        }

        static IEnumerable<CalendarEventAttendee> GetAttendeesForEvent(IEnumerable<AppointmentInvitee> inviteList, AppointmentOrganizer organizer)
        {
            var attendees = (from attendee in inviteList
                             select new CalendarEventAttendee
                             {
                                 Name = attendee.DisplayName,
                                 Email = attendee.Address,
                                 Type = (AttendeeType)attendee.Role + 1
                             })
                            .OrderBy(e => e.Name)
                            .ToList();
            if (organizer != null)
            {
                attendees.Insert(0, new CalendarEventAttendee() { Name = organizer.DisplayName, Email = organizer.Address, IsOrganizer = true });
            }

            return attendees;
        }

        static async Task<string> PlatformCreateCalendarEvent(CalendarEvent newEvent)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            if (string.IsNullOrEmpty(newEvent.CalendarId))
            {
                return string.Empty;
            }

            var instance = await GetInstanceAsync();

            var appointment = new Appointment();
            appointment.Subject = newEvent.Title;
            appointment.Details = newEvent.Description ?? string.Empty;
            appointment.Location = newEvent.Location ?? string.Empty;
            appointment.StartTime = newEvent.StartDate;
            appointment.Duration = newEvent.EndDate.HasValue ? newEvent.EndDate.Value - newEvent.StartDate : TimeSpan.FromDays(1);
            appointment.AllDay = newEvent.AllDay;
            appointment.Uri = !string.IsNullOrEmpty(newEvent.Url) ? new Uri(newEvent.Url) : null;

            if (newEvent.RecurrancePattern != null)
            {
                appointment.Recurrence = newEvent.RecurrancePattern.ConvertRule();
            }

            var calendar = await instance.GetAppointmentCalendarAsync(newEvent.CalendarId);
            await calendar.SaveAppointmentAsync(appointment);

            if (!string.IsNullOrEmpty(appointment.LocalId))
                return appointment.LocalId;

            throw new ArgumentException("[UWP]: Could not create appointment with supplied parameters");
        }

        static async Task<bool> PlatformUpdateCalendarEvent(CalendarEvent eventToUpdate)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var existingEvent = await PlatformGetEventAsync(eventToUpdate.Id);

            Appointment thisEvent = null;
            var instance = await GetInstanceAsync();
            if (string.IsNullOrEmpty(eventToUpdate.CalendarId) || existingEvent == null)
            {
                return false;
            }
            else if (existingEvent.CalendarId != eventToUpdate.CalendarId)
            {
                await PlatformDeleteCalendarEvent(existingEvent.Id, existingEvent.CalendarId);
                thisEvent = new Appointment();
            }
            else
            {
                thisEvent = await instance.GetAppointmentAsync(eventToUpdate.Id);
            }

            if (eventToUpdate.RecurrancePattern != null)
            {
                thisEvent.Recurrence = eventToUpdate.RecurrancePattern.ConvertRule();
            }

            var url = eventToUpdate.Url;
            if (!string.IsNullOrWhiteSpace(url))
            {
                url = eventToUpdate.Url;
                if (!Regex.IsMatch(url, @"^https?:\/\/", RegexOptions.IgnoreCase))
                {
                    url = "http://" + url;
                }
            }

            thisEvent.Subject = eventToUpdate.Title;
            thisEvent.Details = eventToUpdate.Description;
            thisEvent.Location = eventToUpdate.Location;
            thisEvent.StartTime = eventToUpdate.StartDate;
            thisEvent.Duration = eventToUpdate.EndDate.HasValue ? eventToUpdate.EndDate.Value - eventToUpdate.StartDate : TimeSpan.FromDays(1);
            thisEvent.AllDay = eventToUpdate.AllDay;
            thisEvent.Uri = !string.IsNullOrEmpty(url) ? new Uri(url) : null;

            var calendar = await instance.GetAppointmentCalendarAsync(eventToUpdate.CalendarId);
            await calendar.SaveAppointmentAsync(thisEvent);

            if (!string.IsNullOrEmpty(thisEvent.LocalId))
            {
                return true;
            }
            throw new ArgumentException("[UWP]: Could not update appointment with supplied parameters");
        }

        static async Task<bool> PlatformSetEventRecurrenceEndDate(string eventId, DateTimeOffset recurrenceEndDate)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var existingEvent = await PlatformGetEventAsync(eventId);
            var instance = await GetInstanceAsync();

            if (string.IsNullOrEmpty(existingEvent?.CalendarId))
            {
                return false;
            }
            var thisEvent = await instance.GetAppointmentAsync(existingEvent.Id);

            if (existingEvent.RecurrancePattern != null)
            {
                existingEvent.RecurrancePattern.EndDate = recurrenceEndDate;
                existingEvent.RecurrancePattern.TotalOccurrences = null;
                thisEvent.Recurrence = existingEvent.RecurrancePattern.ConvertRule();
            }

            var calendar = await instance.GetAppointmentCalendarAsync(existingEvent.CalendarId);
            await calendar.SaveAppointmentAsync(thisEvent);
            if (string.IsNullOrEmpty(thisEvent.LocalId))
            {
                throw new ArgumentException("[UWP]: Could not update appointments Recurrence End Date with supplied parameters");
            }
            return true;
        }

        static AppointmentRecurrence ConvertRule(this RecurrenceRule recurrenceRule)
        {
            var eventRecurrence = new AppointmentRecurrence();
            eventRecurrence.Unit = (AppointmentRecurrenceUnit)recurrenceRule.Frequency;
            eventRecurrence.Interval = recurrenceRule.Interval;
            eventRecurrence.Until = recurrenceRule.EndDate;
            eventRecurrence.Occurrences = recurrenceRule.TotalOccurrences;

            switch (recurrenceRule.Frequency)
            {
                case RecurrenceFrequency.Daily:
                case RecurrenceFrequency.Weekly:
                    if (recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0)
                    {
                        eventRecurrence.DaysOfWeek = recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0 ? (AppointmentDaysOfWeek)recurrenceRule.DaysOfTheWeek.Sum(x => (int)x) : 0;
                        eventRecurrence.Unit = AppointmentRecurrenceUnit.Weekly;
                    }
                    break;
                case RecurrenceFrequency.Monthly:
                case RecurrenceFrequency.MonthlyOnDay:
                    if (recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0)
                    {
                        eventRecurrence.DaysOfWeek = (AppointmentDaysOfWeek)recurrenceRule.DaysOfTheWeek.Sum(x => (int)x);
                        eventRecurrence.WeekOfMonth = (AppointmentWeekOfMonth)recurrenceRule.WeekOfMonth;
                        eventRecurrence.Unit = AppointmentRecurrenceUnit.MonthlyOnDay;
                    }
                    else
                    {
                        eventRecurrence.Day = (uint)recurrenceRule.DayOfTheMonth;
                    }
                    break;
                case RecurrenceFrequency.Yearly:
                case RecurrenceFrequency.YearlyOnDay:
                    if (recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0)
                    {
                        eventRecurrence.WeekOfMonth = (AppointmentWeekOfMonth)recurrenceRule.WeekOfMonth;
                        eventRecurrence.DaysOfWeek = (AppointmentDaysOfWeek)recurrenceRule.DaysOfTheWeek.Sum(x => (int)x);
                        eventRecurrence.Unit = AppointmentRecurrenceUnit.YearlyOnDay;
                    }
                    else
                    {
                        eventRecurrence.Day = (uint)recurrenceRule.DayOfTheMonth;
                    }
                    eventRecurrence.Month = (uint)recurrenceRule.MonthOfTheYear;
                    break;
            }
            return eventRecurrence;
        }

        static async Task<string> PlatformCreateCalendar(Calendar newCalendar)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var instance = await GetInstanceAsync();

            var calendar = await instance.CreateAppointmentCalendarAsync(newCalendar.Name);

            if (calendar != null)
                return calendar.LocalId;

            throw new ArgumentException("[UWP]: Could not create appointment with supplied parameters");
        }

        static async Task<bool> PlatformDeleteCalendarEventInstanceByDate(string eventId, string calendarId, DateTimeOffset dateOfInstanceUtc)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException("[UWP]: You must supply an event id to delete an event.");
            }
            var calendarEvent = await PlatformGetEventInstanceAsync(eventId, dateOfInstanceUtc);

            if (calendarEvent.CalendarId != calendarId)
            {
                throw new ArgumentOutOfRangeException("[UWP]: Supplied event does not belong to supplied calendar");
            }

            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            var rect = new Windows.Foundation.Rect(0, 0, mainDisplayInfo.Width, mainDisplayInfo.Height);

            if (await AppointmentManager.ShowRemoveAppointmentAsync(calendarEvent.Id, rect, Windows.UI.Popups.Placement.Default, calendarEvent.StartDate))
            {
                return true;
            }
            return false;
        }

        static async Task<bool> PlatformDeleteCalendarEvent(string eventId, string calendarId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException("[UWP]: You must supply an event id to delete an event.");
            }
            var calendarEvent = await PlatformGetEventAsync(eventId);

            if (calendarEvent.CalendarId != calendarId)
            {
                throw new ArgumentOutOfRangeException("[UWP]: Supplied event does not belong to supplied calendar");
            }

            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            var rect = new Windows.Foundation.Rect(0, 0, mainDisplayInfo.Width, mainDisplayInfo.Height);

            if (await AppointmentManager.ShowRemoveAppointmentAsync(eventId, rect, Windows.UI.Popups.Placement.Default))
            {
                return true;
            }
            return false;
        }

        static async Task<bool> PlatformAddAttendeeToEvent(CalendarEventAttendee newAttendee, string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var instance = await GetInstanceAsync();

            var calendarEvent = await instance.GetAppointmentAsync(eventId);
            var calendar = await instance.GetAppointmentCalendarAsync(calendarEvent.CalendarId);
            var cntInvitiees = calendarEvent.Invitees.Count;

            if (calendarEvent == null)
                throw new ArgumentException("[UWP]: You must supply a valid event id to add an attendee to.");

            calendarEvent.Invitees.Add(new AppointmentInvitee() { DisplayName = newAttendee.Name, Address = newAttendee.Email, Role = (AppointmentParticipantRole)(newAttendee.Type - 1) });
            await calendar.SaveAppointmentAsync(calendarEvent);

            return calendarEvent.Invitees.Count == cntInvitiees + 1;
        }

        static async Task<bool> PlatformRemoveAttendeeFromEvent(CalendarEventAttendee newAttendee, string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var instance = await GetInstanceAsync();

            var calendarEvent = await instance.GetAppointmentAsync(eventId);
            var calendar = await instance.GetAppointmentCalendarAsync(calendarEvent.CalendarId);

            if (calendarEvent == null)
                throw new ArgumentException("[UWP]: You must supply a valid event id to remove an attendee from.");

            var attendeeToRemove = calendarEvent.Invitees.Where(x => x.DisplayName == newAttendee.Name && x.Address == newAttendee.Email).FirstOrDefault();

            calendarEvent.Invitees.Remove(attendeeToRemove);

            await calendar.SaveAppointmentAsync(calendarEvent);

            return attendeeToRemove != null;
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
