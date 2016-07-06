
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

        [TestCase("lolz")]
        [TestCase("rpz91")]
        [TestCase("html")]
        public void AssertNonsenseParseAsOne(string nonsence)
        {
            var qarg = new Queue<string>();
            var sce = new SuccessThresholdExposed();

            qarg.Enqueue(nonsence);
            qarg.Enqueue("lé");
            sce.TakeArgumentsExposed(qarg);

            Assert.AreEqual(sce.MinScore, 1F);
        }
    }
}
