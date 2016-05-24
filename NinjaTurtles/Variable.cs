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

using Mono.Cecil.Cil;

namespace NinjaTurtlesMutation
{
    internal class Variable
    {
        public Variable(VariableType type, int index, string name)
        {
            Type = type;
            Index = index;
            Name = name;
        }

        public VariableType Type { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public object Operand { get; set; }

        public OpCode GetReadOpCode()
        {
            switch (Type)
            {
                case VariableType.Local:
                    return OpCodes.Ldloc;
                case VariableType.Parameter:
                    return OpCodes.Ldarg;
                default:
                    return OpCodes.Ldfld;
            }
        }

        public OpCode GetWriteOpCode()
        {
            switch (Type)
            {
                case VariableType.Local:
                    return OpCodes.Stloc;
                case VariableType.Parameter:
                    return OpCodes.Starg;
                default:
                    return OpCodes.Stfld;
            }
        }
    }
}