namespace Cron
{
    public class RepeatingExpressionSection : ExpressionSectionBase
    {
        public SimpleExpressionSection StartValue { get; private set; }
        public SimpleExpressionSection Frequency { get; private set; }

        static public bool TryParse(string s, ExpressionSectionType type, out RepeatingExpressionSection result)
        {
            result = default(RepeatingExpressionSection);
            if (string.IsNullOrEmpty(s)) return false;
            string[] parts = s.Split('/');
            if (parts.Length != 2) return false;

            if (string.IsNullOrEmpty(parts[0])) return false;
            if (parts[0].Contains(",")) return false;
            if (parts[0].Contains("/")) return false;
            if (parts[0].Contains("-")) return false;

            ExpressionSectionBase startValue;
            if (!TryParse(parts[0], type, out startValue)) return false;
            if (startValue is EveryOccurenceExpressionSection)
            {
                if (!(TryParse("0", type, out startValue))) TryParse("1", type, out startValue);
            }
            else if (!(startValue is SimpleExpressionSection)) return false;

            if (string.IsNullOrEmpty(parts[1])) return false;
            if (parts[1].Contains(",")) return false;
            if (parts[1].Contains("/")) return false;
            if (parts[1].Contains("-")) return false;

            ExpressionSectionBase frequency;
            if (!TryParse(parts[1], type, out frequency)) return false;
            if (!(frequency is SimpleExpressionSection)) return false;

            result = new RepeatingExpressionSection();
            result.Type = type;
            result.StartValue = startValue as SimpleExpressionSection;
            result.Frequency = frequency as SimpleExpressionSection;
            return true;
        }

        protected internal override int NextValue(int currentValue)
        {
            if (currentValue < StartValue.Value) return StartValue.Value;
            if (currentValue == StartValue.Value) return StartValue.Value + Frequency.Value;

            int tentativeValue = currentValue + Frequency.Value - ((currentValue - StartValue.Value) % Frequency.Value);
            switch (Type)
            {
                case ExpressionSectionType.Minute:
                    if (tentativeValue > 59) return StartValue.Value;
                    break;
                case ExpressionSectionType.Hour:
                    if (tentativeValue > 23) return StartValue.Value;
                    break;
                case ExpressionSectionType.DayOfMonth:
                    if (tentativeValue > 31) return StartValue.Value;
                    break;
            }
            return tentativeValue;
        }

        protected internal override int FirstValue()
        {
            return StartValue.Value;
        }

        protected internal override bool IsMatch(int value)
        {
            if (value < StartValue.Value) return false;
            return (value - StartValue.Value) % Frequency.Value == 0;
        }
    }
}