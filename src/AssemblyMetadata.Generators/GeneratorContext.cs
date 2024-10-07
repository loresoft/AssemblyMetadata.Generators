using Microsoft.CodeAnalysis;

namespace AssemblyMetadata.Generators;

public record GeneratorContext(
    EquatableArray<Diagnostic> Diagnostics,
    EquatableArray<AssemblyConstant> Constants
);
