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
    /// A concrete implementation of <see cref="IMethodTurtle" /> that groups
    /// variables of the same data type, and for each IL statement that writes
    /// to one of these (using Stloc), replaces that write with the write of
    /// each different variable.
    /// </summary>
    public sealed class VariableWriteSubstitutionTurtle : MethodTurtle
    {
        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public override string Description
        {
            get { return "Substituting method variables on writes/stores"; }
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
            var variablesByType = GroupMethodVariablesByType(method);

            if (!variablesByType.Any(kv => variablesByType[kv.Key].Count > 1))
            {
                yield break;
            }

            foreach (var keyValuePair in variablesByType.Where(kv => kv.Value.Count > 1))
            {
                var indices = keyValuePair.Value.ToArray();
                var ldargOperands = GetOperandsForVariables(method);
                var returnVariableIndex = GetIndexOfReturnVariableInDebugCode(method);
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode != OpCodes.Stloc) continue;
                    if (returnVariableIndex.HasValue
                        && ((VariableDefinition)instruction.Operand).Index == returnVariableIndex.Value
                        && instruction.Previous.OpCode == OpCodes.Ldc_I4
                        && (int)instruction.Previous.Operand == 0)
                    {
                        continue;
                    }
                    
                    int ldlocIndex = ((VariableDefinition)instruction.Operand).Index;
                    int oldIndex = ldlocIndex;
                    int parameterPosition = Array.IndexOf(indices, oldIndex);
                    if (parameterPosition == -1) continue;

                    OpCode originalOpCode = instruction.OpCode;
                    object originalOperand = instruction.Operand;
                    foreach (var sequence in indices)
                    {
                        if (sequence == oldIndex) continue;

                        if (instruction.IsPartOfCompilerGeneratedDispose())
                        {
                            continue;
                        }

                        instruction.OpCode = sequence >= 0 ? OpCodes.Ldloc : OpCodes.Ldarg;
                        instruction.Operand = ldargOperands[sequence];

                        var output =
                            string.Format(
                                "Variable write substitution {0}.V{1} => {0}.V{2} at {3:x4} in {4}.{5}",
                                keyValuePair.Key.Name,
                                oldIndex,
                                sequence,
                                instruction.Offset,
                                method.DeclaringType.Name,
                                method.Name);

                        yield return PrepareTests(assembly, method, fileName, output);

                    }
                    instruction.OpCode = originalOpCode;
                    instruction.Operand = originalOperand;
                }
            }
        }

        private static int? GetIndexOfReturnVariableInDebugCode(MethodDefinition method)
        {
            int? returnVariableIndex = null;
            var loadReturnVariableInstruction = method.Body.Instructions.Last().Previous;
            if (loadReturnVariableInstruction.OpCode == OpCodes.Ldloc)
            {
                returnVariableIndex = ((VariableDefinition)loadReturnVariableInstruction.Operand).Index;
            }
            if (returnVariableIndex.HasValue)
            {
                // A variable that is only ever read to be returned is either
                // injected in debug mode by the compiler, or is explicitly
                // declared and used when compiled in release mode. We treat
                // both cases the same.
                bool isVariableEverReadBeforeReturn = false;
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode != OpCodes.Ldloc) continue;
                    if (instruction == method.Body.Instructions.Last().Previous) continue;

                    if (((VariableDefinition)instruction.Operand).Index == returnVariableIndex.Value)
                    {
                        isVariableEverReadBeforeReturn = true;
                        break;
                    }
                }
                if (isVariableEverReadBeforeReturn)
                {
                    returnVariableIndex = null;
                }
            }
            return returnVariableIndex;
        }

        private static IDictionary<TypeReference, IList<int>> GroupMethodVariablesByType(MethodDefinition method)
        {
            IDictionary<TypeReference, IList<int>> variables = new Dictionary<TypeReference, IList<int>>();
            foreach (var variable in method.Body.Variables)
            {
                var type = variable.VariableType;
                if (!variables.ContainsKey(type))
                {
                    variables.Add(type, new List<int>());
                }
                variables[type].Add(variable.Index);
            }
            return variables;
        }

        private static IDictionary<int, object> GetOperandsForVariables(MethodDefinition method)
        {
            IDictionary<int, object> operands = new Dictionary<int, object>();
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Ldloc) continue;
                
                var variableDefinition = (VariableDefinition)instruction.Operand;
                int index = variableDefinition.Index;
                
                if (!operands.ContainsKey(index))
                {
                    operands.Add(index, variableDefinition);
                }
            }
            return operands;
        }
    }
}
