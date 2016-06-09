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

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtlesMutation.Turtles
{
    /// <summary>
    /// An implementation of <see cref="IMethodTurtle" /> that identifies local
    /// variables, method parameters and fields of the same type, and permutes
    /// any reads from them. For example, if two <see cref="Int32" />
    /// parameters <c>a</c> and <c>b</c> exist, along with a local variable
    /// <c>c</c> of the same type, then a read from <c>a</c> will be replaced
    /// by one from <c>b</c> and <c>c</c> in turn, and so on.
    /// </summary>
    public class VariableReadTurtle : MethodTurtleBase
    {
        /// <summary>
        /// Gets a description of the current turtle.
        /// </summary>
        public override string Description
        {
            get { return "Replacing reads from local variables, fields and parameters."; }
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
            var variablesByType = GroupVariablesByType(method);
            PopulateOperandsInVariables(method, variablesByType);

            if (!variablesByType.Any(kv => variablesByType[kv.Key].Count > 1))
            {
                yield break;
            }

            foreach (var keyValuePair in variablesByType.Where(kv => kv.Value.Count > 1))
            {
                var variables = keyValuePair.Value.ToList();
                for (int index = 0; index < method.Body.Instructions.Count; index++)
                {
                    var instruction = method.Body.Instructions[index];
                    if (instruction.OpCode.Name.StartsWith("ldloc") && instruction.Next.OpCode == OpCodes.Ret) continue;

                    int oldIndex = -1;
                    if (instruction.OpCode == OpCodes.Ldarg)
                    {
                        var parameterDefinition = instruction.Operand as ParameterDefinition;
                        if (parameterDefinition == null)
                            continue;
                        int parameterIndex = parameterDefinition.Sequence;
                        oldIndex = variables.FindIndex(v => v.Type == VariableType.Parameter && v.Index == parameterIndex);
                    }
                    if (instruction.OpCode == OpCodes.Ldloc)
                    {
                        var variableDefinition = instruction.Operand as VariableDefinition;
                        if (variableDefinition == null)
                            continue;
                        int variableIndex = variableDefinition.Index;
                        oldIndex = variables.FindIndex(v => v.Type == VariableType.Local && v.Index == variableIndex);
                    }
                    if (instruction.OpCode == OpCodes.Ldfld)
                    {
                        var fieldDefinition = instruction.Operand as FieldDefinition;
                        if (fieldDefinition == null)
                            continue;
                        string fieldName = fieldDefinition.Name;
                        oldIndex = variables.FindIndex(v => v.Type == VariableType.Field && v.Name == fieldName);
                    }

                    if (oldIndex < 0) continue;

                    OpCode originalOpCode = instruction.OpCode;
                    object originalOperand = instruction.Operand;
                    var originalVariable = variables[oldIndex];

                    for (int newIndex = 0; newIndex < variables.Count; newIndex++)
                    {
                        if (newIndex == oldIndex) continue;
                        var variable = variables[newIndex];
                        if (variable.Operand == null) continue;

                        if (variable.Type == VariableType.Parameter
                            && instruction.OpCode == OpCodes.Ldloc
                            && instruction.Previous.OpCode == OpCodes.Stloc
                            && ((VariableDefinition)instruction.Operand).Index == ((VariableDefinition)instruction.Previous.Operand).Index
                            && instruction.Previous.Previous.OpCode == OpCodes.Ldarg
                            && ((ParameterDefinition)instruction.Previous.Previous.Operand).Index == variable.Index)
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

                        instruction.OpCode = variable.GetReadOpCode();
                        instruction.Operand = variable.Operand;

                        var description =
                            string.Format(
                                "{0:x4}: read substitution {1}.{2} => {1}.{3}",
                                originalOffsets[index],
                                keyValuePair.Key.Name,
                                originalVariable.Name,
                                variable.Name);

                        var mutantMetaData = DoYield(method, module, description, Description, index);
                        yield return mutantMetaData;
                    }
                    instruction.OpCode = originalOpCode;
                    instruction.Operand = originalOperand;
                }
            }
        }

        private static IDictionary<TypeReference, IList<Variable>> GroupVariablesByType(MethodDefinition method)
        {
            IDictionary<TypeReference, IList<Variable>> variables = new Dictionary<TypeReference, IList<Variable>>();
            int offset = method.IsStatic ? 0 : 1;
            foreach (var parameter in method.Parameters)
            {
                var type = parameter.ParameterType;
                if (!variables.ContainsKey(type))
                {
                    variables.Add(type, new List<Variable>());
                }
                variables[type].Add(new Variable(VariableType.Parameter, parameter.Index + offset, parameter.Name));
            }
            foreach (var variable in method.Body.Variables)
            {
                // HACKTAG: These variables seem to be used to store input parameters. Mutating variable
                // reads in this case may well lead to a surviving mutant.
                if (variable.Name.StartsWith("CS$6$"))
                {
                    continue;
                }
                var type = variable.VariableType;
                if (!variables.ContainsKey(type))
                {
                    variables.Add(type, new List<Variable>());
                }
                variables[type].Add(new Variable(VariableType.Local, variable.Index, variable.ToString()));
            }
            foreach (var field in method.DeclaringType.Fields)
            {
                if (field.Name == "<>1__state") continue;
                var type = field.FieldType;
                if (!variables.ContainsKey(type))
                {
                    variables.Add(type, new List<Variable>());
                }
                variables[type].Add(new Variable(VariableType.Field, -1, field.Name));
            }
            return variables;
        }

        private static void PopulateOperandsInVariables(MethodDefinition method, IDictionary<TypeReference, IList<Variable>> variables)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldarg)
                {
                    var parameterDefinition = instruction.Operand as ParameterDefinition;
                    if (parameterDefinition == null)
                        continue;
                    int sequence = parameterDefinition.Sequence;
                    if (!variables.ContainsKey(parameterDefinition.ParameterType)) continue;
                    var variable =
                        variables[parameterDefinition.ParameterType]
                            .SingleOrDefault(v => v.Type == VariableType.Parameter && v.Index == sequence);
                    if (variable != null)
                    {
                        variable.Operand = instruction.Operand;
                    }
                }
                if (instruction.OpCode == OpCodes.Ldloc)
                {
                    var variableDefinition = instruction.Operand as VariableDefinition;
                    if (variableDefinition == null)
                        continue;
                    int index = variableDefinition.Index;
                    if (!variables.ContainsKey(variableDefinition.VariableType)) continue;
                    var variable =
                        variables[variableDefinition.VariableType]
                            .SingleOrDefault(v => v.Type == VariableType.Local && v.Index == index);
                    if (variable != null)
                    {
                        variable.Operand = instruction.Operand;
                    }
                }
                if (instruction.OpCode == OpCodes.Ldfld)
                {
                    var fieldDefinition = instruction.Operand as FieldDefinition;
                    if (fieldDefinition == null)
                        continue;
                    string name = fieldDefinition.Name;
                    if (!variables.ContainsKey(fieldDefinition.FieldType)) continue;
                    var variable =
                        variables[fieldDefinition.FieldType]
                            .SingleOrDefault(v => v.Type == VariableType.Field && v.Name == name);
                    if (variable != null)
                    {
                        variable.Operand = instruction.Operand;
                    }
                }
            }
        }
    }
}
