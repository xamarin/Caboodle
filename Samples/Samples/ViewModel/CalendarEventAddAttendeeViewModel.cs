using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class CalendarEventAddAttendeeViewModel : BaseViewModel
    {
        public CalendarEventAddAttendeeViewModel(string eventId, string eventName)
        {
            EventId = eventId;
            EventName = eventName;
            CreateCalendarEventAttendee = new Command(CreateCalendarEventAttendeeCommand);
        }

        public string EventName { get; set; }

        public string EventId { get; set; }

        string name;

        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                {
                    OnPropertyChanged(nameof(CanCreateAttendee));
                }
            }
        }

        string emailAddress;

        public string EmailAddress
        {
            get => emailAddress;
            set
            {
                if (SetProperty(ref emailAddress, value))
                {
                    OnPropertyChanged(nameof(CanCreateAttendee));
                }
            }
        }

        public List<string> AttendeeTypes { get; } = new List<string>()
        {
            AttendeeType.Optional.ToString(),
            AttendeeType.Required.ToString(),
            AttendeeType.Resource.ToString(),
        };

        string selectedAttendeeType = AttendeeType.Optional.ToString();

        public string SelectedAttendeeType
        {
            get => selectedAttendeeType;
            set => SetProperty(ref selectedAttendeeType, value);
        }

        public bool IsValidEmail(string emailaddress)
        {
            try
            {
                return EmailAddress == new MailAddress(emailAddress).Address;
            }
            catch
            {
                return false;
            }
        }

        public bool CanCreateAttendee => IsValidEmail(EmailAddress) && !string.IsNullOrWhiteSpace(Name);

        public ICommand CreateCalendarEventAttendee { get; }

        async void CreateCalendarEventAttendeeCommand()
        {
            var newAttendee = new CalendarEventAttendee()
            {
                Name = Name,
                Email = EmailAddress,
                Type = (AttendeeType)Enum.Parse(typeof(AttendeeType), SelectedAttendeeType)
            };

            var result = await Calendars.AddAttendeeToEvent(newAttendee, EventId);

            await DisplayAlertAsync("Added event attendee: " + newAttendee.Name);
        }
    }
}
