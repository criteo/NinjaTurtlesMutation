using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cron
{
    /// <summary>
    /// A simple implementation of a 'cron' expression. This supports all of the syntax at
    /// http://quartz.sourceforge.net/javadoc/org/quartz/CronTrigger.html except for the linked
    /// calendar (special character C) functionality.
    /// </summary>
    public class Expression
    {
        private readonly ExpressionSectionBase _dayOfMonthSection;
        private readonly ExpressionSectionBase _dayOfWeekSection;
        private readonly ExpressionSectionBase _hourSection;
        private readonly ExpressionSectionBase _minuteSection;
        private readonly ExpressionSectionBase _monthSection;

        [DebuggerStepThrough]
        private Expression(ExpressionSectionBase minuteSection,
                           ExpressionSectionBase hourSection,
                           ExpressionSectionBase dayOfMonthSection,
                           ExpressionSectionBase monthSection,
                           ExpressionSectionBase dayOfWeekSection
            )
        {
            if (minuteSection.Type != ExpressionSectionType.Minute
                || hourSection.Type != ExpressionSectionType.Hour
                || dayOfMonthSection.Type != ExpressionSectionType.DayOfMonth
                || dayOfWeekSection.Type != ExpressionSectionType.DayOfWeek
                || monthSection.Type != ExpressionSectionType.Month)
            {
                throw new ArgumentException("Invalid constructor call.");
            }
            _minuteSection = minuteSection;
            _hourSection = hourSection;
            _dayOfMonthSection = dayOfMonthSection;
            _monthSection = monthSection;
            _dayOfWeekSection = dayOfWeekSection;
        }

        public ExpressionSectionBase MinuteSection
        {
            [DebuggerStepThrough]
            get { return _minuteSection; }
        }

        public ExpressionSectionBase HourSection
        {
            [DebuggerStepThrough]
            get { return _hourSection; }
        }

        public ExpressionSectionBase DayOfMonthSection
        {
            [DebuggerStepThrough]
            get { return _dayOfMonthSection; }
        }

        public ExpressionSectionBase MonthSection
        {
            [DebuggerStepThrough]
            get { return _monthSection; }
        }

        public ExpressionSectionBase DayOfWeekSection
        {
            [DebuggerStepThrough]
            get { return _dayOfWeekSection; }
        }

        static public bool TryParse(string s, out Expression result)
        {
            result = default(Expression);
            if (string.IsNullOrEmpty(s)) return false;

            string[] parts = s.Split(' ');
            if (parts.Length != 5L) return false;

            ExpressionSectionBase minuteSection;
            ExpressionSectionBase hourSection;
            ExpressionSectionBase dayOfMonthSection;
            ExpressionSectionBase monthSection;
            ExpressionSectionBase dayOfWeekSection;

            if (!ExpressionSectionBase.TryParse(parts[0],
                                                ExpressionSectionType.Minute,
                                                out minuteSection)) return false;
            if (!ExpressionSectionBase.TryParse(parts[1],
                                                ExpressionSectionType.Hour,
                                                out hourSection)) return false;
            if (!ExpressionSectionBase.TryParse(parts[2],
                                                ExpressionSectionType.DayOfMonth,
                                                out dayOfMonthSection)) return false;
            if (!ExpressionSectionBase.TryParse(parts[3],
                                                ExpressionSectionType.Month,
                                                out monthSection)) return false;
            if (!ExpressionSectionBase.TryParse(parts[4],
                                                ExpressionSectionType.DayOfWeek,
                                                out dayOfWeekSection)) return false;

            if (DaySectionsConflict(dayOfWeekSection, dayOfMonthSection)) return false;

            result = new Expression(minuteSection, hourSection, dayOfMonthSection, monthSection, dayOfWeekSection);
            return true;
        }

        static private bool DaySectionsConflict(ExpressionSectionBase dayOfWeek, ExpressionSectionBase dayOfMonth)
        {
            if (dayOfWeek is IgnoreExpressionSection) return dayOfMonth is IgnoreExpressionSection;
            if (dayOfMonth is IgnoreExpressionSection) return dayOfWeek is IgnoreExpressionSection;
            return !(dayOfWeek is EveryOccurenceExpressionSection && dayOfMonth is EveryOccurenceExpressionSection);
        }

        public bool IsMatch(DateTime when)
        {
            return MinuteSection.IsMatch(when)
                   && HourSection.IsMatch(when)
                   && DayOfMonthSection.IsMatch(when)
                   && DayOfWeekSection.IsMatch(when)
                   && MonthSection.IsMatch(when);
        }

        public DateTime GetNextValid(DateTime when)
        {
            var workingValue = new DateTime(when.Year, when.Month, when.Day, when.Hour, when.Minute, 0);
            workingValue = workingValue.AddMinutes(1);
            bool processDayOfWeek = !(DayOfWeekSection is IgnoreExpressionSection);
            bool processDayOfMonth = !processDayOfWeek && !(DayOfMonthSection is IgnoreExpressionSection);

            while (!IsMatch(workingValue))
            {
                if (MinuteSection.Next(ref workingValue))
                {
                    if (HourSection.Next(ref workingValue))
                    {
                        if (processDayOfWeek && DayOfWeekSection.Next(ref workingValue))
                        {
                            MonthSection.Next(ref workingValue);
                            DayOfWeekSection.First(ref workingValue);
                        }
                        else if (processDayOfMonth && DayOfMonthSection.Next(ref workingValue))
                        {
                            MonthSection.Next(ref workingValue);
                            DayOfMonthSection.First(ref workingValue);
                        }
                        HourSection.First(ref workingValue);
                    }
                    MinuteSection.First(ref workingValue);
                }
            }
            return workingValue;
        }

        static public bool IsMatch(DateTime when, IEnumerable<Expression> expressions)
        {
            foreach (Expression expression in expressions)
            {
                if (expression.IsMatch(when)) return true;
            }
            return false;
        }

        static public DateTime GetNextValid(DateTime when, IEnumerable<Expression> expressions)
        {
            DateTime nextValid = DateTime.MaxValue;
            foreach (Expression expression in expressions)
            {
                DateTime thisNextValid = expression.GetNextValid(when);
                if (thisNextValid < nextValid) nextValid = thisNextValid;
            }
            return nextValid;
        }
    }
}