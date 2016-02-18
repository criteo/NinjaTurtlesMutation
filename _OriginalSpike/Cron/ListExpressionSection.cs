using System.Collections.Generic;
using System.Diagnostics;

namespace Cron
{
    public class ListExpressionSection : ExpressionSectionBase
    {
        private readonly List<ExpressionSectionBase> _sections = new List<ExpressionSectionBase>();

        public IList<ExpressionSectionBase> Sections
        {
            [DebuggerStepThrough]
            get { return _sections; }
        }

        static public bool TryParse(string s, ExpressionSectionType type, out ListExpressionSection result)
        {
            result = default(ListExpressionSection);
            if (string.IsNullOrEmpty(s)) return false;

            string[] parts = s.Split(',');
            if (parts.Length <= 1) return false;
            var sections = new List<ExpressionSectionBase>();

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) return false;
                if (part.Contains(",")) return false;
                if (part.Contains("/")) return false;
                if (part.Contains("-")) return false;
                if (part.Contains("?")) return false;

                ExpressionSectionBase section;
                if (!TryParse(part, type, out section)) return false;
                sections.Add(section);
            }

            result = new ListExpressionSection();
            result.Type = type;
            result._sections.AddRange(sections);

            return true;
        }

        protected internal override int NextValue(int currentValue)
        {
            int nextValue = 99999;
            foreach (var section in Sections)
            {
                int thisFirstValue = section.NextValue(currentValue);
                if (thisFirstValue < nextValue && thisFirstValue > currentValue)
                {
                    nextValue = thisFirstValue;
                }
            }
            if (nextValue == 99999) return currentValue;
            return nextValue;
        }

        protected internal override int FirstValue()
        {
            int firstValue = 99999;
            foreach (var section in Sections)
            {
                int thisFirstValue = section.FirstValue();
                if (thisFirstValue < firstValue)
                {
                    firstValue = thisFirstValue;
                }
            }
            return firstValue;
        }

        protected internal override bool IsMatch(int value)
        {
            foreach (var section in Sections)
            {
                if (section.IsMatch(value)) return true;
            }
            return false;
        }
    }
}