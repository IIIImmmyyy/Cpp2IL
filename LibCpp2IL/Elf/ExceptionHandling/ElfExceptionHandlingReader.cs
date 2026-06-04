using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using LibCpp2IL.Logging;

namespace LibCpp2IL.Elf.ExceptionHandling;

internal static class ElfExceptionHandlingReader
{
    public static IReadOnlyList<ElfExceptionRegion> Read(ElfFile elf)
    {
        try
        {
            var ehFrame = elf.GetSectionByName(".eh_frame");
            var ehFrameHeader = elf.GetSectionByName(".eh_frame_hdr") ?? CreateEhFrameHeaderFromProgramHeader(elf);
            if (ehFrame == null && ehFrameHeader == null)
            {
                return Array.Empty<ElfExceptionRegion>();
            }

            if (ehFrame != null)
            {
                var regions = ReadEhFrame(elf, ehFrame);
                if (regions.Count > 0)
                {
                    return regions;
                }
            }

            return ehFrameHeader == null
                ? Array.Empty<ElfExceptionRegion>()
                : ReadEhFrameHeader(elf, ehFrameHeader);
        }
        catch (Exception ex)
        {
            LibLogger.VerboseNewline($"\tSkipping ELF exception metadata: {ex.Message}");
            return Array.Empty<ElfExceptionRegion>();
        }
    }

    private static ElfSectionHeaderEntry? CreateEhFrameHeaderFromProgramHeader(ElfFile elf)
    {
        var header = elf.GetProgramHeaderOfType(ElfProgramEntryType.PT_GNU_EH_FRAME);
        return header == null
            ? null
            : new ElfSectionHeaderEntry
            {
                Name = ".eh_frame_hdr",
                VirtualAddress = header.VirtualAddress,
                RawAddress = header.RawAddress,
                Size = header.RawSize
            };
    }

    private static IReadOnlyList<ElfExceptionRegion> ReadEhFrame(ElfFile elf, ElfSectionHeaderEntry ehFrame)
    {
        var raw = elf.GetRawBinaryContent();
        var regions = new List<ElfExceptionRegion>();
        var cies = new Dictionary<int, CieInfo>();
        var offset = checked((int)ehFrame.RawAddress);
        var end = checked((int)(ehFrame.RawAddress + ehFrame.Size));

        while (offset + 4 <= end)
        {
            var entryStart = offset;
            var length = ReadUInt32(raw, ref offset);
            if (length == 0)
            {
                break;
            }

            if (length == uint.MaxValue)
            {
                LibLogger.VerboseNewline("\tSkipping extended .eh_frame entry length.");
                break;
            }

            var contentStart = offset;
            var entryEnd = checked(contentStart + (int)length);
            if (entryEnd > end || entryEnd > raw.Length || offset + 4 > entryEnd)
            {
                break;
            }

            var cieId = ReadUInt32(raw, ref offset);
            if (cieId == 0)
            {
                cies[entryStart] = ReadCie(elf, contentStart, entryEnd);
                offset = entryEnd;
                continue;
            }

            if (TryReadFdeEntry(elf, entryStart, cies, out var region) && region != null)
            {
                regions.Add(region);
            }

            offset = entryEnd;
        }

        return regions;
    }

    private static IReadOnlyList<ElfExceptionRegion> ReadEhFrameHeader(ElfFile elf, ElfSectionHeaderEntry ehFrameHeader)
    {
        var raw = elf.GetRawBinaryContent();
        var regions = new List<ElfExceptionRegion>();
        var cies = new Dictionary<int, CieInfo>();
        var fdeOffsets = new HashSet<int>();
        var offset = checked((int)ehFrameHeader.RawAddress);
        var end = checked((int)(ehFrameHeader.RawAddress + ehFrameHeader.Size));
        if (offset + 4 > end)
        {
            return regions;
        }

        var version = raw[offset++];
        if (version != 1)
        {
            LibLogger.VerboseNewline($"\tUnsupported .eh_frame_hdr version {version}.");
            return regions;
        }

        var ehFramePtrEncoding = raw[offset++];
        var fdeCountEncoding = raw[offset++];
        var tableEncoding = raw[offset++];
        _ = ReadEncodedPointer(
            elf,
            raw,
            ref offset,
            ehFramePtrEncoding,
            applyRelative: true,
            dataRelativeBase: ehFrameHeader.VirtualAddress);

        if (fdeCountEncoding == DwarfExceptionEncoding.Omit || tableEncoding == DwarfExceptionEncoding.Omit)
        {
            return regions;
        }

        var fdeCount = ReadEncodedUnsignedValue(raw, ref offset, DwarfExceptionEncoding.WithoutRelative(fdeCountEncoding), elf);
        for (var index = 0UL; index < fdeCount && offset < end; index++)
        {
            _ = ReadEncodedPointer(
                elf,
                raw,
                ref offset,
                tableEncoding,
                applyRelative: true,
                dataRelativeBase: ehFrameHeader.VirtualAddress);
            var fdeAddress = ReadEncodedPointer(
                elf,
                raw,
                ref offset,
                tableEncoding,
                applyRelative: true,
                dataRelativeBase: ehFrameHeader.VirtualAddress);
            if (fdeAddress == 0 || !elf.TryMapVirtualAddressToRaw(fdeAddress, out var fdeRaw) || fdeRaw < 0)
            {
                continue;
            }

            var fdeOffset = checked((int)fdeRaw);
            if (!fdeOffsets.Add(fdeOffset))
            {
                continue;
            }

            if (TryReadFdeEntry(elf, fdeOffset, cies, out var region) && region != null)
            {
                regions.Add(region);
            }
        }

        return regions;
    }

