namespace Cpp2IL.Core.ISIL;

public enum IsilSystemRegister
{
    TPIDR_EL0
}

public readonly struct IsilSystemRegisterOperand(IsilSystemRegister register) : IsilOperandData
{
    public readonly IsilSystemRegister Register = register;

    public override string ToString() => Register.ToString();
}
