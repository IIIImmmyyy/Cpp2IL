using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using LibCpp2IL;

namespace Cpp2IL.Core.Utils;

/// <summary>
/// ARM64 PC-relative immediate branch 的编码类别。
/// 寄存器间接跳转不包含目标立即数，因此不属于本索引的交叉引用范围。
/// </summary>
[Flags]
public enum Arm64BranchXrefKind
{
    None = 0,
    UnconditionalB = 1 << 0,
    CallBl = 1 << 1,
    ConditionalB = 1 << 2,
    AllImmediate = UnconditionalB | CallBl | ConditionalB
}

/// <summary>
/// 一条 ARM64 immediate branch 对目标虚拟地址形成的代码交叉引用。
/// </summary>
public readonly record struct Arm64BranchXref(
    ulong SourceAddress,
    Arm64BranchXrefKind Kind);

/// <summary>
/// 对 ARM64 主可执行段做一次线性扫描，并按目标虚拟地址建立轻量交叉引用索引。
/// 索引只解码分支编码，不保留完整反汇编结果；需要查询多个目标时应复用同一个实例。
/// </summary>
public sealed class Arm64BranchXrefIndex
{
    private readonly Dictionary<ulong, List<Arm64BranchXref>> _referencesByTarget = new();

    private Arm64BranchXrefIndex()
    {
    }

    /// <summary>
    /// 从指定 Binary 的主可执行段（ELF/PE 中通常是 .text）建立索引。
    /// </summary>
    public static Arm64BranchXrefIndex Build(Il2CppBinary binary)
    {
        ArgumentNullException.ThrowIfNull(binary);
        return Build(
            binary.GetEntirePrimaryExecutableSection(),
            binary.GetVirtualAddressOfPrimaryExecutableSection(),
            binary.IsBigEndian);
    }

    /// <summary>
    /// 从一段连续 ARM64 指令建立索引。公开该入口便于无 Binary 上下文的工具和测试复用。
    /// </summary>
    public static Arm64BranchXrefIndex Build(
        ReadOnlySpan<byte> executableSection,
        ulong sectionVirtualAddress,
        bool isBigEndian)
    {
        var index = new Arm64BranchXrefIndex();

        for (var offset = 0; offset + sizeof(uint) <= executableSection.Length; offset += sizeof(uint))
        {
            var rawInstruction = isBigEndian
                ? BinaryPrimitives.ReadUInt32BigEndian(executableSection.Slice(offset, sizeof(uint)))
                : BinaryPrimitives.ReadUInt32LittleEndian(executableSection.Slice(offset, sizeof(uint)));
            var sourceAddress = sectionVirtualAddress + (ulong)offset;

            if (!TryDecodeImmediateBranch(rawInstruction, sourceAddress, out var targetAddress, out var kind))
            {
                continue;
            }

            if (!index._referencesByTarget.TryGetValue(targetAddress, out var references))
            {
                references = [];
                index._referencesByTarget[targetAddress] = references;
            }

            references.Add(new Arm64BranchXref(sourceAddress, kind));
        }

        return index;
    }

    /// <summary>
    /// 返回目标地址的交叉引用；返回顺序与指令在可执行段中的地址顺序一致。
    /// </summary>
    public IReadOnlyList<Arm64BranchXref> GetReferencesTo(ulong targetAddress)
        => _referencesByTarget.TryGetValue(targetAddress, out var references)
            ? references
            : Array.Empty<Arm64BranchXref>();

    /// <summary>
    /// 返回指定分支类别的来源虚拟地址。
    /// </summary>
    public IReadOnlyList<ulong> GetSourcesTo(
        ulong targetAddress,
        Arm64BranchXrefKind kinds)
    {
        if (kinds == Arm64BranchXrefKind.None ||
            !_referencesByTarget.TryGetValue(targetAddress, out var references))
        {
            return Array.Empty<ulong>();
        }

        return references
            .Where(reference => (reference.Kind & kinds) != 0)
            .Select(reference => reference.SourceAddress)
            .ToArray();
    }

    /// <summary>
    /// 返回指定分支类别指向目标地址的数量，不创建来源地址数组。
    /// </summary>
    public int CountReferencesTo(
        ulong targetAddress,
        Arm64BranchXrefKind kinds)
    {
        if (kinds == Arm64BranchXrefKind.None ||
            !_referencesByTarget.TryGetValue(targetAddress, out var references))
        {
            return 0;
        }

        return references.Count(reference => (reference.Kind & kinds) != 0);
    }

    private static bool TryDecodeImmediateBranch(
        uint instruction,
        ulong sourceAddress,
        out ulong targetAddress,
        out Arm64BranchXrefKind kind)
    {
        // B 与 BL 共享 signed imm26；先符号扩展 26 位立即数，再按指令定义左移 2 位。
        var opcode = instruction >> 26;
        if (opcode is 0b000101 or 0b100101)
        {
            var signedOffset = ((long)(instruction & 0x03FF_FFFF) << 38) >> 36;
            targetAddress = (ulong)((long)sourceAddress + signedOffset);
            kind = opcode == 0b000101
                ? Arm64BranchXrefKind.UnconditionalB
                : Arm64BranchXrefKind.CallBl;
            return true;
        }

        // 保留原 key-function 索引支持的 B.cond 编码，供现有调用者复用；
        // FindDirectBReferences 会严格过滤它，不把条件跳转算作直接 B 引用。
        if ((instruction & 0xFF00_0010) == 0x5400_0000)
        {
            var signedOffset = ((long)((instruction >> 5) & 0x7_FFFF) << 45) >> 43;
            targetAddress = (ulong)((long)sourceAddress + signedOffset);
            kind = Arm64BranchXrefKind.ConditionalB;
            return true;
        }

        targetAddress = 0;
        kind = Arm64BranchXrefKind.None;
        return false;
    }
}

/// <summary>
/// 针对当前已加载 ARM64 Binary 的单目标便捷查询入口。
/// 每次调用只在本次查询生命周期内建立索引，不持有跨 Binary 的静态缓存。
/// </summary>
public static class Arm64BranchXrefHelper
{
    /// <summary>
    /// 查找 .text 中所有直接通过无条件 <c>B imm26</c> 跳到目标地址的指令地址。
    /// 不包含 BL、B.cond、BR/BLR、CBZ/CBNZ 或 TBZ/TBNZ。
    /// </summary>
    public static IReadOnlyList<ulong> FindDirectBReferences(ulong targetAddress)
    {
        var binary = LibCpp2IlMain.Binary ?? throw new InvalidOperationException(
            "No IL2CPP binary is currently loaded.");
        var index = Arm64BranchXrefIndex.Build(binary);
        return index.GetSourcesTo(targetAddress, Arm64BranchXrefKind.UnconditionalB);
    }
}
