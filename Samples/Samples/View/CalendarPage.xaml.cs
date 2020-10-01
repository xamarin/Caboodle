using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Samples.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.View
{
    public partial class CalendarPage : BasePage
    {
        public CalendarPage()
        {
            InitializeComponent();
        }

        async void OnAddCalendarButtonClicked(object sender, EventArgs e)
        {
            var modal = new CalendarAddPage();

            modal.BindingContext = new CalendarAddViewModel();
            await Navigation.PushAsync(modal);
        }

        async void OnAddEventButtonClicked(object sender, EventArgs e)
        {
            var modal = new CalendarEventAddPage();

            if (!(SelectedCalendar.SelectedItem is Calendar calendar) || string.IsNullOrEmpty(calendar.Id))
                return;

            modal.BindingContext = new CalendarEventAddViewModel(calendar.Id, calendar.Name);
            await Navigation.PushAsync(modal);
        }

        async void OnEventTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null || !(e.Item is CalendarEvent calendarEvent))
                return;

            calendarEvent = await Calendars.GetEventInstanceAsync(calendarEvent.Id, calendarEvent.StartDate);

            var modal = new CalendarEventPage
            {
                BindingContext = calendarEvent
            };
            await Navigation.PushAsync(modal);
        }
    }
}
