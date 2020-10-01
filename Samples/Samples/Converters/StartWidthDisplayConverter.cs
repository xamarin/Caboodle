using System;
using System.Globalization;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class StartWidthDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool b))
            {
                return 1;
            }
            return b ? 3 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
