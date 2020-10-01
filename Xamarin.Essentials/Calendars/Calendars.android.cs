using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Content;
using Android.Database;
using Android.Provider;
using Java.Security;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        const string andCondition = "AND";
        const string dailyFrequency = "DAILY";
        const string weeklyFrequency = "WEEKLY";
        const string monthlyFrequency = "MONTHLY";
        const string yearlyFrequency = "YEARLY";
        const string byFrequencySearch = "FREQ=";
        const string byDaySearch = "BYDAY=";
        const string byMonthDaySearch = "BYMONTHDAY=";
        const string byMonthSearch = "BYMONTH=";
        const string bySetPosSearch = "BYSETPOS=";
        const string byIntervalSearch = "INTERVAL=";
        const string byCountSearch = "COUNT=";
        const string byUntilSearch = "UNTIL=";

        static async Task<IEnumerable<Calendar>> PlatformGetCalendarsAsync()
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var calendarsUri = CalendarContract.Calendars.ContentUri;
            var calendarsProjection = new List<string>
            {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName
            };
            var queryConditions = $"{CalendarContract.Calendars.InterfaceConsts.Deleted} != 1";

            using (var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver.Query(calendarsUri, calendarsProjection.ToArray(), queryConditions, null, null))
            {
                var calendars = new List<Calendar>();
                while (currentContextContentResolver.MoveToNext())
                {
                    calendars.Add(new Calendar()
                    {
                        Id = currentContextContentResolver.GetString(calendarsProjection.IndexOf(CalendarContract.Calendars.InterfaceConsts.Id)),
                        IsReadOnly = IsCalendarReadOnly((CalendarAccess)currentContextContentResolver.GetInt(calendarsProjection.IndexOf(CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel))),
                        Name = currentContextContentResolver.GetString(calendarsProjection.IndexOf(CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName)),
                    });
                }
                return calendars;
            }
        }

        static bool IsCalendarReadOnly(CalendarAccess accessLevel)
        {
            switch (accessLevel)
            {
                case CalendarAccess.AccessContributor:
                case CalendarAccess.AccessRoot:
                case CalendarAccess.AccessOwner:
                case CalendarAccess.AccessEditor:
                    return true;
                default:
                    return false;
            }
        }

        static async Task<IEnumerable<CalendarEvent>> PlatformGetEventsAsync(string calendarId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var sDate = startDate ?? DateTimeOffset.Now.Add(defaultStartTimeFromNow);
            var eDate = endDate ?? sDate.Add(defaultEndTimeFromStartTime);

            var eventsProjection = new List<string>
            {
                CalendarContract.Instances.EventId,
                CalendarContract.Instances.Begin,
                CalendarContract.Instances.End,
                CalendarContract.Events.InterfaceConsts.EventTimezone,
                CalendarContract.Events.InterfaceConsts.EventEndTimezone,
                CalendarContract.Events.InterfaceConsts.CalendarId,
                CalendarContract.Events.InterfaceConsts.Title
            };
            var instanceUriBuilder = CalendarContract.Instances.ContentUri.BuildUpon();
            ContentUris.AppendId(instanceUriBuilder, sDate.AddMilliseconds(sDate.Offset.TotalMilliseconds).ToUnixTimeMilliseconds());
            ContentUris.AppendId(instanceUriBuilder, eDate.AddMilliseconds(eDate.Offset.TotalMilliseconds).ToUnixTimeMilliseconds());

            var instancesUri = instanceUriBuilder.Build();
            var calendarSpecificEvent = string.Empty;

            if (!string.IsNullOrEmpty(calendarId))
            {
                // Android event ids are always integers
                if (!int.TryParse(calendarId, out var resultId))
                {
                    throw new ArgumentException($"[Android]: No Event found for event Id {calendarId}");
                }
                calendarSpecificEvent = $"{CalendarContract.Events.InterfaceConsts.CalendarId} = {resultId} {andCondition} ";
            }
            calendarSpecificEvent += $"{CalendarContract.Events.InterfaceConsts.Deleted} != 1";

            var instances = new List<CalendarEvent>();
            using (var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver.Query(instancesUri, eventsProjection.ToArray(), calendarSpecificEvent, null, $"{CalendarContract.Instances.Begin} ASC"))
            {
                while (currentContextContentResolver.MoveToNext())
                {
                    var instanceStartTZ = TimeZoneInfo.FindSystemTimeZoneById(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventTimezone)));
                    var eventStartDate = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(currentContextContentResolver.GetLong(eventsProjection.IndexOf(CalendarContract.Instances.Begin))), instanceStartTZ);
                    var instanceEndTZ = TimeZoneInfo.FindSystemTimeZoneById(!string.IsNullOrEmpty(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventEndTimezone))) ? currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventEndTimezone)) : currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventTimezone)));
                    var eventEndDate = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(currentContextContentResolver.GetLong(eventsProjection.IndexOf(CalendarContract.Instances.End))), instanceEndTZ);

                    instances.Add(new CalendarEvent()
                    {
                        Id = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Instances.EventId)),
                        CalendarId = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.CalendarId)),
                        Title = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Title)),
                        StartDate = eventStartDate,
                        EndDate = eventEndDate
                    });
                }
            }
            if (!instances.Any() && !string.IsNullOrEmpty(calendarId))
            {
                // Make sure this calendar exists by testing retrieval
                try
                {
                    await PlatformGetCalendarAsync(calendarId);
                }
                catch (Exception)
                {
                    throw new ArgumentOutOfRangeException($"[Android]: No calendar exists with the Id {calendarId}");
                }
            }
            return instances;
        }

        static Task<Calendar> PlatformGetCalendarAsync(string calendarId)
        {
            var calendarsUri = CalendarContract.Calendars.ContentUri;
            var calendarsProjection = new List<string>
            {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName
            };

            // Android event ids are always integers
            if (!int.TryParse(calendarId, out var resultId))
            {
                throw new ArgumentException($"[Android]: No Event found for event Id {calendarId}");
            }

            var queryConditions = $"{CalendarContract.Calendars.InterfaceConsts.Deleted} != 1 {andCondition} {CalendarContract.Calendars.InterfaceConsts.Id} = {resultId}";

            using (var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver.Query(calendarsUri, calendarsProjection.ToArray(), queryConditions, null, null))
            {
                if (currentContextContentResolver.Count > 0)
                {
                    currentContextContentResolver.MoveToNext();
                    return Task.FromResult(new Calendar()
                    {
                        Id = currentContextContentResolver.GetString(calendarsProjection.IndexOf(CalendarContract.Calendars.InterfaceConsts.Id)),
                        Name = currentContextContentResolver.GetString(calendarsProjection.IndexOf(CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName)),
                    });
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"[Android]: No calendar exists with the Id {calendarId}");
                }
            }
        }

        static async Task<CalendarEvent> PlatformGetEventAsync(string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var eventsUri = CalendarContract.Events.ContentUri;
            var eventsProjection = new List<string>
            {
                CalendarContract.Events.InterfaceConsts.Id,
                CalendarContract.Events.InterfaceConsts.CalendarId,
                CalendarContract.Events.InterfaceConsts.Title,
                CalendarContract.Events.InterfaceConsts.Description,
                CalendarContract.Events.InterfaceConsts.EventLocation,
                CalendarContract.Events.InterfaceConsts.CustomAppUri,
                CalendarContract.Events.InterfaceConsts.AllDay,
                CalendarContract.Events.InterfaceConsts.Dtstart,
                CalendarContract.Events.InterfaceConsts.Dtend,
                CalendarContract.Events.InterfaceConsts.Rrule,
                CalendarContract.Events.InterfaceConsts.Rdate,
                CalendarContract.Events.InterfaceConsts.Organizer,
                CalendarContract.Events.InterfaceConsts.EventTimezone,
                CalendarContract.Events.InterfaceConsts.EventEndTimezone,
            };

            // Android event ids are always integers
            if (!int.TryParse(eventId, out var resultId))
            {
                throw new ArgumentException($"[Android]: No Event found for event Id {eventId}");
            }

            var calendarSpecificEvent = $"{CalendarContract.Events.InterfaceConsts.Id}={resultId}";
            using (var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver.Query(eventsUri, eventsProjection.ToArray(), calendarSpecificEvent, null, null))
            {
                if (currentContextContentResolver.Count > 0)
                {
                    currentContextContentResolver.MoveToNext();
                    var instanceStartTZ = TimeZoneInfo.FindSystemTimeZoneById(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventTimezone)));
                    var eventStartDate = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(currentContextContentResolver.GetLong(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Dtstart))), instanceStartTZ);
                    var instanceEndTZ = TimeZoneInfo.FindSystemTimeZoneById(!string.IsNullOrEmpty(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventEndTimezone))) ? currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventEndTimezone)) : currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventTimezone)));
                    var eventEndDate = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(currentContextContentResolver.GetLong(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Dtend))), instanceEndTZ);

                    var eventResult = new CalendarEvent
                    {
                        Id = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Id)),
                        CalendarId = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.CalendarId)),
                        Title = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Title)),
                        Description = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Description)),
                        Location = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventLocation)),
                        Url = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.CustomAppUri)),
                        StartDate = eventStartDate,
                        EndDate = eventEndDate,
                        Attendees = GetAttendeesForEvent(eventId, currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Organizer))),
                        RecurrancePattern = !string.IsNullOrEmpty(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Rrule))) ? GetRecurranceRuleForEvent(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Rrule))) : null,
                        Reminders = GetRemindersForEvent(eventId)
                    };
                    return eventResult;
                }
                throw new ArgumentOutOfRangeException($"[Android]: No Event found for event Id {eventId}");
            }
        }

        static async Task<CalendarEvent> PlatformGetEventInstanceAsync(string eventId, DateTimeOffset instance)
        {
            await Permissions.RequestAsync<Permissions.CalendarRead>();

            var eventsProjection = new List<string>
            {
                CalendarContract.Instances.EventId,
                CalendarContract.Instances.Begin,
                CalendarContract.Instances.End,
                CalendarContract.Events.InterfaceConsts.CalendarId,
                CalendarContract.Events.InterfaceConsts.Title,
                CalendarContract.Events.InterfaceConsts.Description,
                CalendarContract.Events.InterfaceConsts.EventLocation,
                CalendarContract.Events.InterfaceConsts.CustomAppUri,
                CalendarContract.Events.InterfaceConsts.AllDay,
                CalendarContract.Events.InterfaceConsts.Dtstart,
                CalendarContract.Events.InterfaceConsts.Dtend,
                CalendarContract.Events.InterfaceConsts.Rrule,
                CalendarContract.Events.InterfaceConsts.Rdate,
                CalendarContract.Events.InterfaceConsts.Organizer,
                CalendarContract.Events.InterfaceConsts.EventTimezone,
                CalendarContract.Events.InterfaceConsts.EventEndTimezone,
            };
            var instanceUriBuilder = CalendarContract.Instances.ContentUri.BuildUpon();
            ContentUris.AppendId(instanceUriBuilder, instance.AddDays(-1).AddMilliseconds(instance.Offset.TotalMilliseconds).ToUnixTimeMilliseconds());
            ContentUris.AppendId(instanceUriBuilder, instance.AddMilliseconds(instance.Offset.TotalMilliseconds).ToUnixTimeMilliseconds());

            var instancesUri = instanceUriBuilder.Build();
            var calendarSpecificEvent = $"{CalendarContract.Instances.EventId} = {eventId}";

            using (var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver.Query(instancesUri, eventsProjection.ToArray(), calendarSpecificEvent, null, $"{CalendarContract.Instances.Begin} ASC"))
            {
                if (currentContextContentResolver.MoveToFirst())
                {
                    var instanceStartTZ = TimeZoneInfo.FindSystemTimeZoneById(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventTimezone)));
                    var eventStartDate = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(currentContextContentResolver.GetLong(eventsProjection.IndexOf(CalendarContract.Instances.Begin))), instanceStartTZ);
                    var instanceEndTZ = TimeZoneInfo.FindSystemTimeZoneById(!string.IsNullOrEmpty(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventEndTimezone))) ? currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventEndTimezone)) : currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventTimezone)));
                    var eventEndDate = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(currentContextContentResolver.GetLong(eventsProjection.IndexOf(CalendarContract.Instances.End))), instanceEndTZ);
                    return new CalendarEvent
                    {
                        Id = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Instances.EventId)),
                        CalendarId = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.CalendarId)),
                        Title = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Title)),
                        Description = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Description)),
                        Location = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.EventLocation)),
                        Url = currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.CustomAppUri)),
                        StartDate = eventStartDate,
                        EndDate = eventEndDate,
                        Attendees = GetAttendeesForEvent(eventId, currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Organizer))),
                        RecurrancePattern = !string.IsNullOrEmpty(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Rrule))) ? GetRecurranceRuleForEvent(currentContextContentResolver.GetString(eventsProjection.IndexOf(CalendarContract.Events.InterfaceConsts.Rrule))) : null,
                        Reminders = GetRemindersForEvent(eventId)
                    };
                }
                throw new ArgumentOutOfRangeException($"[Android]: No Event found for event Id {eventId}");
            }
        }

        static IEnumerable<CalendarEventAttendee> GetAttendeesForEvent(string eventId, string organizer)
        {
            var attendeesUri = CalendarContract.Attendees.ContentUri;
            var attendeesProjection = new List<string>
            {
                CalendarContract.Attendees.InterfaceConsts.EventId,
                CalendarContract.Attendees.InterfaceConsts.AttendeeEmail,
                CalendarContract.Attendees.InterfaceConsts.AttendeeName,
                CalendarContract.Attendees.InterfaceConsts.AttendeeType
            };
            var attendeeSpecificAttendees = $"{CalendarContract.Attendees.InterfaceConsts.EventId}={eventId}";
            var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver.Query(attendeesUri, attendeesProjection.ToArray(), attendeeSpecificAttendees, null, null);
            var attendees = new List<CalendarEventAttendee>();
            while (currentContextContentResolver.MoveToNext())
            {
                attendees.Add(new CalendarEventAttendee()
                {
                    Name = currentContextContentResolver.GetString(attendeesProjection.IndexOf(CalendarContract.Attendees.InterfaceConsts.AttendeeName)),
                    Email = currentContextContentResolver.GetString(attendeesProjection.IndexOf(CalendarContract.Attendees.InterfaceConsts.AttendeeEmail)),
                    Type = (AttendeeType)currentContextContentResolver.GetInt(attendeesProjection.IndexOf(CalendarContract.Attendees.InterfaceConsts.AttendeeType)),
                    IsOrganizer = currentContextContentResolver.GetString(attendeesProjection.IndexOf(CalendarContract.Attendees.InterfaceConsts.AttendeeEmail)) == organizer
                });
            }
            currentContextContentResolver.Dispose();
            return attendees.OrderByDescending(x => x.IsOrganizer);
        }

        static IEnumerable<CalendarEventReminder> GetRemindersForEvent(string eventId)
        {
            var remindersUri = CalendarContract.Reminders.ContentUri;
            var remindersProjection = new List<string>
            {
                CalendarContract.Reminders.InterfaceConsts.EventId,
                CalendarContract.Reminders.InterfaceConsts.Minutes
            };
            var remindersSpecificAttendees = $"{CalendarContract.Reminders.InterfaceConsts.EventId}={eventId}";
            var reminders = new List<CalendarEventReminder>();
            using (var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver.Query(remindersUri, remindersProjection.ToArray(), remindersSpecificAttendees, null, null))
            {
                while (currentContextContentResolver.MoveToNext())
                {
                    reminders.Add(new CalendarEventReminder()
                    {
                        MinutesPriorToEventStart = currentContextContentResolver.GetInt(remindersProjection.IndexOf(CalendarContract.Reminders.InterfaceConsts.Minutes))
                    });
                }
            }
            return reminders;
        }

        static async Task<string> PlatformCreateCalendar(Calendar newCalendar)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var calendarUri = CalendarContract.Calendars.ContentUri;
            var currentContextContentResolver = Platform.AppContext.ApplicationContext.ContentResolver;
            var calendarValues = new ContentValues();
            calendarValues.Put(CalendarContract.Calendars.InterfaceConsts.AccountName, "Xamarin.Essentials.Calendar");
            calendarValues.Put(CalendarContract.Calendars.InterfaceConsts.AccountType, CalendarContract.AccountTypeLocal);
            calendarValues.Put(CalendarContract.Calendars.Name, newCalendar.Name);
            calendarValues.Put(CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName, newCalendar.Name);
            calendarValues.Put(CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel, CalendarAccess.AccessOwner.ToString());
            calendarValues.Put(CalendarContract.Calendars.InterfaceConsts.Visible, true);
            calendarValues.Put(CalendarContract.Calendars.InterfaceConsts.SyncEvents, true);
            calendarUri = calendarUri.BuildUpon()
                    .AppendQueryParameter(CalendarContract.CallerIsSyncadapter, "true")
                    .AppendQueryParameter(CalendarContract.Calendars.InterfaceConsts.AccountName, "Xamarin.Essentials.Calendar")
                    .AppendQueryParameter(CalendarContract.Calendars.InterfaceConsts.AccountType, CalendarContract.AccountTypeLocal)
                    .Build();
            var result = currentContextContentResolver.Insert(calendarUri, calendarValues);
            return result.ToString();
        }

        static async Task<string> PlatformCreateCalendarEvent(CalendarEvent newEvent)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var result = 0;
            if (string.IsNullOrEmpty(newEvent.CalendarId))
            {
                return string.Empty;
            }
            var eventUri = CalendarContract.Events.ContentUri;
            var eventValues = SetupContentValues(newEvent);

            var resultUri = Platform.AppContext.ApplicationContext.ContentResolver.Insert(eventUri, eventValues);
            if (int.TryParse(resultUri?.LastPathSegment, out result))
            {
                return result.ToString();
            }
            throw new ArgumentException("[Android]: Could not create appointment with supplied parameters");
        }

        static async Task<bool> PlatformUpdateCalendarEvent(CalendarEvent eventToUpdate)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var thisEvent = await GetEventAsync(eventToUpdate.Id);

            var eventUri = CalendarContract.Events.ContentUri;
            var eventValues = SetupContentValues(eventToUpdate, true);

            if (string.IsNullOrEmpty(eventToUpdate.CalendarId) || thisEvent == null)
            {
                return false;
            }
            else if (thisEvent.CalendarId != eventToUpdate.CalendarId)
            {
                await DeleteCalendarEvent(thisEvent.Id, thisEvent.CalendarId);
                var resultUri = Platform.AppContext.ApplicationContext.ContentResolver.Insert(eventUri, eventValues);
                if (int.TryParse(resultUri?.LastPathSegment, out var result))
                {
                    return true;
                }

                return false;
            }
            else if (Platform.AppContext.ApplicationContext.ContentResolver.Update(eventUri, eventValues, $"{CalendarContract.Attendees.InterfaceConsts.Id}={eventToUpdate.Id}", null) > 0)
            {
                return true;
            }
            throw new ArgumentException("[Android]: Could not update appointment with supplied parameters");
        }

        static async Task<bool> PlatformSetEventRecurrenceEndDate(string eventId, DateTimeOffset recurrenceEndDate)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var existingEvent = await GetEventAsync(eventId);
            if (string.IsNullOrEmpty(existingEvent?.CalendarId))
            {
                return false;
            }
            var thisEvent = await GetEventAsync(eventId);

            thisEvent.RecurrancePattern.EndDate = recurrenceEndDate;
            thisEvent.RecurrancePattern.TotalOccurrences = null;

            var eventUri = CalendarContract.Events.ContentUri;
            var eventValues = SetupContentValues(thisEvent);

            if (!(Platform.AppContext.ApplicationContext.ContentResolver.Update(eventUri, eventValues, $"{CalendarContract.Attendees.InterfaceConsts.Id}={eventId}", null) > 0))
            {
                throw new ArgumentException("[Android]: Could not update appointment with supplied parameters");
            }
            return true;
        }

        static ContentValues SetupContentValues(CalendarEvent newEvent, bool existingEvent = false)
        {
            var eventValues = new ContentValues();
            eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId, newEvent.CalendarId);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Title, newEvent.Title);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Description, newEvent.Description);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventLocation, newEvent.Location);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.CustomAppUri, newEvent.Url);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.AllDay, newEvent.AllDay);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart, newEvent.StartDate.ToUnixTimeMilliseconds().ToString());
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend, newEvent.EndDate.HasValue ? newEvent.EndDate.Value.ToUnixTimeMilliseconds().ToString() : newEvent.StartDate.AddDays(1).ToUnixTimeMilliseconds().ToString());
            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, TimeZoneInfo.Local.Id);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, TimeZoneInfo.Local.Id);
            if (newEvent.RecurrancePattern != null)
            {
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Rrule, newEvent.RecurrancePattern.ConvertRule());
            }
            else if (existingEvent)
            {
                eventValues.PutNull(CalendarContract.Events.InterfaceConsts.Rrule);
                eventValues.PutNull(CalendarContract.Events.InterfaceConsts.Duration);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Deleted, 0);
            }

            return eventValues;
        }

        static async Task<bool> PlatformDeleteCalendarEventInstanceByDate(string eventId, string calendarId, DateTimeOffset dateOfInstanceUtc)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var thisEvent = await GetEventInstanceAsync(eventId, dateOfInstanceUtc);

            var eventUri = ContentUris.WithAppendedId(CalendarContract.Events.ContentExceptionUri, long.Parse(eventId));

            var eventValues = new ContentValues();
            eventValues.Put(CalendarContract.Events.InterfaceConsts.OriginalInstanceTime, thisEvent.StartDate.ToUnixTimeMilliseconds());
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Status, (int)EventsStatus.Canceled);

            var resultUri = Platform.AppContext.ApplicationContext.ContentResolver.Insert(eventUri, eventValues);
            if (int.TryParse(resultUri?.LastPathSegment, out var result))
            {
                return result > 0;
            }
            return false;
        }

        static async Task<bool> PlatformDeleteCalendarEvent(string eventId, string calendarId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException("[Android]: You must supply an event id to delete an event.");
            }

            var calendarEvent = await GetEventAsync(eventId);

            if (calendarEvent.CalendarId != calendarId)
            {
                throw new ArgumentOutOfRangeException("[Android]: Supplied event does not belong to supplied calendar");
            }

            var eventUri = ContentUris.WithAppendedId(CalendarContract.Events.ContentUri, long.Parse(eventId));
            var result = Platform.AppContext.ApplicationContext.ContentResolver.Delete(eventUri, null, null);

            return result > 0;
        }

        static async Task<bool> PlatformAddAttendeeToEvent(CalendarEventAttendee newAttendee, string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var calendarEvent = await GetEventAsync(eventId);

            if (calendarEvent == null)
            {
                throw new ArgumentException("[Android]: You must supply a valid event id to add an attendee to an event.");
            }

            var attendeeUri = CalendarContract.Attendees.ContentUri;
            var attendeeValues = new ContentValues();

            attendeeValues.Put(CalendarContract.Attendees.InterfaceConsts.EventId, eventId);
            attendeeValues.Put(CalendarContract.Attendees.InterfaceConsts.AttendeeEmail, newAttendee.Email);
            attendeeValues.Put(CalendarContract.Attendees.InterfaceConsts.AttendeeName, newAttendee.Name);
            attendeeValues.Put(CalendarContract.Attendees.InterfaceConsts.AttendeeType, (int)newAttendee.Type);

            var resultUri = Platform.AppContext.ApplicationContext.ContentResolver.Insert(attendeeUri, attendeeValues);

            if (int.TryParse(resultUri?.LastPathSegment, out var result))
            {
                return result > 0;
            }

            return false;
        }

        static async Task<bool> PlatformRemoveAttendeeFromEvent(CalendarEventAttendee newAttendee, string eventId)
        {
            await Permissions.RequestAsync<Permissions.CalendarWrite>();

            var calendarEvent = await GetEventAsync(eventId);

            if (calendarEvent == null)
            {
                throw new ArgumentException("[Android]: You must supply a valid event id to remove an attendee from an event.");
            }

            var attendeesUri = CalendarContract.Attendees.ContentUri;
            var attendeeSpecificAttendees = $"{CalendarContract.Attendees.InterfaceConsts.AttendeeName}='{newAttendee.Name}' {andCondition} ";
            attendeeSpecificAttendees += $"{CalendarContract.Attendees.InterfaceConsts.AttendeeEmail}='{newAttendee.Email}'";

            var result = Platform.AppContext.ApplicationContext.ContentResolver.Delete(attendeesUri, attendeeSpecificAttendees, null);

            return result > 0;
        }

        // https://icalendar.org/iCalendar-RFC-5545/3-8-5-3-recurrence-rule.html
        static RecurrenceRule GetRecurranceRuleForEvent(string rule)
        {
            var recurranceRule = new RecurrenceRule();
            if (rule.Contains(byFrequencySearch, StringComparison.Ordinal))
            {
                var ruleFrequency = rule.Substring(rule.IndexOf(byFrequencySearch, StringComparison.Ordinal) + byFrequencySearch.Length);
                ruleFrequency = ruleFrequency.Contains(";") ? ruleFrequency.Substring(0, ruleFrequency.IndexOf(";")) : ruleFrequency;
                switch (ruleFrequency)
                {
                    case dailyFrequency:
                        recurranceRule.Frequency = RecurrenceFrequency.Daily;
                        break;
                    case weeklyFrequency:
                        recurranceRule.Frequency = RecurrenceFrequency.Weekly;
                        break;
                    case monthlyFrequency:
                        recurranceRule.Frequency = RecurrenceFrequency.Monthly;
                        break;
                    case yearlyFrequency:
                        recurranceRule.Frequency = RecurrenceFrequency.Yearly;
                        break;
                }
            }

            if (rule.Contains(byIntervalSearch, StringComparison.Ordinal))
            {
                var ruleInterval = rule.Substring(rule.IndexOf(byIntervalSearch, StringComparison.Ordinal) + byIntervalSearch.Length);
                ruleInterval = ruleInterval.Contains(";") ? ruleInterval.Substring(0, ruleInterval.IndexOf(";", StringComparison.Ordinal)) : ruleInterval;
                recurranceRule.Interval = uint.Parse(ruleInterval);
            }
            else
            {
                recurranceRule.Interval = 1;
            }

            if (rule.Contains(byCountSearch, StringComparison.Ordinal))
            {
                var ruleOccurences = rule.Substring(rule.IndexOf(byCountSearch, StringComparison.Ordinal) + byCountSearch.Length);
                ruleOccurences = ruleOccurences.Contains(";") ? ruleOccurences.Substring(0, ruleOccurences.IndexOf(";", StringComparison.Ordinal)) : ruleOccurences;
                recurranceRule.TotalOccurrences = uint.Parse(ruleOccurences);
            }

            if (rule.Contains(byUntilSearch, StringComparison.Ordinal))
            {
                var ruleEndDate = rule.Substring(rule.IndexOf(byUntilSearch, StringComparison.Ordinal) + byUntilSearch.Length);
                ruleEndDate = ruleEndDate.Contains(";") ? ruleEndDate.Substring(0, ruleEndDate.IndexOf(";", StringComparison.Ordinal)) : ruleEndDate;
                recurranceRule.EndDate = DateTimeOffset.ParseExact(ruleEndDate.Replace("T", string.Empty).Replace("Z", string.Empty), "yyyyMMddHHmmss", null);
            }

            if (rule.Contains(byDaySearch, StringComparison.Ordinal))
            {
                var ruleOccurenceDays = rule.Substring(rule.IndexOf(byDaySearch, StringComparison.Ordinal) + byDaySearch.Length);
                ruleOccurenceDays = ruleOccurenceDays.Contains(";") ? ruleOccurenceDays.Substring(0, ruleOccurenceDays.IndexOf(";", StringComparison.Ordinal)) : ruleOccurenceDays;
                recurranceRule.DaysOfTheWeek = new List<CalendarDayOfWeek>();
                foreach (var ruleOccurenceDay in ruleOccurenceDays.Split(','))
                {
                    var day = ruleOccurenceDay;
                    var regex = new Regex(@"[-]?\d+");
                    var iterationOffset = regex.Match(ruleOccurenceDay);
                    if (iterationOffset.Success)
                    {
                        day = ruleOccurenceDay.Substring(iterationOffset.Index + iterationOffset.Length);

                        if (recurranceRule.Frequency == RecurrenceFrequency.Monthly)
                        {
                            recurranceRule.Frequency = RecurrenceFrequency.MonthlyOnDay;
                        }
                        else
                        {
                            recurranceRule.Frequency = RecurrenceFrequency.YearlyOnDay;
                        }
                        int.TryParse(iterationOffset.Value.Split(',').FirstOrDefault(), out var result);
                        recurranceRule.WeekOfMonth = (IterationOffset)result;
                    }
                    switch (day)
                    {
                        case "MO":
                            recurranceRule.DaysOfTheWeek.Add(CalendarDayOfWeek.Monday);
                            break;
                        case "TU":
                            recurranceRule.DaysOfTheWeek.Add(CalendarDayOfWeek.Tuesday);
                            break;
                        case "WE":
                            recurranceRule.DaysOfTheWeek.Add(CalendarDayOfWeek.Wednesday);
                            break;
                        case "TH":
                            recurranceRule.DaysOfTheWeek.Add(CalendarDayOfWeek.Thursday);
                            break;
                        case "FR":
                            recurranceRule.DaysOfTheWeek.Add(CalendarDayOfWeek.Friday);
                            break;
                        case "SA":
                            recurranceRule.DaysOfTheWeek.Add(CalendarDayOfWeek.Saturday);
                            break;
                        case "SU":
                            recurranceRule.DaysOfTheWeek.Add(CalendarDayOfWeek.Sunday);
                            break;
                    }
                }
            }

            if (rule.Contains(byMonthDaySearch, StringComparison.Ordinal))
            {
                var ruleOccurenceMonthDays = rule?.Substring(rule.IndexOf(byMonthDaySearch, StringComparison.Ordinal) + byMonthDaySearch.Length);
                ruleOccurenceMonthDays = ruleOccurenceMonthDays.Contains(";") ? ruleOccurenceMonthDays.Substring(0, ruleOccurenceMonthDays.IndexOf(";", StringComparison.Ordinal)) : ruleOccurenceMonthDays;
                uint.TryParse(ruleOccurenceMonthDays.Split(',').FirstOrDefault(), out var result);
                recurranceRule.DayOfTheMonth = result;
            }

            if (rule.Contains(byMonthSearch, StringComparison.Ordinal))
            {
                var ruleOccurenceMonths = rule.Substring(rule.IndexOf(byMonthSearch, StringComparison.Ordinal) + byMonthSearch.Length);
                ruleOccurenceMonths = ruleOccurenceMonths.Contains(";") ? ruleOccurenceMonths.Substring(0, ruleOccurenceMonths.IndexOf(";", StringComparison.Ordinal)) : ruleOccurenceMonths;
                recurranceRule.MonthOfTheYear = (MonthOfYear)Convert.ToUInt32(ruleOccurenceMonths.Split(',').FirstOrDefault());
            }

            if (rule.Contains(bySetPosSearch, StringComparison.Ordinal))
            {
                var ruleDayIterationOffset = rule.Substring(rule.IndexOf(bySetPosSearch, StringComparison.Ordinal) + bySetPosSearch.Length);
                ruleDayIterationOffset = ruleDayIterationOffset.Contains(";") ? ruleDayIterationOffset.Substring(0, ruleDayIterationOffset.IndexOf(";", StringComparison.Ordinal)) : ruleDayIterationOffset;
                Enum.TryParse<IterationOffset>(ruleDayIterationOffset.Split(',').FirstOrDefault(), out var result);
                recurranceRule.WeekOfMonth = result;
                if (recurranceRule.Frequency == RecurrenceFrequency.Monthly)
                {
                    recurranceRule.Frequency = RecurrenceFrequency.MonthlyOnDay;
                }
                else
                {
                    recurranceRule.Frequency = RecurrenceFrequency.YearlyOnDay;
                }
            }
            return recurranceRule;
        }

        static string ConvertRule(this RecurrenceRule recurrenceRule)
        {
            var eventRecurrence = string.Empty;

            switch (recurrenceRule.Frequency)
            {
                case RecurrenceFrequency.Daily:
                case RecurrenceFrequency.Weekly:
                    if (recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0)
                    {
                        eventRecurrence += $"{byFrequencySearch}{weeklyFrequency};";
                        eventRecurrence += $"{byDaySearch}{recurrenceRule.DaysOfTheWeek.ToDayString()};";
                    }
                    else
                    {
                        eventRecurrence += $"{byFrequencySearch}{dailyFrequency};";
                    }
                    eventRecurrence += $"{byIntervalSearch}{recurrenceRule.Interval};";
                    break;
                case RecurrenceFrequency.Monthly:
                    eventRecurrence += $"{byFrequencySearch}{monthlyFrequency};";
                    if (recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0)
                    {
                        eventRecurrence += $"{byDaySearch}{recurrenceRule.WeekOfMonth}{recurrenceRule.DaysOfTheWeek.ToDayString()};";
                    }
                    else if (recurrenceRule.DayOfTheMonth != 0)
                    {
                        eventRecurrence += $"{byMonthDaySearch}{recurrenceRule.DayOfTheMonth};";
                    }
                    else
                    {
                        eventRecurrence += $"{byIntervalSearch}{recurrenceRule.Interval};";
                    }
                    break;
                case RecurrenceFrequency.Yearly:
                    eventRecurrence += $"{byFrequencySearch}{yearlyFrequency};";
                    if (recurrenceRule.DaysOfTheWeek != null && recurrenceRule.DaysOfTheWeek.Count > 0)
                    {
                        eventRecurrence += $"{byMonthSearch}{(int)recurrenceRule.MonthOfTheYear};";
                        eventRecurrence += $"{byDaySearch}{recurrenceRule.WeekOfMonth}{recurrenceRule.DaysOfTheWeek.ToDayString()};";
                    }
                    else if (recurrenceRule.DayOfTheMonth != 0)
                    {
                        eventRecurrence += $"{byMonthSearch}{(int)recurrenceRule.MonthOfTheYear};";
                        eventRecurrence += $"{byMonthDaySearch}{recurrenceRule.DayOfTheMonth};";
                    }
                    else
                    {
                        eventRecurrence += $"{byIntervalSearch}{recurrenceRule.Interval};";
                    }
                    break;
            }

            if (recurrenceRule.EndDate.HasValue)
            {
                eventRecurrence += $"UNTIL={recurrenceRule.EndDate.Value.ToUniversalTime():yyyyMMddTHHmmssZ};";
            }
            else if (recurrenceRule.TotalOccurrences.HasValue)
            {
                eventRecurrence += $"COUNT={recurrenceRule.TotalOccurrences.Value};";
            }

            return eventRecurrence.Substring(0, eventRecurrence.Length - 1);
        }

        static string ToShortString(this CalendarDayOfWeek day)
        {
            switch (day)
            {
                case CalendarDayOfWeek.Monday:
                    return "MO";
                case CalendarDayOfWeek.Tuesday:
                    return "TU";
                case CalendarDayOfWeek.Wednesday:
                    return "WE";
                case CalendarDayOfWeek.Thursday:
                    return "TH";
                case CalendarDayOfWeek.Friday:
                    return "FR";
                case CalendarDayOfWeek.Saturday:
                    return "SA";
                case CalendarDayOfWeek.Sunday:
                    return "SU";
            }
            return "INVALID";
        }

        static string ToDayString(this List<CalendarDayOfWeek> dayList)
        {
            var toReturn = string.Empty;
            foreach (var day in dayList)
            {
                toReturn += day.ToShortString() + ",";
            }
            return toReturn.Substring(0, toReturn.Length - 1);
        }
    }
}
