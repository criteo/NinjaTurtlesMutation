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
    /// An implementation of <see cref="IMethodTurtle"/> that replaces the
    /// conditional branch operators
    /// <see cref="OpCodes.Brtrue" /> and <see cref="OpCodes.Brfalse"/> with
    /// their converse and with the operators <see cref="OpCodes.Br"/> (always
    /// branch) and <see cref="OpCodes.Nop" /> (never branch) in turn.
    /// </summary>
    public class BranchConditionTurtle : OpCodeRotationTurtle
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BranchConditionTurtle" />.
        /// </summary>
        public BranchConditionTurtle()
        {
            _opCodes = new Dictionary<OpCode, IEnumerable<OpCode>>
                           {
                               {OpCodes.Brtrue, new[] {OpCodes.Nop, OpCodes.Brfalse, OpCodes.Br}},
                               {OpCodes.Brfalse, new[] {OpCodes.Nop, OpCodes.Brtrue, OpCodes.Br}}
                           };
        }

        /// <summary>
        /// Gets a description of the current turtle.
        /// </summary>
        public override string Description
        {
            get { return "Replacing branch op codes Brtrue and Brfalse."; }
        }
    }
}