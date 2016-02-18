using System;

namespace Cron
{
    public class LastWeekDayOfMonthExpressionSection : ExpressionSectionBase
    {
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
            throw new NotImplementedException();
        }

        protected internal override bool IsMatch(DateTime when)
        {
            if (when.DayOfWeek == DayOfWeek.Saturday) return false;
            if (when.DayOfWeek == DayOfWeek.Sunday) return false;
            return when.AddDays(1).Month != when.Month;    
        }

        static internal bool TryParse(string s, out LastWeekDayOfMonthExpressionSection result)
        {
            result = null;
            if (string.Equals("LW", s))
            {
                result = new LastWeekDayOfMonthExpressionSection();
                result.Type = ExpressionSectionType.DayOfMonth;
                return true;
            }
            return false;
        }
    }
}
