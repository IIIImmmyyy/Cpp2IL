using System;
using System.Collections.Generic;
using System.Linq;
using Cpp2IL.Core.Exceptions;
using Cpp2IL.Core.InstructionSets;
using Cpp2IL.Core.Model.Contexts;
using Disarm;

namespace Cpp2IL.Core.ISIL;

public class IsilBuilder
{
    public List<InstructionSetIndependentInstruction> BackingStatementList;

    public Dictionary<ulong, List<InstructionSetIndependentInstruction>> InstructionAddressMap;

    // (Goto instruction, target instruction address)
    private readonly List<(InstructionSetIndependentInstruction, ulong)> _jumpsToFix;

    private Dictionary<ulong, Arm64Instruction> CompareMap = new();
    public List<Arm64Instruction> Arm64Instructions= new();
    public IsilBuilder(Dictionary<ulong, Arm64Instruction> compareList,List<Arm64Instruction> arm64Instructions)
    {
        CompareMap = compareList;
        Arm64Instructions = arm64Instructions;
        BackingStatementList = [];
        InstructionAddressMap = new();
        _jumpsToFix = [];
    }

    public IsilBuilder()
    {
        BackingStatementList = [];
        InstructionAddressMap = new();
        _jumpsToFix = [];
    }

    public IsilBuilder(List<InstructionSetIndependentInstruction> backingStatementList)
    {
        BackingStatementList = backingStatementList;
        InstructionAddressMap = new();
        _jumpsToFix = [];
    }

    private void AddInstruction(InstructionSetIndependentInstruction instruction)
    {
        if (InstructionAddressMap.TryGetValue(instruction.ActualAddress, out var list))
        {
            list.Add(instruction);
        }
        else
        {
            InstructionAddressMap[instruction.ActualAddress] = [instruction];
        }

        
        BackingStatementList.Add(instruction);
        instruction.InstructionIndex = (uint)BackingStatementList.Count;
    }

    public void FixJumps()
    {
        foreach (var tuple in _jumpsToFix)
        {
            if (InstructionAddressMap.TryGetValue(tuple.Item2, out var list))
            {
                var target = list.First();

                if (target.Equals(tuple.Item1))
                    tuple.Item1.Invalidate("Invalid jump target for instruction: Instruction can't jump to itself");
                else

                    tuple.Item1.Operands = [InstructionSetIndependentOperand.MakeInstruction(target)];
            }
            else
            {
                tuple.Item1.Invalidate("Jump target not found in method." + tuple.Item1.ActualAddress.ToString("X")
                                                                          + " ins " + tuple.Item1 + " addr ? " +
                                                                          tuple.Item2.ToString("X"));
            }
        }
    }
    
    public void VirtualCall(ulong instructionAddress, InstructionSetIndependentOperand dest) => AddInstruction(
        new(InstructionSetIndependentOpCode.VirtualCall, instructionAddress, IsilFlowControl.Continue, dest));
    
