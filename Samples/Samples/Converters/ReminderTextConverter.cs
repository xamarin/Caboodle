using System;
using System.Globalization;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class ReminderTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is int minutes))
                return null;

            return minutes > 0 ? $"{minutes} minutes prior" : "No Reminders";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
