using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class CalendarViewModel : BaseViewModel
    {
        const int endDateDaysToOffset = 14;

        const string endOfDay = "23:59";

        bool alreadyAppeared;

        public CalendarViewModel()
        {
            GetCalendars = new Command(OnClickGetCalendars);
            StartDateSelectedCommand = new Command(OnStartDateSelected);
            StartTimeSelectedCommand = new Command(OnStartTimeSelected);
            EndDateSelectedCommand = new Command(OnEndDateSelected);
            EndTimeSelectedCommand = new Command(OnEndTimeSelected);
            StartDateEnabledCheckBoxChanged = new Command(OnStartCheckboxChanged);
            EndDateEnabledCheckBoxChanged = new Command(OnEndCheckboxChanged);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            if (!alreadyAppeared)
            {
                alreadyAppeared = true;

                RefreshCalendars();
            }
        }

        Calendar selectedCalendar;

        bool startdatePickersEnabled;

        bool enddatePickersEnabled;

        public bool StartDatePickersEnabled
        {
            get => startdatePickersEnabled;
            set => SetProperty(ref startdatePickersEnabled, value);
        }

        public bool EndDatePickersEnabled
        {
            get => enddatePickersEnabled;
            set => SetProperty(ref enddatePickersEnabled, value);
        }

        public bool CanCreateCalendarEvent => !string.IsNullOrWhiteSpace(SelectedCalendar?.Name) && SelectedCalendar?.Name != "All" && !SelectedCalendar.IsReadOnly;

        public ICommand GetCalendars { get; }

        public ICommand StartDateEnabledCheckBoxChanged { get; }

        public ICommand EndDateEnabledCheckBoxChanged { get; }

        public ICommand StartDateSelectedCommand { get; }

        public ICommand StartTimeSelectedCommand { get; }

        public ICommand EndDateSelectedCommand { get; }

        public ICommand EndTimeSelectedCommand { get; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public TimeSpan StartTime { get; set; }

        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(endDateDaysToOffset);

        public TimeSpan EndTime { get; set; } = TimeSpan.Parse(endOfDay);

        public bool HasCalendarReadAccess { get; set; }

        public ObservableCollection<Calendar> CalendarList { get; } = new ObservableCollection<Calendar>();

        public ObservableCollection<CalendarEvent> EventList { get; } = new ObservableCollection<CalendarEvent>();

        public Calendar SelectedCalendar
        {
            get => selectedCalendar;

            set
            {
                if (SetProperty(ref selectedCalendar, value) && selectedCalendar != null)
                {
                    OnChangeRequestCalendarSpecificEvents(selectedCalendar.Id);
                    OnPropertyChanged(nameof(CanCreateCalendarEvent));
                }
            }
        }

        void OnStartCheckboxChanged(object parameter)
        {
            if (parameter is bool b)
            {
                StartDatePickersEnabled = b;

                RefreshEventList(SelectedCalendar?.Id);
            }
        }

        void OnEndCheckboxChanged(object parameter)
        {
            if (parameter is bool b)
            {
                EndDatePickersEnabled = b;

                RefreshEventList(SelectedCalendar?.Id);
            }
        }

        public void RefreshCalendars() => OnClickGetCalendars();

        async void OnClickGetCalendars()
        {
            CalendarList.Clear();

            CalendarList.Add(new Calendar() { Id = null, IsReadOnly = true, Name = "All" });
            var calendars = await Calendars.GetCalendarsAsync();
            foreach (var calendar in calendars)
            {
                CalendarList.Add(calendar);
            }
            SelectedCalendar = CalendarList[0];
        }

        void OnStartDateSelected(object parameter)
        {
            var startDate = parameter as DateTime?;

            if (!startDate.HasValue)
                return;

            startDate += StartTime;

            RefreshEventList(SelectedCalendar?.Id, startDate);
        }

        void OnStartTimeSelected(object parameter)
        {
            if (parameter == null)
                return;

            RefreshEventList(SelectedCalendar?.Id);
        }

        void OnEndDateSelected(object parameter)
        {
            var endDate = parameter as DateTime?;

            if (!endDate.HasValue)
                return;

            endDate += EndTime;
            RefreshEventList(SelectedCalendar?.Id, null, endDate);
        }

        void OnEndTimeSelected(object parameter)
        {
            if (parameter == null)
                return;

            RefreshEventList();
        }

        public void OnChangeRequestCalendarSpecificEvents(string calendarId = null, DateTime? startDateTime = null, DateTime? endDateTime = null) => RefreshEventList(calendarId, startDateTime, endDateTime);

        async void RefreshEventList(string calendarId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate = StartDatePickersEnabled && !startDate.HasValue ? (DateTime?)StartDate.Date + StartTime : startDate;
            endDate = (EndDatePickersEnabled && !endDate.HasValue) ? (DateTime?)EndDate.Date + EndTime : endDate;
            if (!CalendarList.Any())
                return;

            EventList.Clear();

            var events = await Calendars.GetEventsAsync(calendarId, startDate, endDate);
            foreach (var calendarEvent in events)
            {
                EventList.Add(calendarEvent);
            }
        }
    }
}
