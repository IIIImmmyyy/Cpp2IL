using System;
using System.Buffers.Binary;
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
    private Arm64BranchIndex? _branchIndex;

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

    private Arm64BranchIndex GetBranchIndex()
    {
        if (_branchIndex == null)
        {
            var binary = LibCpp2IlMain.Binary!;
            var executableSection = binary.GetEntirePrimaryExecutableSection();
            _branchIndex = Arm64BranchIndex.Build(
                executableSection,
                binary.GetVirtualAddressOfPrimaryExecutableSection(),
                binary.IsBigEndian);
        }

        return _branchIndex;
    }

    protected override IEnumerable<ulong> FindAllThunkFunctions(ulong addr, uint maxBytesBack = 0, params ulong[] addressesToIgnore)
    {
        // Key-function thunk discovery only needs direct B/BL callers, so use the
        // compact branch index instead of retaining a full .text disassembly.
        var matchingJmps = GetBranchIndex().GetBranchSourcesTo(addr);

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
        return GetBranchIndex().GetCallerCount(toWhere);
    }

    private sealed class Arm64BranchIndex
    {
        private readonly Dictionary<ulong, List<ulong>> _branchSourcesByTarget = new();

        private Arm64BranchIndex()
        {
        }

        public static Arm64BranchIndex Build(ReadOnlySpan<byte> executableSection, ulong sectionVirtualAddress, bool isBigEndian)
        {
            var index = new Arm64BranchIndex();

            for (var offset = 0; offset + 4 <= executableSection.Length; offset += 4)
            {
                var rawInstruction = isBigEndian
                    ? BinaryPrimitives.ReadUInt32BigEndian(executableSection.Slice(offset, 4))
                    : BinaryPrimitives.ReadUInt32LittleEndian(executableSection.Slice(offset, 4));

                var branchAddress = sectionVirtualAddress + (ulong)offset;
                if (!TryDecodeDirectBranch(rawInstruction, branchAddress, out var target))
                    continue;

                if (!index._branchSourcesByTarget.TryGetValue(target, out var sources))
                {
                    sources = [];
                    index._branchSourcesByTarget[target] = sources;
                }

                sources.Add(branchAddress);
            }

            return index;
        }

        public IReadOnlyList<ulong> GetBranchSourcesTo(ulong target)
        {
            return _branchSourcesByTarget.TryGetValue(target, out var sources)
                ? sources
                : Array.Empty<ulong>();
        }

        public int GetCallerCount(ulong target)
        {
            return _branchSourcesByTarget.TryGetValue(target, out var sources) ? sources.Count : 0;
        }

        private static bool TryDecodeDirectBranch(uint instruction, ulong address, out ulong target)
        {
            // ARM64 B/BL immediate encodings are 000101 and 100101 in bits 31..26.
            var opcode = instruction >> 26;
            if (opcode is 0b000101 or 0b100101)
            {
                var signedOffset = ((long)(instruction & 0x03FF_FFFF) << 38) >> 36;
                target = (ulong)((long)address + signedOffset);
                return true;
            }

            // The previous Disarm-based filter matched Arm64Mnemonic.B, which also
            // includes conditional B.<cond> immediate instructions.
            if ((instruction & 0xFF00_0010) == 0x5400_0000)
            {
                var signedOffset = ((long)((instruction >> 5) & 0x7_FFFF) << 45) >> 43;
                target = (ulong)((long)address + signedOffset);
                return true;
            }

            target = 0;
            return false;
        }
    }
}
