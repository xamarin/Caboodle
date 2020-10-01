using System;
using System.Globalization;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class CheckBoxEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CheckedChangedEventArgs eventArgs))
                return value;

            return eventArgs.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
