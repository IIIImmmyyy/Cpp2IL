using System.Collections.Generic;

namespace Cpp2IL.Core.ISIL;

public class InstructionSetIndependentInstruction : IsilOperandData
{
    public InstructionSetIndependentOpCode OpCode;
    public InstructionSetIndependentOperand[] Operands;
    public ulong ActualAddress;
    public uint InstructionIndex = 0;
    public IsilFlowControl FlowControl;

    // 仅条件跳转使用：保留原生目标，表示普通比较为假时跳转；ISIL -> IR 消费后交换两臂。
    public bool JumpWhenFalse;

    public InstructionSetIndependentInstruction(InstructionSetIndependentOpCode opCode, ulong address, IsilFlowControl flowControl, params InstructionSetIndependentOperand[] operands)
    {
        OpCode = opCode;
        Operands = operands;
        ActualAddress = address;
        FlowControl = flowControl;
        OpCode.Validate(this);
    }

    public override string ToString() => $"{InstructionIndex:000} {OpCode}{(JumpWhenFalse ? " [when false]" : "")} {string.Join(", ", Operands)}";

    /// <summary>
    /// Marks the instruction as <see cref="IsilMnemonic.Invalid"/>.
    /// </summary>
    /// <param name="reason">The reason that this instruction is being invalidated.</param>
    public void Invalidate(string reason)
    {
        OpCode = InstructionSetIndependentOpCode.Invalid;
        Operands = [InstructionSetIndependentOperand.MakeImmediate(reason)];
        FlowControl = IsilFlowControl.Continue;
        JumpWhenFalse = false;
    }
}
