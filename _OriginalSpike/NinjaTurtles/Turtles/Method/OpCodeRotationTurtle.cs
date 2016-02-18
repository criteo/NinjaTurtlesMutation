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

using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles.Method
{
    /// <summary>
    /// An abstract implementation of <see cref="IMethodTurtle" /> that
    /// replaces opcode according to the mapping specified in
    /// <see pref="OpCodeMap" />.
    /// </summary>
    public abstract class OpCodeRotationTurtle : MethodTurtle
    {
        /// <summary>
        /// Defines a mapping from input opcodes to a set of replacement output
        /// opcodes for mutation purposes.
        /// </summary>
        public abstract IDictionary<OpCode, IEnumerable<OpCode>> OpCodeMap { get; } 

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
                if (!OpCodeMap.Any(o => o.Key.Equals(instruction.OpCode))) continue;

                var originalCode = instruction.OpCode;
                foreach (var opCode in OpCodeMap[originalCode].Where(opCode => originalCode != opCode))
                {
                    if (instruction.IsPartOfCompilerGeneratedDispose())
                    {
                        continue;
                    }

                    instruction.OpCode = opCode;
                    var output = string.Format("OpCode change {0} => {1} at {2:x4} in {3}.{4}",
                                               originalCode.Name, opCode.Name, instruction.Offset,
                                               method.DeclaringType.Name, method.Name);

                    yield return PrepareTests(assembly, method, fileName, output);
                }
                instruction.OpCode = originalCode;
            }
        }
    }
}