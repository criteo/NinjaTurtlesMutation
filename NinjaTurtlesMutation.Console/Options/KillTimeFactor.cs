using System;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class KillTimeFactor : Option
    {
        protected override void TakeArguments(System.Collections.Generic.Queue<string> queue)
        {
            var arg = queue.Dequeue();
            try
            {
                Factor = float.Parse(arg.Replace(',', '.'),
                    System.Globalization.CultureInfo.InvariantCulture);
                if (Factor < 0)
                    throw new InvalidArgumentValueException("KillTimeFactor", "Factor cannot be lower than 0", arg);
            }
            catch (FormatException)
            {
                throw new OptionArgumentParseException("KillTimeFactor", arg);
            }
        }

        public float Factor { get; private set; }
    }
}