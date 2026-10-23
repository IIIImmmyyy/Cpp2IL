using LibCpp2IL.BinaryStructures;

namespace Cpp2IL.Core.Model.Contexts;

/// <summary>
/// Synthetic type for a value that holds an IL2CPP runtime method pointer (<c>MethodInfo*</c>) - the
/// value produced by loading a method-metadata global. 
/// </summary>
public class RuntimeMethodInfoAnalysisContext(MethodAnalysisContext representedMethod, AssemblyAnalysisContext referencedFrom)
    : ReferencedTypeAnalysisContext(referencedFrom)
{
    /// <summary>The method whose runtime MethodInfo this value points to.</summary>
    public MethodAnalysisContext RepresentedMethod { get; } = representedMethod;

    // A pointer-sized runtime handle; there is no Il2CppType enum value for the Il2CppMethodInfo struct.
    public override Il2CppTypeEnum Type => Il2CppTypeEnum.IL2CPP_TYPE_I;

    public override string DefaultName => "Il2CppMethodInfo";

    public override string DefaultNamespace => "";

    public override bool IsValueType => false;
}
