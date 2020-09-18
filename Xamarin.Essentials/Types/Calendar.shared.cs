using System;
using System.Collections.Generic;

namespace Xamarin.Essentials
{
    public class Calendar
    {
        public Calendar()
        {
        }

        public Calendar(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class CalendarEvent
    {
        public CalendarEvent()
        {
        }

        public CalendarEvent(string id, string calendarId, string title)
        {
            Id = id;
            CalendarId = calendarId;
            Title = title;
        }

        public string Id { get; set; }

        public string CalendarId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public bool AllDay { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public TimeSpan Duration =>
            AllDay ? TimeSpan.FromDays(1) : EndDate - StartDate;

        public IEnumerable<CalendarEventAttendee> Attendees { get; set; }
    }

    public class CalendarEventAttendee
    {
        public CalendarEventAttendee()
        {
        }

        public CalendarEventAttendee(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
