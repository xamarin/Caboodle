using System;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class CalendarEventDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var e = value as CalendarEvent;
            if (e == null)
                return "<unknown>";

            if (e.AllDay)
                return "All day";

            var format = Application.Current.Resources["DateDisplayFormatter"] as string;

            return $"{string.Format(format, e.StartDate)} to {string.Format(format, e.EndDate)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
