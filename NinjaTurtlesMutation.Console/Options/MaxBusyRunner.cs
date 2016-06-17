using System;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class MaxBusyRunner : Option
    {
        protected override void TakeArguments(System.Collections.Generic.Queue<string> queue)
        {
            try
            {
                MaxBusyRunnersValue = int.Parse(queue.Dequeue());
                if (MaxBusyRunnersValue < 1)
                    MaxBusyRunnersValue = 8;
            }
            catch (FormatException) { MaxBusyRunnersValue = 8;}
        }

        public int MaxBusyRunnersValue { get; private set; }
    }
}
