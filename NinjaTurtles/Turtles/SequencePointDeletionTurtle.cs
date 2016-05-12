#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012-14 David Musgrove and others.

#endregion

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles
{
    /// <summary>
    /// An implementation of <see cref="IMethodTurtle" /> that removes from the
    /// compiled IL each sequence point in turn (with the exception of
    /// structurally vital ones and compiler generated ones).
    /// </summary>
    public class SequencePointDeletionTurtle : MethodTurtleBase
    {
        /// <summary>
        /// Gets a description of the current turtle.
        /// </summary>
        public override string Description
        {
            get { return "Deleting sequence points from IL."; }
        }

        /// <summary>
        /// Performs the actual code mutations, returning each with
        /// <c>yield</c> for the calling code to use.
        /// </summary>
        /// <remarks>
        /// Implementing classes should yield the result obtained by calling
        /// the <see mref="DoYield" /> method.
        /// </remarks>
        /// <param name="method">
        ///     A <see cref="MethodDefinition" /> for the method on which mutation
        ///     testing is to be carried out.
        /// </param>
        /// <param name="module">
        ///     A <see cref="Module" /> representing the main module of the
        ///     containing assembly.
        /// </param>
        /// <param name="originalOffsets"></param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutantMetaData" /> structures.
        /// </returns>
        protected override IEnumerable<MutantMetaData> CreateMutant(MethodDefinition method, Module module, int[] originalOffsets)
        {
            var sequence = new Dictionary<int, OpCode>();
            var startIndex = -1;
            var instructionsMaxIndex = method.Body.Instructions.Count - 1;
            for (int index = 0; index < method.Body.Instructions.Count; index++)
            {
                var instruction = method.Body.Instructions[index];
                if (IsSequenceStartingInstruction(instruction))
                {
                    startIndex = index;
                    sequence.Clear();
                }
                if (startIndex >= 0)
                    sequence.Add(index, instruction.OpCode);
                if (!IsLastSequenceInstruction(index, instructionsMaxIndex, instruction) || !ShouldDeleteSequence(method.Body, sequence))
                    continue;
                var originalInstruction = ReplaceOpcodeAndOperand(method, startIndex, OpCodes.Br, instruction.Next);

                var codes = string.Join(", ", sequence.Values.Select(o => o.Code));
                var description = string.Format("{0:x4}: deleting {1}", originalOffsets[startIndex], codes);
                MutantMetaData mutation = DoYield(method, module, description, Description, startIndex);
                yield return mutation;

                ReplaceOpcodeAndOperand(method, startIndex, originalInstruction.opcode, originalInstruction.operand);
            }
        }

        private bool IsLastSequenceInstruction(int currentIndex, int maxIndex, Instruction instruction)
        {
            return currentIndex == maxIndex || instruction.Next.SequencePoint != null;
        }

        private bool IsSequenceStartingInstruction(Instruction instruction)
        {
            return instruction.SequencePoint != null && instruction.SequencePoint.StartLine != 0xfeefee;
        }

        private bool ShouldDeleteSequence(MethodBody method, IDictionary<int, OpCode> opCodes)
        {
            if (opCodes.Values.All(o => o == OpCodes.Nop)) return false;
            if (opCodes.Values.All(o => o == OpCodes.Pop)) return false;
            if (opCodes.Values.All(o => o == OpCodes.Leave)) return false;
            if (opCodes.Values.Any(o => o == OpCodes.Ret)) return false;

            if (IsCompilerGeneratedDebugReturn(method, opCodes))
                return false;

            if (IsCallingBaseConstructor(method, opCodes))
                return false;

            // If compiler-generated dispose, don't delete.
            if (method.Instructions[opCodes.Keys.First()].IsPartOfCompilerGeneratedDispose())
                return false;

            // If setting default value to a field, don't delete.
            if (method.Instructions[opCodes.Keys.First()].FollowsSequence(OpCodes.Ldarg, OpCodes.Ldc_I4, OpCodes.Stfld)
                && (int)method.Instructions[opCodes.Keys.First() + 1].Operand == 0)
                return false;

            return true;
        }

        private static bool IsCompilerGeneratedDebugReturn(MethodBody method, IDictionary<int, OpCode> opCodes)
        {
            // If just setting compiler-generated return variable in Debug mode, don't delete.
            if (opCodes.Values.Last().Code != Code.Br)
                return false;
            if (method.Instructions.Count - 1 < opCodes.Keys.Last() + 2)
                return false;
            if (((Instruction)method.Instructions[opCodes.Keys.Last()].Operand).Offset !=
                method.Instructions[opCodes.Keys.Last() + 1].Offset)
                return false;
            if (method.Instructions[opCodes.Keys.Last() + 2].OpCode != OpCodes.Ret)
                return false;
            return true;
        }

        private static bool IsCallingBaseConstructor(MethodBody method, IDictionary<int, OpCode> opCodes)
        {
            // If calling base constructor, don't delete.
            if (opCodes.All(kv => kv.Value != OpCodes.Call))
                return false;
            if (((MethodReference)method.Instructions[opCodes.First(kv => kv.Value == OpCodes.Call).Key].Operand).Name !=
                Methods.CONSTRUCTOR)
                return false;
            return true;
        }
    }
}