    public void LSR(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.LSR, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2));
    }
    public void SDIV(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.SDIV, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2));
    }
    public void BFXIL(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2, InstructionSetIndependentOperand op3)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.BFXIL, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2, op3));
    }
    public void UBFX(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2, InstructionSetIndependentOperand op3)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.UBFX, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2, op3));
    }
    
    public void FNMUL(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.FNMUL, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2));
    }
    public void UDIV(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.UDIV, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2));
    }
    public void ASR(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.ASR, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2));
    }
    public void Store(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,InstructionSetIndependentOperand storeType)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.Store, instructionAddress, IsilFlowControl.Continue, dest,
            op1,storeType));
    }
    public void Load(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,InstructionSetIndependentOperand loadType)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.Load, instructionAddress, IsilFlowControl.Continue, dest,
            op1,loadType));
    }
    public void FABS(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.FABS, instructionAddress, IsilFlowControl.Continue, dest,
            op1));
    }
    public void FCMGT(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand left,
        InstructionSetIndependentOperand right)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.FCMGT, instructionAddress,
            IsilFlowControl.Continue, dest, left, right));
    }
    public void BFI(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2, InstructionSetIndependentOperand op3)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.BFI, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2, op3));
    }
    public void LSL(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.LSL, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2));
    }
    public void FloorVector(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.Floor, instructionAddress, IsilFlowControl.Continue,
            dest, op1));
    }
    public void CeilingVector(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.Ceiling, instructionAddress, IsilFlowControl.Continue,
            dest, op1));
    }
    public void UZP1(ulong instructionAddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.UZP1, instructionAddress, IsilFlowControl.Continue, dest,
            op1, op2));
    }
    public void REV64(ulong address,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.REV64, address, IsilFlowControl.Continue, dest, op1));
    }
    public void MADD(ulong address,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2, InstructionSetIndependentOperand op3)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.MADD, address, IsilFlowControl.Continue, dest, op1, op2,
            op3));
    }

    public void BFM(ulong address,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand op1,
        InstructionSetIndependentOperand op2, InstructionSetIndependentOperand op3)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.BFM, address, IsilFlowControl.Continue, dest, op1, op2,
            op3));
    }
    
    public void MOVK(ulong adddress,
        InstructionSetIndependentOperand dest, InstructionSetIndependentOperand imm,
        InstructionSetIndependentOperand shift)
    {
        AddInstruction(new(InstructionSetIndependentOpCode.MOVK, adddress, IsilFlowControl.Continue, dest, imm,
            shift));
    }
    
    public void CompareTempMove(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) =>
        AddInstruction(new(InstructionSetIndependentOpCode.CompareTempMove,
        instructionAddress, IsilFlowControl.Continue, dest, src));
    public void Move(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(new(InstructionSetIndependentOpCode.Move,
        instructionAddress, IsilFlowControl.Continue, dest, src));

    public void ReadSystemRegister(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand systemRegister) => AddInstruction(new(
        InstructionSetIndependentOpCode.ReadSystemRegister,
        instructionAddress,
        IsilFlowControl.Continue,
        dest,
        systemRegister));
    
        
    public void LoadRegisterToVector(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src)
        => AddInstruction(new(InstructionSetIndependentOpCode.LoadRegisterToVector, instructionAddress,
            IsilFlowControl.Continue, dest, src));
    public void LoadImmToVector(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src)
        => AddInstruction(new(InstructionSetIndependentOpCode.LoadImmToVector, instructionAddress,
            IsilFlowControl.Continue, dest, src));
    public void LoadAddress(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(new(InstructionSetIndependentOpCode.LoadAddress,
        instructionAddress, IsilFlowControl.Continue, dest, src));

    public void VectorElementStore(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src)
        => AddInstruction(new(InstructionSetIndependentOpCode.VectorElementStore, instructionAddress,
            IsilFlowControl.Continue, dest, src));
    
    public void INS(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(new(InstructionSetIndependentOpCode.INS,
        instructionAddress, IsilFlowControl.Continue, dest, src));
    public void DUP( ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(new(InstructionSetIndependentOpCode.DUP,
        instructionAddress, IsilFlowControl.Continue, dest, src));
    public void VectorElementLoad(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(new(InstructionSetIndependentOpCode.VectorElementLoad,
        instructionAddress, IsilFlowControl.Continue, dest, src));
    
    public void FMOV(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(new(InstructionSetIndependentOpCode.FMOV,
        instructionAddress, IsilFlowControl.Continue, dest, src));
    public void ShiftStack(ulong instructionAddress, int amount) => AddInstruction(
        new(InstructionSetIndependentOpCode.ShiftStack, instructionAddress, IsilFlowControl.Continue,
            InstructionSetIndependentOperand.MakeImmediate(amount)));

    public void Push(ulong instructionAddress, InstructionSetIndependentOperand stackPointerRegister,
        InstructionSetIndependentOperand operand) => AddInstruction(new(InstructionSetIndependentOpCode.Push,
        instructionAddress, IsilFlowControl.Continue, stackPointerRegister, operand));

    public void Pop(ulong instructionAddress, InstructionSetIndependentOperand stackPointerRegister,
        InstructionSetIndependentOperand operand) => AddInstruction(new(InstructionSetIndependentOpCode.Pop,
        instructionAddress, IsilFlowControl.Continue, operand, stackPointerRegister));

    public void Exchange(ulong instructionAddress, InstructionSetIndependentOperand place1,
        InstructionSetIndependentOperand place2) => AddInstruction(new(InstructionSetIndependentOpCode.Exchange,
        instructionAddress, IsilFlowControl.Continue, place1, place2));

    public void CastType(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src,
        InstructionSetIndependentOperand castType) => AddInstruction(new(InstructionSetIndependentOpCode.CastBaseType,
        instructionAddress, IsilFlowControl.Continue, dest, src, castType));

    public void FloatToInteger(
        ulong instructionAddress,
        InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src,
        IsilFloatToIntegerRoundingMode roundingMode,
        IsilIntegerSignedness signedness,
        int targetBitWidth) => AddInstruction(new(
        InstructionSetIndependentOpCode.FloatToInteger,
        instructionAddress,
        IsilFlowControl.Continue,
        dest,
        src,
        InstructionSetIndependentOperand.MakeFloatToIntegerConversion(roundingMode, signedness, targetBitWidth)));

    public void IntegerToFloat(
        ulong instructionAddress,
        InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src,
        IsilIntegerSignedness signedness,
        int sourceBitWidth) => AddInstruction(new(
        InstructionSetIndependentOpCode.IntegerToFloat,
        instructionAddress,
        IsilFlowControl.Continue,
        dest,
        src,
        InstructionSetIndependentOperand.MakeIntegerToFloatConversion(signedness, sourceBitWidth)));
    
    
    public void Subtract(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right) => AddInstruction(
        new(InstructionSetIndependentOpCode.Subtract, instructionAddress, IsilFlowControl.Continue, dest, left, right));
    
   public void ExtractVector(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand arg0, InstructionSetIndependentOperand arg1,InstructionSetIndependentOperand offset) => AddInstruction( 
        new(InstructionSetIndependentOpCode.ExtractVector, instructionAddress, IsilFlowControl.Continue, dest, arg0,
            arg1, offset));
    public void SIMDMath(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right ,IsilMnemonic mnemonic) => AddInstruction(
        new(InstructionSetIndependentOpCode.SIMDMath,
            instructionAddress, IsilFlowControl.Continue, dest, left, right,InstructionSetIndependentOperand.MakeSimdMathType(mnemonic)));
    public void VectorPairwiseAdd(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(
        new(InstructionSetIndependentOpCode.VectorPairwiseAdd, instructionAddress, IsilFlowControl.Continue, dest, src));
    public void Add(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right) => AddInstruction(
        new(InstructionSetIndependentOpCode.Add, instructionAddress, IsilFlowControl.Continue, dest, left, right));

    public void Xor(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right) => AddInstruction(
        new(InstructionSetIndependentOpCode.Xor, instructionAddress, IsilFlowControl.Continue, dest, left, right));

    // The following 4 had their opcode implemented but not the builder func
    // I don't know why
    public void ShiftLeft(ulong instructionAddress, InstructionSetIndependentOperand left,
        InstructionSetIndependentOperand right) => AddInstruction(new(InstructionSetIndependentOpCode.ShiftLeft,
        instructionAddress, IsilFlowControl.Continue, left, right));

    public void ShiftRight(ulong instructionAddress, InstructionSetIndependentOperand left,
        InstructionSetIndependentOperand right) => AddInstruction(new(InstructionSetIndependentOpCode.ShiftRight,
        instructionAddress, IsilFlowControl.Continue, left, right));

    public void And(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right) => AddInstruction(
        new(InstructionSetIndependentOpCode.And, instructionAddress, IsilFlowControl.Continue, dest, left, right));

    public void Or(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
        => AddInstruction(new(InstructionSetIndependentOpCode.Or, instructionAddress, IsilFlowControl.Continue, dest,
            left, right));

    public void FSQRT(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src) => AddInstruction(new(InstructionSetIndependentOpCode.FSQRT,
        instructionAddress, IsilFlowControl.Continue, dest, src));
    public void FMAX(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left,
        InstructionSetIndependentOperand right) => AddInstruction(new(InstructionSetIndependentOpCode.FMAX,
        instructionAddress, IsilFlowControl.Continue, dest, left, right));
    public void FMINNM(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left,
        InstructionSetIndependentOperand right) => AddInstruction(new(InstructionSetIndependentOpCode.FMINNM,
        instructionAddress, IsilFlowControl.Continue, dest, left, right));
    public void FMIN(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left,
        InstructionSetIndependentOperand right) => AddInstruction(new(InstructionSetIndependentOpCode.FMIN,
        instructionAddress, IsilFlowControl.Continue, dest, left, right));
  
    public void FABD(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right) => AddInstruction(new(
        InstructionSetIndependentOpCode.FABD,
        instructionAddress, IsilFlowControl.Continue, dest, left, right));

    public void Not(ulong instructionAddress, InstructionSetIndependentOperand dest,InstructionSetIndependentOperand src) =>
        AddInstruction(new(InstructionSetIndependentOpCode.Not, instructionAddress, IsilFlowControl.Continue,dest, src));

    public void Neg(ulong instructionAddress,InstructionSetIndependentOperand dest, InstructionSetIndependentOperand src) =>
        AddInstruction(new(InstructionSetIndependentOpCode.Neg, instructionAddress, IsilFlowControl.Continue, dest,src));

    public void Multiply(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src1, InstructionSetIndependentOperand src2) => AddInstruction(
        new(InstructionSetIndependentOpCode.Multiply, instructionAddress, IsilFlowControl.Continue, dest, src1, src2));

    public void Divide(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand src1, InstructionSetIndependentOperand src2) => AddInstruction(
        new(InstructionSetIndependentOpCode.Divide, instructionAddress, IsilFlowControl.Continue, dest, src1, src2));

    public void Call(ulong instructionAddress, ulong dest, params InstructionSetIndependentOperand[] args) =>
        AddInstruction(new(InstructionSetIndependentOpCode.Call, instructionAddress, IsilFlowControl.MethodCall,
            PrepareCallOperands(dest, args)));

    public void CallNoReturn(ulong instructionAddress, ulong dest, params InstructionSetIndependentOperand[] args) =>
        AddInstruction(new(InstructionSetIndependentOpCode.CallNoReturn, instructionAddress,
            IsilFlowControl.MethodCall, PrepareCallOperands(dest, args)));

    public void CallRegister(ulong instructionAddress, InstructionSetIndependentOperand dest, bool noReturn = false) =>
        AddInstruction(new(
            noReturn ? InstructionSetIndependentOpCode.CallNoReturn : InstructionSetIndependentOpCode.Call,
            instructionAddress, IsilFlowControl.MethodCall, dest));

    public void Return(ulong instructionAddress, InstructionSetIndependentOperand? returnValue = null) =>
        AddInstruction(new(InstructionSetIndependentOpCode.Return, instructionAddress, IsilFlowControl.MethodReturn,
            returnValue != null
                ?
                [
                    returnValue.Value
                ]
                : []));

    public void Goto(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.Goto, IsilFlowControl.UnconditionalJump);

    public void JumpIfEqual(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfEqual, IsilFlowControl.ConditionalJump);

    public void JumpIfSign(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfSign, IsilFlowControl.ConditionalJump);

    public void JumpIfNotSign(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfNotSign, IsilFlowControl.ConditionalJump);

    public void JumpIfNotEqual(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfNotEqual, IsilFlowControl.ConditionalJump);


    public void JumpIfGreater(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfGreater, IsilFlowControl.ConditionalJump);

    public void JumpIfLess(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfLess, IsilFlowControl.ConditionalJump);

    public void JumpIfGreaterOrEqual(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfGreaterOrEqual, IsilFlowControl.ConditionalJump);

    public void JumpIfLessOrEqual(ulong instructionAddress, ulong target) => CreateJump(instructionAddress, target,
        InstructionSetIndependentOpCode.JumpIfLessOrEqual, IsilFlowControl.ConditionalJump);

    public void AssignIfLessThan(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
    {
        AssignCondition(instructionAddress, InstructionSetIndependentOpCode.AssignIfLessThan, dest, left, right);
    }
    public void AssignIfLessOrEqual(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
    {
        AssignCondition(instructionAddress, InstructionSetIndependentOpCode.AssignIfLessOrEqual, dest, left, right);
    }
    public void AssignIfGreaterOrEqual(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
    {
        AssignCondition(instructionAddress, InstructionSetIndependentOpCode.AssignIfGreaterOrEqual, dest, left, right);
    }

    public void AssignIfGreaterThan(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
    {
        AssignCondition(instructionAddress, InstructionSetIndependentOpCode.AssignIfGreaterThan, dest, left, right);
    }

    public void AssignIfEqual(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
    {
        AssignCondition(instructionAddress, InstructionSetIndependentOpCode.AssignIfEqual, dest, left, right);
    }

    public void AssignIfNotEqual(ulong instructionAddress, InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
    {
        AssignCondition(instructionAddress, InstructionSetIndependentOpCode.AssignIfNotEqual, dest, left, right);
    }

    private void AssignCondition(ulong instructionAddress, InstructionSetIndependentOpCode opCode,
        InstructionSetIndependentOperand dest,
        InstructionSetIndependentOperand left, InstructionSetIndependentOperand right)
    {
        var newInstruction = new InstructionSetIndependentInstruction(
            opCode,
            instructionAddress,
            IsilFlowControl.Continue,
            dest,
            left,
            right
        );
        AddInstruction(newInstruction);
    }

    private void CreateJump(ulong instructionAddress, ulong target, InstructionSetIndependentOpCode independentOpCode,
        IsilFlowControl flowControl)
    {
        var newInstruction = new InstructionSetIndependentInstruction(
            independentOpCode,
            instructionAddress,
            flowControl
        );
        AddInstruction(newInstruction);
        _jumpsToFix.Add((newInstruction, target));
    }


    public void Compare(ulong instructionAddress, InstructionSetIndependentOperand left,
        InstructionSetIndependentOperand right)
        => AddInstruction(new(InstructionSetIndependentOpCode.Compare, instructionAddress, IsilFlowControl.Continue,
            left, right));

    public void NotImplemented(ulong instructionAddress, string text) => AddInstruction(
        new(InstructionSetIndependentOpCode.NotImplemented, instructionAddress, IsilFlowControl.Continue,
            InstructionSetIndependentOperand.MakeImmediate(text)));

    public void Invalid(ulong instructionAddress, string text) => AddInstruction(new(
        InstructionSetIndependentOpCode.Invalid, instructionAddress, IsilFlowControl.Continue,
        InstructionSetIndependentOperand.MakeImmediate(text)));

    public void Interrupt(ulong instructionAddress) => AddInstruction(new(InstructionSetIndependentOpCode.Interrupt,
        instructionAddress, IsilFlowControl.Interrupt));

    public void Nop(ulong instructionAddress) => AddInstruction(new(InstructionSetIndependentOpCode.Nop,
        instructionAddress, IsilFlowControl.Continue));

    private InstructionSetIndependentOperand[] PrepareCallOperands(ulong dest, InstructionSetIndependentOperand[] args)
    {
        var parameters = new InstructionSetIndependentOperand[args.Length + 1];
        parameters[0] = InstructionSetIndependentOperand.MakeImmediate(dest);
        Array.Copy(args, 0, parameters, 1, args.Length);
        return parameters;
    }
}
