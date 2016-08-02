using System;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class MaxBusyRunner : Option
    {
        protected override void TakeArguments(System.Collections.Generic.Queue<string> queue)
        {
            var arg = queue.Dequeue();
            try
            {
                MaxBusyRunnersValue = int.Parse(arg);
                if (MaxBusyRunnersValue < 1)
                    throw new InvalidArgumentValueException("MaxBusyRunner", "MaxBusyRunner minimum is 1", arg);
            }
            catch (FormatException)
            {
                throw new OptionArgumentParseException("MaxBusyRunner", arg);
            }
        }

        public int MaxBusyRunnersValue { get; private set; }
    }
}
