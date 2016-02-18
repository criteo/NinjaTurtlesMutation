using System;
using System.Text.RegularExpressions;

namespace Cron
{
    public class NearestWeekdayExpressionSection : SimpleExpressionSection
    {
        static private readonly Regex VALIDATE_REGEX = new Regex("^(?<DayNumber>[1-9]|(1[0-9])|(2[0-9])|(3[0-1]))W$");

        protected internal override int NextValue(int currentValue)
        {
            return (currentValue % 31) + 1;
        }

        protected internal override int FirstValue()
        {
            return 1;
        }

        protected internal override bool IsMatch(int value)
        {
            throw new System.NotImplementedException();
        }

        protected internal override bool IsMatch(DateTime when)
        {
            if (when.Day == Value && IsWeekDay(when)) return true;
            if (when.Day + 1 == Value && when.DayOfWeek == DayOfWeek.Friday) return true;
            if (when.Day - 1 == Value && when.DayOfWeek == DayOfWeek.Monday) return true;
            if (when.Day == 3 && Value == 1 && when.DayOfWeek == DayOfWeek.Monday) return true;
            return false;
        }

        protected internal override void First(ref DateTime when)
        {
            when = when.AddDays(1 - when.Day);
            while (!IsMatch(when)) when = when.AddDays(1);
        }

        static private bool IsWeekDay(DateTime date)
        {
            return !(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }

        static public bool TryParse(string s, out NearestWeekdayExpressionSection result)
        {
            result = null;
            Match match = VALIDATE_REGEX.Match(s);
            if (match.Success)
            {
                result = new NearestWeekdayExpressionSection();
                result.Type = ExpressionSectionType.DayOfMonth;
                result.Value = int.Parse(match.Groups["DayNumber"].Value);
                return true;
            }
            return false;
        }
    }
}
