using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public static class RecurrenceEndType
    {
        public const string AfterOccurences = "After a set number of times";
        public const string Indefinitely = "Indefinitely";
        public const string UntilEndDate = "Continues until a specified date";
    }

    public class CalendarDayOfWeekSwitch : INotifyPropertyChanged
    {
        bool isChecked;

        public CalendarDayOfWeekSwitch(CalendarDayOfWeek val, PropertyChangedEventHandler propertyChangedCallBack)
        {
            Day = val;
            PropertyChanged += propertyChangedCallBack;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CalendarDayOfWeek Day { get; set; }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (value != isChecked)
                {
                    isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public override string ToString() => Day.ToString();

        protected void OnPropertyChanged([CallerMemberName]string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class CalendarEventAddViewModel : BaseViewModel
    {
        static TimeSpan RoundToNearestMinutes(TimeSpan input, int minutes)
        {
            var totalMinutes = (int)(input + new TimeSpan(0, minutes / 2, 0)).TotalMinutes;

            return new TimeSpan(0, totalMinutes - (totalMinutes % minutes), 0);
        }

        bool allDay;
        string description;
        DateTime endDate = DateTime.Now.Date;
        TimeSpan endTime = RoundToNearestMinutes(DateTime.Now.AddHours(2).TimeOfDay, 30);
        string eventLocation;
        string eventTitle;
        bool isMonthDaySpecific = true;
        DateTime? recurrenceEndDate;
        uint? recurrenceEndInterval;
        uint recurrenceInterval = 1;
        CalendarDayOfWeek? selectedMonthWeekRecurrenceDay = null;
        CalendarDayOfWeek selectedRecurrenceDay;
        string selectedRecurrenceEndType = "Indefinitely";
        uint selectedRecurrenceMonthDay = 1;
        IterationOffset? selectedRecurrenceMonthWeek = null;
        RecurrenceFrequency? selectedRecurrenceType = null;
        MonthOfYear selectedRecurrenceYearlyMonth = MonthOfYear.January;
        DateTime startDate = DateTime.Now.Date;
        TimeSpan startTime = RoundToNearestMinutes(DateTime.Now.AddHours(1).TimeOfDay, 30);
        string url;

        public CalendarEventAddViewModel(string calendarId, string calendarName, CalendarEvent existingEvent = null)
        {
            CalendarId = calendarId;
            CalendarName = calendarName;
            RecurrenceDays = new ObservableCollection<CalendarDayOfWeekSwitch>()
            {
                new CalendarDayOfWeekSwitch(CalendarDayOfWeek.Sunday, OnChildCheckBoxChangedEvent),
                new CalendarDayOfWeekSwitch(CalendarDayOfWeek.Monday, OnChildCheckBoxChangedEvent),
                new CalendarDayOfWeekSwitch(CalendarDayOfWeek.Tuesday, OnChildCheckBoxChangedEvent),
                new CalendarDayOfWeekSwitch(CalendarDayOfWeek.Wednesday, OnChildCheckBoxChangedEvent),
                new CalendarDayOfWeekSwitch(CalendarDayOfWeek.Thursday, OnChildCheckBoxChangedEvent),
                new CalendarDayOfWeekSwitch(CalendarDayOfWeek.Friday, OnChildCheckBoxChangedEvent),
                new CalendarDayOfWeekSwitch(CalendarDayOfWeek.Saturday, OnChildCheckBoxChangedEvent)
            };

            if (existingEvent != null)
            {
                EventId = existingEvent.Id;
                EventTitle = existingEvent.Title;
                Description = existingEvent.Description;
                EventLocation = existingEvent.Location;
                Url = existingEvent.Url;
                AllDay = existingEvent.AllDay;
                StartDate = existingEvent.StartDate.Date;
                EndDate = existingEvent.EndDate?.Date ?? existingEvent.StartDate.Date;
                StartTime = existingEvent.StartDate.TimeOfDay;
                EndTime = existingEvent.EndDate?.TimeOfDay ?? existingEvent.StartDate.TimeOfDay;
                if (existingEvent.RecurrancePattern != null)
                {
                    SelectedRecurrenceType = existingEvent.RecurrancePattern.Frequency;
                    RecurrenceInterval = existingEvent.RecurrancePattern.Interval;

                    var selectedDays = existingEvent.RecurrancePattern.DaysOfTheWeek != null && existingEvent.RecurrancePattern.DaysOfTheWeek.Any() ? new ObservableCollection<CalendarDayOfWeekSwitch>(existingEvent.RecurrancePattern.DaysOfTheWeek.ConvertAll(x => new CalendarDayOfWeekSwitch(x, OnChildCheckBoxChangedEvent)).ToList()) : new ObservableCollection<CalendarDayOfWeekSwitch>();
                    foreach (var r in selectedDays)
                    {
                        var recurrenceDay = RecurrenceDays.FirstOrDefault(x => x.Day == r.Day);
                        if (recurrenceDay != null)
                            recurrenceDay.IsChecked = true;
                    }
                    switch (existingEvent.RecurrancePattern.Frequency)
                    {
                        case RecurrenceFrequency.MonthlyOnDay:
                        case RecurrenceFrequency.YearlyOnDay:
                            SelectedRecurrenceType = SelectedRecurrenceType == RecurrenceFrequency.MonthlyOnDay ? RecurrenceFrequency.Monthly : RecurrenceFrequency.Yearly;
                            IsMonthDaySpecific = false;
                            SelectedRecurrenceMonthWeek = existingEvent.RecurrancePattern.WeekOfMonth;
                            SelectedMonthWeekRecurrenceDay = existingEvent.RecurrancePattern.DaysOfTheWeek.FirstOrDefault();
                            break;
                        case RecurrenceFrequency.Monthly:
                        case RecurrenceFrequency.Yearly:
                            SelectedRecurrenceMonthDay = existingEvent.RecurrancePattern.DayOfTheMonth;
                            break;
                    }
                    if (existingEvent.RecurrancePattern.MonthOfTheYear.HasValue)
                    {
                        SelectedRecurrenceYearlyMonth = existingEvent.RecurrancePattern.MonthOfTheYear.Value;
                    }
                    if (existingEvent.RecurrancePattern.EndDate.HasValue)
                    {
                        RecurrenceEndDate = existingEvent.RecurrancePattern.EndDate.Value.DateTime;
                        SelectedRecurrenceEndType = RecurrenceEndType.UntilEndDate;
                    }
                    else if (existingEvent.RecurrancePattern.TotalOccurrences.HasValue)
                    {
                        RecurrenceEndInterval = existingEvent.RecurrancePattern.TotalOccurrences.Value;
                        SelectedRecurrenceEndType = RecurrenceEndType.AfterOccurences;
                    }
                    else
                    {
                        RecurrenceEndDate = null;
                        SelectedRecurrenceEndType = RecurrenceEndType.Indefinitely;
                    }
                }
            }
            CreateOrUpdateEvent = new Command(CreateOrUpdateCalendarEvent);
        }

        public bool AllDay
        {
            get => allDay;
            set
            {
                if (SetProperty(ref allDay, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                    OnPropertyChanged(nameof(DisplayTimeInformation));
                }
            }
        }

        public string CalendarId { get; set; }

        public string CalendarName { get; set; }

        public bool CanAlterRecurrence => SelectedRecurrenceType != null;

        public bool CanCreateOrUpdateEvent => !string.IsNullOrWhiteSpace(EventTitle)
            && ((EndDate.Date == StartDate.Date && (EndTime > StartTime || AllDay)) || EndDate.Date > StartDate.Date)
            && (!CanAlterRecurrence || StartDate < RecurrenceEndDate || SelectedRecurrenceEndType == RecurrenceEndType.Indefinitely || (RecurrenceEndInterval.HasValue && RecurrenceEndInterval.Value > 0))
            && IsValidUrl(Url);

        public ICommand CreateOrUpdateEvent { get; }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public bool DisplayTimeInformation => !AllDay && !CanAlterRecurrence;

        public DateTime EndDate
        {
            get => endDate;
            set
            {
                if (SetProperty(ref endDate, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public TimeSpan EndTime
        {
            get => endTime;
            set
            {
                if (SetProperty(ref endTime, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public string EventActionText => string.IsNullOrEmpty(EventId) ? "Add Event" : "Update Event";

        public string EventId { get; set; }

        public string EventLocation
        {
            get => eventLocation;
            set => SetProperty(ref eventLocation, value);
        }

        public string EventTitle
        {
            get => eventTitle;
            set
            {
                if (SetProperty(ref eventTitle, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public bool IsMonthDaySpecific
        {
            get => isMonthDaySpecific;
            set
            {
                if (SetProperty(ref isMonthDaySpecific, value))
                {
                    if (value)
                    {
                        SelectedRecurrenceMonthDay = 1;
                        SelectedRecurrenceMonthWeek = null;
                        SelectedMonthWeekRecurrenceDay = null;
                    }
                    else
                    {
                        SelectedRecurrenceMonthDay = 0;
                        SelectedRecurrenceMonthWeek = IterationOffset.First;
                        SelectedMonthWeekRecurrenceDay = CalendarDayOfWeek.Monday;
                    }
                }
            }
        }

        public List<CalendarDayOfWeek> MonthWeekRecurrenceDay { get; set; } = new List<CalendarDayOfWeek>()
        {
            CalendarDayOfWeek.Monday,
            CalendarDayOfWeek.Tuesday,
            CalendarDayOfWeek.Wednesday,
            CalendarDayOfWeek.Thursday,
            CalendarDayOfWeek.Friday,
            CalendarDayOfWeek.Saturday,
            CalendarDayOfWeek.Sunday
        };

        public ObservableCollection<CalendarDayOfWeekSwitch> RecurrenceDays { get; }

        public DateTime? RecurrenceEndDate
        {
            get => recurrenceEndDate;
            set
            {
                if (SetProperty(ref recurrenceEndDate, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public uint? RecurrenceEndInterval
        {
            get => recurrenceEndInterval;
            set
            {
                if (SetProperty(ref recurrenceEndInterval, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public ObservableCollection<string> RecurrenceEndTypes { get; } = new ObservableCollection<string>()
        {
            RecurrenceEndType.Indefinitely,
            RecurrenceEndType.AfterOccurences,
            RecurrenceEndType.UntilEndDate
        };

        public uint RecurrenceInterval
        {
            get => recurrenceInterval;
            set => SetProperty(ref recurrenceInterval, value);
        }

        public ObservableCollection<uint> RecurrenceMonthDay { get; set; } = new ObservableCollection<uint>()
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        };

        public List<IterationOffset> RecurrenceMonthWeek { get; } = new List<IterationOffset>()
        {
            IterationOffset.First,
            IterationOffset.Second,
            IterationOffset.Third,
            IterationOffset.Fourth,
            IterationOffset.Last
        };

        public List<RecurrenceFrequency> RecurrenceTypes { get; } = new List<RecurrenceFrequency>()
        {
            RecurrenceFrequency.Daily,
            RecurrenceFrequency.Weekly,
            RecurrenceFrequency.Monthly,
            RecurrenceFrequency.Yearly
        };

        public List<MonthOfYear> RecurrenceYearlyMonth { get; } = new List<MonthOfYear>()
        {
            MonthOfYear.January,
            MonthOfYear.February,
            MonthOfYear.March,
            MonthOfYear.April,
            MonthOfYear.May,
            MonthOfYear.June,
            MonthOfYear.July,
            MonthOfYear.August,
            MonthOfYear.September,
            MonthOfYear.October,
            MonthOfYear.November,
            MonthOfYear.December
        };

        public CalendarDayOfWeek? SelectedMonthWeekRecurrenceDay
        {
            get => selectedMonthWeekRecurrenceDay;
            set => SetProperty(ref selectedMonthWeekRecurrenceDay, value);
        }

        public CalendarDayOfWeek SelectedRecurrenceDay
        {
            get => selectedRecurrenceDay;
            set
            {
                if (SetProperty(ref selectedRecurrenceDay, value))
                {
                    SetCheckBoxes(value);
                }
            }
        }

        public string SelectedRecurrenceEndType
        {
            get => selectedRecurrenceEndType;

            set
            {
                if (SetProperty(ref selectedRecurrenceEndType, value) && selectedRecurrenceEndType != null)
                {
                    switch (value)
                    {
                        case RecurrenceEndType.Indefinitely:
                            RecurrenceEndDate = null;
                            RecurrenceEndInterval = null;
                            break;
                        case RecurrenceEndType.AfterOccurences:
                            RecurrenceEndDate = null;
                            RecurrenceEndInterval = !RecurrenceEndInterval.HasValue ? 1 : RecurrenceEndInterval;
                            break;
                        case RecurrenceEndType.UntilEndDate:
                            RecurrenceEndInterval = null;
                            RecurrenceEndDate = !RecurrenceEndDate.HasValue ? DateTime.Now.AddMonths(6) : RecurrenceEndDate;
                            break;
                        default:
                            RecurrenceEndDate = null;
                            RecurrenceEndInterval = null;
                            break;
                    }
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public uint SelectedRecurrenceMonthDay
        {
            get => selectedRecurrenceMonthDay;
            set
            {
                if (SetProperty(ref selectedRecurrenceMonthDay, value))
                {
                    if (value != 0)
                    {
                        IsMonthDaySpecific = true;
                    }
                }
            }
        }

        public IterationOffset? SelectedRecurrenceMonthWeek
        {
            get => selectedRecurrenceMonthWeek;
            set => SetProperty(ref selectedRecurrenceMonthWeek, value);
        }

        public RecurrenceFrequency? SelectedRecurrenceType
        {
            get => selectedRecurrenceType;

            set
            {
                if (SetProperty(ref selectedRecurrenceType, value))
                {
                    OnPropertyChanged(nameof(CanAlterRecurrence));
                    OnPropertyChanged(nameof(DisplayTimeInformation));
                    OnPropertyChanged(nameof(SelectedRecurrenceTypeDisplay));
                }
            }
        }

        public string SelectedRecurrenceTypeDisplay => SelectedRecurrenceType.ToString().Replace("ily", "yly").Replace("ly", "(s)");

        public MonthOfYear SelectedRecurrenceYearlyMonth
        {
            get => selectedRecurrenceYearlyMonth;
            set
            {
                if (SetProperty(ref selectedRecurrenceYearlyMonth, value))
                {
                    var days = Enumerable.Range(1, DateTime.DaysInMonth(StartDate.Year, (int)value)).Select(x => (uint)x).ToList();
                    RecurrenceMonthDay.Clear();
                    days.ForEach(x => RecurrenceMonthDay.Add(x));
                }
            }
        }

        public DateTime StartDate
        {
            get => startDate;
            set
            {
                if (SetProperty(ref startDate, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public TimeSpan StartTime
        {
            get => startTime;
            set
            {
                if (SetProperty(ref startTime, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        public string Url
        {
            get => url;
            set
            {
                if (SetProperty(ref url, value))
                {
                    OnPropertyChanged(nameof(CanCreateOrUpdateEvent));
                }
            }
        }

        bool IsUpdatingCheckBoxGroup { get; set; } = false;

        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return true;
            }
            else if (!Regex.IsMatch(url, @"^https?:\/\/", RegexOptions.IgnoreCase))
            {
                url = "http://" + url;
                Url = url;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }

            return false;
        }

        async void CreateOrUpdateCalendarEvent()
        {
            var startDto = new DateTimeOffset(StartDate + StartTime);
            var endDto = new DateTimeOffset(EndDate + EndTime);
            var newEvent = new CalendarEvent()
            {
                Id = EventId,
                CalendarId = CalendarId,
                Title = EventTitle,
                AllDay = AllDay,
                Description = Description,
                Location = EventLocation,
                Url = url,
                StartDate = startDto,
                EndDate = !AllDay ? !CanAlterRecurrence ? (DateTimeOffset?)endDto : new DateTimeOffset(StartDate + EndTime) : null
            };

            if (CanAlterRecurrence)
            {
                List<CalendarDayOfWeek> daysOfTheWeek = null;
                var selectedFrequency = SelectedRecurrenceType;
                IterationOffset? dayIterationOffset = null;
                switch (selectedFrequency)
                {
                    case RecurrenceFrequency.Daily:
                    case RecurrenceFrequency.Weekly:
                        daysOfTheWeek = RecurrenceDays.Where(x => x.IsChecked).ToList().ConvertAll(x => x.Day);
                        break;

                    default:
                        if (SelectedMonthWeekRecurrenceDay.HasValue)
                        {
                            daysOfTheWeek = new List<CalendarDayOfWeek>()
                            {
                                SelectedMonthWeekRecurrenceDay.Value
                            };
                        }

                        if (SelectedRecurrenceMonthWeek != null)
                        {
                            dayIterationOffset = SelectedRecurrenceMonthWeek;
                        }
                        break;
                }

                newEvent.RecurrancePattern = new RecurrenceRule()
                {
                    Frequency = selectedFrequency,
                    Interval = RecurrenceInterval,
                    DaysOfTheWeek = daysOfTheWeek,
                    DayOfTheMonth = SelectedRecurrenceMonthDay,
                    WeekOfMonth = dayIterationOffset,
                    MonthOfTheYear = SelectedRecurrenceYearlyMonth,
                    EndDate = RecurrenceEndDate,
                    TotalOccurrences = RecurrenceEndInterval
                };
            }

            if (string.IsNullOrEmpty(EventId))
            {
                var eventId = await Calendars.CreateCalendarEvent(newEvent);
                await DisplayAlertAsync("Created event id: " + eventId);
            }
            else
            {
                if (await Calendars.UpdateCalendarEvent(newEvent))
                {
                    await DisplayAlertAsync("Updated event id: " + newEvent.Id);
                }
            }
        }

        void OnChildCheckBoxChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            if (!(sender is CalendarDayOfWeekSwitch calendarDayOfWeek))
            {
                return;
            }
            if (!IsUpdatingCheckBoxGroup)
            {
                SelectedRecurrenceDay = (CalendarDayOfWeek)RecurrenceDays.Where(x => x.IsChecked).Sum(x => (int)x.Day);
            }
        }

        void SetCheckBoxes(CalendarDayOfWeek bitFlagValue)
        {
            try
            {
                IsUpdatingCheckBoxGroup = true;
                foreach (var day in RecurrenceDays)
                {
                    day.IsChecked = bitFlagValue.HasFlag(day.Day);
                }
            }
            finally
            {
                IsUpdatingCheckBoxGroup = false;
            }
        }
    }
}
