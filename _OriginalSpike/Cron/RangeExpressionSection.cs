namespace Cron
{
    public class RangeExpressionSection : ExpressionSectionBase
    {
        public SimpleExpressionSection StartValue { get; private set; }
        public SimpleExpressionSection EndValue { get; private set; }

        static public bool TryParse(string s, ExpressionSectionType type, out RangeExpressionSection result)
        {
            result = default(RangeExpressionSection);
            if (string.IsNullOrEmpty(s)) return false;

            string[] parts = s.Split('-');
            if (parts.Length != 2) return false;

            if (string.IsNullOrEmpty(parts[0])) return false;
            if (parts[0].Contains(",")) return false;
            if (parts[0].Contains("/")) return false;
            if (parts[0].Contains("-")) return false;
            if (parts[0].Contains("?")) return false;

            ExpressionSectionBase startValue;
            if (!TryParse(parts[0], type, out startValue)) return false;
            if (!(startValue is SimpleExpressionSection)) return false;

            if (string.IsNullOrEmpty(parts[1])) return false;
            if (parts[1].Contains(",")) return false;
            if (parts[1].Contains("/")) return false;
            if (parts[1].Contains("-")) return false;
            if (parts[1].Contains("?")) return false;

            ExpressionSectionBase endValue;
            if (!TryParse(parts[1], type, out endValue)) return false;

            result = new RangeExpressionSection();
            result.Type = type;
            result.StartValue = startValue as SimpleExpressionSection;
            result.EndValue = endValue as SimpleExpressionSection;
            return true;
        }

        protected internal override int NextValue(int currentValue)
        {
            if (currentValue < EndValue.Value)
            {
                return currentValue + 1;
            }
            return currentValue;
        }

        protected internal override int FirstValue()
        {
            return StartValue.FirstValue();
        }

        protected internal override bool IsMatch(int value)
        {
            return value >= StartValue.Value && value <= EndValue.Value;
        }
    }
}