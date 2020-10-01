using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class CalendarAddViewModel : BaseViewModel
    {
        public CalendarAddViewModel()
        {
            CreateCalendar = new Command(CreateCalendarCommand);
        }

        public string CalendarId { get; set; }

        string calendarName;

        public string CalendarName
        {
            get => calendarName;
            set
            {
                if (SetProperty(ref calendarName, value))
                {
                    OnPropertyChanged(nameof(CanCreateCalendar));
                }
            }
        }

        public bool CanCreateCalendar => !string.IsNullOrWhiteSpace(CalendarName);

        public ICommand CreateCalendar { get; }

        async void CreateCalendarCommand()
        {
            var newCalendar = new Calendar()
            {
                Name = CalendarName
            };

            var calendarId = await Calendars.CreateCalendar(newCalendar);

            await DisplayAlertAsync("Created calendar id: " + calendarId);
        }
    }
}
