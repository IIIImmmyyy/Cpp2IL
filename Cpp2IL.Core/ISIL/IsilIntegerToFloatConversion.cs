namespace Cpp2IL.Core.ISIL;

/// <summary>
/// Describes the integer source semantics of an integer-to-floating-point conversion.
/// The destination floating-point width and vector shape remain encoded by the destination operand.
/// </summary>
public readonly struct IsilIntegerToFloatConversion(
    IsilIntegerSignedness signedness,
    int sourceBitWidth) : IsilOperandData
{
    public IsilIntegerSignedness Signedness => signedness;
    public int SourceBitWidth => sourceBitWidth;

    public override string ToString()
    {
        return $"{signedness}, i{sourceBitWidth}";
    }
}
