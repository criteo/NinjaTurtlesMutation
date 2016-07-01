using System;
using System.Collections.Generic;
using NinjaTurtlesMutation.Turtles;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class TurtlesTypes : Option
    {
        public HashSet<Type> Types = new HashSet<Type>();

        private static readonly IDictionary<char, Type> _turtlesKeys = new Dictionary<char, Type>()
        {
            { 'A', typeof(ArithmeticOperatorTurtle) },
            { 'T', typeof(BitwiseOperatorTurtle) },
            { 'B', typeof(BranchConditionTurtle) },
            { 'C', typeof(ConditionalBoundaryTurtle) },
            { 'S', typeof(SequencePointDeletionTurtle) },
            { 'R', typeof(VariableReadTurtle) },
            { 'W', typeof(VariableWriteTurtle) },
        };

        protected override void TakeArguments(Queue<string> queue)
        {
            var arg = queue.Dequeue();
            arg = arg.ToUpper();
            foreach (char c in arg)
            {
                if (!_turtlesKeys.ContainsKey(c))
                    continue;
                Types.Add(_turtlesKeys[c]);
            }
        }
    }
}
