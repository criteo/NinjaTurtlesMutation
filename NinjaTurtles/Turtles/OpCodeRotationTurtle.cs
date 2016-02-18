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

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles
{
    /// <summary>
    /// An abstract base class for implementations of
    /// <see cref="IMethodTurtle" /> that operator by replacing a number of
    /// IL OpCodes with a list of replacements in turn.
    /// </summary>
    /// <remarks>
    /// Classes extending this one only need to set the value of the
    /// <see fref="_opCodes" /> field to an appropriate dictionary of source
    /// and target OpCodes.
    /// </remarks>
    public abstract class OpCodeRotationTurtle : MethodTurtleBase
    {
        /// <summary>
        /// An <see cref="IDictionary{K,V}" /> containing source OpCodes as
        /// keys, and <see cref="IEnumerable{T}" />s of OpCodes as each key's
        /// possible replacements.
        /// </summary>
        protected IDictionary<OpCode, IEnumerable<OpCode>> _opCodes;

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
            for (int index = 0; index < method.Body.Instructions.Count; index++)
            {
                var instruction = method.Body.Instructions[index];
                if (_opCodes.ContainsKey(instruction.OpCode))
                {
                    if (instruction.IsMeaninglessUnconditionalBranch()) continue;

                    var originalOpCode = instruction.OpCode;

                    foreach (var opCode in _opCodes[originalOpCode])
                    {
                        instruction.OpCode = opCode;
                        var description = string.Format("{0:x4}: {1} => {2}", originalOffsets[index], originalOpCode.Code, opCode.Code);
                        MutantMetaData mutation = DoYield(method, module, description, index);
                        yield return mutation;
                    }

                    instruction.OpCode = originalOpCode;
                }
            }
        }
    }
}
