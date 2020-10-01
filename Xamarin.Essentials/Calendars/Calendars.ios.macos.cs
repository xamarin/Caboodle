using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EventKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        static EKEventStore eventStore;

        static EKEventStore EventStore =>
            eventStore ??= new EKEventStore();

        static Task<IEnumerable<Calendar>> PlatformGetCalendarsAsync()
        {
            var calendars = EventStore.GetCalendars(EKEntityType.Event);

            return Task.FromResult<IEnumerable<Calendar>>(ToCalendars(calendars).ToList());
        }

        static EKCalendar NativeGetCalendar(string calendarId)
        {
            var calendars = EventStore.GetCalendars(EKEntityType.Event);

            var calendar = calendars.FirstOrDefault(c => c.CalendarIdentifier == calendarId);
            if (calendar == null)
                throw InvalidCalendar(calendarId);
            return calendar;
        }

        static Task<Calendar> PlatformGetCalendarAsync(string calendarId)
        {
            var calendar = NativeGetCalendar(calendarId);

            return Task.FromResult(ToCalendar(calendar));
        }

        static Task<IEnumerable<CalendarEvent>> PlatformGetEventsAsync(string calendarId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            var startDateToConvert = startDate ?? DateTimeOffset.Now.Add(defaultStartTimeFromNow);
            var endDateToConvert = endDate ?? startDateToConvert.Add(defaultEndTimeFromStartTime); // NOTE: 4 years is the maximum period that a iOS calendar events can search

            var sDate = startDateToConvert.ToNSDate();
            var eDate = endDateToConvert.ToNSDate();

            var calendars = EventStore.GetCalendars(EKEntityType.Event);
            if (!string.IsNullOrEmpty(calendarId))
            {
                var calendar = calendars.FirstOrDefault(c => c.CalendarIdentifier == calendarId);
                if (calendar == null)
                    throw InvalidCalendar(calendarId);

                calendars = new[] { calendar };
            }

            var query = EventStore.PredicateForEvents(sDate, eDate, calendars);
            var events = EventStore.EventsMatching(query);

            return Task.FromResult<IEnumerable<CalendarEvent>>(ToEvents(events).OrderBy(e => e.StartDate).ToList());
        }

        static DateTimeOffset ToDateTimeOffsetWithTimeZone(this NSDate originalDate, NSTimeZone timeZone)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone != null ? timeZone.Name : NSTimeZone.LocalTimeZone.Name);
            return TimeZoneInfo.ConvertTime(originalDate.ToDateTime(), timeZoneInfo);
        }

        static async Task<CalendarEvent> PlatformGetEventAsync(string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new ArgumentException($"[iOS]: No Event found for event Id {eventId}");
            }

            if (!(EventStore.GetCalendarItem(eventId) is EKEvent calendarEvent))
            {
                throw new ArgumentOutOfRangeException($"[iOS]: No Event found for event Id {eventId}");
            }
            RecurrenceRule recurrenceRule = null;
            if (calendarEvent.HasRecurrenceRules)
            {
                recurrenceRule = GetRecurrenceRule(calendarEvent.RecurrenceRules[0], calendarEvent.TimeZone);
            }
            List<CalendarEventReminder> alarms = null;
            if (calendarEvent.HasAlarms)
            {
                alarms = new List<CalendarEventReminder>();
                foreach (var a in calendarEvent.Alarms)
                {
                    alarms.Add(new CalendarEventReminder() { MinutesPriorToEventStart = (calendarEvent.StartDate.ToDateTime() - a.AbsoluteDate.ToDateTime()).Minutes });
                }
            }
            var attendees = calendarEvent.Attendees != null ? GetAttendeesForEvent(calendarEvent.Attendees) : new List<CalendarEventAttendee>();
            if (calendarEvent.Organizer != null)
            {
                attendees.ToList().Insert(0, new CalendarEventAttendee
                {
                    Name = calendarEvent.Organizer.Name,
                    Email = calendarEvent.Organizer.Name,
                    Type = calendarEvent.Organizer.ParticipantRole.ToAttendeeType(),
                    IsOrganizer = true
                });
            }

            return new CalendarEvent
            {
                Id = calendarEvent.CalendarItemIdentifier,
                CalendarId = calendarEvent.Calendar.CalendarIdentifier,
                Title = calendarEvent.Title,
                Description = calendarEvent.Notes,
                Location = calendarEvent.Location,
                Url = calendarEvent.Url != null ? calendarEvent.Url.ToString() : string.Empty,
                StartDate = calendarEvent.StartDate.ToDateTimeOffsetWithTimeZone(calendarEvent.TimeZone),
                EndDate = !calendarEvent.AllDay ? (DateTimeOffset?)calendarEvent.EndDate.ToDateTimeOffsetWithTimeZone(calendarEvent.TimeZone) : null,
                Attendees = attendees,
                RecurrancePattern = recurrenceRule,
                Reminders = alarms
            };
        }

        static async Task<CalendarEvent> PlatformGetEventInstanceAsync(string eventId, DateTimeOffset instanceDate)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new ArgumentException($"[iOS]: No Event found for event Id {eventId}");
            }

            var calendarEvent = EventStore.GetCalendarItem(eventId) as EKEvent;
            var instanceOfEvent = (await GetEventsAsync(calendarEvent.Calendar.CalendarIdentifier, instanceDate, instanceDate.AddDays(1))).FirstOrDefault(x => x.Id == eventId);

            calendarEvent.StartDate = instanceOfEvent.StartDate.ToNSDate();
            calendarEvent.EndDate = instanceOfEvent.AllDay ? null : instanceOfEvent.EndDate.Value.ToNSDate();
            if (calendarEvent == null)
            {
                throw new ArgumentOutOfRangeException($"[iOS]: No Event found for event Id {eventId}");
            }

            RecurrenceRule recurrenceRule = null;
            if (calendarEvent.HasRecurrenceRules)
            {
                recurrenceRule = GetRecurrenceRule(calendarEvent.RecurrenceRules[0], calendarEvent.TimeZone);
            }
            List<CalendarEventReminder> alarms = null;
            if (calendarEvent.HasAlarms)
            {
                alarms = new List<CalendarEventReminder>();
                foreach (var a in calendarEvent.Alarms)
                {
                    alarms.Add(new CalendarEventReminder() { MinutesPriorToEventStart = (calendarEvent.StartDate.ToDateTimeOffsetWithTimeZone(calendarEvent.TimeZone) - a.AbsoluteDate.ToDateTimeOffsetWithTimeZone(calendarEvent.TimeZone)).Minutes });
                }
            }
            var attendees = calendarEvent.Attendees != null ? GetAttendeesForEvent(calendarEvent.Attendees) : new List<CalendarEventAttendee>();
            if (calendarEvent.Organizer != null)
            {
                attendees.ToList().Insert(0, new CalendarEventAttendee
                {
                    Name = calendarEvent.Organizer.Name,
                    Email = calendarEvent.Organizer.Name,
                    Type = calendarEvent.Organizer.ParticipantRole.ToAttendeeType(),
                    IsOrganizer = true
                });
            }
            return new CalendarEvent
            {
                Id = calendarEvent.CalendarItemIdentifier,
                CalendarId = calendarEvent.Calendar.CalendarIdentifier,
                Title = calendarEvent.Title,
                Description = calendarEvent.Notes,
                Location = calendarEvent.Location,
                Url = calendarEvent.Url != null ? calendarEvent.Url.ToString() : string.Empty,
                StartDate = calendarEvent.StartDate.ToDateTimeOffsetWithTimeZone(calendarEvent.TimeZone),
                EndDate = !calendarEvent.AllDay ? (DateTimeOffset?)calendarEvent.EndDate.ToDateTimeOffsetWithTimeZone(calendarEvent.TimeZone) : null,
                Attendees = attendees,
                RecurrancePattern = recurrenceRule,
                Reminders = alarms
            };
        }

        static RecurrenceRule GetRecurrenceRule(this EKRecurrenceRule iOSRule, NSTimeZone timeZone)
        {
            var recurrenceRule = new RecurrenceRule
            {
                Frequency = (RecurrenceFrequency)iOSRule.Frequency
            };

            if (iOSRule.DaysOfTheWeek != null)
            {
                recurrenceRule = iOSRule.DaysOfTheWeek.ConvertToCalendarDayOfWeekList(recurrenceRule);
            }
            recurrenceRule.Interval = (uint)iOSRule.Interval;

            if (iOSRule.SetPositions != null)
            {
                if (iOSRule.SetPositions.Length > 0)
                {
                    var day = iOSRule.SetPositions[0] as NSNumber;
                    recurrenceRule.WeekOfMonth = (IterationOffset)((int)day);
                    if (recurrenceRule.Frequency == RecurrenceFrequency.Monthly)
                    {
                        recurrenceRule.Frequency = RecurrenceFrequency.MonthlyOnDay;
                    }
                    else
                    {
                        recurrenceRule.Frequency = RecurrenceFrequency.YearlyOnDay;
                    }
                }
            }

            if (iOSRule.DaysOfTheMonth != null)
            {
                if (iOSRule.DaysOfTheMonth.Count() > 0)
                {
                    recurrenceRule.DayOfTheMonth = (uint)iOSRule.DaysOfTheMonth?.FirstOrDefault();
                }
            }

            if (iOSRule.MonthsOfTheYear != null)
            {
                if (iOSRule.MonthsOfTheYear.Count() > 0)
                {
                    recurrenceRule.MonthOfTheYear = (MonthOfYear)(uint)iOSRule.MonthsOfTheYear?.FirstOrDefault();
                }
            }

            recurrenceRule.EndDate = iOSRule.RecurrenceEnd?.EndDate?.ToDateTimeOffsetWithTimeZone(timeZone);

            recurrenceRule.TotalOccurrences = (uint?)iOSRule.RecurrenceEnd?.OccurrenceCount;

            return recurrenceRule;
        }

        static RecurrenceRule ConvertToCalendarDayOfWeekList(this EKRecurrenceDayOfWeek[] recurrenceDays, RecurrenceRule rule)
        {
            var enumValues = Enum.GetValues(typeof(CalendarDayOfWeek));

            rule.DaysOfTheWeek = recurrenceDays.ToList().Select(x => (CalendarDayOfWeek)enumValues.GetValue(Convert.ToInt32(x.DayOfTheWeek))).ToList();

            foreach (var day in recurrenceDays)
            {
                if (day.WeekNumber != 0)
                {
                    if (rule.Frequency == RecurrenceFrequency.Monthly)
                    {
                        rule.Frequency = RecurrenceFrequency.MonthlyOnDay;
                    }
                    else
                    {
                        rule.Frequency = RecurrenceFrequency.YearlyOnDay;
                    }
                    rule.WeekOfMonth = (IterationOffset)(int)(day.WeekNumber - 1);
                }
            }
            return rule;
        }

        static IEnumerable<CalendarEventAttendee> GetAttendeesForEvent(IEnumerable<EKParticipant> inviteList)
        {
            var attendees = (from attendee in inviteList
                             select new CalendarEventAttendee
                             {
                                 Name = attendee.Name,
                                 Email = attendee.Name,
                                 Type = attendee.ParticipantRole.ToAttendeeType()
                             })
                            .OrderBy(attendee => attendee.Name)
                            .ToList();

            return attendees;
        }

        static AttendeeType ToAttendeeType(this EKParticipantRole role)
        {
            switch (role)
            {
                case EKParticipantRole.Required:
                    return AttendeeType.Required;
                case EKParticipantRole.Optional:
                    return AttendeeType.Optional;
                case EKParticipantRole.NonParticipant:
                case EKParticipantRole.Chair:
                    return AttendeeType.Resource;
                case EKParticipantRole.Unknown:
                default:
                    return AttendeeType.None;
            }
        }

        static async Task<string> PlatformCreateCalendarEvent(CalendarEvent newEvent)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            if (string.IsNullOrEmpty(newEvent.CalendarId))
            {
                return string.Empty;
            }

            var calendarEvent = EKEvent.FromStore(EventStore);
            calendarEvent = SetUpEvent(calendarEvent, newEvent);
            if (EventStore.SaveEvent(calendarEvent, EKSpan.FutureEvents, true, out _))
            {
                return calendarEvent.EventIdentifier;
            }
            throw new ArgumentException("[iOS]: Could not create appointment with supplied parameters");
        }

        static async Task<bool> PlatformUpdateCalendarEvent(CalendarEvent eventToUpdate)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var existingEvent = await GetEventAsync(eventToUpdate.Id);
            EKEvent thisEvent;
            if (string.IsNullOrEmpty(eventToUpdate.CalendarId) || existingEvent == null)
            {
                return false;
            }
            else if (existingEvent.CalendarId != eventToUpdate.CalendarId)
            {
                await DeleteCalendarEvent(existingEvent.Id, existingEvent.CalendarId);
                thisEvent = EKEvent.FromStore(EventStore);
            }
            else
            {
                thisEvent = EventStore.GetCalendarItem(eventToUpdate.Id) as EKEvent;
            }

            thisEvent = SetUpEvent(thisEvent, eventToUpdate);
            if (EventStore.SaveEvent(thisEvent, EKSpan.FutureEvents, true, out _))
            {
                return true;
            }
            throw new ArgumentException("[iOS]: Could not update appointment with supplied parameters");
        }

        static async Task<bool> PlatformSetEventRecurrenceEndDate(string eventId, DateTimeOffset recurrenceEndDate)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var existingEvent = await GetEventAsync(eventId);
            if (string.IsNullOrEmpty(existingEvent?.CalendarId))
            {
                return false;
            }
            var thisEvent = EventStore.GetCalendarItem(eventId) as EKEvent;

            existingEvent.RecurrancePattern.EndDate = recurrenceEndDate;
            existingEvent.RecurrancePattern.TotalOccurrences = null;
            thisEvent = SetUpEvent(thisEvent, existingEvent);
            if (!EventStore.SaveEvent(thisEvent, EKSpan.FutureEvents, true, out _))
            {
                throw new ArgumentException("[iOS]: Could not update appointments recurrence dates with supplied parameters");
            }
            return true;
        }

        static EKEvent SetUpEvent(EKEvent eventToUpdate, CalendarEvent eventToUpdateFrom)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(NSTimeZone.LocalTimeZone.Name);
            eventToUpdate.Title = eventToUpdateFrom.Title;
            eventToUpdate.Calendar = EventStore.GetCalendar(eventToUpdateFrom.CalendarId);
            eventToUpdate.Notes = eventToUpdateFrom.Description;
            eventToUpdate.Location = eventToUpdateFrom.Location;
            eventToUpdate.AllDay = eventToUpdateFrom.AllDay;
            eventToUpdate.StartDate = TimeZoneInfo.ConvertTime(eventToUpdateFrom.StartDate, timeZoneInfo).ToNSDate();
            eventToUpdate.TimeZone = NSTimeZone.LocalTimeZone;
            eventToUpdate.Url = !string.IsNullOrWhiteSpace(eventToUpdateFrom.Url) ? new NSUrl(eventToUpdateFrom.Url) : null;
            eventToUpdate.EndDate = eventToUpdateFrom.EndDate.HasValue ? TimeZoneInfo.ConvertTime(eventToUpdateFrom.EndDate.Value, timeZoneInfo).ToNSDate() : TimeZoneInfo.ConvertTime(eventToUpdateFrom.StartDate, timeZoneInfo).AddDays(1).ToNSDate();
            if (eventToUpdateFrom.RecurrancePattern != null && eventToUpdateFrom.RecurrancePattern.Frequency != null)
            {
                eventToUpdate.RecurrenceRules = new EKRecurrenceRule[1] { eventToUpdateFrom.RecurrancePattern.ConvertRule() };
            }
            return eventToUpdate;
        }

        static EKRecurrenceFrequency ConvertToiOS(this RecurrenceFrequency? recurrenceFrequency)
        {
            switch (recurrenceFrequency)
            {
                case RecurrenceFrequency.Daily:
                    return EKRecurrenceFrequency.Daily;
                case RecurrenceFrequency.Weekly:
                    return EKRecurrenceFrequency.Weekly;
                case RecurrenceFrequency.Monthly:
                case RecurrenceFrequency.MonthlyOnDay:
                    return EKRecurrenceFrequency.Monthly;
                case RecurrenceFrequency.Yearly:
                case RecurrenceFrequency.YearlyOnDay:
                    return EKRecurrenceFrequency.Yearly;
                default:
                    return EKRecurrenceFrequency.Daily;
            }
        }

        static EKRecurrenceDayOfWeek[] ConvertToiOS(this List<CalendarDayOfWeek> daysOfTheWeek)
        {
            if (daysOfTheWeek == null || !daysOfTheWeek.Any())
                return null;

            var toReturn = new List<EKRecurrenceDayOfWeek>();
            foreach (var day in daysOfTheWeek)
            {
                toReturn.Add(EKRecurrenceDayOfWeek.FromDay(day.ConvertToiOS()));
            }
            return toReturn.ToArray();
        }

        static NSNumber[] ConvertToiOS(this int dayOfTheMonth) => new NSNumber[1] { dayOfTheMonth };

        static EKDay ConvertToiOS(this CalendarDayOfWeek day) => (EKDay)Math.Log((int)day, 2);

        static EKRecurrenceRule ConvertRule(this RecurrenceRule recurrenceRule) => new EKRecurrenceRule(
                type: recurrenceRule.Frequency.ConvertToiOS(),
                interval: (nint)recurrenceRule.Interval,
                days: recurrenceRule.Frequency != RecurrenceFrequency.Daily ? recurrenceRule.DaysOfTheWeek.ConvertToiOS() : null,
                monthDays: (recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0) ? null : ((int)recurrenceRule.DayOfTheMonth).ConvertToiOS(),
                months: recurrenceRule.Frequency == RecurrenceFrequency.Yearly ? ((int)recurrenceRule.MonthOfTheYear).ConvertToiOS() : null,
                weeksOfTheYear: null,
                daysOfTheYear: null,
                setPositions: recurrenceRule.Frequency == RecurrenceFrequency.Yearly || recurrenceRule.Frequency == RecurrenceFrequency.Monthly ? ((int)recurrenceRule.WeekOfMonth).ConvertToiOS() : null,
                end: recurrenceRule.EndDate.HasValue ? EKRecurrenceEnd.FromEndDate(TimeZoneInfo.ConvertTime(recurrenceRule.EndDate.Value, TimeZoneInfo.Local).ToNSDate()) : recurrenceRule.TotalOccurrences.HasValue ? EKRecurrenceEnd.FromOccurrenceCount((nint)recurrenceRule.TotalOccurrences.Value) : null);

        static async Task<bool> PlatformDeleteCalendarEventInstanceByDate(string eventId, string calendarId, DateTimeOffset dateOfInstanceUtc)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException("[iOS]: You must supply an event id to delete an event.");
            }
            var calendar = NativeGetCalendar(calendarId);
            var query = EventStore.PredicateForEvents(dateOfInstanceUtc.ToNSDate(), dateOfInstanceUtc.AddDays(1).ToNSDate(), new[] { calendar });
            var events = EventStore.EventsMatching(query);
            var thisEvent = events.FirstOrDefault(x => x.CalendarItemIdentifier == eventId);

            if ((thisEvent?.Calendar.CalendarIdentifier ?? string.Empty) != calendarId)
            {
                throw new ArgumentOutOfRangeException("[iOS]: Supplied event does not belong to supplied calendar.");
            }

            if (EventStore.RemoveEvent(thisEvent, EKSpan.ThisEvent, true, out var error))
            {
                return true;
            }
            throw new Exception(error.DebugDescription);
        }

        static async Task<bool> PlatformDeleteCalendarEvent(string eventId, string calendarId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException("[iOS]: You must supply an event id to delete an event.");
            }

            var calendarEvent = EventStore.GetCalendarItem(eventId) as EKEvent;

            if ((calendarEvent?.Calendar.CalendarIdentifier ?? string.Empty) != calendarId)
            {
                throw new ArgumentOutOfRangeException("[iOS]: Supplied event does not belong to supplied calendar.");
            }

            if (EventStore.RemoveEvent(calendarEvent, EKSpan.FutureEvents, true, out var error))
            {
                return true;
            }
            throw new Exception(error.DebugDescription);
        }

        static async Task<string> PlatformCreateCalendar(Calendar newCalendar)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var calendar = EKCalendar.Create(EKEntityType.Event, EventStore);
            calendar.Title = newCalendar.Name;
            var source = EventStore.Sources.FirstOrDefault(x => x.SourceType == EKSourceType.Local);
            calendar.Source = source;

            if (EventStore.SaveCalendar(calendar, true, out var error))
            {
                return calendar.CalendarIdentifier;
            }
            throw new Exception(error.DebugDescription);
        }

