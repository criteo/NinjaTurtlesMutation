using System;

namespace Cron
{
    public class EveryOccurenceExpressionSection : ExpressionSectionBase
    {
        static private readonly EveryOccurenceExpressionSection _dayOfMonthInstance = new EveryOccurenceExpressionSection
                                                                                          {
                                                                                              Type =
                                                                                                  ExpressionSectionType.
                                                                                                  DayOfMonth
                                                                                          };

        static private readonly EveryOccurenceExpressionSection _dayOfWeekInstance = new EveryOccurenceExpressionSection
                                                                                         {
                                                                                             Type =
                                                                                                 ExpressionSectionType.
                                                                                                 DayOfWeek
                                                                                         };

        static private readonly EveryOccurenceExpressionSection _hourInstance = new EveryOccurenceExpressionSection
                                                                                    {
                                                                                        Type =
                                                                                            ExpressionSectionType.Hour
                                                                                    };

        static private readonly EveryOccurenceExpressionSection _minuteInstance = new EveryOccurenceExpressionSection
                                                                                      {
                                                                                          Type =
                                                                                              ExpressionSectionType.
                                                                                              Minute
                                                                                      };

        static private readonly EveryOccurenceExpressionSection _monthInstance = new EveryOccurenceExpressionSection
                                                                                     {
                                                                                         Type =
                                                                                             ExpressionSectionType.Month
                                                                                     };

        static public bool TryParse(string s, ExpressionSectionType type, out EveryOccurenceExpressionSection result)
        {
            result = null;
            if (!string.Equals("*", s, StringComparison.OrdinalIgnoreCase)) return false;

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
                default:
                    result = _monthInstance;
                    break;
            }

            return true;
        }

        protected internal override int NextValue(int currentValue)
        {
            switch (Type)
            {
                case ExpressionSectionType.Minute:
                    return (currentValue + 1) % 60;
                case ExpressionSectionType.Hour:
                    return (currentValue + 1) % 24;
                case ExpressionSectionType.DayOfWeek:
                    return (currentValue + 1) % 7;
                case ExpressionSectionType.DayOfMonth:
                    return (currentValue % 31) + 1;
                default:
                    return (currentValue % 12) + 1;
            }
        }

        protected internal override int FirstValue()
        {
            switch (Type)
            {
                case ExpressionSectionType.DayOfMonth:
                    return 1;
                default:
                    return 0;
            }
        }

        protected internal override bool IsMatch(int value)
        {
            return true;
        }
    }
}