    private static bool TryReadFdeEntry(
        ElfFile elf,
        int entryStart,
        Dictionary<int, CieInfo> cies,
        out ElfExceptionRegion? region)
    {
        region = null;
        try
        {
            var raw = elf.GetRawBinaryContent();
            var offset = entryStart;
            if (offset < 0 || offset + 4 > raw.Length)
            {
                return false;
            }

            var length = ReadUInt32(raw, ref offset);
            if (length == 0 || length == uint.MaxValue)
            {
                return false;
            }

            var contentStart = offset;
            var entryEnd = checked(contentStart + (int)length);
            if (entryEnd > raw.Length || offset + 4 > entryEnd)
            {
                return false;
            }

            var ciePointerField = offset;
            var cieId = ReadUInt32(raw, ref offset);
            if (cieId == 0 || cieId > int.MaxValue)
            {
                return false;
            }

            var cieStart = ciePointerField - (int)cieId;
            if (cieStart < 0)
            {
                return false;
            }

            if (!cies.TryGetValue(cieStart, out var cie))
            {
                cie = ReadCieAt(elf, cieStart);
                cies[cieStart] = cie;
            }

            var initialLocation = ReadEncodedPointer(
                elf,
                raw,
                ref offset,
                cie.FdePointerEncoding,
                applyRelative: true);
            var range = ReadEncodedUnsignedValue(
                raw,
                ref offset,
                DwarfExceptionEncoding.WithoutRelative(cie.FdePointerEncoding),
                elf);
            var functionEnd = initialLocation + range;

            var lsda = 0UL;
            if (cie.Augmentation.StartsWith('z') && offset < entryEnd)
            {
                var augmentationLength = ReadUleb128(raw, ref offset);
                var augmentationEnd = checked(offset + (int)augmentationLength);
                if (cie.LsdaEncoding != DwarfExceptionEncoding.Omit && offset < augmentationEnd)
                {
                    lsda = ReadEncodedPointer(
                        elf,
                        raw,
                        ref offset,
                        cie.LsdaEncoding,
                        applyRelative: true);
                }
            }

            if (lsda == 0)
            {
                return true;
            }

            var callSites = ReadLsda(elf, lsda, initialLocation);
            if (callSites.Count > 0)
            {
                region = new ElfExceptionRegion(initialLocation, functionEnd, callSites);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static CieInfo ReadCieAt(ElfFile elf, int entryStart)
    {
        var raw = elf.GetRawBinaryContent();
        var offset = entryStart;
        var length = ReadUInt32(raw, ref offset);
        if (length == 0 || length == uint.MaxValue)
        {
            throw new InvalidOperationException("Unsupported CIE entry length.");
        }

        var contentStart = offset;
        var entryEnd = checked(contentStart + (int)length);
        var cieId = ReadUInt32(raw, ref offset);
        if (cieId != 0)
        {
            throw new InvalidOperationException("Referenced .eh_frame entry is not a CIE.");
        }

        return ReadCie(elf, contentStart, entryEnd);
    }

    private static CieInfo ReadCie(ElfFile elf, int contentStart, int entryEnd)
    {
        var raw = elf.GetRawBinaryContent();
        var offset = contentStart + 4;
        var version = raw[offset++];
        var augmentation = ReadNullTerminatedAscii(raw, ref offset, entryEnd);
        _ = ReadUleb128(raw, ref offset);
        _ = ReadSleb128(raw, ref offset);
        if (version == 1)
        {
            _ = raw[offset++];
        }
        else
        {
            _ = ReadUleb128(raw, ref offset);
        }

        var fdePointerEncoding = DwarfExceptionEncoding.Absptr;
        var lsdaEncoding = DwarfExceptionEncoding.Omit;

        if (augmentation.StartsWith('z'))
        {
            var augmentationLength = ReadUleb128(raw, ref offset);
            var augmentationEnd = checked(offset + (int)augmentationLength);
            foreach (var marker in augmentation.Skip(1))
            {
                switch (marker)
                {
                    case 'L':
                        lsdaEncoding = raw[offset++];
                        break;
                    case 'P':
                        var personalityEncoding = raw[offset++];
                        _ = ReadEncodedPointer(elf, raw, ref offset, personalityEncoding, applyRelative: true);
                        break;
                    case 'R':
                        fdePointerEncoding = raw[offset++];
                        break;
                }
            }

            offset = Math.Min(augmentationEnd, entryEnd);
        }

        return new CieInfo(augmentation, fdePointerEncoding, lsdaEncoding);
    }

    private static IReadOnlyList<ElfExceptionCallSite> ReadLsda(ElfFile elf, ulong lsdaAddress, ulong functionStart)
    {
        if (!elf.TryMapVirtualAddressToRaw(lsdaAddress, out var rawAddress) || rawAddress < 0)
        {
            return Array.Empty<ElfExceptionCallSite>();
        }

        var raw = elf.GetRawBinaryContent();
        var offset = checked((int)rawAddress);
        var lpStartEncoding = raw[offset++];
        var lpStart = lpStartEncoding == DwarfExceptionEncoding.Omit
            ? functionStart
            : ReadEncodedPointer(elf, raw, ref offset, lpStartEncoding, applyRelative: true);
        if (lpStart == 0)
        {
            lpStart = functionStart;
        }

        var typeTableEncoding = raw[offset++];
        if (typeTableEncoding != DwarfExceptionEncoding.Omit)
        {
            _ = ReadUleb128(raw, ref offset);
        }

        var callSiteEncoding = raw[offset++];
        var callSiteTableLength = ReadUleb128(raw, ref offset);
        var tableEnd = checked(offset + (int)callSiteTableLength);
        var callSites = new List<ElfExceptionCallSite>();

        while (offset < tableEnd)
        {
            var startOffset = ReadEncodedUnsignedValue(
                raw,
                ref offset,
                DwarfExceptionEncoding.WithoutRelative(callSiteEncoding),
                elf);
            var length = ReadEncodedUnsignedValue(
                raw,
                ref offset,
                DwarfExceptionEncoding.WithoutRelative(callSiteEncoding),
                elf);
            var landingOffset = ReadEncodedUnsignedValue(
                raw,
                ref offset,
                DwarfExceptionEncoding.WithoutRelative(callSiteEncoding),
                elf);
            var action = (long)ReadUleb128(raw, ref offset);

            if (length == 0)
            {
                continue;
            }

            var start = lpStart + startOffset;
            var end = start + length;
            var landingPad = landingOffset == 0 ? 0 : lpStart + landingOffset;
            callSites.Add(new ElfExceptionCallSite(start, end, landingPad, action));
        }

        return callSites;
    }

    private static ulong ReadEncodedPointer(
        ElfFile elf,
        byte[] raw,
        ref int offset,
        byte encoding,
        bool applyRelative,
        ulong dataRelativeBase = 0)
    {
        if (encoding == DwarfExceptionEncoding.Omit)
        {
            return 0;
        }

        var locationRaw = (ulong)offset;
        var value = ReadEncodedSignedOrUnsigned(raw, ref offset, encoding, elf, out var isSigned);
        if (value == 0)
        {
            return 0;
        }

        var result = isSigned ? unchecked((ulong)(long)value) : value;
        if (applyRelative)
        {
            result = ApplyRelativeBase(elf, result, encoding, locationRaw, dataRelativeBase);
        }

        if ((encoding & 0x80) != 0 && elf.TryMapVirtualAddressToRaw(result, out var indirectRaw) && indirectRaw >= 0)
        {
            var indirectOffset = checked((int)indirectRaw);
            result = elf.is32Bit
                ? BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(indirectOffset, 4))
                : BinaryPrimitives.ReadUInt64LittleEndian(raw.AsSpan(indirectOffset, 8));
        }

        return result;
    }

    private static ulong ReadEncodedUnsignedValue(byte[] raw, ref int offset, byte encoding, ElfFile elf)
    {
        return ReadEncodedSignedOrUnsigned(raw, ref offset, encoding, elf, out _);
    }

    private static ulong ReadEncodedSignedOrUnsigned(
        byte[] raw,
        ref int offset,
        byte encoding,
        ElfFile elf,
        out bool isSigned)
    {
        isSigned = false;
        switch (encoding & 0x0F)
        {
            case 0x00:
                return ReadNativePointer(raw, ref offset, elf);
            case 0x01:
                return ReadUleb128(raw, ref offset);
            case 0x02:
                return ReadUInt16(raw, ref offset);
            case 0x03:
                return ReadUInt32(raw, ref offset);
            case 0x04:
                return ReadUInt64(raw, ref offset);
            case 0x09:
                isSigned = true;
                return unchecked((ulong)ReadSleb128(raw, ref offset));
            case 0x0A:
                isSigned = true;
                return unchecked((ulong)ReadInt16(raw, ref offset));
            case 0x0B:
                isSigned = true;
                return unchecked((ulong)ReadInt32(raw, ref offset));
            case 0x0C:
                isSigned = true;
                return unchecked((ulong)ReadInt64(raw, ref offset));
            default:
                throw new NotSupportedException($"Unsupported DW_EH_PE format 0x{encoding:X2}.");
        }
    }

    private static ulong ApplyRelativeBase(
        ElfFile elf,
        ulong value,
        byte encoding,
        ulong locationRaw,
        ulong dataRelativeBase)
    {
        var relativeKind = encoding & 0x70;
        switch (relativeKind)
        {
            case 0:
                return value;
            case 0x10:
                if (!TryRawToVirtual(elf, locationRaw, out var locationVa))
                {
                    throw new InvalidOperationException("Cannot map encoded pointer location to a virtual address.");
                }

                return locationVa + value;
            case 0x30:
                return dataRelativeBase + value;
            default:
                throw new NotSupportedException($"Unsupported DW_EH_PE relative kind 0x{relativeKind:X2}.");
        }
    }

    private static bool TryRawToVirtual(ElfFile elf, ulong rawAddress, out ulong virtualAddress)
    {
        foreach (var section in elf.SectionHeaderEntries)
        {
            if (section.Size == 0 || rawAddress < section.RawAddress || rawAddress >= section.RawAddress + section.Size)
            {
                continue;
            }

            virtualAddress = section.VirtualAddress + rawAddress - section.RawAddress;
            return true;
        }

        foreach (var segment in elf.ProgramHeaderEntries.Where(segment => segment.Type == ElfProgramEntryType.PT_LOAD))
        {
            if (segment.RawSize == 0 || rawAddress < segment.RawAddress || rawAddress >= segment.RawAddress + segment.RawSize)
            {
                continue;
            }

            virtualAddress = segment.VirtualAddress + rawAddress - segment.RawAddress;
            return true;
        }

        virtualAddress = 0;
        return false;
    }

    private static ulong ReadNativePointer(byte[] raw, ref int offset, ElfFile elf)
    {
        return elf.is32Bit ? ReadUInt32(raw, ref offset) : ReadUInt64(raw, ref offset);
    }

    private static ushort ReadUInt16(byte[] raw, ref int offset)
    {
        var value = BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(offset, 2));
        offset += 2;
        return value;
    }

    private static uint ReadUInt32(byte[] raw, ref int offset)
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(offset, 4));
        offset += 4;
        return value;
    }

    private static ulong ReadUInt64(byte[] raw, ref int offset)
    {
        var value = BinaryPrimitives.ReadUInt64LittleEndian(raw.AsSpan(offset, 8));
        offset += 8;
        return value;
    }

    private static short ReadInt16(byte[] raw, ref int offset)
    {
        var value = BinaryPrimitives.ReadInt16LittleEndian(raw.AsSpan(offset, 2));
        offset += 2;
        return value;
    }

    private static int ReadInt32(byte[] raw, ref int offset)
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(offset, 4));
        offset += 4;
        return value;
    }

    private static long ReadInt64(byte[] raw, ref int offset)
    {
        var value = BinaryPrimitives.ReadInt64LittleEndian(raw.AsSpan(offset, 8));
        offset += 8;
        return value;
    }

    private static ulong ReadUleb128(byte[] raw, ref int offset)
    {
        ulong result = 0;
        var shift = 0;
        while (offset < raw.Length)
        {
            var b = raw[offset++];
            result |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0)
            {
                return result;
            }

            shift += 7;
        }

        throw new InvalidOperationException("ULEB128 extends past the end of the ELF image.");
    }

    private static long ReadSleb128(byte[] raw, ref int offset)
    {
        long result = 0;
        var shift = 0;
        byte b;
        do
        {
            b = raw[offset++];
            result |= (long)(b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        if (shift < 64 && (b & 0x40) != 0)
        {
            result |= -1L << shift;
        }

        return result;
    }

    private static string ReadNullTerminatedAscii(byte[] raw, ref int offset, int limit)
    {
        var start = offset;
        while (offset < limit && raw[offset] != 0)
        {
            offset++;
        }

        var value = System.Text.Encoding.ASCII.GetString(raw, start, offset - start);
        offset++;
        return value;
    }

    private sealed record CieInfo(string Augmentation, byte FdePointerEncoding, byte LsdaEncoding);
}
