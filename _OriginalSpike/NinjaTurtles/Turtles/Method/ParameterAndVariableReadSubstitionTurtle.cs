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
    /// parameters and variables of the same data type, and for each IL
    /// statement that reads one of these (using Ldarg or Ldloc), replaces that
    /// read with the read of each different parameter or variable.
    /// </summary>
    public sealed class ParameterAndVariableReadSubstitutionTurtle : MethodTurtle
    {
        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public override string Description
        {
            get { return "Substituting method parameters and variables on reads/loads"; }
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
            var parametersAndVariablesByType = GroupMethodParametersAndVariablesByType(method);

            if (!parametersAndVariablesByType.Any(kv => parametersAndVariablesByType[kv.Key].Count > 1))
            {
                yield break;
            }

            foreach (var keyValuePair in parametersAndVariablesByType.Where(kv => kv.Value.Count > 1))
            {
                var indices = keyValuePair.Value.ToArray();
                var ldargOperands = GetOperandsForParametersAndVariables(method);
                foreach (var instruction in method.Body.Instructions)
                {
                    int? oldIndex = null;
                    if (instruction.OpCode == OpCodes.Ldarg)
                    {
                        int ldargIndex = ((ParameterDefinition)instruction.Operand).Sequence;
                        if (method.IsStatic || ldargIndex > 0)
                        {
                            oldIndex = -1 - ldargIndex;
                        }
                    }
                    if (instruction.OpCode == OpCodes.Ldloc && instruction.Next.OpCode != OpCodes.Ret)
                    {
                        int ldlocIndex = ((VariableDefinition)instruction.Operand).Index;
                        oldIndex = ldlocIndex;
                    }
                    
                    if (!oldIndex.HasValue) continue;
                    
                    int parameterPosition = Array.IndexOf(indices, oldIndex.Value);
                    if (parameterPosition == -1) continue;

                    OpCode originalOpCode = instruction.OpCode;
                    object originalOperand = instruction.Operand;
                    foreach (var sequence in indices)
                    {
                        if (sequence == oldIndex.Value) continue;

                        if (instruction.OpCode == OpCodes.Ldloc
                            && instruction.Previous.OpCode == OpCodes.Stloc
                            && ((VariableDefinition)instruction.Operand).Index == ((VariableDefinition)instruction.Previous.Operand).Index
                            && instruction.Previous.Previous.OpCode == OpCodes.Ldarg
                            && ((ParameterDefinition)instruction.Previous.Previous.Operand).Index == -1 -sequence)
                        {
                            // The .NET compiler sometimes adds a pointless
                            // cache of a parameter into a local variable
                            // (oddly, Mono doesn't seem to). We need to not
                            // mutate in this scenario.
                            continue;
                        }

                        if (instruction.IsPartOfCompilerGeneratedDispose())
                        {
                            continue;
                        }
                            
                        instruction.OpCode = sequence >= 0 ? OpCodes.Ldloc : OpCodes.Ldarg;
                        instruction.Operand = ldargOperands[sequence];

                        var output =
                            string.Format(
                                "Parameter/variable read substitution {0}.{1} => {0}.{2} at {3:x4} in {4}.{5}",
                                keyValuePair.Key.Name,
                                GetIndexAsString(oldIndex.Value),
                                GetIndexAsString(sequence),
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

        private string GetIndexAsString(int index)
        {
            return index < 0 ? "P" + (-1 - index) : "V" + index;
        }

        private static IDictionary<TypeReference, IList<int>> GroupMethodParametersAndVariablesByType(MethodDefinition method)
        {
            IDictionary<TypeReference, IList<int>> parametersAndVariables = new Dictionary<TypeReference, IList<int>>();
            int offset = method.IsStatic ? 0 : 1;
            foreach (var parameter in method.Parameters)
            {
                var type = parameter.ParameterType;
                if (!parametersAndVariables.ContainsKey(type))
                {
                    parametersAndVariables.Add(type, new List<int>());
                }
                parametersAndVariables[type].Add(-1 - parameter.Index - offset);
            }
            foreach (var variable in method.Body.Variables)
            {
                var type = variable.VariableType;
                if (!parametersAndVariables.ContainsKey(type))
                {
                    parametersAndVariables.Add(type, new List<int>());
                }
                parametersAndVariables[type].Add(variable.Index);
            }
            return parametersAndVariables;
        }

        private static IDictionary<int, object> GetOperandsForParametersAndVariables(MethodDefinition method)
        {
            IDictionary<int, object> operands = new Dictionary<int, object>();
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldarg)
                {
                    var parameterDefinition = (ParameterDefinition)instruction.Operand;
                    int sequence = parameterDefinition.Sequence;
                    if (!operands.ContainsKey(-1 - sequence))
                    {
                        operands.Add(-1 - sequence, parameterDefinition);
                    }
                }
            }
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldloc)
                {
                    var variableDefinition = (VariableDefinition)instruction.Operand;
                    int index = variableDefinition.Index;
                    if (!operands.ContainsKey(index))
                    {
                        operands.Add(index, variableDefinition);
                    }
                }
            }
            return operands;
        }
    }
}
