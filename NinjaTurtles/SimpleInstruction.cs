using Mono.Cecil.Cil;

namespace NinjaTurtlesMutation
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
