using System;
using System.Linq;
using Cpp2IL.Core.Extensions;

namespace Cpp2IL.Core.ISIL;

public class InstructionSetIndependentOpCode
{
    
    public static readonly InstructionSetIndependentOpCode CompareTempMove = new(IsilMnemonic.CompareTempMove, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Move = new(IsilMnemonic.Move, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode ReadSystemRegister = new(IsilMnemonic.ReadSystemRegister, 2,
        InstructionSetIndependentOperand.OperandType.Register,
        InstructionSetIndependentOperand.OperandType.SystemRegister);
    
    
    public static readonly InstructionSetIndependentOpCode LoadRegisterToVector =
        new(IsilMnemonic.LoadRegisterToVector, 2,
            InstructionSetIndependentOperand.OperandType.Register, InstructionSetIndependentOperand.OperandType.Register);
    public static readonly InstructionSetIndependentOpCode LoadImmToVector =
        new(IsilMnemonic.LoadImmToVector, 2,
            InstructionSetIndependentOperand.OperandType.Register, InstructionSetIndependentOperand.OperandType.Immediate);
    public static readonly InstructionSetIndependentOpCode BFM= new(IsilMnemonic.BFM, 4,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode MOVK = new(IsilMnemonic.MOVK, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Store = new(IsilMnemonic.Store, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Load= new(IsilMnemonic.Load, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode LoadAddress = new(IsilMnemonic.LoadAddress, 2,
        InstructionSetIndependentOperand.OperandType.NotStack,
        InstructionSetIndependentOperand.OperandType.MemoryOrStack);
    public static readonly InstructionSetIndependentOpCode BFXIL = new(IsilMnemonic.BFXIL, 4,
    InstructionSetIndependentOperand.OperandType.Any,
    InstructionSetIndependentOperand.OperandType.Any,
    InstructionSetIndependentOperand.OperandType.Any,
    InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode UBFX= new  (IsilMnemonic.UBFX, 4,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode FNMUL = new(IsilMnemonic.FNMUL, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public  static readonly InstructionSetIndependentOpCode UDIV = new(IsilMnemonic.UDIV, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode SDIV = new(IsilMnemonic.SDIV, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static   readonly InstructionSetIndependentOpCode ASR = new(IsilMnemonic.ASR, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode LSR = new(IsilMnemonic.LSR, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode FABS = new(IsilMnemonic.FABS, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode BFI= new(IsilMnemonic.BFI, 4,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode LSL= new(IsilMnemonic.LSL, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Floor = new(IsilMnemonic.Floor, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Ceiling = new(IsilMnemonic.Ceiling, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode UZP1 = new(IsilMnemonic.UZP1, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode REV64 = new(IsilMnemonic.REV64, 2,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode MADD = new(IsilMnemonic.MADD, 4,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode VectorElementStore = new(IsilMnemonic.VectorElementStore, 2,
        InstructionSetIndependentOperand.OperandType.Register, InstructionSetIndependentOperand.OperandType.Register);
    public static readonly  InstructionSetIndependentOpCode VectorElementLoad = new(IsilMnemonic.VectorElementLoad, 2,
        InstructionSetIndependentOperand.OperandType.Register, InstructionSetIndependentOperand.OperandType.Register);
    
    public static readonly InstructionSetIndependentOpCode INS= new(IsilMnemonic.INS, 2,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode DUP= new(IsilMnemonic.DUP, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode FMOV = new(IsilMnemonic.FMOV,
        2, InstructionSetIndependentOperand.OperandType.Any
    );
    public static readonly InstructionSetIndependentOpCode CastBaseType= new(IsilMnemonic.Cast2BaseType, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.CastType);
    public static readonly InstructionSetIndependentOpCode FloatToInteger = new(IsilMnemonic.FloatToInteger, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.FloatToIntegerConversion);
    public static readonly InstructionSetIndependentOpCode IntegerToFloat = new(IsilMnemonic.IntegerToFloat, 3,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.IntegerToFloatConversion);
    public static readonly InstructionSetIndependentOpCode Call = new(IsilMnemonic.Call);
    public static readonly InstructionSetIndependentOpCode VirtualCall = new(IsilMnemonic.VirtualCall);
    public static readonly InstructionSetIndependentOpCode VTableCall = new(IsilMnemonic.VTableCall,1,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode RGCTXCall = new(IsilMnemonic.RGCTXCall,1,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode CallNoReturn = new(IsilMnemonic.CallNoReturn);
    public static readonly InstructionSetIndependentOpCode Exchange = new(IsilMnemonic.Exchange, 2,
        InstructionSetIndependentOperand.OperandType.NotStack, InstructionSetIndependentOperand.OperandType.NotStack);
    
    public static readonly InstructionSetIndependentOpCode ExtractVector =  new(IsilMnemonic.ExtractVector, 4,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode SIMDMath = new(IsilMnemonic.SIMDMath, 4,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any ,InstructionSetIndependentOperand.OperandType.SimdMathType);
    public static readonly InstructionSetIndependentOpCode VectorPairwiseAdd = new(IsilMnemonic.VectorPairwiseAdd, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Add = new(IsilMnemonic.Add, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode Subtract = new(IsilMnemonic.Subtract, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    
    public static readonly InstructionSetIndependentOpCode FMAX = new(IsilMnemonic.FMAX, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Multiply = new(IsilMnemonic.Multiply, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode Divide = new(IsilMnemonic.Divide, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode ShiftLeft = new(IsilMnemonic.ShiftLeft, 2,
        InstructionSetIndependentOperand.OperandType.NotStack, InstructionSetIndependentOperand.OperandType.NotStack);

    public static readonly InstructionSetIndependentOpCode ShiftRight = new(IsilMnemonic.ShiftRight, 2,
        InstructionSetIndependentOperand.OperandType.NotStack, InstructionSetIndependentOperand.OperandType.NotStack);

    public static readonly InstructionSetIndependentOpCode And = new(IsilMnemonic.And, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode Or = new(IsilMnemonic.Or, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode Xor = new(IsilMnemonic.Xor, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    
    public static readonly InstructionSetIndependentOpCode FMINNM= new(IsilMnemonic.FMINNM, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode FMIN = new(IsilMnemonic.FMIN, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode FSQRT = new(IsilMnemonic.FSQRT, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode FABD = new(IsilMnemonic.FABD, 3,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any,
        InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode Not = new(IsilMnemonic.Not, 2,
        InstructionSetIndependentOperand.OperandType.Any,
            InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode Neg = new(IsilMnemonic.Neg, 2,
        InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode Compare = new(IsilMnemonic.Compare, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);

    //public static readonly InstructionSetIndependentOpCode CompareNotEqual = new(IsilMnemonic.CompareNotEqual, 2, InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    //public static readonly InstructionSetIndependentOpCode CompareLessThan = new(IsilMnemonic.CompareLessThan, 2, InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    //public static readonly InstructionSetIndependentOpCode CompareGreaterThan = new(IsilMnemonic.CompareGreaterThan, 2, InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    //public static readonly InstructionSetIndependentOpCode CompareLessThanOrEqual = new(IsilMnemonic.CompareLessThanOrEqual, 2, InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    //public static readonly InstructionSetIndependentOpCode CompareGreaterThanOrEqual = new(IsilMnemonic.CompareGreaterThanOrEqual, 2, InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Any);
    public static readonly InstructionSetIndependentOpCode ShiftStack = new(IsilMnemonic.ShiftStack, 1,
        InstructionSetIndependentOperand.OperandType.Immediate);

    public static readonly InstructionSetIndependentOpCode Push = new(IsilMnemonic.Push, 2,
        InstructionSetIndependentOperand.OperandType.Register, InstructionSetIndependentOperand.OperandType.Any);

    public static readonly InstructionSetIndependentOpCode Pop = new(IsilMnemonic.Pop, 2,
        InstructionSetIndependentOperand.OperandType.Any, InstructionSetIndependentOperand.OperandType.Register);

    public static readonly InstructionSetIndependentOpCode Return = new(IsilMnemonic.Return, 1,
        InstructionSetIndependentOperand.OperandType.NotStack);

    public static readonly InstructionSetIndependentOpCode Goto = new(IsilMnemonic.Goto, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfEqual = new(IsilMnemonic.JumpIfEqual, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfNotEqual = new(IsilMnemonic.JumpIfNotEqual, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfSign = new(IsilMnemonic.JumpIfSign, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfNotSign = new(IsilMnemonic.JumpIfNotSign, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfGreater = new(IsilMnemonic.JumpIfGreater, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfLess = new(IsilMnemonic.JumpIfLess, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfGreaterOrEqual = new(IsilMnemonic.JumpIfGreaterOrEqual,
        1, InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode JumpIfLessOrEqual = new(IsilMnemonic.JumpIfLessOrEqual, 1,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode AssignIfNotEqual = new(IsilMnemonic.AssignIfNotEqual, 3,
        InstructionSetIndependentOperand.OperandType.Instruction);
    
    public static readonly InstructionSetIndependentOpCode AssignIfGreaterThan = new(IsilMnemonic.AssignIfGreaterThan, 3,
        InstructionSetIndependentOperand.OperandType.Instruction);  
    public static readonly InstructionSetIndependentOpCode AssignIfGreaterOrEqual= new(IsilMnemonic.AssignIfGreaterOrEqual, 3,
        InstructionSetIndependentOperand.OperandType.Instruction);
    public static readonly InstructionSetIndependentOpCode AssignIfEqual = new(IsilMnemonic.AssignIfEqual, 3,
        InstructionSetIndependentOperand.OperandType.Instruction);

    public static readonly InstructionSetIndependentOpCode AssignIfLessThan = new(IsilMnemonic.AssignIfLessThan, 3,
        InstructionSetIndependentOperand.OperandType.Instruction);
    public static readonly InstructionSetIndependentOpCode AssignIfLessOrEqual = new(IsilMnemonic.AssignIfLessOrEqual, 3,
        InstructionSetIndependentOperand.OperandType.Instruction);
    public static readonly InstructionSetIndependentOpCode Interrupt = new(IsilMnemonic.Interrupt, 0);
    public static readonly InstructionSetIndependentOpCode Nop = new(IsilMnemonic.Nop, 0);

    public static readonly InstructionSetIndependentOpCode NotImplemented = new(IsilMnemonic.NotImplemented, 1,
        InstructionSetIndependentOperand.OperandType.Immediate);

    public static readonly InstructionSetIndependentOpCode Invalid = new(IsilMnemonic.Invalid, 1,
        InstructionSetIndependentOperand.OperandType.Immediate);
    
    public static readonly InstructionSetIndependentOpCode StackFrame= new(IsilMnemonic.StackFrame, 1,
        InstructionSetIndependentOperand.OperandType.StackOffset);

    public readonly IsilMnemonic Mnemonic;
    public readonly InstructionSetIndependentOperand.OperandType[] PermittedOperandTypes;
    public readonly int MaxOperands;

    public InstructionSetIndependentOpCode(IsilMnemonic mnemonic)
    {
        Mnemonic = mnemonic;
        MaxOperands = int.MaxValue;
        PermittedOperandTypes = [];
    }

    public InstructionSetIndependentOpCode(IsilMnemonic mnemonic, int maxOperands)
    {
        Mnemonic = mnemonic;
        MaxOperands = maxOperands;
        PermittedOperandTypes = InstructionSetIndependentOperand.OperandType.Any.Repeat(maxOperands).ToArray();
    }

    private InstructionSetIndependentOpCode(IsilMnemonic mnemonic, int maxOperands,
        params InstructionSetIndependentOperand.OperandType[] permittedOperandTypes)
    {
        Mnemonic = mnemonic;
        PermittedOperandTypes = permittedOperandTypes;
        MaxOperands = maxOperands;
    }

    public void Validate(InstructionSetIndependentInstruction instruction)
    {
        var operands = instruction.Operands;

        if (operands.Length > MaxOperands)
        {
            instruction.Invalidate($"Too many operands! We have {operands.Length} but we only allow {MaxOperands}");
            return;
        }

        if (PermittedOperandTypes.Length == 0)
            return;
        if (Mnemonic.ToString().StartsWith("Assign")
            || Mnemonic==IsilMnemonic.StackFrame)
        {
            return;
        }

        if (Mnemonic==IsilMnemonic.FMOV || Mnemonic==IsilMnemonic.Neg)
        {
            return;
        }
        for (var i = 0; i < operands.Length; i++)
        {
            if ((operands[i].Type & PermittedOperandTypes[i]) == 0)
            {
                instruction.Invalidate(
                    $"Operand {operands[i]} at index {i} (0-based) is of type {operands[i].Type}, which is not permitted for this index of a {Mnemonic} instruction");
                return;
            }
        }
    }

    public override string ToString() => Mnemonic.ToString();
}
