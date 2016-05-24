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
using Mono.Cecil.Cil;

namespace NinjaTurtlesMutation.Turtles
{
    /// <summary>
    /// An implementation of <see cref="IMethodTurtle"/> that replaces each of
    /// the arithmetic operators <see cref="OpCodes.Add" />,
    /// <see cref="OpCodes.Sub" />, <see cref="OpCodes.Mul" />,
    /// <see cref="OpCodes.Div" /> and <see cref="OpCodes.Rem" /> with each
    /// of the others in turn.
    /// </summary>
    public class ArithmeticOperatorTurtle : OpCodeRotationTurtle
	{
        /// <summary>
        /// Initializes a new instance of 
        /// <see cref="ArithmeticOperatorTurtle" />.
        /// </summary>
        public ArithmeticOperatorTurtle()
        {
            _opCodes = new Dictionary<OpCode, IEnumerable<OpCode>>
                           {
                               {OpCodes.Add, new[] {OpCodes.Sub, OpCodes.Mul, OpCodes.Div, OpCodes.Rem}},
                               {OpCodes.Sub, new[] {OpCodes.Add, OpCodes.Mul, OpCodes.Div, OpCodes.Rem}},
                               {OpCodes.Mul, new[] {OpCodes.Add, OpCodes.Sub, OpCodes.Div, OpCodes.Rem}},
                               {OpCodes.Div, new[] {OpCodes.Add, OpCodes.Sub, OpCodes.Mul, OpCodes.Rem}},
                               {OpCodes.Rem, new[] {OpCodes.Add, OpCodes.Sub, OpCodes.Mul, OpCodes.Div}}
                           };
        }

        /// <summary>
        /// Gets a description of the current turtle.
        /// </summary>
        public override string Description
        {
            get { return "Rotating arithmetic operators +, -, *, / and %."; }
        }
	}
}

