using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TCalendar = Tizen.Pims.Calendar;
using TCalendarViews = Tizen.Pims.Calendar.CalendarViews;

namespace Xamarin.Essentials
{
    public static partial class Calendars
    {
        static TCalendar.CalendarManager manager = new TCalendar.CalendarManager();

        static Task<IEnumerable<Calendar>> PlatformGetCalendarsAsync()
        {
            var recordList = manager.Database.GetAll(TCalendarViews.Book.Uri, 0, 0);

            var calendarList = new List<Calendar>();
            recordList.MoveFirst();
            for (var i = 0; i < recordList.Count; i++)
            {
                calendarList.Add(ToCalendar(recordList.GetCurrentRecord()));
                recordList.MoveNext();
            }
            return Task.FromResult<IEnumerable<Calendar>>(calendarList);
        }

        static Calendar ToCalendar(TCalendar.CalendarRecord calendarRecord)
        {
            return new Calendar
            {
                Id = calendarRecord.Get<int>(TCalendarViews.Book.Id).ToString(CultureInfo.InvariantCulture),
                Name = calendarRecord.Get<string>(TCalendarViews.Book.Name),
            };
        }

        static Task<Calendar> PlatformGetCalendarAsync(string calendarId)
        {
            if (!int.TryParse(calendarId, out _))
                throw InvalidCalendar(calendarId);

            var query = new TCalendar.CalendarQuery(TCalendarViews.Book.Uri);
            var filter = new TCalendar.CalendarFilter(
                TCalendarViews.Book.Uri,
                TCalendarViews.Book.Id,
                TCalendar.CalendarFilter.IntegerMatchType.Equal,
                int.Parse(calendarId));
            query.SetFilter(filter);

            var recordList = manager.Database.GetRecordsWithQuery(query, 0, 0);
            if (recordList.Count <= 0)
                throw InvalidCalendar(calendarId);

            recordList.MoveFirst();
            return Task.FromResult(ToCalendar(recordList.GetCurrentRecord()));
        }

        static Task<IEnumerable<CalendarEvent>> PlatformGetEventsAsync(string calendarId, DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            var sDate = startDate ?? DateTimeOffset.Now.Add(defaultStartTimeFromNow);
            var eDate = endDate ?? sDate.Add(defaultEndTimeFromStartTime);
            var sTime = new TCalendar.CalendarTime(sDate.UtcTicks);
            var eTime = new TCalendar.CalendarTime(eDate.UtcTicks);

            var query = new TCalendar.CalendarQuery(TCalendarViews.Event.Uri);
            var filter = new TCalendar.CalendarFilter(
                TCalendarViews.Event.Uri,
                TCalendarViews.Event.Start,
                TCalendar.CalendarFilter.IntegerMatchType.GreaterThanOrEqual,
                sTime);
            filter.AddCondition(
                TCalendar.CalendarFilter.LogicalOperator.And,
                TCalendarViews.Event.End,
                TCalendar.CalendarFilter.IntegerMatchType.LessThanOrEqual,
                eTime);
            if (!string.IsNullOrEmpty(calendarId) && !int.TryParse(calendarId, out _))
            {
                filter.AddCondition(
                    TCalendar.CalendarFilter.LogicalOperator.And,
                    TCalendarViews.Event.BookId,
                    TCalendar.CalendarFilter.IntegerMatchType.Equal,
                    int.Parse(calendarId));
            }
            query.SetFilter(filter);
            query.SetSort(TCalendarViews.Event.Start, true);

            var recordList = manager.Database.GetRecordsWithQuery(query, 0, 0);
            if (recordList.Count <= 0)
                throw InvalidCalendar(calendarId);

            var eventList = new List<CalendarEvent>();
            recordList.MoveFirst();
            for (var i = 0; i < recordList.Count; i++)
            {
                eventList.Add(ToEvent(recordList.GetCurrentRecord()));
                recordList.MoveNext();
            }
            return Task.FromResult<IEnumerable<CalendarEvent>>(eventList);
        }

        static CalendarEvent ToEvent(TCalendar.CalendarRecord eventRecord)
        {
            var sTime = eventRecord.Get<TCalendar.CalendarTime>(TCalendarViews.Event.Start);
            var eTime = eventRecord.Get<TCalendar.CalendarTime>(TCalendarViews.Event.End);
            var sDate = DateTimeOffset.FromUnixTimeMilliseconds(sTime.UtcTime.Ticks);
            var eDate = DateTimeOffset.FromUnixTimeMilliseconds(eTime.UtcTime.Ticks);

            return new CalendarEvent
            {
                Id = eventRecord.Get<int>(TCalendarViews.Event.Id).ToString(CultureInfo.InvariantCulture),
                CalendarId = eventRecord.Get<int>(TCalendarViews.Event.BookId).ToString(CultureInfo.InvariantCulture),
                Title = eventRecord.Get<string>(TCalendarViews.Event.Summary),
                Description = eventRecord.Get<string>(TCalendarViews.Event.Description),
                Location = eventRecord.Get<string>(TCalendarViews.Event.Location),
                AllDay = eventRecord.Get<bool>(TCalendarViews.Event.IsAllday),
                StartDate = sDate,
                EndDate = eDate,
                Attendees = PlatformGetAttendees(eventRecord.Get<string>(TCalendarViews.Event.Id)),
            };
        }

        static IEnumerable<CalendarEventAttendee> PlatformGetAttendees(string eventId)
        {
            if (!int.TryParse(eventId, out _))
                throw InvalidCalendar(eventId);

            var eventUri = TCalendarViews.Event.Uri;
            var query = new TCalendar.CalendarQuery(eventUri);
            var filter = new TCalendar.CalendarFilter(
                TCalendarViews.Event.Uri,
                TCalendarViews.Event.Id,
                TCalendar.CalendarFilter.StringMatchType.FullString,
                eventId);
            query.SetFilter(filter);

            var recordList = manager.Database.GetRecordsWithQuery(query, 0, 0);
            if (recordList.Count <= 0)
                throw InvalidEvent(eventId);

            var attendeeList = new List<CalendarEventAttendee>();
            recordList.MoveFirst();
            for (var i = 0; i < recordList.Count; i++)
            {
                attendeeList.Add(ToAttendee(recordList.GetCurrentRecord()));
                recordList.MoveNext();
            }
            return attendeeList;
        }

        static CalendarEventAttendee ToAttendee(TCalendar.CalendarRecord attendeeRecord)
        {
            return new CalendarEventAttendee
            {
                Name = attendeeRecord.Get<string>(TCalendarViews.Event.Summary),
                Email = attendeeRecord.Get<string>(TCalendarViews.Event.EmailId),
            };
        }

        static Task<CalendarEvent> PlatformGetEventAsync(string eventId)
        {
            if (!int.TryParse(eventId, out _))
                throw InvalidCalendar(eventId);

            var eventUri = TCalendarViews.Event.Uri;
            var query = new TCalendar.CalendarQuery(eventUri);
            var filter = new TCalendar.CalendarFilter(
                eventUri,
                TCalendarViews.Event.Id,
                TCalendar.CalendarFilter.IntegerMatchType.Equal,
                int.Parse(eventId));
            query.SetFilter(filter);

            var recordList = manager.Database.GetRecordsWithQuery(query, 0, 0);
            if (recordList.Count <= 0)
                throw InvalidEvent(eventId);

            recordList.MoveFirst();
            return Task.FromResult(ToEvent(recordList.GetCurrentRecord()));
        }
    }
}
