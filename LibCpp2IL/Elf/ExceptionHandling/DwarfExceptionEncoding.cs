namespace LibCpp2IL.Elf.ExceptionHandling;

internal static class DwarfExceptionEncoding
{
    public const byte Omit = 0xFF;
    public const byte Absptr = 0x00;

    public static byte WithoutRelative(byte encoding) => (byte)(encoding & ~0x70);
}
