using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis;

namespace AssemblyMetadata.Generators;

[ExcludeFromCodeCoverage]
public class GeneratorContext : IEquatable<GeneratorContext>
{
    public GeneratorContext(IEnumerable<Diagnostic> diagnostics, IEnumerable<AssemblyConstant> constants)
    {
        Diagnostics = new EquatableArray<Diagnostic>(diagnostics);
        Constants = new EquatableArray<AssemblyConstant>(constants);
    }

    public EquatableArray<Diagnostic> Diagnostics { get; }

    public EquatableArray<AssemblyConstant> Constants { get; }

    public bool Equals(GeneratorContext other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Diagnostics.Equals(other.Diagnostics)
            && Constants.Equals(other.Constants);
    }

    public override bool Equals(object value) => value is GeneratorContext context && Equals(context);

    public override int GetHashCode() => HashCode.Seed.CombineAll(Diagnostics).CombineAll(Constants);

    public static bool operator ==(GeneratorContext left, GeneratorContext right) => Equals(left, right);

    public static bool operator !=(GeneratorContext left, GeneratorContext right) => !Equals(left, right);
}
