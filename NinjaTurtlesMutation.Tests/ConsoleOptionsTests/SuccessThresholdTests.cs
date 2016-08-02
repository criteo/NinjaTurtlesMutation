
using System.Collections.Generic;
using NinjaTurtlesMutation.Console.Options;
using NinjaTurtlesMutation.Turtles;
using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.ConsoleOptionsTests
{
    [TestFixture]
    public class SuccessThresholdTests
    {
        class SuccessThresholdExposed : SuccessThreshold
        {
            public void TakeArgumentsExposed(Queue<string> queue)
            {
                TakeArguments(queue);
            }
        }

        [TestCase("0.1", 0.1F)]
        [TestCase("0,1", 0.1F)]
        [TestCase("0", 0F)]
        [TestCase("1", 1F)]
        public void AssertArgumentIsParsed(string anArgument, float expected)
        {
            var qarg = new Queue<string>();
            var sce = new SuccessThresholdExposed();

            qarg.Enqueue(anArgument);
            qarg.Enqueue("Fake");
            qarg.Enqueue("plouf");
            qarg.Enqueue("wow, much code");
            sce.TakeArgumentsExposed(qarg);
            
            Assert.AreEqual(sce.MinScore, expected);
        }

        [TestCase("-1")]
        [TestCase("2")]
        public void OutOfRangeValueThrow(string badValue)
        {
            var qarg = new Queue<string>();
            var sce  = new SuccessThresholdExposed();

            qarg.Enqueue(badValue);
            qarg.Enqueue("rededesintox");
            Assert.Throws<InvalidArgumentValueException>(() => sce.TakeArgumentsExposed(qarg));
        }

        [TestCase("lolz")]
        [TestCase("rpz91")]
        [TestCase("html")]
        public void NonsenseThrow(string nonsense)
        {
            var qarg = new Queue<string>();
            var sce = new SuccessThresholdExposed();

            qarg.Enqueue(nonsense);
            qarg.Enqueue("lé");
            Assert.Throws<OptionArgumentParseException>(() => sce.TakeArgumentsExposed(qarg));
        }
    }
}
