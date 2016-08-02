using System.Collections.Generic;
using NinjaTurtlesMutation.Console.Options;
using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.ConsoleOptionsTests
{
    class ParallelLevelTest
    {
        class ParralelLevelExposed : ParallelLevel
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
            var plvl = new ParralelLevelExposed();

            qarg.Enqueue(arg);
            qarg.Enqueue("Fake");
            qarg.Enqueue("plouf");
            qarg.Enqueue("wow, much code");
            plvl.TakeArgumentsExposed(qarg);

            Assert.AreEqual(plvl.ParallelValue, expected);
        }

        [TestCase("0")]
        [TestCase("-1")]
        public void LessThanOneThrow(string badValue)
        {
            var qarg = new Queue<string>();
            var plvl = new ParralelLevelExposed();

            qarg.Enqueue(badValue);
            Assert.Throws<InvalidArgumentValueException>(() => plvl.TakeArgumentsExposed(qarg));
        }

        [TestCase("lolz")]
        [TestCase("rpz91")]
        [TestCase("html")]
        public void NonsenseThrow(string nonsense)
        {
            var qarg = new Queue<string>();
            var plvl = new ParralelLevelExposed();

            qarg.Enqueue(nonsense);
            qarg.Enqueue("lé");
            Assert.Throws<OptionArgumentParseException>(() => plvl.TakeArgumentsExposed(qarg));
        }
    }
}
