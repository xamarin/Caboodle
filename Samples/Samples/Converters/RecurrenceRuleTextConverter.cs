using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.Converters
{
    public class RecurrenceRuleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is RecurrenceRule rule))
                return null;

            var toReturn = $"Occurs ";

            if (rule.Interval > 0)
            {
                if (rule.Interval == 1)
                {
                    toReturn += $"Every ";
                }
                else
                {
                    toReturn += $"Every {((int)rule.Interval).ToOrdinal()} ";
                }
                switch (rule.Frequency)
                {
                    case RecurrenceFrequency.Daily:
                        toReturn += "Day ";
                        break;
                    case RecurrenceFrequency.Weekly:
                        toReturn += "Week ";
                        break;
                    case RecurrenceFrequency.Monthly:
                    case RecurrenceFrequency.MonthlyOnDay:
                        toReturn += "Month ";
                        break;
                    case RecurrenceFrequency.Yearly:
                    case RecurrenceFrequency.YearlyOnDay:
                        toReturn += "Year ";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (rule.WeekOfMonth != null && (rule.Frequency == RecurrenceFrequency.MonthlyOnDay || rule.Frequency == RecurrenceFrequency.YearlyOnDay))
            {
                toReturn += $"on the {rule.WeekOfMonth} ";
                if (rule.DaysOfTheWeek?.Count > 0)
                {
                    toReturn += $"[";
                    toReturn = rule.DaysOfTheWeek.Aggregate(toReturn, (current, d) => current + $"{d}, ");
                    toReturn = toReturn.Substring(0, toReturn.Length - 2) + "] ";
                }
                if (rule.Frequency == RecurrenceFrequency.YearlyOnDay)
                {
                    toReturn += $"in {rule.MonthOfTheYear.ToString()} ";
                }
            }
            else if (rule.DaysOfTheWeek?.Count > 0)
            {
                toReturn += $"On: [";
                toReturn = rule.DaysOfTheWeek.Aggregate(toReturn, (current, d) => current + $"{d}, ");
                toReturn = toReturn.Substring(0, toReturn.Length - 2) + "] ";
            }

            if (rule.TotalOccurrences > 0)
            {
                toReturn += $"For the next {rule.TotalOccurrences} occurrences ";
            }

            if (rule.EndDate.HasValue)
            {
                toReturn += $"Until {rule.EndDate.Value.DateTime.ToShortDateString()} ";
            }

            return toReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
