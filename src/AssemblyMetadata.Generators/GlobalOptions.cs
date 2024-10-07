namespace AssemblyMetadata.Generators;

public record GlobalOptions(
    string? AssemblyName,
    string? DefineConstants,
    string? RootNamespace,
    string? ThisAssemblyNamespace
);
