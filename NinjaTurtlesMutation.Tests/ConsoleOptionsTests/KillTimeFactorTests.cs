
using System.Collections.Generic;
using NinjaTurtlesMutation.Console.Options;
using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.ConsoleOptionsTests
{
    [TestFixture]
    public class KillTimeFactorTests
    {
        class KillTimeFactorExposed : KillTimeFactor
        {
            public void TakeArgumentsExposed(Queue<string> queue)
            {
                TakeArguments(queue);
            }
        }

        [TestCase("1.1", 1.1F)]
        [TestCase("1,1", 1.1F)]
        [TestCase("0", 0F)]
        [TestCase("2", 2F)]
        public void AssertArgumentIsParsed(string anArgument, float expected)
        {
            var qarg = new Queue<string>();
            var killTimeFactorExposed = new KillTimeFactorExposed();

            qarg.Enqueue(anArgument);
            qarg.Enqueue("Fake");
            qarg.Enqueue("plouf");
            qarg.Enqueue("wow, much code");
            killTimeFactorExposed.TakeArgumentsExposed(qarg);
            
            Assert.AreEqual(killTimeFactorExposed.Factor, expected);
        }

        [TestCase("-1")]
        [TestCase("-1.1")]
        [TestCase("-0.1")]
        [TestCase("-0,1")]
        public void NegativeValueThrow(string someNegativeStr)
        {
            var qarg = new Queue<string>();
            var killTimeFactorExposed = new KillTimeFactorExposed();

            qarg.Enqueue(someNegativeStr);
            qarg.Enqueue("lé");
            Assert.Throws<InvalidArgumentValueException>(() => killTimeFactorExposed.TakeArgumentsExposed(qarg));
        }

        [TestCase("lolz")]
        [TestCase("rpz91")]
        [TestCase("html")]
        public void NonsenseThrow(string nonsense)
        {
            var qarg = new Queue<string>();
            var killTimeFactorExposed = new KillTimeFactorExposed();

            qarg.Enqueue(nonsense);
            qarg.Enqueue("lé");
            Assert.Throws<OptionArgumentParseException>(() => killTimeFactorExposed.TakeArgumentsExposed(qarg));
        }
    }
}
