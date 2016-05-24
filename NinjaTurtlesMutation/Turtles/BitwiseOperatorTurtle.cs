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
    /// the bitwise operators <see cref="OpCodes.Or" />,
    /// <see cref="OpCodes.And" /> and <see cref="OpCodes.Xor" /> with each
    /// of the others in turn.
    /// </summary>
    public class BitwiseOperatorTurtle : OpCodeRotationTurtle
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BitwiseOperatorTurtle" />.
        /// </summary>
        public BitwiseOperatorTurtle()
        {
            _opCodes = new Dictionary<OpCode, IEnumerable<OpCode>>
                           {
                               {OpCodes.Or, new[] {OpCodes.And, OpCodes.Xor}},
                               {OpCodes.And, new[] {OpCodes.Or, OpCodes.Xor}},
                               {OpCodes.Xor, new[] {OpCodes.Or, OpCodes.And}}
                           };
        }

        /// <summary>
        /// Gets a description of the current turtle.
        /// </summary>
        public override string Description
        {
            get { return "Rotating bitwise operators |, & and ^."; }
        }
    }
}
