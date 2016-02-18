using System;

namespace Cron
{
    public abstract class ExpressionSectionBase
    {
        public ExpressionSectionType Type { get; protected set; }

        static public bool ValidateValue(ExpressionSectionType type, int value)
        {
            switch (type)
            {
                case ExpressionSectionType.Minute:
                    return value >= 0 && value <= 59;
                case ExpressionSectionType.Hour:
                    return value >= 0 && value <= 23;
                case ExpressionSectionType.DayOfMonth:
                    return value >= 1 && value <= 31;
                case ExpressionSectionType.Month:
                    return value >= 1 && value <= 12;
                default:
                    return value >= 0 && value <= 7;
            }
        }

        static public bool TryParse(string s, ExpressionSectionType type, out ExpressionSectionBase result)
        {
            result = default(ExpressionSectionBase);

            if (string.IsNullOrEmpty(s)) return false;

            EveryOccurenceExpressionSection every;
            if (EveryOccurenceExpressionSection.TryParse(s, type, out every))
            {
                result = every;
                return true;
            }
            IgnoreExpressionSection ignore;
            if (IgnoreExpressionSection.TryParse(s, type, out ignore))
            {
                if (type != ExpressionSectionType.DayOfMonth && type != ExpressionSectionType.DayOfWeek) return false;
                result = ignore;
                return true;
            }

            RangeExpressionSection range;
            if (RangeExpressionSection.TryParse(s, type, out range))
            {
                result = range;
                return true;
            }

            RepeatingExpressionSection repeating;
            if (RepeatingExpressionSection.TryParse(s, type, out repeating))
            {
                result = repeating;
                return true;
            }

            ListExpressionSection list;
            if (ListExpressionSection.TryParse(s, type, out list))
            {
                result = list;
                return true;
            }

            switch (type)
            {
                case ExpressionSectionType.Month:
                    MonthExpressionSection month;
                    if (!MonthExpressionSection.TryParse(s, out month)) return false;
                    result = month;
                    return true;
                case ExpressionSectionType.DayOfWeek:
                    DayOfWeekExpressionSection dayOfWeek;
                    if (DayOfWeekExpressionSection.TryParse(s, out dayOfWeek))
                    {
                        result = dayOfWeek;
                        return true;
                    }
                    SpecifiedWeekAndWeekDayExpressionSection specifiedDayAndWeekDay;
                    if (SpecifiedWeekAndWeekDayExpressionSection.TryParse(s, out specifiedDayAndWeekDay))
                    {
                        result = specifiedDayAndWeekDay;
                        return true;
                    }
                    goto default;
                case ExpressionSectionType.DayOfMonth:
                    LastDayOfMonthExpressionSection lastDayOfMonth;
                    if (LastDayOfMonthExpressionSection.TryParse(s, out lastDayOfMonth))
                    {
                        result = lastDayOfMonth;
                        return true;
                    }
                    NearestWeekdayExpressionSection nearestWeekDay;
                    if (NearestWeekdayExpressionSection.TryParse(s, out nearestWeekDay))
                    {
                        result = nearestWeekDay;
                        return true;
                    }
                    LastWeekDayOfMonthExpressionSection lastWeekDay;
                    if (LastWeekDayOfMonthExpressionSection.TryParse(s, out lastWeekDay))
                    {
                        result = lastWeekDay;
                        return true;
                    }
                    goto default;
                default:
                    SimpleExpressionSection simple;
                    if (!SimpleExpressionSection.TryParse(s, type, out simple)) return false;
                    result = simple;
                    return true;
            }
        }

        protected internal abstract int NextValue(int currentValue);

        protected internal virtual bool Next(ref DateTime when)
        {
            int currentValue = -1;
            switch (Type)
            {
                case ExpressionSectionType.Minute:
                    currentValue = when.Minute;
                    break;
                case ExpressionSectionType.Hour:
                    currentValue = when.Hour;
                    break;
                case ExpressionSectionType.DayOfWeek:
                    currentValue = (int)when.DayOfWeek;
                    break;
                case ExpressionSectionType.DayOfMonth:
                    currentValue = when.Day;
                    break;
                case ExpressionSectionType.Month:
                    currentValue = when.Month;
                    break;
            }

            int nextValue = NextValue(currentValue);
            if (nextValue <= currentValue 
                && Type != ExpressionSectionType.DayOfWeek
                && Type != ExpressionSectionType.Month) return true;

            switch (Type)
            {
                case ExpressionSectionType.Minute:
                    int minute = when.Minute;
                    when = when.AddMinutes(nextValue - minute);
                    break;
                case ExpressionSectionType.Hour:
                    int hour = when.Hour;
                    when = when.AddHours(nextValue - hour);
                    break;
                case ExpressionSectionType.DayOfWeek:
                    var dayOfWeek = (int)when.DayOfWeek;
                    if (dayOfWeek >= nextValue)
                    {
                        when = when.AddDays(7);
                        nextValue = FirstValue();
                    }
                    when = when.AddDays(nextValue - dayOfWeek);
                    break;
                case ExpressionSectionType.DayOfMonth:
                    int day = when.Day;
                    when = when.AddDays(nextValue - day);
                    break;
                case ExpressionSectionType.Month:
                    int month = when.Month;
                    if (month >= nextValue)
                    {
                        when = when.AddYears(1);
                        month = when.Month;
                        nextValue = NextValue(0);
                    }
                    when = when.AddMonths(nextValue - month);
                    break;
            }
            return false;
        }

        protected internal abstract int FirstValue();

        protected internal virtual void First(ref DateTime when)
        {
            int nextValue = FirstValue();
            switch (Type)
            {
                case ExpressionSectionType.Minute:
                    int minute = when.Minute;
                    when = when.AddMinutes(nextValue - minute);
                    break;
                case ExpressionSectionType.Hour:
                    int hour = when.Hour;
                    when = when.AddHours(nextValue - hour);
                    break;
                case ExpressionSectionType.DayOfWeek:
                    when = when.AddDays(1 - when.Day);
                    while (!IsMatch((int)when.DayOfWeek)) when = when.AddDays(1);
                    break;
                case ExpressionSectionType.DayOfMonth:
                    when = when.AddDays(1 - when.Day);
                    while (!IsMatch(when)) when = when.AddDays(1);
                    break;
            }
        }

        protected internal abstract bool IsMatch(int value);

        protected virtual internal bool IsMatch(DateTime when)
        {
            switch (Type)
            {
                case ExpressionSectionType.Minute:
                    return IsMatch(when.Minute);
                case ExpressionSectionType.Hour:
                    return IsMatch(when.Hour);
                case ExpressionSectionType.DayOfMonth:
                    return IsMatch(when.Day);
                case ExpressionSectionType.DayOfWeek:
                    return IsMatch((int)when.DayOfWeek);
                default:
                    return IsMatch(when.Month);
            }
        }
    }
}