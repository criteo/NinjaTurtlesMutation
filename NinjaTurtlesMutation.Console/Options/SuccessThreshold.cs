using System;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class SuccessThreshold : Option
    {
        protected override void TakeArguments(System.Collections.Generic.Queue<string> queue)
        {
            var arg = queue.Dequeue();
            try
            {
                MinScore = float.Parse(arg.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                if (MinScore < 0 || MinScore > 1)
                    throw new InvalidArgumentValueException("SuccessThreshold", "Minimum score must be between 0 and 1",
                        arg);
            }
            catch (FormatException)
            {
                throw new OptionArgumentParseException("SuccessThreshold", arg);
            }
        }

        public float MinScore { get; private set; }
    }
}