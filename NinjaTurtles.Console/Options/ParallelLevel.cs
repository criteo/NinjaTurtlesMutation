using System;

namespace NinjaTurtles.Console.Options
{
    internal class ParallelLevel : Option
    {
        protected override void TakeArguments(System.Collections.Generic.Queue<string> queue)
        {
            try
            {
                ParallelValue = int.Parse(queue.Dequeue());
                if (ParallelValue < 1)
                    ParallelValue = 8;
            }
            catch (FormatException) {ParallelValue = 8;}
        }

        public int ParallelValue { get; private set; }
    }
}
