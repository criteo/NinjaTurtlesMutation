using System.Collections.Generic;
using NinjaTurtlesMutation.Console.Options;
using NinjaTurtlesMutation.Turtles;
using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.ConsoleOptionsTests
{
    [TestFixture]
    public class TurtlesTypesTests
    {
        class TurtleTypesExposed : TurtlesTypes
        {
            public void TakeArgumentsExposed(Queue<string> queue)
            {
                TakeArguments(queue);
            }
        }

        [Test]
        public void AssertAllTurtlesCanBeAdded()
        {
            var qarg = new Queue<string>();
            var tte = new TurtleTypesExposed();

            qarg.Enqueue("ATBCSRW");
            qarg.Enqueue("Fake");
            qarg.Enqueue("plouf");
            qarg.Enqueue("wow, much code");
            tte.TakeArgumentsExposed(qarg);

            Assert.IsTrue(tte.Types.Count == 7);
            Assert.IsTrue(tte.Types.Contains(typeof(ArithmeticOperatorTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(BitwiseOperatorTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(BranchConditionTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(ConditionalBoundaryTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(SequencePointDeletionTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(VariableReadTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(VariableWriteTurtle)));
        }

        [Test]
        public void AssertLowercaseAddTurtles()
        {
            var qarg = new Queue<string>();
            var tte = new TurtleTypesExposed();

            qarg.Enqueue("ATBCSRW".ToLower());
            qarg.Enqueue("Fake");
            qarg.Enqueue("plouf");
            qarg.Enqueue("wow, much code");
            tte.TakeArgumentsExposed(qarg);

            Assert.IsTrue(tte.Types.Count == 7);
            Assert.IsTrue(tte.Types.Contains(typeof(ArithmeticOperatorTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(BitwiseOperatorTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(BranchConditionTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(ConditionalBoundaryTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(SequencePointDeletionTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(VariableReadTurtle)));
            Assert.IsTrue(tte.Types.Contains(typeof(VariableWriteTurtle)));
        }
    }
}
