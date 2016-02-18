using System;

namespace Cron
{
    public class LastDayOfMonthExpressionSection : ExpressionSectionBase
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
            return when.AddDays(1).Month != when.Month;
        }

        static internal bool TryParse(string s, out LastDayOfMonthExpressionSection result)
        {
            result = null;
            if (string.Equals("L", s))
            {
                result = new LastDayOfMonthExpressionSection();
                result.Type = ExpressionSectionType.DayOfMonth;
                return true;
            }
            return false;
        }
    }
}
