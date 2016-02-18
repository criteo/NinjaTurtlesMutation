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
    /// any arithmetic operator in the method body with an alternative.
    /// </summary>
    /// <remarks>
    /// This turtle replaces each arithmetic operator that it finds with each
    /// of the alternatives, from this set:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Operator</term>
    ///         <term>IL Opcode</term>
    ///     </listheader>
    ///     <item>
    ///         <description>+</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.Add" /></description>
    ///     </item>
    ///     <item>
    ///         <description>-</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.Sub" /></description>
    ///     </item>
    ///     <item>
    ///         <description>*</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.Mul" /></description>
    ///     </item>
    ///     <item>
    ///         <description>/</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.Div" /></description>
    ///     </item>
    ///     <item>
    ///         <description>% / Mod</description>
    ///         <description><see cref="System.Reflection.Emit.OpCodes.Rem" /></description>
    ///     </item>
    /// </list>
    /// </remarks>
    public sealed class ArithmeticOperatorTurtle : OpCodeRotationTurtle
    {
        private static readonly IDictionary<OpCode, IEnumerable<OpCode>> _opCodeMap 
            = new Dictionary<OpCode, IEnumerable<OpCode>>
                {
                    {OpCodes.Add, new[] {OpCodes.Rem, OpCodes.Sub, OpCodes.Mul, OpCodes.Div}},
                    {OpCodes.Sub, new[] {OpCodes.Rem, OpCodes.Add, OpCodes.Mul, OpCodes.Div}},
                    {OpCodes.Mul, new[] {OpCodes.Rem, OpCodes.Sub, OpCodes.Add, OpCodes.Div}},
                    {OpCodes.Div, new[] {OpCodes.Rem, OpCodes.Sub, OpCodes.Mul, OpCodes.Add}},
                    {OpCodes.Rem, new[] {OpCodes.Add, OpCodes.Sub, OpCodes.Mul, OpCodes.Div}}
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
            get { return "Rotating arithmetic operators"; }
        }
    }
}
