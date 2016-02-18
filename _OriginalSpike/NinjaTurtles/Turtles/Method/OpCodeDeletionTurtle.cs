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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles.Method
{
    /// <summary>
    /// A concrete implementation of <see cref="IMethodTurtle" /> that deletes
    /// each IL statement in the method body in turn, yielding at each stage
    /// to allow the test suite to be run. Statements that have no effect (e.g.
    /// a Nop or a branch to the next statement) are skipped, as their deletion
    /// will always create an equivalent mutant.    
    /// </summary>
    public sealed class OpCodeDeletionTurtle : MethodTurtle
    {
        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public override string Description
        {
            get { return "Deleting OpCodes in turn"; }
        }

        /// <summary>
        /// When implemented in a subclass, performs the actual mutations on
        /// the source assembly
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="assembly">
        /// An <see cref="AssemblyDefinition" /> for the containing assembly.
        /// </param>
        /// <param name="fileName">
        /// The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutationTestMetaData" /> structures.
        /// </returns>
        protected override IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                if (!ShouldDeleteOpCode(instruction)) continue;

                var originalCode = instruction.OpCode;
                var originalOperand = instruction.Operand;

                instruction.OpCode = OpCodes.Nop;
                instruction.Operand = null;
                var output = string.Format("OpCode deletion {0} at {1:x4} in {2}.{3}", originalCode.Name,
                                           instruction.Offset, method.DeclaringType.Name, method.Name);

                yield return PrepareTests(assembly, method, fileName, output);

                instruction.OpCode = originalCode;
                instruction.Operand = originalOperand;
            }
        }

        private bool ShouldDeleteOpCode(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Newobj) return false;
            if (instruction.OpCode == OpCodes.Nop) return false;
            if (instruction.OpCode == OpCodes.Ret) return false;
            if (instruction.OpCode == OpCodes.Br
                && instruction.Next.Offset == ((Instruction)instruction.Operand).Offset)
            {
                return false;
            }
            if (instruction.OpCode == OpCodes.Endfinally) return false;
            if (instruction.IsNumericConversion()) return false;
            if (IsOpCodeSettingOutputParameterToDefaultValue(instruction)) return false;
            if (IsOpCodeSettingLocalVariableToDefaultValue(instruction)) return false;
            if (IsOpCodeSwitchDefaultThatHasAlreadyBeenCateredFor(instruction)) return false;
            if (instruction.IsPartOfCompilerGeneratedDispose()) return false;
            return true;
        }

        private static bool IsOpCodeSwitchDefaultThatHasAlreadyBeenCateredFor(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Br
                && instruction.Previous.OpCode == OpCodes.Switch
                && ((Instruction[])instruction.Previous.Operand).Any(i => i.Offset == ((Instruction)instruction.Operand).Offset))
            {
                return true;
            }
            return false;
        }

        private static bool IsOpCodeSettingLocalVariableToDefaultValue(Instruction instruction)
        {
            if (instruction.Next.OpCode == OpCodes.Stloc
                && instruction.IsLdcI()
                && instruction.GetLongValue() == 0L)
            {
                return true;
            }
            if (instruction.OpCode == OpCodes.Stloc
                && instruction.Previous.IsLdcI()
                && instruction.Previous.GetLongValue() == 0L)
            {
                return true;
            }
            if (instruction.Next.OpCode == OpCodes.Stloc
                && instruction.IsLdcR()
// ReSharper disable CompareOfFloatsByEqualityOperator
                && instruction.GetDoubleValue() == 0D)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                return true;
            }
            if (instruction.OpCode == OpCodes.Stloc
                && instruction.Previous.IsLdcR()
// ReSharper disable CompareOfFloatsByEqualityOperator
                && instruction.Previous.GetDoubleValue() == 0D)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                return true;
            }
            if (instruction.OpCode == OpCodes.Stobj
                && instruction.Previous.OpCode == OpCodes.Ldnull
                && instruction.Previous.Previous.OpCode == OpCodes.Ldloca)
            {
                return true;
            }
            return false;
        }

        private static bool IsOpCodeSettingOutputParameterToDefaultValue(Instruction instruction)
        {
            if (instruction.Next.IsStindI()
                && instruction.IsLdcI()
                && instruction.GetLongValue() == 0L)
            {
                return true;
            }
            if (instruction.IsStindI()
                && instruction.Previous.IsLdcI()
                && instruction.Previous.GetLongValue() == 0L)
            {
                return true;
            }
            if (instruction.Next.IsStindR()
                && instruction.IsLdcR()
// ReSharper disable CompareOfFloatsByEqualityOperator
                && instruction.GetLongValue() == 0D)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                return true;
            }
            if (instruction.IsStindR()
                && instruction.Previous.IsLdcR()
                // ReSharper disable CompareOfFloatsByEqualityOperator
                && instruction.Previous.GetLongValue() == 0D)
            // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                return true;
            }
            if (instruction.OpCode == OpCodes.Ldnull
                && instruction.Next.OpCode == OpCodes.Stind_Ref)
            {
                return true;
            }
            if (instruction.Next.OpCode == OpCodes.Ldnull
                && instruction.Next.Next.OpCode == OpCodes.Stind_Ref)
            {
                return true;
            }
            if (instruction.OpCode == OpCodes.Stind_Ref
                && instruction.Previous.OpCode == OpCodes.Ldnull)
            {
                return true;
            }
            return false;
        }
    }
}
      
