namespace LibCpp2IL.Elf;

/// <summary>
/// A dynamic import reached through one ELF PLT stub.
/// The entry is binary metadata only; consumers decide whether a symbol has runtime semantics.
/// </summary>
public sealed class ElfPltImportEntry
{
    public required string SymbolName { get; init; }

    public ulong PltAddress { get; init; }

    public ulong RelocationAddress { get; init; }

    public ElfRelocationType RelocationType { get; init; }

    public ulong SymbolIndex { get; init; }

    public long Addend { get; init; }
}
