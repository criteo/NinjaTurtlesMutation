using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;

namespace NinjaTurtles
{
    public struct SimpleInstruction
    {
        public OpCode opcode;
        public object operand;

        public SimpleInstruction(Instruction instruction)
        {
            opcode = instruction.OpCode;
            operand = instruction.Operand;
        }
    }
}
