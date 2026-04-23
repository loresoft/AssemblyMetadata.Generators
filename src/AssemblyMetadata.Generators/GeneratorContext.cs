namespace AssemblyMetadata.Generators;

public readonly record struct GeneratorContext(
    EquatableArray<AssemblyConstant> Constants
);
