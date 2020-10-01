using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Samples.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class RecurrenceTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is RecurrenceFrequency recurrenceType) || !(parameter is RecurrenceFrequency expectedResult))
            {
                return false;
            }

            switch (expectedResult)
            {
                case RecurrenceFrequency.Monthly:
                    return recurrenceType == RecurrenceFrequency.MonthlyOnDay ||
                           recurrenceType == RecurrenceFrequency.Monthly ||
                           recurrenceType == RecurrenceFrequency.YearlyOnDay ||
                           recurrenceType == RecurrenceFrequency.Yearly;
                case RecurrenceFrequency.Yearly:
                    return recurrenceType == RecurrenceFrequency.Yearly ||
                           recurrenceType == RecurrenceFrequency.YearlyOnDay;
                default:
                    return recurrenceType == expectedResult;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool switchValue) || !(parameter is RecurrenceFrequency expectedResult) || switchValue == false)
            {
                return false;
            }

            return expectedResult;
        }
    }
}
