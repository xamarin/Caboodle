using System;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class TimeSelectedEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PropertyChangedEventArgs eventArgs) || eventArgs.PropertyName != nameof(TimePicker.Time))
                return null;

            return eventArgs.PropertyName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
