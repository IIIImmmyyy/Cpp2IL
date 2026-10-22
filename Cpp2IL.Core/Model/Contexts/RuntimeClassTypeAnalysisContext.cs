using LibCpp2IL.BinaryStructures;

namespace Cpp2IL.Core.Model.Contexts;

/// <summary>
/// Synthetic type for a value that holds an IL2CPP runtime class pointer (<c>Il2CppClass*</c>) - the
/// value produced by loading a type-metadata global. Such a value is <b>not</b> an instance of the
/// type it describes, nor a <see cref="System.Type"/>; it is the runtime metadata object for that
/// type, passed to runtime helpers such as <c>il2cpp_codegen_object_new</c> or
/// <c>il2cpp_runtime_class_init</c>.
///
/// <see cref="RepresentedType"/> records which managed type the class describes, so later passes can
/// recover it (for example, to type the result of an <c>object_new</c> as an instance of that type).
/// It has no fields of its own, so it never produces (bogus) managed field accesses, and it lowers to
/// <see cref="System.IntPtr"/> in emitted IL since there is no managed type behind it.
/// </summary>
public class RuntimeClassTypeAnalysisContext(TypeAnalysisContext representedType, AssemblyAnalysisContext referencedFrom)
    : ReferencedTypeAnalysisContext(referencedFrom)
{
    /// <summary>The managed type whose runtime class this value points to.</summary>
    public TypeAnalysisContext RepresentedType { get; } = representedType;

    // A pointer-sized runtime handle; there is no Il2CppType enum value for the Il2CppClass struct.
    public override Il2CppTypeEnum Type => Il2CppTypeEnum.IL2CPP_TYPE_I;

    public override string DefaultName => $"Il2CppClass<{RepresentedType.FullName}>";

    public override string DefaultNamespace => "";

    public override bool IsValueType => false;
}
