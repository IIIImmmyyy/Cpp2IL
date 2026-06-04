namespace LibCpp2IL.Elf.ExceptionHandling;

/// <summary>
/// One LSDA call-site table row. Start/end describe the protected range, and LandingPad is
/// the exceptional entry address. Action is kept for diagnostics and future catch typing.
/// </summary>
public sealed class ElfExceptionCallSite
{
    public ElfExceptionCallSite(ulong start, ulong end, ulong landingPad, long action)
    {
        Start = start;
        End = end;
        LandingPad = landingPad;
        Action = action;
    }

    public ulong Start { get; }

    public ulong End { get; }

    public ulong LandingPad { get; }

    public long Action { get; }
}
