using System.Collections.Generic;
using NinjaTurtlesMutation.Console.Options;
using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.ConsoleOptionsTests
{
    [TestFixture]
    public class MaxBusyRunnerTest
    {
        class MaxBusyRunnerExposed : MaxBusyRunner
        {
            public void TakeArgumentsExposed(Queue<string> queue)
            {
                TakeArguments(queue);
            }
        }

        [TestCase("1", 1)]
        [TestCase("2", 2)]
        [TestCase("100", 100)]
        public void GoodArgumentIsParsed(string arg, int expected)
        {
            var qarg = new Queue<string>();
            var mbre = new MaxBusyRunnerExposed();

            qarg.Enqueue(arg);
            qarg.Enqueue("Fake");
            qarg.Enqueue("plouf");
            qarg.Enqueue("wow, much code");
            mbre.TakeArgumentsExposed(qarg);

            Assert.AreEqual(mbre.MaxBusyRunnersValue, expected);
        }

        [TestCase("0")]
        [TestCase("-1")]
        public void LessThanOneThrow(string badValue)
        {
            var qarg = new Queue<string>();
            var mbre = new MaxBusyRunnerExposed();

            qarg.Enqueue(badValue);
            Assert.Throws<InvalidArgumentValueException>(() => mbre.TakeArgumentsExposed(qarg));
        }

        [TestCase("lolz")]
        [TestCase("rpz91")]
        [TestCase("html")]
        public void NonsenseThrow(string nonsense)
        {
            var qarg = new Queue<string>();
            var mbre = new MaxBusyRunnerExposed();

            qarg.Enqueue(nonsense);
            qarg.Enqueue("lé");
            Assert.Throws<OptionArgumentParseException>(() => mbre.TakeArgumentsExposed(qarg));
        }
    }
}
