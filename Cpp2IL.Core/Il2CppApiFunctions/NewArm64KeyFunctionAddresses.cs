using System;
using System.Collections.Generic;
using System.Linq;
using Disarm;
using Cpp2IL.Core.Logging;
using Cpp2IL.Core.Model.Contexts;
using Cpp2IL.Core.Utils;
using LibCpp2IL;
using LibCpp2IL.Reflection;

namespace Cpp2IL.Core.Il2CppApiFunctions;

public class NewArm64KeyFunctionAddresses : BaseKeyFunctionAddresses
{
    private Arm64BranchXrefIndex? _branchIndex;

    public override void Find(ApplicationAnalysisContext applicationAnalysisContext)
    {
        try
        {
            base.Find(applicationAnalysisContext);
        }
        finally
        {
            ClearBranchIndex();
        }
    }

    private void ClearBranchIndex()
    {
        _branchIndex = null;
    }

    private Arm64BranchXrefIndex GetBranchIndex()
    {
        if (_branchIndex == null)
        {
            var binary = LibCpp2IlMain.Binary!;
            var executableSection = binary.GetEntirePrimaryExecutableSection();
            _branchIndex = Arm64BranchXrefIndex.Build(
                executableSection,
                binary.GetVirtualAddressOfPrimaryExecutableSection(),
                binary.IsBigEndian);
        }

        return _branchIndex;
    }

    protected override IEnumerable<ulong> FindAllThunkFunctions(ulong addr, uint maxBytesBack = 0, params ulong[] addressesToIgnore)
    {
        // Key-function thunk discovery 保留原有 B/BL/B.cond 查询语义，
        // 但复用公共轻量索引，避免持有完整 .text 反汇编结果。
        var matchingJmps = GetBranchIndex().GetSourcesTo(
            addr,
            Arm64BranchXrefKind.AllImmediate);

        foreach (var matchingJmpAddress in matchingJmps)
        {
            if (addressesToIgnore.Contains(matchingJmpAddress)) continue;

            //Find this instruction in the raw file
            var offsetInPe = (ulong)LibCpp2IlMain.Binary!.MapVirtualAddressToRaw(matchingJmpAddress);
            if (offsetInPe == 0 || offsetInPe == (ulong)(LibCpp2IlMain.Binary.RawLength - 1))
                continue;

            //get next and previous bytes
            var previousByte = LibCpp2IlMain.Binary.GetByteAtRawAddress(offsetInPe - 1);
            var nextByte = LibCpp2IlMain.Binary.GetByteAtRawAddress(offsetInPe + 4);

            //Double-cc = thunk
            if (previousByte == 0xCC && nextByte == 0xCC)
            {
                yield return matchingJmpAddress;
                continue;
            }

            if (nextByte == 0xCC && maxBytesBack > 0)
            {
                for (ulong backtrack = 1; backtrack < maxBytesBack && offsetInPe - backtrack > 0; backtrack++)
                {
                    if (addressesToIgnore.Contains(matchingJmpAddress - (backtrack - 1)))
                        //Move to next jmp
                        break;

                    if (LibCpp2IlMain.Binary.GetByteAtRawAddress(offsetInPe - backtrack) == 0xCC)
                    {
                        yield return matchingJmpAddress - (backtrack - 1);
                        break;
                    }
                }
            }
        }
    }

    protected override ulong GetObjectIsInstFromSystemType()
    {
        Logger.Verbose("\tTrying to use System.Type::IsInstanceOfType to find il2cpp::vm::Object::IsInst...");
        var typeIsInstanceOfType = LibCpp2IlReflection.GetType("Type", "System")?.Methods?.FirstOrDefault(m => m.Name == "IsInstanceOfType");
        if (typeIsInstanceOfType == null)
        {
            Logger.VerboseNewline("Type or method not found, aborting.");
            return 0;
        }

        //IsInstanceOfType is a very simple ICall, that looks like this:
        //  Il2CppClass* klass = vm::Class::FromIl2CppType(type->type.type);
        //  return il2cpp::vm::Object::IsInst(obj, klass) != NULL;
        //The last call is to Object::IsInst

        Logger.Verbose($"IsInstanceOfType found at 0x{typeIsInstanceOfType.MethodPointer:X}...");
        var instructions = NewArm64Utils.GetArm64MethodBodyAtVirtualAddress(typeIsInstanceOfType.MethodPointer, true);

        var lastCall = instructions.LastOrDefault(i => i.Mnemonic == Arm64Mnemonic.BL);

        if (lastCall.Mnemonic == Arm64Mnemonic.INVALID)
        {
            Logger.VerboseNewline("Method does not match expected signature. Aborting.");
            return 0;
        }

        Logger.VerboseNewline($"Success. IsInst found at 0x{lastCall.BranchTarget:X}");
        return lastCall.BranchTarget;
    }

    protected override ulong FindFunctionThisIsAThunkOf(ulong thunkPtr, bool prioritiseCall = false)
    {
        var instructions = NewArm64Utils.GetArm64MethodBodyAtVirtualAddress(thunkPtr, true);

        try
        {
            var target = prioritiseCall ? Arm64Mnemonic.BL : Arm64Mnemonic.B;
            var matchingCall = instructions.FirstOrDefault(i => i.Mnemonic == target);

            if (matchingCall.Mnemonic == Arm64Mnemonic.INVALID)
            {
                target = target == Arm64Mnemonic.BL ? Arm64Mnemonic.B : Arm64Mnemonic.BL;
                matchingCall = instructions.First(i => i.Mnemonic == target);
            }

            return matchingCall.BranchTarget;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    protected override int GetCallerCount(ulong toWhere)
    {
        return GetBranchIndex().CountReferencesTo(
            toWhere,
            Arm64BranchXrefKind.AllImmediate);
    }
}