#pragma warning disable IDE0060 // Remove unused parameter - placeholder methods
        static Task<bool> PlatformAddAttendeeToEvent(CalendarEventAttendee newAttendee, string eventId) => throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<bool> PlatformRemoveAttendeeFromEvent(CalendarEventAttendee newAttendee, string eventId) => throw ExceptionUtils.NotSupportedOrImplementedException;
#pragma warning restore IDE0060 // Remove unused parameter

        // Not possible at this point in time from what I've found - https://stackoverflow.com/questions/28826222/add-invitees-to-calendar-event-programmatically-ios
        // The following code is some initial experimentation to add an attendee following a similar approach
        // to https://github.com/builttoroam/device_calendar/blob/b14d1fde51956906ebe9593df4388fc3a1e26907/ios/Classes/SwiftDeviceCalendarPlugin.swift#L648-L658
        /*
        static async Task<bool> PlatformAddAttendeeToEvent(CalendarEventAttendee newAttendee, string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var attendee = ObjCRuntime.Class.GetHandle("EKAttendee");

            var attendeeObject = ObjCRuntime.Runtime.GetNSObject(attendee);
            var email = new NSString("emailAddress");

            // tst.Init();
            attendeeObject.SetValueForKey(new NSString(newAttendee.Email), email);

            var result = attendeeObject as EKParticipant;
            return true;
        }

        static async Task<bool> PlatformRemoveAttendeeFromEvent(CalendarEventAttendee newAttendee, string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var calendarEvent = EventStore.GetCalendarItem(eventId) as EKEvent;

            var calendarEventAttendees = calendarEvent.Attendees.ToList();
            calendarEventAttendees.RemoveAll(x => x.Name == newAttendee.Name);

            // calendarEvent.Attendees = calendarEventAttendees; - readonly cannot be done at this stage.

            if (EventStore.SaveEvent(calendarEvent, EKSpan.FutureEvents, true, out var error))
            {
                return true;
            }
            throw new Exception(error.DebugDescription);
        }*/

        static IEnumerable<Calendar> ToCalendars(IEnumerable<EKCalendar> native)
        {
            foreach (var calendar in native)
            {
                yield return ToCalendar(calendar);
            }
        }

        static Calendar ToCalendar(EKCalendar calendar) =>
            new Calendar
            {
                Id = calendar.CalendarIdentifier,
                Name = calendar.Title,
                IsReadOnly = !calendar.AllowsContentModifications
            };

        static IEnumerable<CalendarEvent> ToEvents(IEnumerable<EKEvent> native)
        {
            foreach (var e in native)
            {
                yield return ToEvent(e);
            }
        }

        static CalendarEvent ToEvent(EKEvent native) =>
            new CalendarEvent
            {
                Id = native.CalendarItemIdentifier,
                CalendarId = native.Calendar.CalendarIdentifier,
                Title = native.Title,
                Description = native.Notes,
                Location = native.Location,
                AllDay = native.AllDay,
                StartDate = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(native.StartDate.SecondsSince1970),
                EndDate = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(native.EndDate.SecondsSince1970),
                Attendees = native.Attendees != null
                    ? ToAttendees(native.Attendees).ToList()
                    : new List<CalendarEventAttendee>()
            };

        static IEnumerable<CalendarEventAttendee> ToAttendees(IEnumerable<EKParticipant> inviteList)
        {
            foreach (var attendee in inviteList)
            {
                yield return new CalendarEventAttendee
                {
                    Name = attendee.Name,
                    Email = attendee.Name
                };
            }
        }
    }
}
