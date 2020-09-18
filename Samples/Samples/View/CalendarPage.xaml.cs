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

        async void OnEventTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is CalendarEvent evt)
            {
                var calendarEvent = await Calendars.GetEventAsync(evt.Id);

                var page = new CalendarEventPage
                {
                    BindingContext = calendarEvent
                };

                await Navigation.PushAsync(page);
            }
        }
    }
}
