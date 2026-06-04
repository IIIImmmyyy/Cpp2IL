using System.Collections.Generic;

namespace LibCpp2IL.Elf.ExceptionHandling;

/// <summary>
/// One native function's exception metadata recovered from ELF unwind tables.
/// </summary>
public sealed class ElfExceptionRegion
{
    public ElfExceptionRegion(ulong functionStart, ulong functionEnd, IReadOnlyList<ElfExceptionCallSite> callSites)
    {
        FunctionStart = functionStart;
        FunctionEnd = functionEnd;
        CallSites = callSites;
    }

    public ulong FunctionStart { get; }

    public ulong FunctionEnd { get; }

    public IReadOnlyList<ElfExceptionCallSite> CallSites { get; }
}
