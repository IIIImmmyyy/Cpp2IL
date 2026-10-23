namespace LibCpp2IL;

public enum MetadataUsageType : uint
{
    TypeInfo = 1,
    Type = 2, //REMOVED in v106.1
    MethodDef = 3,
    FieldInfo = 4,
    StringLiteral = 5,
    MethodRef = 6,
    FieldRva = 7,
}
