using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LibCpp2IL.Elf;

public sealed class ElfPltImportTable
{
    private readonly Dictionary<string, ElfPltImportEntry> _entriesBySymbol;
    private readonly Dictionary<ulong, ElfPltImportEntry> _entriesByPltAddress;

    public ElfPltImportTable(IEnumerable<ElfPltImportEntry> entries)
    {
        Entries = entries.ToArray();
        _entriesBySymbol = new Dictionary<string, ElfPltImportEntry>(StringComparer.Ordinal);
        _entriesByPltAddress = new Dictionary<ulong, ElfPltImportEntry>();

        foreach (var entry in Entries)
        {
            _entriesBySymbol.TryAdd(entry.SymbolName, entry);
            _entriesByPltAddress.TryAdd(entry.PltAddress, entry);
        }
    }

    public IReadOnlyList<ElfPltImportEntry> Entries { get; }

    public bool TryGetBySymbol(
        string symbolName,
        [NotNullWhen(true)] out ElfPltImportEntry? entry)
    {
        return _entriesBySymbol.TryGetValue(symbolName, out entry);
    }

    public bool TryGetByPltAddress(
        ulong pltAddress,
        [NotNullWhen(true)] out ElfPltImportEntry? entry)
    {
        return _entriesByPltAddress.TryGetValue(pltAddress, out entry);
    }

    public ElfPltImportEntry GetBySymbol(string symbolName)
    {
        return _entriesBySymbol[symbolName];
    }
}
