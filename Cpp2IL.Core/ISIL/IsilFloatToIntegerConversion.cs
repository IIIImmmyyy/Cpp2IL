namespace Cpp2IL.Core.ISIL;

public enum IsilFloatToIntegerRoundingMode
{
    TowardZero,
    TowardMinusInfinity,
    TowardPlusInfinity,
    ToNearest,
    ToNearestAwayFromZero
}

public enum IsilIntegerSignedness
{
    Signed,
    Unsigned
}

public readonly struct IsilFloatToIntegerConversion(
    IsilFloatToIntegerRoundingMode roundingMode,
    IsilIntegerSignedness signedness,
    int targetBitWidth) : IsilOperandData
{
    public IsilFloatToIntegerRoundingMode RoundingMode => roundingMode;
    public IsilIntegerSignedness Signedness => signedness;
    public int TargetBitWidth => targetBitWidth;

    public override string ToString()
    {
        return $"{roundingMode}, {signedness}, i{targetBitWidth}";
    }
}
