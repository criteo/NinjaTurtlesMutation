using System;
using System.Text.RegularExpressions;

namespace Cron
{
    public class SpecifiedWeekAndWeekDayExpressionSection : SimpleExpressionSection
    {
        static private readonly Regex VALIDATE_REGEX = new Regex("^(?<DayNumber>[1-7])#(?<WeekNumber>[1-5])$");

        public int Week { get; set; }

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
            if (when.DayOfWeek != (DayOfWeek)Value) return false;
            if (when.AddDays(-7 * Week).Month == when.Month) return false;
            return (when.AddDays(-7 * (Week - 1)).Month == when.Month);
        }

        protected internal override void First(ref DateTime when)
        {
            when = when.AddDays(1 - when.Day);
            while (!IsMatch(when)) when = when.AddDays(1);
        }

        static internal bool TryParse(string s, out SpecifiedWeekAndWeekDayExpressionSection result)
        {
            result = null;
            Match match = VALIDATE_REGEX.Match(s);
            if (match.Success)
            {
                result = new SpecifiedWeekAndWeekDayExpressionSection();
                result.Type = ExpressionSectionType.DayOfWeek;
                result.Value = int.Parse(match.Groups["DayNumber"].Value) - 1;
                result.Week = int.Parse(match.Groups["WeekNumber"].Value);
                return true;    
            }
            return false;
        }
    }
}
