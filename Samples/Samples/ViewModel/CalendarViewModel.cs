using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class CalendarViewModel : BaseViewModel
    {
        bool hasAppeared;

        Calendar selectedCalendar;

        bool useStartDateTime;
        DateTime startDate;
        TimeSpan startTime;

        bool useEndDateTime;
        DateTime endDate;
        TimeSpan endTime;

        public CalendarViewModel()
        {
            startDate = DateTime.Now.Date; // from today
            endDate = startDate + TimeSpan.FromDays(14); // to 14 days from now

            startTime = DateTime.Now.TimeOfDay; // from now
            endTime = TimeSpan.FromDays(1) - TimeSpan.FromSeconds(1); // to the end of the day

            RefreshCalendarsCommand = new Command(OnRefreshCalendars);
        }

        public ICommand RefreshCalendarsCommand { get; }

        public ObservableCollection<Calendar> CalendarList { get; } = new ObservableCollection<Calendar>();

        public ObservableCollection<CalendarEvent> EventList { get; } = new ObservableCollection<CalendarEvent>();

        public Calendar SelectedCalendar
        {
            get => selectedCalendar;
            set => SetProperty(ref selectedCalendar, value, onChanged: OnRefreshEvents);
        }

        public bool UseStartDateTime
        {
            get => useStartDateTime;
            set => SetProperty(ref useStartDateTime, value, onChanged: OnRefreshEvents);
        }

        public bool UseEndDateTime
        {
            get => useEndDateTime;
            set => SetProperty(ref useEndDateTime, value, onChanged: OnRefreshEvents);
        }

        public DateTime StartDate
        {
            get => startDate;
            set => SetProperty(ref startDate, value, onChanged: OnRefreshEvents);
        }

        public TimeSpan StartTime
        {
            get => startTime;
            set => SetProperty(ref startTime, value, onChanged: OnRefreshEvents);
        }

        public DateTime EndDate
        {
            get => endDate;
            set => SetProperty(ref endDate, value, onChanged: OnRefreshEvents);
        }

        public TimeSpan EndTime
        {
            get => endTime;
            set => SetProperty(ref endTime, value, onChanged: OnRefreshEvents);
        }

        public DateTimeOffset? StartDateTime =>
            UseStartDateTime
                ? (StartDate + StartTime)
                : (DateTimeOffset?)null;

        public DateTimeOffset? EndDateTime =>
            UseEndDateTime
                ? (EndDate + EndTime)
                : (DateTimeOffset?)null;

        public override void OnAppearing()
        {
            base.OnAppearing();

            if (!hasAppeared)
            {
                OnRefreshCalendars();
                hasAppeared = true;
            }
        }

        async void OnRefreshCalendars()
        {
            var calendars = await Calendars.GetCalendarsAsync();

            CalendarList.Clear();
            CalendarList.Add(new Calendar { Id = null, Name = "All" });

            foreach (var calendar in calendars)
            {
                CalendarList.Add(calendar);
            }

            SelectedCalendar = CalendarList[0];
        }

        async void OnRefreshEvents()
        {
            var events = await Calendars.GetEventsAsync(
                SelectedCalendar?.Id,
                StartDateTime,
                EndDateTime);

            EventList.Clear();

            foreach (var e in events)
            {
                EventList.Add(e);
            }
        }
    }
}
