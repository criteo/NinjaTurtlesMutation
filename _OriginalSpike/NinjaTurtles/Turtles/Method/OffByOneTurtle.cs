using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles.Method
{
    /// <summary>
    /// A concrete implementation of <see cref="IMethodTurtle" /> that increments
    /// and decrements integer constants.  This should catch a class of error where
    /// assertions are not carried out on results of for loops and their ilk.
    /// </summary>
    public class OffByOneTurtle : MethodTurtle
    {
        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public override string Description
        {
            get { return @"Incremeting/Decrementing integer constants in turn"; }
        }

        /// <summary>
        /// When implemented in a subclass, performs the actual mutations on
        /// the source assembly
        /// </summary>
        /// <param name="method">A <see cref="MethodDefinition"/> for the method on which mutation
        /// testing is to be carried out.</param>
        /// <param name="assembly">An <see cref="AssemblyDefinition"/> for the containing assembly.</param>
        /// <param name="fileName">The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of
        /// <see cref="MutationTestMetaData"/> structures.
        /// </returns>
        protected override IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                if (ShouldMutateOpCodePlus1(instruction))
                {
                    var originalOperand = instruction.Operand;
                    yield return OffByOneDoMutate(method, assembly, fileName, instruction, 1);

                    instruction.Operand = originalOperand;
                }

                if (ShouldMutateOpCodeMinus1(instruction))
                {
                    var originalOperand = instruction.Operand;
                    yield return OffByOneDoMutate(method, assembly, fileName, instruction, -1);

                    instruction.Operand = originalOperand;
                }
            }
        }

        private MutationTestMetaData OffByOneDoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName,
                                                      Instruction instruction, int offset)
        {
            var originalOperand = instruction.Operand;
            instruction.Operand = (int)originalOperand + offset;

            var output = string.Format("OpCode {0}: {1} {2} -> {3} at {4:x4} in {5}.{6}", offset,
                                       instruction.OpCode.Name, originalOperand, instruction.Operand,
                                       instruction.Offset, method.DeclaringType.Name, method.Name);

            return PrepareTests(assembly, method, fileName, output);
        }

        private static bool ShouldMutateOpCodePlus1(Instruction instruction)
        {
            if (!(instruction.OpCode == OpCodes.Ldc_I4 || instruction.OpCode == OpCodes.Ldc_I8))
            {
                return false;
            }

            if (instruction.Next.OpCode == OpCodes.Add)
            {
                return false;
            }

            if (instruction.Next.OpCode == OpCodes.Newarr)
            {
                return false;
            }
            return true;
        }

        private static bool ShouldMutateOpCodeMinus1(Instruction instruction)
        {
            if (!(instruction.OpCode == OpCodes.Ldc_I4 || instruction.OpCode == OpCodes.Ldc_I8))
            {
                return false;
            }

            if (instruction.Next.OpCode == OpCodes.Sub)
            {
                return false;
            }

            return true;
        }
    }
}
