using System;

namespace Cron
{
    public class MonthExpressionSection : SimpleExpressionSection
    {
        public Month Month { get; set; }

        public override int Value
        {
            get { return (int)Month; }
            set { Month = (Month)value; }
        }

        static public bool TryParse(string s, out MonthExpressionSection result)
        {
            result = default(MonthExpressionSection);
            if (string.IsNullOrEmpty(s)) return false;

            int month;

            if (int.TryParse(s, out month))
            {
                if (!ValidateValue(ExpressionSectionType.Month, month)) return false;
            }
            else if (string.Equals("JAN", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 1;
            }
            else if (string.Equals("FEB", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 2;
            }
            else if (string.Equals("MAR", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 3;
            }
            else if (string.Equals("APR", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 4;
            }
            else if (string.Equals("MAY", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 5;
            }
            else if (string.Equals("JUN", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 6;
            }
            else if (string.Equals("JUL", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 7;
            }
            else if (string.Equals("AUG", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 8;
            }
            else if (string.Equals("SEP", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 9;
            }
            else if (string.Equals("OCT", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 10;
            }
            else if (string.Equals("NOV", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 11;
            }
            else if (string.Equals("DEC", s, StringComparison.OrdinalIgnoreCase))
            {
                month = 12;
            }
            else
            {
                return false;
            }

            result = new MonthExpressionSection();
            result.Type = ExpressionSectionType.Month;
            result.Month = (Month)month;
            return true;
        }
    }
}