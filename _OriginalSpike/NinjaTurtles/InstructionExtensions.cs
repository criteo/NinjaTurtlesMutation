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
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles
{
    internal static class InstructionExtensions
    {
        internal static bool IsNumericConversion(this Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Conv_I
                || instruction.OpCode == OpCodes.Conv_I1
                || instruction.OpCode == OpCodes.Conv_I2
                || instruction.OpCode == OpCodes.Conv_I4
                || instruction.OpCode == OpCodes.Conv_I8
                || instruction.OpCode == OpCodes.Conv_R4
                || instruction.OpCode == OpCodes.Conv_R8
                || instruction.OpCode == OpCodes.Conv_U
                || instruction.OpCode == OpCodes.Conv_U1
                || instruction.OpCode == OpCodes.Conv_U2
                || instruction.OpCode == OpCodes.Conv_U4
                || instruction.OpCode == OpCodes.Conv_U8;
        }

        internal static bool IsLdcI(this Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Ldc_I4
                   || instruction.OpCode == OpCodes.Ldc_I8;
        }

        internal static bool IsStindI(this Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Stind_I
                   || instruction.OpCode == OpCodes.Stind_I1
                   || instruction.OpCode == OpCodes.Stind_I2
                   || instruction.OpCode == OpCodes.Stind_I4
                   || instruction.OpCode == OpCodes.Stind_I8;
        }

        internal static bool IsLdcR(this Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Ldc_R4
                   || instruction.OpCode == OpCodes.Ldc_R8;
        }

        internal static bool IsStindR(this Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Stind_R4
                   || instruction.OpCode == OpCodes.Stind_R8;
        }

        internal static long GetLongValue(this Instruction instruction)
        {
            return Convert.ToInt64(instruction.Operand);
        }

        internal static double GetDoubleValue(this Instruction instruction)
        {
            return Convert.ToDouble(instruction.Operand);
        }

        internal static bool FollowsSequence(this Instruction instruction, params OpCode[] sequence)
        {
            if (instruction.OpCode != sequence[0]) return false;
            if (sequence.Length == 1) return true;
            var newSequence = new OpCode[sequence.Length - 1];
            Array.Copy(sequence, 1, newSequence, 0, newSequence.Length);
            return instruction.Next.FollowsSequence(newSequence);
        }

        internal static bool IsPartOfSequence(this Instruction instruction, params OpCode[] sequence)
        {
            if (!sequence.Distinct().Contains(instruction.OpCode)) return false;
            var startInstruction = instruction;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (startInstruction == null) break;
                if (startInstruction.FollowsSequence(sequence)) return true;
                startInstruction = startInstruction.Previous;
            }
            return false;
        }

        internal static bool IsPartOfCompilerGeneratedDispose(this Instruction instruction)
        {
            if (instruction.IsPartOfSequence(OpCodes.Leave,
                OpCodes.Ldloc, OpCodes.Ldnull, OpCodes.Ceq,
                OpCodes.Stloc, OpCodes.Ldloc, OpCodes.Brtrue,
                OpCodes.Ldloc, OpCodes.Callvirt))
            {
                while (instruction.OpCode != OpCodes.Callvirt)
                {
                    instruction = instruction.Next;
                }
                var method = ((MethodReference)instruction.Operand);
                return method.Name == "Dispose";
            }
            if (instruction.IsPartOfSequence(OpCodes.Leave,
                OpCodes.Ldloc, OpCodes.Ldnull, OpCodes.Ceq,
                OpCodes.Brtrue,
                OpCodes.Ldloc, OpCodes.Callvirt))
            {
                while (instruction.OpCode != OpCodes.Callvirt)
                {
                    instruction = instruction.Next;
                }
                var method = ((MethodReference)instruction.Operand);
                return method.Name == "Dispose";
            }
            return false;
        }
    }
}
