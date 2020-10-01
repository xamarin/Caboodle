using System;
using System.Globalization;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class DateSelectedEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventArgs = value as DateChangedEventArgs;

            if (eventArgs == null)
                return value;

            return eventArgs.NewDate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
