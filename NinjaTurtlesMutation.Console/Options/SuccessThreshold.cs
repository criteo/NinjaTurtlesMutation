using System;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class SuccessThreshold : Option
    {
        protected override void TakeArguments(System.Collections.Generic.Queue<string> queue)
        {
            try
            {
                MinScore = float.Parse(queue.Dequeue().Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                if (MinScore < 0 || MinScore > 1)
                    MinScore = 1;
            }
            catch (FormatException) { MinScore = 1; }
        }

        public float MinScore { get; private set; }
    }
}