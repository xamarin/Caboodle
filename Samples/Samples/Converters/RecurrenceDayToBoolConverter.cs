using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Samples.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class RecurrenceDayToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is CalendarDayOfWeek recurrenceDays) || !(parameter is CalendarDayOfWeek expectedResult))
            {
                return false;
            }

            return recurrenceDays == expectedResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool switchValue) || !(parameter is CalendarDayOfWeek expectedResult) || switchValue == false)
            {
                return null;
            }

            return expectedResult;
        }
    }
}
