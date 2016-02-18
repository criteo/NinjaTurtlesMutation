namespace Cron
{
    public class SimpleExpressionSection : ExpressionSectionBase
    {
        public virtual int Value { get; set; }

        static public bool TryParse(string s, ExpressionSectionType type, out SimpleExpressionSection result)
        {
            result = default(SimpleExpressionSection);
            if (string.IsNullOrEmpty(s)) return false;

            int value;

            if (!int.TryParse(s, out value)) return false;
            if (!ValidateValue(type, value)) return false;

            result = new SimpleExpressionSection();
            result.Type = type;
            result.Value = value;
            return true;
        }

        protected internal override int NextValue(int currentValue)
        {
            return Value;
        }

        protected internal override int FirstValue()
        {
            return Value;
        }

        protected internal override bool IsMatch(int value)
        {
            return value == Value;
        }
    }
}