using System;

namespace Cron
{
    public class DayOfWeekExpressionSection : SimpleExpressionSection
    {
        public DayOfWeek DayOfWeek { get; set; }

        public override int Value
        {
            get { return (int)DayOfWeek; }
            set
            {
                if (value == 7)
                {
                    DayOfWeek = DayOfWeek.Sunday;
                }
                else
                {
                    DayOfWeek = (DayOfWeek)value;
                }
            }
        }

        static public bool TryParse(string s, out DayOfWeekExpressionSection result)
        {
            result = default(DayOfWeekExpressionSection);

            int dayOfWeek;

            if (int.TryParse(s, out dayOfWeek))
            {
                if (!ValidateValue(ExpressionSectionType.DayOfWeek, dayOfWeek)) return false;
            }
            else if (string.Equals("SUN", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 0;
            }
            else if (string.Equals("MON", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 1;
            }
            else if (string.Equals("TUE", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 2;
            }
            else if (string.Equals("WED", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 3;
            }
            else if (string.Equals("THU", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 4;
            }
            else if (string.Equals("FRI", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 5;
            }
            else if (string.Equals("SAT", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 6;
            }
            else if (string.Equals("L", s, StringComparison.OrdinalIgnoreCase))
            {
                dayOfWeek = 6;
            }
            else
            {
                return false;
            }

            result = new DayOfWeekExpressionSection();
            result.Type = ExpressionSectionType.DayOfWeek;
            result.Value = dayOfWeek;
            return true;
        }
    }
}