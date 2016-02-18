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

using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles.Method
{
    /// <summary>
    /// A concrete implementation of <see cref="IMethodTurtle" /> that replaces
    /// any bitwise combination operator in the method body with an alternative.
    /// </summary>
    /// <remarks>
    /// This turtle replaces each bitwise operator that it finds with each
    /// of the alternatives, from this set:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Operator</term>
    ///         <term>IL Opcode</term>
    ///     </listheader>
    ///     <item>
    ///         <description>|| / Or</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.Or" /></description>
    ///     </item>
    ///     <item>
    ///         <description>&amp;&amp; / And</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.And" /></description>
    ///     </item>
    ///     <item>
    ///         <description>^ / Xor</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.Xor" /></description>
    ///     </item>
    /// </list>
    /// Note that boolean operators compile to bitwise operators in IL, and so
    /// are also covered by this mutation.
    /// </remarks>
    public sealed class BitwiseOperatorTurtle : OpCodeRotationTurtle
    {
        private static readonly IDictionary<OpCode, IEnumerable<OpCode>> _opCodeMap 
            = new Dictionary<OpCode, IEnumerable<OpCode>>
                {
                    {OpCodes.Or, new[] {OpCodes.And, OpCodes.Xor}},
                    {OpCodes.And, new[] {OpCodes.Xor, OpCodes.Or}},
                    {OpCodes.Xor, new[] {OpCodes.And, OpCodes.Or}}
                };

        /// <summary>
        /// Defines a mapping from input opcodes to a set of replacement output
        /// opcodes for mutation purposes.
        /// </summary>
        public override IDictionary<OpCode, IEnumerable<OpCode>> OpCodeMap
        {
            get { return _opCodeMap; }
        }

        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public override string Description
        {
            get { return "Rotating boolean or bitwise operators"; }
        }
    }
}
