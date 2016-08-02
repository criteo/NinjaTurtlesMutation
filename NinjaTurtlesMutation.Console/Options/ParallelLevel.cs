using System;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class ParallelLevel : Option
    {
        protected override void TakeArguments(System.Collections.Generic.Queue<string> queue)
        {
            var arg = queue.Dequeue();
            try
            {
                ParallelValue = int.Parse(arg);
                if (ParallelValue < 1)
                    throw new InvalidArgumentValueException("ParallelLevel", "ParallelLevel minimum is 1", arg);
            }
            catch (FormatException)
            {
                throw new OptionArgumentParseException("ParallelLevel", arg);
            }
        }

        public int ParallelValue { get; private set; }
    }
}
