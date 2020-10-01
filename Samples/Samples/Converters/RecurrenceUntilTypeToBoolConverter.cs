using System;
using System.Globalization;
using Samples.ViewModel;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class RecurrenceUntilTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string type))
            {
                return false;
            }
            switch (type)
            {
                case RecurrenceEndType.UntilEndDate:
                    return true;
                case RecurrenceEndType.Indefinitely:
                case RecurrenceEndType.AfterOccurences:
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
