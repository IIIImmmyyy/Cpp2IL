using System.Buffers.Binary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace LibCpp2IL.Elf;

public static class ElfPltImportReader
{
    private const ulong Aarch64PltHeaderSize = 0x20;
    private const ulong Aarch64PltEntrySize = 0x10;
    private const ulong Elf64DynSymbolSize = 0x18;
    private const ulong Elf64RelaSize = 0x18;

    /// <summary>
    /// Builds the AArch64 PLT import table from already-read ELF sections.
    /// This keeps section/header ownership in LibCpp2IL while exposing the extra import-stub relation.
    /// </summary>
    public static bool TryReadAArch64RelaPltImports(
        byte[] raw,
        IReadOnlyList<ElfSectionHeaderEntry> sections,
        bool is32Bit,
        bool isBigEndian,
        InstructionSetId instructionSetId,
        [NotNullWhen(true)] out ElfPltImportTable? table,
        [NotNullWhen(false)] out string? reason)
    {
        table = null;
        if (is32Bit)
        {
            reason = "PLT import parsing currently supports ELF64 only.";
            return false;
        }

        if (isBigEndian)
        {
            reason = "PLT import parsing currently supports little-endian ELF64 only.";
            return false;
        }

        if (instructionSetId != DefaultInstructionSets.ARM_V8)
        {
            reason = $"PLT import parsing currently supports AArch64 only, got {instructionSetId}.";
            return false;
        }

        var plt = GetSectionByName(sections, ".plt");
        if (plt == null)
        {
            reason = "ELF section .plt was not found.";
            return false;
        }

        var relaPlt = GetSectionByName(sections, ".rela.plt");
        if (relaPlt == null)
        {
            reason = "ELF section .rela.plt was not found.";
            return false;
        }

        var dynSym = GetSectionByName(sections, ".dynsym");
        if (dynSym == null)
        {
            reason = "ELF section .dynsym was not found.";
            return false;
        }

        if (dynSym.LinkedSectionIndex < 0 || dynSym.LinkedSectionIndex >= sections.Count)
        {
            reason = "ELF .dynsym has an invalid linked string-table section index.";
            return false;
        }

        var dynStr = sections[dynSym.LinkedSectionIndex];
        if (dynStr.Type != ElfSectionEntryType.SHT_STRTAB)
        {
            reason = "ELF .dynsym does not link to a string-table section.";
            return false;
        }

        var relaEntrySize = relaPlt.EntrySize > 0 ? (ulong)relaPlt.EntrySize : Elf64RelaSize;
        var dynSymEntrySize = dynSym.EntrySize > 0 ? (ulong)dynSym.EntrySize : Elf64DynSymbolSize;
        if (relaEntrySize != Elf64RelaSize || dynSymEntrySize != Elf64DynSymbolSize)
        {
            reason = $"Unsupported ELF64 .rela.plt/.dynsym entry sizes {relaEntrySize}/{dynSymEntrySize}.";
            return false;
        }

        if (!IsRangeInFile(raw, relaPlt.RawAddress, relaPlt.Size) ||
            !IsRangeInFile(raw, dynSym.RawAddress, dynSym.Size) ||
            !IsRangeInFile(raw, dynStr.RawAddress, dynStr.Size))
        {
            reason = "ELF PLT import sections are outside the raw file range.";
            return false;
        }

        var relocationCount = relaPlt.Size / relaEntrySize;
        var symbolCount = dynSym.Size / dynSymEntrySize;
        var entries = new List<ElfPltImportEntry>((int)Math.Min(relocationCount, int.MaxValue));

        for (var index = 0UL; index < relocationCount; index++)
        {
            var relocationOffset = checked((int)(relaPlt.RawAddress + index * relaEntrySize));
            var relocationAddress = ReadUInt64(raw, relocationOffset);
            var relocationInfo = ReadUInt64(raw, relocationOffset + 8);
            var addend = ReadInt64(raw, relocationOffset + 16);
            var symbolIndex = relocationInfo >> 32;
            var relocationType = (ElfRelocationType)(relocationInfo & 0xFFFF_FFFF);

            if (relocationType != ElfRelocationType.R_AARCH64_JUMP_SLOT)
            {
                reason = $"Unsupported .rela.plt relocation type {relocationType} at index {index}.";
                return false;
            }

            if (symbolIndex >= symbolCount)
            {
                reason = $".rela.plt relocation at index {index} references missing symbol index {symbolIndex}.";
                return false;
            }

            var symbolOffset = checked((int)(dynSym.RawAddress + symbolIndex * dynSymEntrySize));
            var nameOffset = ReadUInt32(raw, symbolOffset);
            var symbolName = ReadNullTerminatedString(raw, dynStr.RawAddress, dynStr.Size, nameOffset);
            if (string.IsNullOrEmpty(symbolName))
            {
                reason = $".rela.plt relocation at index {index} references an unnamed dynamic symbol.";
                return false;
            }

            entries.Add(new ElfPltImportEntry
            {
                SymbolName = symbolName,
                PltAddress = plt.VirtualAddress + Aarch64PltHeaderSize + index * Aarch64PltEntrySize,
                RelocationAddress = relocationAddress,
                RelocationType = relocationType,
                SymbolIndex = symbolIndex,
                Addend = addend
            });
        }

        table = new ElfPltImportTable(entries);
        reason = null;
        return true;
    }

    private static ElfSectionHeaderEntry? GetSectionByName(
        IReadOnlyList<ElfSectionHeaderEntry> sections,
        string name)
    {
        return sections.FirstOrDefault(section => string.Equals(section.Name, name, StringComparison.Ordinal));
    }

    private static bool IsRangeInFile(byte[] raw, ulong offset, ulong size)
    {
        return offset <= (ulong)raw.Length &&
               size <= (ulong)raw.Length - offset;
    }

    private static uint ReadUInt32(byte[] raw, int offset)
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(offset, 4));
    }

    private static ulong ReadUInt64(byte[] raw, int offset)
    {
        return BinaryPrimitives.ReadUInt64LittleEndian(raw.AsSpan(offset, 8));
    }

    private static long ReadInt64(byte[] raw, int offset)
    {
        return BinaryPrimitives.ReadInt64LittleEndian(raw.AsSpan(offset, 8));
    }

    private static string ReadNullTerminatedString(
        byte[] raw,
        ulong stringTableOffset,
        ulong stringTableSize,
        uint nameOffset)
    {
        if (nameOffset >= stringTableSize)
            return string.Empty;

        var start = checked((int)(stringTableOffset + nameOffset));
        var maxEnd = checked((int)(stringTableOffset + stringTableSize));
        var end = start;
        while (end < maxEnd && raw[end] != 0)
            end++;

        return Encoding.UTF8.GetString(raw, start, end - start);
    }
}
