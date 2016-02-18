using System;

namespace Cron
{
    public class IgnoreExpressionSection : ExpressionSectionBase
    {
        static private readonly IgnoreExpressionSection _dayOfMonthInstance = new IgnoreExpressionSection
                                                                                          {
                                                                                              Type =
                                                                                                  ExpressionSectionType.
                                                                                                  DayOfMonth
                                                                                          };

        static private readonly IgnoreExpressionSection _dayOfWeekInstance = new IgnoreExpressionSection
                                                                                         {
                                                                                             Type =
                                                                                                 ExpressionSectionType.
                                                                                                 DayOfWeek
                                                                                         };

        static private readonly IgnoreExpressionSection _hourInstance = new IgnoreExpressionSection
                                                                                    {
                                                                                        Type =
                                                                                            ExpressionSectionType.Hour
                                                                                    };

        static private readonly IgnoreExpressionSection _minuteInstance = new IgnoreExpressionSection
                                                                                      {
                                                                                          Type =
                                                                                              ExpressionSectionType.
                                                                                              Minute
                                                                                      };

        static private readonly IgnoreExpressionSection _monthInstance = new IgnoreExpressionSection
                                                                                     {
                                                                                         Type =
                                                                                             ExpressionSectionType.Month
                                                                                     };

        static public bool TryParse(string s, ExpressionSectionType type, out IgnoreExpressionSection result)
        {
            result = default(IgnoreExpressionSection);
            if (string.IsNullOrEmpty(s)) return false;
            if (!string.Equals("?", s, StringComparison.OrdinalIgnoreCase)) return false;

            switch (type)
            {
                case ExpressionSectionType.DayOfMonth:
                    result = _dayOfMonthInstance;
                    break;
                case ExpressionSectionType.DayOfWeek:
                    result = _dayOfWeekInstance;
                    break;

                case ExpressionSectionType.Hour:
                    result = _hourInstance;
                    break;

                case ExpressionSectionType.Minute:
                    result = _minuteInstance;
                    break;

                case ExpressionSectionType.Month:
                    result = _monthInstance;
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Unexpected {0} {1}",
                                                                      typeof(ExpressionSectionType).Name,
                                                                      type));
            }

            return true;
        }

        protected internal override int NextValue(int currentValue)
        {
            return currentValue;
        }

        protected internal override int FirstValue()
        {
            return 0;
        }

        protected internal override void First(ref DateTime when)
        {
            return;
        }

        protected internal override bool Next(ref DateTime when)
        {
            return false;
        }

        protected internal override bool IsMatch(int value)
        {
            return true;
        }
    }
}